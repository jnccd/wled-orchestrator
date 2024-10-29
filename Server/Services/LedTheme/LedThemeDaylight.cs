using Server.Helper;
namespace Server.Services.LedTheme;

public class LedThemeDaylight : LedTheme
{
    public override LedSegmentState? GetNewState(LedThemeInput input)
    {
        var dayTimePercent = input.Time.TimeOfDay.TotalDays;
        return new(GetColors(dayTimePercent), GetBrightness(dayTimePercent));
    }

    // Code below is adapted from legacy wledOrchestrator proj
    readonly Color SkyColor = new(142, 215, 253);
    readonly Color SunColor = new(252, 200, 20);
    readonly int ColorArrayResolution = 300;
    static readonly double sunRise = 0.3;
    static readonly double sunTime = 0.5;

    static readonly double halfSunTime = sunTime / 2;
    static readonly double sunTop = sunRise + halfSunTime;
    static readonly double sunSet = sunRise + sunTime;

    static readonly double dayLightFunMult = -Math.Log2(0.05) / (halfSunTime * halfSunTime);
    readonly Func<double, double> DayLightFunction = (x) => Math.Pow(2, -((x - sunTop) * (x - sunTop)) * dayLightFunMult);

    readonly double invertedSunSize = 150;

    public byte GetBrightness(double curDayTimePercent)
    {
        // Set Brightness
        var funOut = DayLightFunction(curDayTimePercent);
        var bri = (funOut * 255) + 0;

        return (byte)bri;
    }

    public Color[] GetColors(double curDayTimePercent)
    {
        // Create Color Array
        Color[] colors = new Color[ColorArrayResolution];
        for (int i = 0; i < ColorArrayResolution; i++)
        {
            var sunRiseDayTime = curDayTimePercent / sunTime - sunRise;

            var x = (i / (double)ColorArrayResolution) - sunRiseDayTime;
            var gaussianSun = Math.Exp(-(x * x) * invertedSunSize);
            colors[i] = SkyColor.Lerp(SunColor, gaussianSun);
        }

        return colors;
    }
}