namespace Server.Helper;

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
