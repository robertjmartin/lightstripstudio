using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChristmasLightServer.Controllers
{
    [ApiController]
    [Route("api/BlocklyScript/{id?}")]
    public class BlocklyScriptController : ControllerBase
    {
        private readonly ILogger<BlocklyScriptController> _logger;
        private readonly IBlocklyScriptService _service;
        public BlocklyScriptController(ILogger<BlocklyScriptController> logger, IBlocklyScriptService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult> Get(string id)
        {
            try
            {
                var script = await _service.Get(new Guid(id));

                return Ok(JsonSerializer.Serialize<BlocklyScript>(script));
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpPut]
        public ActionResult Put(string? id, [FromBody] JsonElement body)
        {
            try
            {
                var name = body.GetProperty("Name").Deserialize<string>();
                var script = body.GetProperty("Script").Deserialize<string>();

                if (name is null || script is null)
                {
                    return BadRequest();
                }
      
                Guid guid = id is null ? Guid.NewGuid() : Guid.Parse(id);
                _service.Save(guid, new BlocklyScript() { guid = guid, Name = name, Script = script });
                return Ok(guid.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
   
        }

    }
}
