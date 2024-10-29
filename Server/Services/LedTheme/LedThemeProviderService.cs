using Server.Helper;
using Server.Services.WledCommunicator;
namespace Server.Services.LedTheme;

public record LedSegment(string WledServerAddress, int SegmentIndex);

public record LedSegmentState(Color[] Colors, int Brightness);

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(LedThemeProviderService))]
public interface ILedThemeProviderService
{
    public Dictionary<LedSegment, LedTheme> LedSegmentToTheme { get; set; }

    public LedSegmentState? GetNewLedState(LedSegment ledSegment);
}

public class LedThemeProviderService(
    ILoggerService logger)
    : ILedThemeProviderService
{
    public Dictionary<LedSegment, LedTheme> LedSegmentToTheme { get; set; } = [];

    // TODO: Make themes changeable
    public LedSegmentState? GetNewLedState(LedSegment ledSegment)
    {
        // Populate with default value if empty
        LedSegmentToTheme[ledSegment] = LedSegmentToTheme.GetValueOrDefault(ledSegment) ?? new LedThemeDaylight();

        return LedSegmentToTheme[ledSegment].GetNewState(new(DateTime.Now));
    }
}