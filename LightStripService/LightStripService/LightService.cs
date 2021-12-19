using MoreLinq;

namespace ChristmasLightServer
{ 
    public class LightService : ILightService
    {
        private readonly Queue<IEnumerable<SetLightColorCommand>> _lightStripChangeQueue;
        private readonly Queue<string> _lightStripScriptQueue;
        private readonly Thread _worker;
        private readonly ILogger<LightService> _logger;
        private readonly EventWaitHandle _newWorkEvent;
        private readonly EventWaitHandle _commandRegisteredEvent;
        private readonly EventWaitHandle _resetEvent;
        private readonly Mutex _workAvailableMutex;

        private readonly Dictionary<string, ScriptStream> _streams;

        private ICommandSender? _commandSender;
    
        public LightService(ILogger<LightService> logger)
        {
            _logger = logger;
            _lightStripChangeQueue = new Queue<IEnumerable<SetLightColorCommand>>();
            _lightStripScriptQueue = new Queue<string>();
            _streams = new Dictionary<string, ScriptStream>();
            _newWorkEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _commandRegisteredEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _resetEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            _commandSender = null;            
            _workAvailableMutex = new Mutex(false);

            _worker = new Thread(new ThreadStart(this.Worker));
            _worker.Start();
        }

        public void RegisterCommand(ICommandSender command)
        {
            _logger.LogInformation("RegisterCommandSender");
            _commandSender = command;
            _commandRegisteredEvent.Set();
        }

        public void QueueStripChange(IEnumerable<SetLightColorCommand> colors)
        {
            _workAvailableMutex.WaitOne();
            _lightStripChangeQueue.Enqueue(colors);
            _logger.LogInformation($"SettingNewWorkEvent");
            _newWorkEvent.Set();
            _workAvailableMutex.ReleaseMutex();
        }

        public void QueueStripScript(string script)
        {
            Reset();
            _workAvailableMutex.WaitOne();
            _lightStripScriptQueue.Enqueue(script);
            _logger.LogInformation($"SettingNewWorkEvent");
            _newWorkEvent.Set();
            _workAvailableMutex.ReleaseMutex();
        }

        public void Reset()
        {
            _logger.LogInformation("Starting reset");
            if (_commandSender is not null ) _commandSender.CancelSend();
            _lightStripChangeQueue.Clear();
            _lightStripScriptQueue.Clear();
            _resetEvent.Set();
        }

        private void SendCommand(ICommand command)
        {
            SendCommands(new[] { command });
        }

        private void SendCommands(IEnumerable<ICommand> commands)
        {
            bool sent = false;

            while (!sent)
            {
                if (_resetEvent.WaitOne(0) == true)
                {
                    return;
                }

                if (_commandSender is null)
                {
                    _logger.LogError("CommandSenderOffline");
                    _commandRegisteredEvent.WaitOne();
                }
                else if (!_commandSender.IsConnected())
                {
                    _commandSender = null;
                }
                else
                {
                    var commandBatches = commands.Batch(400);

                    foreach (var batch in commandBatches)
                    {
                        if (_resetEvent.WaitOne(0) == true)
                        {
                            return;
                        }
                        _commandSender.SendCommands(batch);                        
                    }
                    sent = true;
                }
            }
        }

        void RunLightStripScript(string script)
        {
            _logger.LogInformation("starting script");
            var parser = new ScriptParser(_logger);

            parser.StartScript(script);

            IEnumerable<ICommand> commands;

            while (true)
            {
                if (_resetEvent.WaitOne(0) == true)
                {
                    return;
                }

                commands = parser.GetCommandsFromScript();
                if (!commands.Any())
                {
                    break;
                }

                SendCommands(commands);                
            }
        }

        private bool isMoreWork()
        {
            bool moreWork = false;
            _workAvailableMutex.WaitOne();
            if (_lightStripChangeQueue.Count > 0 || _lightStripScriptQueue.Count > 0)
            {
                moreWork = true;
            }
            _workAvailableMutex.ReleaseMutex();
            return moreWork;
        }

        private void Worker()
        {
            while (true)
            {
                if (_resetEvent.WaitOne(0) == true)
                {
                    _logger.LogInformation("sending stopped");
                    _resetEvent.Reset();
                    if (_commandSender is not null)
                    {
                        _logger.LogInformation("waiting 500");
                        _commandSender.SendCommands(new[] { new ResetCommand() });
                        _commandSender.ResetBytesFree();
                        Thread.Sleep(500);
                    }
                }                
                   
                if (_lightStripScriptQueue.Count > 0)
                {
                    var script = _lightStripScriptQueue.Dequeue();
                    RunLightStripScript(script);
                }

                if (_lightStripChangeQueue.Count > 0)
                {
                    var newColors = _lightStripChangeQueue.Dequeue();
                    SendCommands(newColors);
                }

                if (isMoreWork())
                {
                    continue;
                }
                               
                _newWorkEvent.WaitOne(100);
            }
        }

        public string StartStreamFromScript(string script)
        {
            var scriptStream = new ScriptStream(_logger, script);
            var id = Guid.NewGuid().ToString().Substring(0,5);
            _streams.Add(id, scriptStream);

            if (_commandSender != null)
            {
                SendCommand(new StartStreamCommand(id));
            }

            return id;
        }

        public byte[] GetStreamData(string streamId, int byteCount)
        {
            var commands = _streams[streamId].ReadBytes(byteCount);

            var data = new List<byte>();
            foreach (var command in commands)
            {
                data.AddRange(command.ToBytes());
            }

            return data.ToArray();
        }
    }
}
