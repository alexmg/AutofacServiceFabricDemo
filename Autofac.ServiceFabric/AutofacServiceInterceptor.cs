using Castle.DynamicProxy;

namespace Autofac.ServiceFabric
{
    public class AutofacServiceInterceptor : IInterceptor
    {
        private readonly ILifetimeScope _lifetimescope;

        public AutofacServiceInterceptor(ILifetimeScope lifetimescope)
        {
            _lifetimescope = lifetimescope;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            var methodName = invocation.Method.Name;

            if (methodName == "OnCloseAsync" || methodName == "OnAbort")
                _lifetimescope.Dispose();
        }
    }
}
