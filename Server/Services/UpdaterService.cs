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
    readonly CancellationTokenSource cts = new();
    readonly IWledCommunicatorService communicatorService = communicatorService;
    const int ledUpdateIntervalMillis = 2000;

    public void StartUpdateThread()
    {
        cts.Cancel();
        updateTask?.Dispose();
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