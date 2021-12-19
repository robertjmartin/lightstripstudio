using Microsoft.AspNetCore.Mvc;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/stream/{id}")]
    public class StreamController : ControllerBase
    {
        private readonly ILogger<StreamController> _logger;
        private readonly ILightService _lightService;

        public StreamController(ILogger<StreamController> logger, ILightService lightService)
        {
            _logger = logger;
            _lightService = lightService;
        }

        [HttpGet]
        public IActionResult Get(string id)
        {
            try
            {
                var maxResponseSizeHeader = Request.Headers["max-response-size"];
                if (maxResponseSizeHeader.Count != 1) throw new ArgumentException("maxResponseSizeHeader");   
                var maxResponseSize = Convert.ToInt32(maxResponseSizeHeader.First());
                return File( _lightService.GetStreamData(id, maxResponseSize), "application/octet-stream");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}