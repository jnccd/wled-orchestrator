namespace Server.Helper
{
    public record Color(byte R, byte G, byte B);

    public static class ColorExtensions
    {
        public static string ToHex(this Color c) => $"{c.R:X2}{c.G:X2}{c.B:X2}";
        public static Color Lerp(this Color c, Color b, double x) =>
            new((byte)(c.R * (1 - x) + b.R * x),
                (byte)(c.G * (1 - x) + b.G * x),
                (byte)(c.B * (1 - x) + b.B * x));
    }
}
