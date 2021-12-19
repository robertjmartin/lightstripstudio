namespace ChristmasLightServer
{
    public class ScriptStream : IDisposable
    {
        private readonly ScriptParser _parser;
        private readonly ILogger _logger;
        private readonly Queue<ICommand> _commands;

        public ScriptStream(ILogger logger, string script)
        {
            _logger = logger;
            _commands = new Queue<ICommand>();
            _parser = new ScriptParser(_logger);
            _parser.StartScript(script);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // todo make more async
        public IEnumerable<ICommand> ReadBytes(int byteCount)
        {
            while (byteCount > 10)
            {
                _commands.TryDequeue(out ICommand? cmd);

                if (cmd != null)
                {
                    byteCount -= cmd.Length;

                    yield return cmd;
                }
                else
                {
                    var newCommands = _parser.GetCommandsFromScript();

                    if (newCommands.Any())
                    {
                        foreach (var newCmd in newCommands )
                        {
                            _commands.Enqueue(newCmd);
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"End of script stream");
                        yield break;
                    }                    
                }
            }
        }        
    }
}
