using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Autofac;
using Autofac.ServiceFabric;
using DemoShared;

namespace DemoStatefulService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                var builder = new ContainerBuilder();

                // Register a type that be injected into the actor service.
                builder.Register(c => new Logger()).As<ILogger>();

                // Register the interceptors for managing lifetime scope disposal.
                builder.RegisterModule<AutofacServiceFabricModule>();

                // Register all stateful services in the specified assemblies.
                builder.RegisterStatefulServices(Assembly.GetExecutingAssembly());

                // Register a single stateful service.
                //builder.RegisterStatefulService<DemoStatefulService>();

                using (var container = builder.Build())
                {
                    // Register a stateful service factory with the ServiceRuntime.
                    container.RegisterStatefulServiceFactory<DemoStatefulService>("DemoStatefulServiceType");

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(DemoStatefulService).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
