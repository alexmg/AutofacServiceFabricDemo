using System;
using System.Linq;
using System.Reflection;
using Autofac.Extras.DynamicProxy;
using Autofac.Util;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.ServiceFabric
{
    public static class AutofacActorExtensions
    {
        public static void RegisterActor<TActor>(this ContainerBuilder builder) where TActor : ActorBase
        {
            builder.RegisterActor(typeof(TActor));
        }

        public static void RegisterActor(this ContainerBuilder builder, Type actorType)
        {
            if (!actorType.IsActorType())
                throw new ArgumentException($"The type {actorType.FullName} is not a valid actor type", nameof(actorType));

            builder.RegisterActorWithContainer(actorType);
        }

        public static void RegisterActors(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var actorTypes = assemblies
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsActorType());

            foreach (var actorType in actorTypes)
            {
                builder.RegisterActorWithContainer(actorType);
            }
        }

        public static void RegisterActorServiceFactory<TActor>(this IContainer container) where TActor : ActorBase
        {
            ActorRuntime.RegisterActorAsync<TActor>((context, actorTypeInfo) =>
            {
                return new ActorService(context, actorTypeInfo, (actorService, actorId) =>
                {
                    var lifetimeScope = container.BeginLifetimeScope();
                    var actor = lifetimeScope.Resolve<TActor>(
                        TypedParameter.From(actorService), 
                        TypedParameter.From(actorId));
                    return actor;
                });
            }).GetAwaiter().GetResult();
        }

        private static bool IsActorType(this Type type)
        {
            return type.IsAssignableTo<ActorBase>() && type.IsClass && type.IsPublic && !type.IsAbstract;
        }

        private static void RegisterActorWithContainer(this ContainerBuilder builder, Type actorType)
        {
            builder.RegisterType(actorType)
                .InstancePerLifetimeScope()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(AutofacActorInterceptor));
        }
    }
}
