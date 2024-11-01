using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.LedTheme.Themes;
namespace Server.Services.LedTheme;

public record LedGroupState(Color[] Colors, int Brightness);

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(LedThemeProviderService))]
public interface ILedThemeProviderService
{
    public LedGroupState? GetNewLedState(LedSegment ledSegment);
}

public class LedThemeProviderService(IDataStoreService dataStore) : ILedThemeProviderService
{
    readonly LedThemeDefault defaultTheme = new();

    public LedGroupState? GetNewLedState(LedSegment ledSegment)
    {
        var ledSegmentGroup = dataStore.Data.Groups.FirstOrDefault(x => x.LedSegments.Contains(ledSegment));

        if (ledSegmentGroup == null) return defaultTheme.GetNewModifiedState(GetNewLedThemeInput());

        return ledSegmentGroup.Theme.GetNewModifiedState(GetNewLedThemeInput());
    }

    static LedThemeInput GetNewLedThemeInput() => new(DateTime.Now);
}