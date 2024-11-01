using Server.Helper;
using Server.Services.LedTheme;
using Server.Services.WledCommunicator;
namespace Server.Services;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(UpdaterService))]
public interface IUpdaterService
{
    public void StartUpdateThread();
}

public class UpdaterService(
    IWledCommunicatorService communicatorService,
    ILedThemeProviderService ledThemeProvider,
    ILoggerService logger)
    : IUpdaterService
{
    Task? updateTask;
    CancellationTokenSource? cts;
    const int ledUpdateIntervalMillis = 500;

    public void StartUpdateThread()
    {
        logger.WriteLine("Starting Led Update Loop...");

        cts?.Cancel();
        updateTask?.Dispose();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        updateTask = Task.Run(() =>
        {
            communicatorService.FindLEDs();

            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    UpdateLedSegments();
                }
                catch (Exception ex)
                {
                    logger.WriteLine(ex, LogLevel.Error);
                }

                Task.Delay(ledUpdateIntervalMillis).Wait();
            }
        }, cts.Token);
    }

    private void UpdateLedSegments()
    {
        foreach (var led in communicatorService.Leds)
        {
            var themeBrightnesses = new List<int>();

            foreach (var (seg, i) in led.state.Seg.WithIndex())
            {
                LedSegment segment = new(led.address, i);
                var newLedState = ledThemeProvider.GetNewLedState(segment);
                if (newLedState == null) continue;
                communicatorService.SetLedColorsOnWledSegment(newLedState.Colors, segment);
                themeBrightnesses.Add(newLedState.Brightness);
            }

            communicatorService.SetBrightnessOnWledServer((int)themeBrightnesses.Average(), led.address);
        }
    }
}