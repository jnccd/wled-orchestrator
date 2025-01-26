using Server.Helper;

namespace Server.Services.LedTheme.Themes;

public class LedThemeSingleColor(ColorHsv? color = null) : LedTheme
{
    public ColorHsv Color { get; set; } = color ?? new ColorRgb(0, 255, 255).RgbToHSV();

    public override LedGroupState? GetNewState(LedThemeInput input) => new([Color]);
}