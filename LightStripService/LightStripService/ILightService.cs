namespace ChristmasLightServer
{
    public interface ILightService
    {
        public void QueueStripChange(IEnumerable<SetLightColorCommand> colors);
        public void QueueStripScript(string script);   
        public void Reset();
        public void RegisterCommand(ICommandSender command);
        public string StartStreamFromScript(string script);
        public byte[] GetStreamData(string streamId, int byteCount);
    }
}
