using Server.Helper;

namespace Server.Services.LedTheme.Themes;

public class LedThemeSingleColor(Color? color = null) : LedTheme
{
    public Color Color { get; set; } = color ?? new(0, 255, 255);
    public byte Brightness { get; set; } = 240;

    public override LedGroupState? GetNewState(LedThemeInput input) => new([Color], Brightness);
}