using Server.Helper;
using Server.Services;

namespace Server
{
    public static class BackgroundServiceHook
    {
        public static void StartBackgroundServices(this IServiceProvider services)
        {
            _ = Task.Run(() =>
            {
                Task.Delay(1).Wait();

                if (services.GetService(typeof(IUpdaterService)) is not IUpdaterService updaterService) return;
                updaterService.StartUpdateThread();
            });
        }
    }
}