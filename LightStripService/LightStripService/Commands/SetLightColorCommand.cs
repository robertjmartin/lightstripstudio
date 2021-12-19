namespace ChristmasLightServer
{
    public class SetLightColorCommand : ICommand
    {
        public SetLightColorCommand(UInt16 id, Color color)
        {
            // two bits reserved for command id
            if ((id & 0xC000) != 0)
            {
                throw new ArgumentException("LightColor: Invalid ID");
            }

            Id = id;
            Color = color;
        }
        public UInt16 Id { get; private set; }
        public Color Color { get; private set; }

        public int Length
        {
            get
            {
                return 5;
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            var idBytes = BitConverter.GetBytes(Id);

            yield return idBytes[1];
            yield return idBytes[0];
            yield return Color.Red;
            yield return Color.Green;
            yield return Color.Blue;
        }
    }
}
