using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.ServiceFabric;
using DemoActor.Interfaces;
using DemoShared;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace DemoActor
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
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see https://aka.ms/servicefabricactorsplatform

                var builder = new ContainerBuilder();

                // Register a type that be injected into the actor service.
                builder.Register(c => new Logger()).As<ILogger>();

                // Register the interceptors for managing lifetime scope disposal.
                builder.RegisterModule<AutofacServiceFabricModule>();

                // Register all actors in the specified assemblies.
                builder.RegisterActors(Assembly.GetExecutingAssembly());

                // Register a single actor.
                //builder.RegisterActor<DemoActor>();

                using (var container = builder.Build())
                {
                    // Register an actor service factory with the ActorRuntime.
                    container.RegisterActorServiceFactory<DemoActor>();

                    Task.Run(() =>
                    {
                        Task.Delay(TimeSpan.FromSeconds(15));

                        // Invoke the actor to create an instance.
                        var actorId = ActorId.CreateRandom();
                        var actorProxy = ActorProxy.Create<IDemoActor>(actorId);
                        var count = actorProxy.GetCountAsync(new CancellationToken()).GetAwaiter().GetResult();

                        Debug.WriteLine($"Actor {actorId} has count {count}");

                        // Delete the actor to trigger deactive.
                        var actorServiceProxy = ActorServiceProxy.Create(
                            new Uri("fabric:/AutofacServiceFabricDemo/DemoActorService"), actorId);

                        actorServiceProxy.DeleteActorAsync(actorId, new CancellationToken()).GetAwaiter().GetResult();
                    });

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
