namespace Server.Services.LedTheme.Themes;

public class LedThemeDefault : LedTheme
{
    public override LedGroupState? GetNewState(LedThemeInput input) => new([new(0, 255, 255)], 200);
}