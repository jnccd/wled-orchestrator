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
    public LedGroupState? GetNewLedState(LedSegment ledSegment)
    {
        var ledSegmentGroup = dataStore.Data.Groups.FirstOrDefault(x => x.LedSegments.Contains(ledSegment));
        if (ledSegmentGroup == null) return null;
        if (ledSegmentGroup.Theme == null) return null;
        return ledSegmentGroup.Theme!.GetNewModifiedState(GetNewLedThemeInput());
    }

    static LedThemeInput GetNewLedThemeInput() => new(DateTime.Now);
}