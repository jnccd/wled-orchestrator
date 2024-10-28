using Server.Helper;
namespace Server.Services.LedTheme;

public record LedThemeInput(DateTime Time);

public abstract class LedTheme
{
    public static LedTheme GetInstance() => null;

    public abstract LedSegmentState GetNewState(LedThemeInput input);
}