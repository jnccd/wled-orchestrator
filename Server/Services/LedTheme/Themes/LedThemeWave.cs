using Server.Endpoints;
using Server.Helper;
namespace Server.Services.LedTheme.Themes;

public class LedThemeWave() : LedTheme(LedThemePreviewType.Min)
{
    public override LedGroupState? GetNewState(LedThemeInput input)
    {
        var dayTimePercent = input.Time.TimeOfDay.TotalMilliseconds / 10000 * WaveSpeed;
        return new(GetColors(dayTimePercent));
    }

    readonly int ColorArrayResolution = 300;
    [GenerateFrontendForm]
    public ColorHsv Color1 { get; set; } = new ColorRgb(142, 215, 253).RgbToHSV();

    [GenerateFrontendForm]
    public ColorHsv Color2 { get; set; } = new ColorRgb(252, 200, 20).RgbToHSV();

    [GenerateFrontendForm]
    public double WaveLength { get; set; } = 100;

    [GenerateFrontendForm]
    public double WaveSpeed { get; set; } = 100;

    public ColorHsv[] GetColors(double waveTime)
    {
        // Create Color Array
        ColorHsv[] colors = new ColorHsv[ColorArrayResolution];
        for (int i = 0; i < ColorArrayResolution; i++)
        {
            var waveState = Math.Cos((double)(waveTime + (i * WaveLength / ColorArrayResolution)) * 2 * Math.PI / WaveLength) / 2 + 0.5;
            colors[i] = Color1.Lerp(Color2, waveState);
        }

        return colors;
    }
}