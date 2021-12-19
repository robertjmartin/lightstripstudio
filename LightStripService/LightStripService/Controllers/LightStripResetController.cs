using Microsoft.AspNetCore.Mvc;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/lightstrip/reset")]
    public class LightStripResetController : ControllerBase
    {
        private readonly ILogger<LightStripScriptController> _logger;
        private readonly ILightService _service;

        public LightStripResetController(ILogger<LightStripScriptController> logger, ILightService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public ActionResult Push()
        {
            _service.Reset();
            return Ok();
        }
    }
}
