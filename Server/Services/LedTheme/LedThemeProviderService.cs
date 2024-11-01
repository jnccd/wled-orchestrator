using Server.Helper;
using Server.Services.LedTheme.Themes;
namespace Server.Services.LedTheme;

public record LedSegment(string WledServerAddress, int SegmentIndex)
{
    public Guid Id = Guid.NewGuid();
    public string WledServerAddress = WledServerAddress;
    public int SegmentIndex = SegmentIndex;
}

public record LedGroupState(Color[] Colors, int Brightness);

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(LedThemeProviderService))]
public interface ILedThemeProviderService
{
    public LedGroupState? GetNewLedState(LedSegment ledSegment);
}

public class LedThemeProviderService : ILedThemeProviderService
{
    public Dictionary<LedSegment, LedTheme> LedSegmentToTheme { get; set; } = [];

    // TODO: Make themes changeable
    public LedGroupState? GetNewLedState(LedSegment ledSegment)
    {
        // Populate with default value if empty
        LedSegmentToTheme[ledSegment] = LedSegmentToTheme.GetValueOrDefault(ledSegment) ?? new LedThemeDaylight();

        return LedSegmentToTheme[ledSegment].GetNewModifiedState(new(DateTime.Now));
    }
}