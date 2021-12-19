namespace ChristmasLightServer
{
    public class DelayCommand : ICommand
    {
        private UInt16 _delay;

        public DelayCommand(UInt16 delay)
        {
            _delay = delay;
        }               

        public int Length
        { 
            get 
            {
                return 2;
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            var delayBytes = BitConverter.GetBytes(_delay);

            yield return (byte)(0x10);
            yield return delayBytes[1];
            yield return delayBytes[0]; 
        }
    }
}
