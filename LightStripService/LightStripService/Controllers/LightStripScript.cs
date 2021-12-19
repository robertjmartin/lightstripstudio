using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/lightstrip/script")]
    public class LightStripScriptController : ControllerBase
    {
        private readonly ILogger<LightStripScriptController> _logger;
        private readonly ILightService _service;

        public LightStripScriptController(ILogger<LightStripScriptController> logger, ILightService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public ActionResult Push([FromBody] JsonElement body, [FromHeader] bool stream)
        {
            try
            {
                var script = body.GetProperty("Script").Deserialize<string>();

                if (stream)
                {
                    return Ok(_service.StartStreamFromScript(script));
                }
                else
                {
                    _service.QueueStripScript(script);
                    return Ok();
                }
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
