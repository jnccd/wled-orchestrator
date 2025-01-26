using Server.Helper;
using Server.Services.LedTheme;

class WakeupModifier : LedThemeModifier
{
    public double FadeTimeMinutes { get; set; } = 20;
    public TimeSpan WakeUpDayTime { get; set; } = TimeSpan.FromHours(8);

    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        var minDiffToWakeUp = (input.Time.TimeOfDay - WakeUpDayTime).TotalMinutes;
        var minBrightness = FadeTimeMinutes - minDiffToWakeUp;

        if (state.Brightness < minBrightness)
            state.Brightness = (int)minBrightness;

        state.Colors = [.. state.Colors.Select(x => new ColorHsv(x.H, x.S, x.V < minBrightness ? (int)minBrightness : x.V))];

        return state;
    }
}