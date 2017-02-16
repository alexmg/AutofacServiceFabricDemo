namespace Autofac.ServiceFabric
{
    public class AutofacServiceFabricModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacActorInterceptor>()
                .InstancePerLifetimeScope();

            builder.RegisterType<AutofacServiceInterceptor>()
                .InstancePerLifetimeScope();
        }
    }
}
