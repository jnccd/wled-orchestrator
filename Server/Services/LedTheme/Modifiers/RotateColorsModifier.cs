using Server.Endpoints;
using Server.Helper;
using Server.Services.LedTheme;

class RotateColorsModifier : LedThemeModifier
{
    [GenerateFrontendForm]
    public double Amount { get; set; } = 0;

    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input)
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
