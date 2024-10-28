using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Server;

[AttributeUsage(AttributeTargets.Interface)]
public class RegisterImplementation(ServiceRegisterType serviceRegisterType, Type serviceType) : Attribute
{
    public readonly ServiceRegisterType serviceRegisterType = serviceRegisterType;
    public readonly Type serviceType = serviceType;
}

public enum ServiceRegisterType { Singleton, Transient }

public static class Configuration
{
    public static void ConfigureWebhost(this WebApplicationBuilder builder)
    {
        ushort port = string.IsNullOrWhiteSpace(builder.Configuration["PORT"]) ?
            (ushort)7778 :
            Convert.ToUInt16(builder.Configuration["PORT"]);

        //X509Certificate2 x509 = GetCertificateFromConfig(builder);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, port, listenOptions =>
            {
                //listenOptions.UseHttps(x509);
            });
        });
    }

    // // Enable if HTTPS is needed
    // private static X509Certificate2 GetCertificateFromConfig(WebApplicationBuilder builder)
    // {
    //     char _s = Path.DirectorySeparatorChar;
    //     if (string.IsNullOrWhiteSpace(builder.Configuration["CERT_PATH"]))
    //     {
    //         Logger.WriteLine($"CERT_PATH is empty!");
    //         throw new ArgumentException("CERT_PATH is empty!");
    //     }
    //     var certPem = File.ReadAllText($"{builder.Configuration["CERT_PATH"]}{_s}fullchain.pem");
    //     var keyPem = File.ReadAllText($"{builder.Configuration["CERT_PATH"]}{_s}privkey.pem");
    //     var x509 = X509Certificate2.CreateFromPem(certPem, keyPem);
    //     return x509;
    // }

    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        Type[] serviceTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                               from interfaceType in domainAssembly.GetTypes()
                               where interfaceType.Module == typeof(Configuration).Module
                                   && interfaceType.CustomAttributes.Any(x => x.AttributeType == typeof(RegisterImplementation))
                                   && interfaceType.IsInterface
                               select interfaceType).ToArray();

        foreach (var interfaceType in serviceTypes)
        {
            var attr = interfaceType.CustomAttributes.First(x => x.AttributeType == typeof(RegisterImplementation));

            var args = attr.ConstructorArguments;
            ServiceRegisterType serviceRegisterType = (ServiceRegisterType)(args[0].Value ?? 0);
            Type? serviceImplType = (Type?)args[1].Value;
            if (serviceImplType == null) continue;

            if (serviceRegisterType == ServiceRegisterType.Singleton)
                builder.Services.AddSingleton(interfaceType, serviceImplType);
            else if (serviceRegisterType == ServiceRegisterType.Transient)
                builder.Services.AddTransient(interfaceType, serviceImplType);
        }
    }
}
