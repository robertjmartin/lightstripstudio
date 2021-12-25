using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChristmasLightServer
{  
    public struct BlocklyScriptEntry
    {
        public BlocklyScriptEntry(BlocklyScript script)
        {
            guid = script.guid;
            Name = script.Name;
        }

        [JsonInclude]
        public Guid guid { get; set; }

        [JsonInclude]
        public string Name { get; set; }

        [JsonIgnore]
        public string ScriptFileName
        {
            get
            {
                return guid.ToString() + ".blocklyScript"; ;
            }
        }

        [JsonIgnore]
        public string EntryFileName
        {
            get
            {
                return guid.ToString() + ".json";
            }
        }
    }

    public struct BlocklyScript
    { 
        [JsonInclude]
        public Guid guid { get; set; }

        [JsonInclude]
        public string Name { get; set; }

        [JsonInclude]
        public string Script { get; set; }
    }

    public class BlocklyScriptService : IBlocklyScriptService
    {
        private readonly ILogger<BlocklyScriptService> _logger;
        private readonly string _rootPath;
        private readonly Dictionary<Guid, BlocklyScriptEntry> _entrys;
        private bool _initialized = false;

        public BlocklyScriptService(ILogger<BlocklyScriptService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _rootPath = configuration["bspath"];
            _entrys = new Dictionary<Guid, BlocklyScriptEntry>();
        }

        public async Task<BlocklyScript> Get(Guid id)
        {
            if (!_initialized)
            {
                await LoadEntries();
                _initialized = true;
            }

            if (_entrys.TryGetValue(id, out BlocklyScriptEntry entry))
            {
                var script = JsonSerializer.Deserialize<BlocklyScript>(await File.ReadAllTextAsync(Path.Combine(_rootPath, entry.ScriptFileName)));
                return script;
            }
            throw new FileNotFoundException();
        }

        public async Task<IEnumerable<BlocklyScriptEntry>> GetBlocklyScripts()
        {
            if (!_initialized)
            {
                await LoadEntries();
                _initialized = true;
            }
            return _entrys.Values;
        }

        public async Task<bool> Save(Guid id, BlocklyScript script)
        {
            if (!_initialized)
            {
                await LoadEntries();
                _initialized = true;
            }

            try
            {
                var entry = new BlocklyScriptEntry(script);

                _entrys[id] = entry;
                await File.WriteAllTextAsync(Path.Combine(_rootPath, entry.EntryFileName), JsonSerializer.Serialize(entry));
                await File.WriteAllTextAsync(Path.Combine(_rootPath, entry.ScriptFileName), JsonSerializer.Serialize(script));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        private async Task LoadEntries()
        {
            var timer = new Stopwatch();
            timer.Start();
            string ScriptPathRoot = _rootPath;
            var entryFiles = Directory.EnumerateFiles(ScriptPathRoot, "*.json");

            foreach (var entryFile in entryFiles)
            {
                var text = await File.ReadAllTextAsync(entryFile);
                var entry = JsonSerializer.Deserialize<BlocklyScriptEntry>(text);
                _entrys.Add(entry.guid, entry);
            }
            timer.Stop();
            _logger.LogInformation($"Loaded {_entrys.Count} entries in {timer.ElapsedMilliseconds}ms.");
        }
    }
}
