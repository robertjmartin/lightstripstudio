using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/BlocklyScripts")]
    public class BlocklyScriptsController : ControllerBase
    {
        private readonly ILogger<BlocklyScriptsController> _logger;
        private readonly IBlocklyScriptService _service;

        public BlocklyScriptsController(ILogger<BlocklyScriptsController> logger, IBlocklyScriptService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var enties = await _service.GetBlocklyScripts();
            var result = JsonSerializer.Serialize(enties);
            return Ok(result);
        }
    }
}
