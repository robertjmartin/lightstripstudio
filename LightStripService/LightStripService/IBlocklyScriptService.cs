namespace ChristmasLightServer
{
    public interface IBlocklyScriptService
    {
        public Task<IEnumerable<BlocklyScriptEntry>> GetBlocklyScripts();
        public Task<BlocklyScript> Get(Guid id);
        public Task<bool> Save(Guid id, BlocklyScript script);
    }
}
