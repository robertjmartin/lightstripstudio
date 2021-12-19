namespace ChristmasLightServer
{
    public class ResetCommand : ICommand
    {
        public ResetCommand()
        {
        }

        public int Length
        {
            get
            {
                return 1;
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            yield return (byte)(0x20);
        }
    }
}
