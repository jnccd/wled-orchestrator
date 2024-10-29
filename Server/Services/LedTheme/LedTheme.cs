using Server.Helper;
namespace Server.Services.LedTheme;

public record LedThemeInput(DateTime Time);

public abstract class LedTheme
{
    public abstract LedSegmentState? GetNewState(LedThemeInput input);
}