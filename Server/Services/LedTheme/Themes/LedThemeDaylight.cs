using Server.Endpoints;
using Server.Helper;
namespace Server.Services.LedTheme.Themes;

/// <summary>
/// A virtual window on a spring sky. The sun is placed on the strip by its real compass bearing as
/// seen through the window, and the sky brightness follows the suns height above the horizon. Arc and
/// brightness are both computed for a fixed spring day, so the window stays bright on early winter
/// evenings, and both are driven by UTC so daylight savings cannot shift the sun.
/// </summary>
public class LedThemeDaylight : LedTheme
{
    public override LedGroupState? GetNewState(LedThemeInput input)
    {
        // Local time carries the DST offset; the sun does not. Deriving the sky from the real UTC
        // instant keeps the arc the same before and after a clock change.
        var sun = SolarPositionCalculator.Calculate(
            Latitude, Longitude, (int)Math.Round(BaseDayOfYear),
            input.Time.ToUniversalTime().TimeOfDay,
            Math.Clamp(ExtraDaylightMinutes, 0, 240) / 60);
        return new(GetColors(sun, out var defaultColor), DefaultColor: defaultColor);
    }

    [GenerateFrontendForm]
    public ColorHsv SkyColor { get; set; } = new ColorRgb(142, 215, 253).RgbToHSV();

    // The sky takes on this colour near the sun while the sun is low (the orange of a sunrise/sunset).
    [GenerateFrontendForm]
    public ColorHsv SunsetSkyColor { get; set; } = new ColorRgb(255, 130, 50).RgbToHSV();

    [GenerateFrontendForm]
    public ColorHsv SunColor { get; set; } = new ColorRgb(252, 200, 20).RgbToHSV();

    // The sun turns this colour while it sits near the horizon (the red of a real sunrise/sunset).
    [GenerateFrontendForm]
    public ColorHsv SunsetColor { get; set; } = new ColorRgb(255, 80, 20).RgbToHSV();

    // Where the window is and which way it faces. Azimuth is in compass degrees: 0 = north,
    // 90 = east, 180 = south, 270 = west (so 315 is a north-west window).
    [GenerateFrontendForm(InputType: "number-input", MinValue: -90, MaxValue: 90)]
    public double Latitude { get; set; } = 48;

    [GenerateFrontendForm(InputType: "number-input", MinValue: -180, MaxValue: 180)]
    public double Longitude { get; set; } = 11.5;

    [GenerateFrontendForm(InputType: "number-input", MinValue: 0, MaxValue: 360)]
    public double WindowAzimuth { get; set; } = 180;

    // How much sky the window shows: the strip spans this many degrees centered on WindowAzimuth.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 10, MaxValue: 360)]
    public double WindowFieldOfView { get; set; } = 180;

    // The sun arc (and with it the brightness curve) is computed for this day of the year instead of
    // today, so a spring day keeps the window bright in early winter evenings.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 1, MaxValue: 365)]
    public double BaseDayOfYear { get; set; } = 120;

    // Stretches the day beyond what the base date can give: the sun rises this many minutes earlier
    // and sets this many minutes later, keeping the same sunrise/sunset bearings and noon height, so
    // the window stays bright longer without changing the shape of the arc. 0 = the real day.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 0, MaxValue: 240)]
    public double ExtraDaylightMinutes { get; set; } = 0;

    // Angular sigma of the suns glow in degrees of sky. A wider window fits more sky around it, so
    // the same size covers fewer leds.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 1, MaxValue: 90)]
    public double SunSize { get; set; } = 25;

    // Angular sigma of the warm sky lobe in degrees. Much wider than the sun, so the sunset side of
    // the window goes orange while the rest keeps its colour; set it wide (e.g. 180) to warm the
    // whole window at dawn/dusk.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 10, MaxValue: 360)]
    public double SkyWarmthSize { get; set; } = 45;

    // How many degrees the sun has to climb above the horizon before the red sun and the warm sky are
    // back to normal, i.e. how long the sunrise/sunset colour phase lasts.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 1, MaxValue: 45)]
    public double ReddeningHeight { get; set; } = 6;

    // Sky brightness in percent: before/after the twilight, when the sun touches the horizon and at
    // the highest point of the day.
    [GenerateFrontendForm(InputType: "number-input", MinValue: 0, MaxValue: 100)]
    public double NightBrightness { get; set; } = 1;

    [GenerateFrontendForm(InputType: "number-input", MinValue: 0, MaxValue: 100)]
    public double HorizonBrightness { get; set; } = 40;

    [GenerateFrontendForm(InputType: "number-input", MinValue: 0, MaxValue: 100)]
    public double DayBrightness { get; set; } = 100;

    readonly int ColorArrayResolution = 300;

    // Civil twilight ends about 6 degrees below the horizon; that is where the sky reaches the night
    // brightness, so the sun glow has faded out by then. The band below only matters when the night
    // brightness is set high, and keeps the sun from showing up in the middle of the night.
    const double TwilightElevationDeg = -6;
    const double SunOffElevationDeg = -12;
    const double DegToRad = Math.PI / 180;

    public ColorHsv[] GetColors(SolarPosition sun, out ColorHsv defaultColor)
    {
        var fieldOfView = Math.Clamp(WindowFieldOfView, 1, 360);

        // Place the sun on the strip by its bearing relative to the window: the strip covers
        // +/- fieldOfView/2 degrees around WindowAzimuth.
        var bearingFromWindow = WrapAzimuth(sun.AzimuthDeg - WindowAzimuth);
        var sunPosition = 0.5 + bearingFromWindow / fieldOfView;

        // No hard cut offs: at the windows edges the glow slides in and out over the strip bounds
        // because the gaussian extends past the suns centre, and a setting sun fades away with the
        // sky brightness (it is already dim at the horizon level) rather than blinking off.
        var sunVisibility = SmoothStep(SunOffElevationDeg, TwilightElevationDeg, sun.ElevationDeg);

        // A low sun is red: fade from SunsetColor at the horizon to SunColor once it has climbed a
        // few degrees, so the redness only covers the sunrise/sunset itself.
        var reddening = SmoothStep(Math.Max(ReddeningHeight, 0.1), 0, sun.ElevationDeg);
        var sunColor = SunColor.Lerp(SunsetColor, reddening);

        var brightness = GetBrightness(sun) / 100;
        var sigma = Math.Max(SunSize, 0.5) / fieldOfView; // strip units
        var skyWarmthSigma = Math.Max(SkyWarmthSize, 1) / fieldOfView;

        // The theme as a single colour: the sky (no sun glow, it would only be a bright dot on one
        // LED) with the same dusk warmth the sky gets. Used by segments that have only one LED.
        defaultColor = SkyColor.Lerp(SunsetSkyColor, reddening);
        defaultColor.V *= brightness;

        // Create Color Array
        ColorHsv[] colors = new ColorHsv[ColorArrayResolution];
        for (int i = 0; i < ColorArrayResolution; i++)
        {
            var x = i / (double)ColorArrayResolution - sunPosition;

            // Same timings as the red sun, but spread wide: the sky glows orange around the suns
            // bearing and fades back to SkyColor away from it.
            var warmth = reddening * Math.Exp(-(x * x) / (skyWarmthSigma * skyWarmthSigma));
            var sky = warmth > 0 ? SkyColor.Lerp(SunsetSkyColor, warmth) : SkyColor;

            var gaussianSun = Math.Exp(-(x * x) / (sigma * sigma)) * sunVisibility;
            colors[i] = sky.Lerp(sunColor, gaussianSun);

            colors[i].V *= brightness;
        }

        return colors;
    }

    double GetBrightness(SolarPosition sun)
    {
        var night = Math.Clamp(NightBrightness, 0, 100);
        var horizon = Math.Clamp(HorizonBrightness, 0, 100);
        var day = Math.Clamp(DayBrightness, 0, 100);

        if (sun.ElevationDeg <= TwilightElevationDeg)
            return night;

        if (sun.ElevationDeg < 0)
            return Lerp(night, horizon, SmoothStep(TwilightElevationDeg, 0, sun.ElevationDeg));

        // Above the horizon the sky brightness follows the suns height: none at the horizon, full at
        // the highest point of the day.
        if (sun.MaxElevationDeg <= 1)
            return horizon;

        var relativeHeight = Math.Sin(sun.ElevationDeg * DegToRad) / Math.Sin(sun.MaxElevationDeg * DegToRad);
        return Lerp(horizon, day, SmoothStep(0, 1, relativeHeight));
    }

    static double Lerp(double a, double b, double x) => a * (1 - x) + b * x;

    static double SmoothStep(double edge0, double edge1, double x)
    {
        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0, 1);
        return t * t * (3 - 2 * t);
    }

    static double WrapAzimuth(double degrees)
    {
        var wrapped = degrees % 360;
        if (wrapped > 180) wrapped -= 360;
        if (wrapped < -180) wrapped += 360;
        return wrapped;
    }
}
