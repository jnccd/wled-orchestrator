using System.Diagnostics;
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
    // Per-LED colors travel over UDP now, so the loop can run at 20fps. The HTTP brightness request
    // below is only sent when the value actually changes (see lastSentBrightness).
    const int ledUpdateIntervalMillis = 50;
    // Brightness value last sent to each server, used to avoid spamming identical HTTP requests every tick.
    readonly Dictionary<string, int> lastSentBrightness = [];

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
            var stopwatch = new Stopwatch();

            while (!cts.Token.IsCancellationRequested)
            {
                stopwatch.Restart();
                try
                {
                    UpdateLedSegments();
                }
                catch (Exception ex)
                {
                    logger.WriteLine(ex, LogLevel.Error);
                }

                int waitMillis = ledUpdateIntervalMillis - (int)stopwatch.ElapsedMilliseconds;
                if (waitMillis > 0)
                    Task.Delay(waitMillis).Wait();
            }
        }, cts.Token);
    }

    private void UpdateLedSegments()
    {
        lock (dataStore.lockject)
        {
            foreach (var ledServer in dataStore.Data.Groups.SelectMany(x => x.LedSegments).GroupBy(x => x.WledServerAddress))
            {
                string serverAddress = ledServer.Key;
                if (!dataStore.Data.Activated)
                {
                    if (lastSentBrightness.GetValueOrDefault(serverAddress) != 0)
                    {
                        communicatorService.SetBrightnessOnWledServer(0, serverAddress);
                        lastSentBrightness[serverAddress] = 0;
                    }
                    continue;
                }

                var themeBrightnesses = new List<int>();

                foreach (var seg in ledServer)
                {
                    var newLedState = ledThemeProvider.GetNewLedState(seg);
                    if (newLedState == null) continue;
                    communicatorService.SetLedColorsOnWledSegment([.. newLedState.Colors.Select(x => x.HsvToRgb())], seg);
                    themeBrightnesses.Add(newLedState.Brightness);
                }

                if (themeBrightnesses.Count == 0) continue;
                var avgBrightness = (int)themeBrightnesses.Average();
                if (lastSentBrightness.GetValueOrDefault(serverAddress) != avgBrightness)
                {
                    communicatorService.SetBrightnessOnWledServer(avgBrightness, serverAddress);
                    lastSentBrightness[serverAddress] = avgBrightness;
                }
            }
        }
    }
}
