using Server.Helper;
using Server.Services.WledCommunicator;
namespace Server.Services.LedTheme;

public record LedSegment(string WledServerAddress, int SegmentIndex);

public record LedSegmentState(Color[] Colors, int Brightness);

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(LedThemeProviderService))]
public interface ILedThemeProviderService
{
    public LedSegmentState GetNewLedState(LedSegment ledSegment);
}

public class LedThemeProviderService(
    IWledCommunicatorService communicatorService,
    ILoggerService logger)
    : ILedThemeProviderService
{
    readonly Dictionary<LedSegment, LedTheme> LedSegmentToTheme = [];

    // TODO: Make themes changeable
    public LedSegmentState GetNewLedState(LedSegment ledSegment) => (LedSegmentToTheme[ledSegment] ?? new LedThemeDefault()).GetNewState(new(DateTime.Now));
}