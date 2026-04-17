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
            Task.Run(communicatorService.FindLEDs);

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
        lock (dataStore.lockject)
        {
            foreach (var ledServer in dataStore.Data.Groups.SelectMany(x => x.LedSegments).GroupBy(x => x.WledServerAddress))
            {
                if (!dataStore.Data.Activated)
                {
                    communicatorService.SetBrightnessOnWledServer(0, ledServer.Key);
                    continue;
                }

                var themeBrightnesses = new List<int>();

                foreach (var (seg, i) in ledServer.WithIndex())
                {
                    var newLedState = ledThemeProvider.GetNewLedState(seg);
                    if (newLedState == null) continue;
                    communicatorService.SetLedColorsOnWledSegment([.. newLedState.Colors.Select(x => x.HsvToRgb())], seg);
                    themeBrightnesses.Add(newLedState.Brightness);
                }

                if (themeBrightnesses == null || themeBrightnesses.Count == 0) continue;
                communicatorService.SetBrightnessOnWledServer((int)themeBrightnesses.Average(), ledServer.Key);
            }
        }
    }
}