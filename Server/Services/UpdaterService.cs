using Server.Helper;
using Server.Services.WledCommunicator;
namespace Server.Services;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(UpdaterService))]
public interface IUpdaterService
{
    public void StartUpdateThread();
}

public class UpdaterService(
    IWledCommunicatorService communicatorService,
    ILoggerService logger)
    : IUpdaterService
{
    Task? updateTask;
    CancellationTokenSource? cts;
    readonly IWledCommunicatorService communicatorService = communicatorService;
    const int ledUpdateIntervalMillis = 2000;

    public void StartUpdateThread()
    {
        logger.WriteLine("Starting Led Update Loop...");

        cts?.Cancel();
        updateTask?.Dispose();
        cts = new CancellationTokenSource();
        updateTask = Task.Run(() =>
        {
            communicatorService.FindLEDs();

            while (true)
            {
                try
                {
                    communicatorService.SetLedColors([new Color(0, 255, 255)]);
                    communicatorService.SetBrightness(128);
                }
                catch (Exception ex)
                {
                    logger.WriteLine(ex, LogLevel.Error);
                }

                Task.Delay(ledUpdateIntervalMillis).Wait();
            }
        }, cts.Token);
    }
}