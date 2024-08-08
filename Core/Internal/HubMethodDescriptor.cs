using System.Linq.Expressions;
using System.Reflection;

namespace LS.Core.Internal
{
    internal sealed class HubMethodDescriptor(MethodInfo methodInfo)
    {
        public MethodInfo Info { get; set; } = methodInfo;
        public ParameterInfo[] Parameters { get; private set; } = methodInfo.GetParameters();
        public Func<Hub, object?[], Task> Invoker { get; } = CreateInvoker(methodInfo);

        private static Func<Hub, object?[], Task> CreateInvoker(MethodInfo methodInfo)
        {
            var hubParam = Expression.Parameter(typeof(Hub), "hub");
            var argsParam = Expression.Parameter(typeof(object?[]), "args");

            var parameters = methodInfo.GetParameters().Select((parameter, i) =>
                Expression.Convert(Expression.ArrayIndex(argsParam, Expression.Constant(i)), parameter.ParameterType)).ToArray();

            var declaringType = methodInfo.DeclaringType ?? throw new InvalidOperationException("DeclaringType is null");
            var call = Expression.Call(Expression.Convert(hubParam, declaringType), methodInfo, parameters);

            var lambda = Expression.Lambda<Func<Hub, object?[], Task>>(call, hubParam, argsParam).Compile();
            return (hub, args) => lambda(hub, args);
        }
    }
}
