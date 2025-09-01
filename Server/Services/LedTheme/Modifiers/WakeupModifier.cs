using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class WakeupModifier : LedThemeModifier
{

    [GenerateFrontendForm]
    public TimeSpan SleepTime { get; set; } = TimeSpan.FromHours(8);
    [GenerateFrontendForm(MaxValue: 45)]
    public double FadeTimeMinutes { get; set; } = 20;
    [GenerateFrontendForm]
    public TimeSpan WakeUpDayTime { get; set; } = TimeSpan.FromHours(8);
    [GenerateFrontendForm]
    public EnumSet<DayOfWeek> ActiveDays { get; set; } = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

    TimeSpan DoubleMinutesToTimeSpan(double min) => new(0, (int)min, (int)(min % 1 * 60));

    TimeSpan DayInvariantTimeOfDayDiff(DateTime timeStamp, TimeSpan target)
    {
        TimeSpan[] testDiffs = [timeStamp.TimeOfDay - target, timeStamp.TimeOfDay.Add(new(24, 0, 0)) - target, timeStamp.TimeOfDay.Add(new(-24, 0, 0)) - target];
        return testDiffs.MinBy(x => x.Duration());
    }

    internal override LedGroupState? Modify(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        if (!ActiveDays.Contains(input.Time.DayOfWeek))
        {
            foreach (var color in state.Colors)
            {
                color.V = 0;
            }
            return state;
        }

        int stateMinBrightness = 0, stateMaxBrightness = 100;

        var wakeUpTimeDiff = DayInvariantTimeOfDayDiff(input.Time, WakeUpDayTime);
        var wakeUpTimeDiffMinutes = wakeUpTimeDiff.TotalMinutes;
        var sleepTimeMinutes = SleepTime.TotalMinutes;

        if (wakeUpTimeDiffMinutes < -sleepTimeMinutes - FadeTimeMinutes * 2) { }
        else if (wakeUpTimeDiffMinutes < -sleepTimeMinutes - FadeTimeMinutes)
        {
            var timeScalar = (wakeUpTimeDiffMinutes - (-sleepTimeMinutes - FadeTimeMinutes * 2)) / FadeTimeMinutes;
            stateMaxBrightness = (int)(100 - 100 * timeScalar);
        }
        else if (wakeUpTimeDiffMinutes < -FadeTimeMinutes)
        {
            stateMaxBrightness = 0;
        }
        else if (wakeUpTimeDiffMinutes < 0)
        {
            var timeScalar = (wakeUpTimeDiffMinutes - (-FadeTimeMinutes)) / FadeTimeMinutes;
            stateMaxBrightness = (int)(100 * timeScalar);
            stateMinBrightness = (int)(100 * timeScalar);
        }
        else if (wakeUpTimeDiffMinutes < FadeTimeMinutes)
        {
            var timeScalar = (wakeUpTimeDiffMinutes - 0) / FadeTimeMinutes;
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
