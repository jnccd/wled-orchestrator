namespace Server.Services;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(UpdaterService))]
public interface IUpdaterService
{

}

public class UpdaterService : IUpdaterService
{

}