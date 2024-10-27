using Server.Services.WledCommunicator;

namespace Server
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class AutoRegister(ServiceRegisterType serviceRegisterType, Type serviceType) : Attribute
    {
        public readonly ServiceRegisterType serviceRegisterType = serviceRegisterType;
        public readonly Type serviceType = serviceType;
    }

    public enum ServiceRegisterType { Singleton, Transient }

    public static class Configuration
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            Type[] assemblyTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                    from assemblyType in domainAssembly.GetTypes()
                                    where assemblyType.Module == typeof(Configuration).Module
                                    select assemblyType).ToArray();
            var serviceTypes = (from interfaceType in assemblyTypes
                                where interfaceType.CustomAttributes.Any(x => x.AttributeType == typeof(AutoRegister))
                                    && interfaceType.IsInterface
                                select interfaceType).ToList();

            foreach (var interfaceType in serviceTypes)
            {
                var attr = interfaceType.CustomAttributes.First(x => x.AttributeType == typeof(AutoRegister));

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