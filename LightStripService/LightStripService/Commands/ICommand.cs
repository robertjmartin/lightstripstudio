namespace ChristmasLightServer
{
    /* High 4 bits of first byte are command id
    
    0x0x Set Light Color
    0x1x Delay
    0x2x Reset
    0x3x Start Stream
    
    */

    public interface ICommand
    {
        public IEnumerable<byte> ToBytes();
        public int Length { get; }
    }
}
