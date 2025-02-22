using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class WakeupModifier : LedThemeModifier
{

    [GenerateFrontendForm]
    public double SleepTimeMinutes { get; set; } = 400;
    [GenerateFrontendForm]
    public double FadeTimeMinutes { get; set; } = 20;
    [GenerateFrontendForm]
    public TimeSpan WakeUpDayTime { get; set; } = TimeSpan.FromHours(8);

    const int maxBrightness = 100;

    TimeSpan DoubleMinutesToTimeSpan(double min) => new(0, (int)min, (int)(min % 1 * 60));

    TimeSpan DayInvariantTimeOfDayDiff(DateTime timeStamp, TimeSpan target)
    {
        TimeSpan[] testDiffs = [timeStamp.TimeOfDay - target, timeStamp.TimeOfDay.Add(new(24, 0, 0)) - target, timeStamp.TimeOfDay.Add(new(-24, 0, 0)) - target];
        return testDiffs.Select(x => x.Duration()).Min();
    }

    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        int stateMinBrightness = 0, stateMaxBrightness = 100;

        var minutesDiffToWakeUp = Math.Abs(DayInvariantTimeOfDayDiff(input.Time, WakeUpDayTime).TotalMinutes);
        stateMinBrightness = (int)(maxBrightness - Math.Abs(minutesDiffToWakeUp) * maxBrightness / FadeTimeMinutes);

        var totalSleepTimeMin = SleepTimeMinutes;
        var midSleepTime = WakeUpDayTime.Subtract(DoubleMinutesToTimeSpan(totalSleepTimeMin / 2 + FadeTimeMinutes / 4));
        var minutesDiffToMidSleepTime = Math.Abs(DayInvariantTimeOfDayDiff(input.Time, midSleepTime).TotalMinutes);
        var b = totalSleepTimeMin / 65;
        stateMaxBrightness = (int)(-b * maxBrightness - Math.Abs(minutesDiffToMidSleepTime) * -b * maxBrightness / (totalSleepTimeMin / 2) + maxBrightness);

        foreach (var color in state.Colors)
        {
            color.V = color.V < stateMinBrightness ? stateMinBrightness : color.V;
            color.V = color.V > stateMaxBrightness ? stateMaxBrightness : color.V;
        }

        return state;
    }
}
