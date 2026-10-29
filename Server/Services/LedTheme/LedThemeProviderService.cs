using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.LedTheme.Themes;

namespace Server.Services.LedTheme;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(LedThemeProviderService))]
public class LedThemeProviderService(DataStoreService dataStore)
{
    public LedGroupState? GetNewLedState(LedSegment ledSegment)
    {
        var ledSegmentGroup = dataStore.Data.Groups.FirstOrDefault(x => x.LedSegments.Contains(ledSegment));
        if (ledSegmentGroup == null) return null;
        if (ledSegmentGroup.Theme == null) return null;
        return ledSegmentGroup.Theme!.GetNewModifiedState(GetNewLedThemeInput(ledSegment));
    }

    static LedThemeInput GetNewLedThemeInput(LedSegment ledSegment) => new(DateTime.Now, ledSegment.Length);
}