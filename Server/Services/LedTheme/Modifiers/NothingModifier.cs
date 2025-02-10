using Server.Helper;
using Server.Services.LedTheme;

class NothingModifier : LedThemeModifier
{
    public override LedGroupState? ModifyState(LedGroupState? state, LedThemeInput input) => state;
}
