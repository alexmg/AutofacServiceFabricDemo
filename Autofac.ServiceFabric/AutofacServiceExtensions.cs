using System;
using System.Linq;
using System.Reflection;
using Autofac.Extras.DynamicProxy;
using Autofac.Util;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.ServiceFabric
{
    public static class AutofacServiceExtensions
    {
        public static void RegisterStatefulService<TService>(this ContainerBuilder builder) 
            where TService : StatefulServiceBase
        {
            builder.RegisterStatefulService(typeof(TService));
        }

        public static void RegisterStatefulService(this ContainerBuilder builder, Type serviceType)
        {
            if (!serviceType.IsServiceType<StatefulServiceBase>())
                throw new ArgumentException($"The type {serviceType.FullName} is not a valid stateful service type", nameof(serviceType));

            builder.RegisterServiceWithContainer(serviceType);
        }

        public static void RegisterStatefulServices(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterServices<StatefulServiceBase>(assemblies);
        }

        public static void RegisterStatelessService<TService>(this ContainerBuilder builder)
            where TService : StatelessService
        {
            builder.RegisterStatelessService(typeof(TService));
        }

        public static void RegisterStatelessService(this ContainerBuilder builder, Type serviceType)
        {
            if (!serviceType.IsServiceType<StatelessService>())
                throw new ArgumentException($"The type {serviceType.FullName} is not a valid stateless service type", nameof(serviceType));

            builder.RegisterServiceWithContainer(serviceType);
        }

        public static void RegisterStatelessServices(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterServices<StatelessService>(assemblies);
        }

        public static void RegisterStatefulServiceFactory<TService>(this IContainer container, string serviceTypeName)
            where TService : StatefulServiceBase
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
            {
                var lifetimeScope = container.BeginLifetimeScope();
                var service = lifetimeScope.Resolve<TService>(TypedParameter.From(context));
                return service;
            }).GetAwaiter().GetResult();
        }

        public static void RegisterStatelessServiceFactory<TService>(this IContainer container, string serviceTypeName)
            where TService : StatelessService
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
            {
                var lifetimeScope = container.BeginLifetimeScope();
                var service = lifetimeScope.Resolve<TService>(TypedParameter.From(context));
                return service;
            }).GetAwaiter().GetResult();
        }

        private static bool IsServiceType<TServiceBase>(this Type type)
        {
            return type.IsAssignableTo<TServiceBase>() && type.IsClass && type.IsPublic && !type.IsAbstract;
        }

        private static void RegisterServiceWithContainer(this ContainerBuilder builder, Type serviceType)
        {
            builder.RegisterType(serviceType)
                .InstancePerLifetimeScope()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(AutofacServiceInterceptor));
        }

        private static void RegisterServices<TServiceBase>(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var serviceTypes = assemblies
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsServiceType<TServiceBase>());

            foreach (var serviceType in serviceTypes)
            {
                builder.RegisterServiceWithContainer(serviceType);
            }
        }
    }
}
