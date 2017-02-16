using Castle.DynamicProxy;

namespace Autofac.ServiceFabric
{
    public class AutofacActorInterceptor : IInterceptor
    {
        private readonly ILifetimeScope _lifetimescope;

        public AutofacActorInterceptor(ILifetimeScope lifetimescope)
        {
            _lifetimescope = lifetimescope;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (invocation.Method.Name == "OnDeactivateAsync")
                _lifetimescope.Dispose();
        }
    }
}
