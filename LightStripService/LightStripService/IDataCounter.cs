namespace ChristmasLightServer
{
    public interface IDataCounter
    {
        public void startReporting();
        public void logBytes(int byteCount);
    }
}
