namespace Server.Helper;

/// <summary>
/// Hsv Color
/// </summary>
/// <param name="H">Hue in [0, 360]</param>
/// <param name="S">Saturation in [0, 100]</param>
/// <param name="V">Value/Brightness in [0, 100]</param>
public class ColorHsv(double H, double S, double V)
{
    /// <summary>
    /// Hue in [0, 360]
    /// </summary>
    public double H { get; set; } = H;

    /// <summary>
    /// Saturation in [0, 100]
    /// </summary>
    public double S { get; set; } = S;

    /// <summary>
    /// Value/Brightness in [0, 100]
    /// </summary>
    public double V { get; set; } = V;
}

public static class ColorHsvExtensions
{
    /// <summary>
    /// Lerps
    /// </summary>
    /// <param name="x">In [0, 1]</param>
    /// <returns></returns>
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
