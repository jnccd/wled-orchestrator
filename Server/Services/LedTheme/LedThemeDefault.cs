using Server.Helper;
namespace Server.Services.LedTheme;

public class LedThemeDefault : LedTheme
{
    public override LedSegmentState GetNewState(LedThemeInput input) => new([new(0, 255, 255)], 200);

    // Singleton
    private static LedThemeDefault? instance;
    protected LedThemeDefault() { }
    public static new LedThemeDefault GetInstance() => instance ??= new();
}