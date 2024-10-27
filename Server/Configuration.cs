using System.Net;
using System.Security.Cryptography.X509Certificates;
using Server.WledCommunicator;

namespace Server
{
    public static class Configuration
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            //builder.Configuration.GetSection(nameof(BasicAuthOptions)).Bind(new BasicAuthOptions());

            builder.Services
                .AddSingleton<IWledCommunicatorService, WledCommunicatorService>()
                ;
        }
    }
}