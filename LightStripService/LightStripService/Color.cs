namespace ChristmasLightServer
{
    public class Color
    {
        public static Color RedColor
        {
            get
            {
                return new Color(255, 0, 0);
            }
        }

        public static Color GreenColor
        {
            get
            {
                return new Color(0, 255, 0);
            }
        }

        public static Color BlueCloor
        {
            get
            {
                return new Color(0, 0, 255);
            }
        }

        public static bool operator== (Color color1, Color color2)
        {
            return (color1.Red == color2.Red && 
                    color1.Green == color2.Green &&
                    color1.Blue == color2.Blue);
        }

        public static bool operator !=(Color color1, Color color2)
        {
            return (color1.Red != color2.Red ||
                    color1.Green != color2.Green ||
                    color1.Blue != color2.Blue);
        }

        public byte Red { get; set; }
        public byte Green{ get; set; }
        public byte Blue { get; set; }

        public Color(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public static Color FromHSV(UInt16 hue, UInt16 saturation, UInt16 value)
        {           
            byte r, g, b;

            hue %= 360;
            var rgb_max = (Byte)(Math.Ceiling(value * 2.55));
            var rgb_min = (Byte)(rgb_max * (100 - saturation) / 100.0);

            var i = (UInt16)(hue / 60);
            var diff = (UInt16)(hue % 60);

            var rgb_adj = (Byte)((rgb_max - rgb_min) * (diff / 60));

            switch (i)
            {
                case 0:
                    r = rgb_max;
                    g = (Byte)(rgb_min + rgb_adj);
                    b = rgb_min;
                    break;
                case 1:
                    r = (Byte)(rgb_max - rgb_adj);
                    g = rgb_max;
                    b = rgb_min;
                    break;
                case 2:
                    r = rgb_min;
                    g = rgb_max;
                    b = (Byte)(rgb_min + rgb_adj);
                    break;
                case 3:
                    r = rgb_min;
                    g = (Byte)(rgb_max - rgb_adj);
                    b = rgb_max;
                    break;
                case 4:
                    r = (Byte)(rgb_min + rgb_adj);
                    g = rgb_min;
                    b = rgb_max;
                    break;
                default:
                    r = rgb_max;
                    g = rgb_min;
                    b = (Byte)(rgb_max - rgb_adj);
                    break;
            }

            return new Color(r, g, b);
        }

        public string AsHexString
        {
            get
            {
                return $"#{Red:X}{Green:X}{Blue:X}";
            }
        }
    }
}
