using Server.Helper;

namespace Server.Services.LedTheme;

public class LedGroupState(ColorHsv[] Colors, int Brightness = 255, ColorHsv? DefaultColor = null)
{
    public ColorHsv[] Colors = Colors;
    public int Brightness = Brightness;

    /// <summary>
    /// The single colour that represents the whole theme. A segment with a single LED cannot show the
    /// themes gradient, so it gets this instead (see LedTheme.GetNewModifiedState). Null means "use
    /// the middle of the colour array".
    /// </summary>
    public ColorHsv? DefaultColor = DefaultColor;
}