namespace ChristmasLightServer
{
    public interface ICommandSender
    {
        public bool IsConnected();
        public bool SendLightCommand(int id, Color color);
        public bool SendCommands(IEnumerable<ICommand> command);
        public void CancelSend();
        public void ResetBytesFree();
    }
}
