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
        return testDiffs.MinBy(x => x.Duration());
    }

    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        int stateMinBrightness = 0, stateMaxBrightness = 100;

        var wakeUpTimeDiff = DayInvariantTimeOfDayDiff(input.Time, WakeUpDayTime);
        var wakeUpTimeDiffMins = wakeUpTimeDiff.TotalMinutes;

        if (wakeUpTimeDiffMins < -SleepTimeMinutes - FadeTimeMinutes * 2) { }
        else if (wakeUpTimeDiffMins < -SleepTimeMinutes - FadeTimeMinutes)
        {
            var timeScalar = (wakeUpTimeDiffMins - (-SleepTimeMinutes - FadeTimeMinutes * 2)) / FadeTimeMinutes;
            stateMaxBrightness = (int)(100 - 100 * timeScalar);
        }
        else if (wakeUpTimeDiffMins < -FadeTimeMinutes)
        {
            stateMaxBrightness = 0;
        }
        else if (wakeUpTimeDiffMins < 0)
        {
            var timeScalar = (wakeUpTimeDiffMins - (-FadeTimeMinutes)) / FadeTimeMinutes;
            stateMaxBrightness = (int)(100 * timeScalar);
            stateMinBrightness = (int)(100 * timeScalar);
        }
        else if (wakeUpTimeDiffMins < FadeTimeMinutes)
        {
            var timeScalar = (wakeUpTimeDiffMins - 0) / FadeTimeMinutes;
            stateMinBrightness = (int)(100 - 100 * timeScalar);
        }

        foreach (var color in state.Colors)
        {
            color.V = color.V < stateMinBrightness ? stateMinBrightness : color.V;
            color.V = color.V > stateMaxBrightness ? stateMaxBrightness : color.V;
        }

        return state;
    }
}
