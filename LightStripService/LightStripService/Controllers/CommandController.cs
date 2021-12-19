using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/command")]
    public class CommandController : ControllerBase, ICommandSender
    {
        private readonly ILogger<CommandController> _logger;
        private readonly ILightService _service;
        private readonly IDataCounter _dataCounter;
        private WebSocket? _webSocket;
        private UInt16 _bytesFree;
        private Mutex _bytesFreeMutex;
        private EventWaitHandle _bytesFreeEvent;
        private EventWaitHandle _cancelSendEvent;

        public CommandController(ILogger<CommandController> logger, ILightService service, IDataCounter dataCounter)
        {
            _logger = logger;
            _service = service;
            _dataCounter = dataCounter;
            _webSocket = null;
            _bytesFreeMutex = new Mutex(false);
            _bytesFreeEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _cancelSendEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _bytesFree = 60000;

            _dataCounter.startReporting();
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _service.RegisterCommand(this);
                await Echo(HttpContext, _webSocket);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        public bool IsConnected()
        {
            return _webSocket is not null && !_webSocket.CloseStatus.HasValue;
        }
        
        public void CancelSend()
        {
            _cancelSendEvent.Set();
        }

        public void ResetBytesFree()
        {
            _bytesFree = 60000;
        }

        private bool SendCommandsInternal(IEnumerable<byte> dataEnum)
        {
            var data = dataEnum.ToArray();
            if (!SendData(data))
            {
                return false;
            }
            _dataCounter.logBytes(data.Length);
            return true;
        }

        public bool SendCommands(IEnumerable<ICommand> commands)
        {
            _cancelSendEvent.Reset();

            List<byte> dataList = new List<byte>() { };

            var commandEnum = commands.GetEnumerator();

            while (commandEnum.MoveNext())
            {
                dataList.AddRange(commandEnum.Current.ToBytes());
            
                if (dataList.Count > 2000)
                {
                   SendCommandsInternal(dataList);
                    dataList.Clear();
                }
            }

            if (dataList.Count > 0)
            {
                SendCommandsInternal(dataList);
            }
            return true;
        }

        private bool SendData (byte[] data)
        {
            try
            {
                while(true)
                {
                    _bytesFreeMutex.WaitOne();
                    //_logger.LogInformation($"trying to send {data.Length} bytesFree = {_bytesFree}");
                    bool free = _bytesFree > data.Length + 1000;
                    if (free)
                    {
                        _bytesFree = (UInt16)(_bytesFree - (UInt16)data.Length);
                        _bytesFreeEvent.Reset();
                        //_logger.LogInformation($"updating bytesFree to {_bytesFree} on send.");
                    }

                    _bytesFreeMutex.ReleaseMutex();

                    if (!free)
                    {
                        //_logger.LogInformation("waiting for bytes free event");
                        if (1 == EventWaitHandle.WaitAny(new [] { _bytesFreeEvent, _cancelSendEvent }))
                        {
                            _logger.LogInformation("cancel send event");
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (_webSocket is not null)
                {
                    Task sendTask = _webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
                    sendTask.Wait();

                    return sendTask.IsCompletedSuccessfully;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool SendLightCommand(int id, Color color)
        {
            try
            {
                if (_webSocket is not null)
                {
                    var buffer = new byte[5];

                    buffer[0] = 0;
                    buffer[1] = (byte)id;
                    buffer[2] = (byte)color.Red;
                    buffer[3] = (byte)color.Green;
                    buffer[4] = (byte)color.Blue;

                    Task sendTask = _webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, 5), WebSocketMessageType.Binary, true, CancellationToken.None);
                    sendTask.Wait();

                    return sendTask.IsCompletedSuccessfully;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    _bytesFreeMutex.WaitOne();
                    _bytesFree += (UInt16)(buffer[1] << 8);
                    _bytesFree += buffer[0];
                    //_logger.LogInformation($"BytesFreeEvent bytesfree now = {_bytesFree}");
                    _bytesFreeEvent.Set();
                    _bytesFreeMutex.ReleaseMutex();
                    

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
