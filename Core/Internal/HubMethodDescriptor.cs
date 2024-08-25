using System.Linq.Expressions;
using System.Reflection;

namespace LogicalServer.Core.Internal
{
    internal sealed class HubMethodDescriptor(MethodInfo methodInfo)
    {
        public MethodInfo Info { get; set; } = methodInfo;
        public Type[] ParameterTypes { get; private set; } = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
        public Func<Hub, object?[], Task<object?>> Invoker { get; } = CreateInvoker(methodInfo);

        private static Func<Hub, object?[], Task<object?>> CreateInvoker(MethodInfo methodInfo)
        {
            var hubParam = Expression.Parameter(typeof(Hub), "hub");
            var argsParam = Expression.Parameter(typeof(object?[]), "args");

            var parameters = methodInfo.GetParameters().Select((parameter, i) =>
                Expression.Convert(Expression.ArrayIndex(argsParam, Expression.Constant(i)), parameter.ParameterType)).ToArray();

            var declaringType = methodInfo.DeclaringType ?? throw new InvalidOperationException("DeclaringType is null");
            var call = Expression.Call(Expression.Convert(hubParam, declaringType), methodInfo, parameters);

            if (methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = methodInfo.ReturnType.GetGenericArguments()[0];
                var lambda = Expression.Lambda(call, hubParam, argsParam).Compile();

                return async (hub, args) =>
                {
                    var task = (Task)lambda.DynamicInvoke(hub, args)!;
                    await task.ConfigureAwait(false);
                    return ((dynamic)task).Result;
                };
            }
            else
            {
                var lambda = Expression.Lambda<Func<Hub, object?[], Task>>(call, hubParam, argsParam).Compile();

                return async (hub, args) =>
                {
                    await lambda(hub, args);
                    return null;
                };
            }
        }
    }
}
