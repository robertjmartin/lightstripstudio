namespace ChristmasLightServer
{
    public class StartStreamCommand : ICommand
    {
        private readonly string _streamId;

        public StartStreamCommand(string streamId)
        {
            if (streamId.Length != 5)
            {
                throw new ArgumentException("streamId");
            }
            _streamId = streamId;
        }

        public int Length
        {
            get
            {
                return 6;
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            yield return (byte)(0x30);
            yield return (byte)_streamId[0];
            yield return (byte)_streamId[1];
            yield return (byte)_streamId[2];
            yield return (byte)_streamId[3];
            yield return (byte)_streamId[4];
        }
    }
}
