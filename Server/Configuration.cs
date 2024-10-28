using Server.Services.WledCommunicator;

namespace Server
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RegisterImplementation(ServiceRegisterType serviceRegisterType, Type serviceType) : Attribute
    {
        public readonly ServiceRegisterType serviceRegisterType = serviceRegisterType;
        public readonly Type serviceType = serviceType;
    }

    public enum ServiceRegisterType { Singleton, Transient }

    public static class Configuration
    {
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
}