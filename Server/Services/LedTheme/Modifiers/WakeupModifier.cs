using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class WakeupModifier : LedThemeModifier
{
    [GenerateFrontendForm]
    public double FadeTimeMinutes { get; set; } = 20;
    [GenerateFrontendForm]
    public TimeSpan WakeUpDayTime { get; set; } = TimeSpan.FromHours(8);

    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        var minDiffToWakeUp = (input.Time.TimeOfDay - WakeUpDayTime).TotalMinutes;
        var minBrightness = 100 - Math.Abs(FadeTimeMinutes - minDiffToWakeUp);

        if (state.Brightness < minBrightness)
            state.Brightness = (int)minBrightness;

        foreach (var color in state.Colors)
            color.V = color.V < minBrightness ? (int)minBrightness : color.V;

        return state;
    }
}
