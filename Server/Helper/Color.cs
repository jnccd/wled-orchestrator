namespace Server.Helper
{
    public record ColorRgb(byte R, byte G, byte B);

    public static class ColorRgbExtensions
    {
        public static string ToHex(this ColorRgb c) => $"{c.R:X2}{c.G:X2}{c.B:X2}";
        public static ColorRgb Lerp(this ColorRgb c, ColorRgb b, double x) =>
            new((byte)(c.R * (1 - x) + b.R * x),
                (byte)(c.G * (1 - x) + b.G * x),
                (byte)(c.B * (1 - x) + b.B * x));

        public static ColorHsv RgbToHSV(this ColorRgb color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            var hue = color.GetHue();
            var saturation = (max == 0) ? 0 : 100d - (100d * min / max);
            var value = max / 2.55;

            return new ColorHsv(hue, saturation, value);
        }

        public static double GetHue(this ColorRgb color)
        {
            var r = color.R / 255d;
            var g = color.G / 255d;
            var b = color.B / 255d;

            var indexedColorValList = new double[] { r, g, b }.WithIndex();
            var max = indexedColorValList.MaxBy(x => x.item);
            var min = indexedColorValList.MinBy(x => x.item);

            double hue = 0;
            if (max.index == 0)
                hue = (g - b) / (max.item - min.item);
            else if (max.index == 1)
                hue = 2.0 + (b - r) / (max.item - min.item);
            else if (max.index == 2)
                hue = 4.0 + (r - g) / (max.item - min.item);

            hue *= 60;
            if (hue < 0)
                hue += 360;

            return hue;
        }
    }

    /// <summary>
    /// Hsv Color
    /// </summary>
    /// <param name="H">Hue in [0, 360]</param>
    /// <param name="S">Saturation in [0, 100]</param>
    /// <param name="V">Value/Brightness in [0, 100]</param>
    public record ColorHsv(double H, double S, double V);

    public static class ColorHsvExtensions
    {
        public static ColorHsv Lerp(this ColorHsv c, ColorHsv b, double x) => c.HsvToRgb().Lerp(b.HsvToRgb(), x).RgbToHSV();

        public static ColorRgb HsvToRgb(this ColorHsv Hsv) // from https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        {
            double H = Hsv.H;
            double S = Hsv.S / 100;
            double V = Hsv.V / 100;

            while (H < 0) { H += 360; }
            ;
            while (H >= 360) { H -= 360; }
            ;
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    // Red is the dominant color
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.
                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            return new ColorRgb(
                (byte)(double.Clamp(R, 0, 1) * 255.0),
                (byte)(double.Clamp(G, 0, 1) * 255.0),
                (byte)(double.Clamp(B, 0, 1) * 255.0));
        }
    }
}
