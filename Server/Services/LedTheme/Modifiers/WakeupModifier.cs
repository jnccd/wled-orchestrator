using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class WakeupModifier : LedThemeModifier
{
    [GenerateFrontendForm]
    public double FadeTimeMinutes { get; set; } = 20;
    [GenerateFrontendForm]
    public TimeSpan WakeUpDayTime { get; set; } = TimeSpan.FromHours(8);

    const int maxBrightness = 100;

    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        var minutesDiffToWakeUp = Math.Abs((input.Time.TimeOfDay - WakeUpDayTime).TotalMinutes);
        var stateMinBrightness = maxBrightness - Math.Abs(minutesDiffToWakeUp) * maxBrightness / FadeTimeMinutes;

        foreach (var color in state.Colors)
            color.V = color.V < stateMinBrightness ? (int)stateMinBrightness : color.V;

        return state;
    }
}
