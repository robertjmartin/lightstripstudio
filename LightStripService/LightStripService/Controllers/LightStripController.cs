using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/lightstrip")]
    public class LightStripController : ControllerBase
    {
        private readonly ILogger<LightStripController> _logger;
        private readonly ILightService _service;

        public LightStripController(ILogger<LightStripController> logger, ILightService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public ActionResult Push(int id, [FromBody] JsonElement body)
        {
            var newColors = body.GetProperty("NewColors").Deserialize<List<SetLightColorCommand>>();
            _service.QueueStripChange(newColors);
            return Ok();
        }
    }
}
