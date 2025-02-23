using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class RotateColorsModifier : LedThemeModifier
{
    [GenerateFrontendForm(MaxValue: 360)]
    public double Amount { get; set; } = 0;

    internal override LedGroupState? Modify(LedGroupState? state, LedThemeInput input)
    {
        if (state == null)
            return null;

        foreach (var color in state.Colors)
        {
            color.H += Amount;
            color.H %= 360;
        }

        return state;
    }
}
