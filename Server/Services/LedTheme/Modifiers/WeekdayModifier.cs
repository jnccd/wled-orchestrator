using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class WeekdayModifier : LedThemeModifier
{
    [GenerateFrontendForm]
    public EnumSet<DayOfWeek> ActiveDays { get; set; } = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

    internal override LedGroupState? Modify(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        if (!ActiveDays.Contains(input.Time.DayOfWeek))
            foreach (var color in state.Colors)
            {
                color.V = 0;
            }

        return state;
    }
}
