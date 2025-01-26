using Server.Helper;
namespace Server.Services.LedTheme.Themes;

public class LedThemeDaylight : LedTheme
{
    public override LedGroupState? GetNewState(LedThemeInput input)
    {
        var dayTimePercent = input.Time.TimeOfDay.TotalDays;
        return new(GetColors(dayTimePercent));
    }

    public ColorHsv SkyColor { get; set; } = new ColorRgb(142, 215, 253).RgbToHSV();
    public ColorHsv SunColor { get; set; } = new ColorRgb(252, 200, 20).RgbToHSV();
    readonly int ColorArrayResolution = 300;
    static readonly double sunRise = 0.3;
    static readonly double sunTime = 0.5;

    static readonly double halfSunTime = sunTime / 2;
    static readonly double sunTop = sunRise + halfSunTime;
    static readonly double sunSet = sunRise + sunTime;

    static readonly double dayLightFunMult = -Math.Log2(0.05) / (halfSunTime * halfSunTime);
    readonly Func<double, double> DayLightFunction = (x) => Math.Pow(2, -((x - sunTop) * (x - sunTop)) * dayLightFunMult);

    public double InvertedSunSize { get; set; } = 150;

    public ColorHsv[] GetColors(double curDayTimePercent)
    {
        // Create Color Array
        ColorHsv[] colors = new ColorHsv[ColorArrayResolution];
        for (int i = 0; i < ColorArrayResolution; i++)
        {
            var sunRiseDayTime = curDayTimePercent / sunTime - sunRise;

            var x = i / (double)ColorArrayResolution - sunRiseDayTime;
            var gaussianSun = Math.Exp(-(x * x) * InvertedSunSize);
            colors[i] = SkyColor.Lerp(SunColor, gaussianSun);
            var funOut = DayLightFunction(curDayTimePercent);
            colors[i] = new ColorHsv(colors[i].H, colors[i].S, colors[i].V * funOut);
        }

        return colors;
    }
}