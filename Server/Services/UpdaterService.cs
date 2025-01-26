using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.LedTheme;
using Server.Services.WledCommunicator;
namespace Server.Services;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(UpdaterService))]
public class UpdaterService(
    WledCommunicatorService communicatorService,
    LedThemeProviderService ledThemeProvider,
    DataStoreService dataStore,
    LoggerService logger)
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
            if (!dataStore.Data.Activated)
            {
                communicatorService.SetBrightnessOnWledServer(0, led.Address);
                continue;
            }

            var themeBrightnesses = new List<int>();

            foreach (var (seg, i) in led.State.Seg.WithIndex())
            {
                LedSegment segment = new(led.Address, i);
                var newLedState = ledThemeProvider.GetNewLedState(segment);
                if (newLedState == null) continue;
                communicatorService.SetLedColorsOnWledSegment([.. newLedState.Colors.Select(x => x.HsvToRgb())], segment);
                themeBrightnesses.Add(newLedState.Brightness);
            }

            communicatorService.SetBrightnessOnWledServer((int)themeBrightnesses.Average(), led.Address);
        }
    }
}