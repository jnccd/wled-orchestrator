using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.LedTheme.Themes;
namespace Server.Services.LedTheme;

public class LedGroupState(ColorHsv[] Colors, int Brightness = 255)
{
    public ColorHsv[] Colors = Colors;
    public int Brightness = Brightness;
}

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(LedThemeProviderService))]
public class LedThemeProviderService(DataStoreService dataStore)
{
    readonly LedThemeSingleColor defaultTheme = new();

    public LedGroupState? GetNewLedState(LedSegment ledSegment)
    {
        var ledSegmentGroup = dataStore.Data.Groups.FirstOrDefault(x => x.LedSegments.Contains(ledSegment));
        if (ledSegmentGroup == null) return null;
        return (ledSegmentGroup.Theme ?? defaultTheme).GetNewModifiedState(GetNewLedThemeInput());
    }

    static LedThemeInput GetNewLedThemeInput() => new(DateTime.Now);
}