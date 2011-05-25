using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class MethodInvokerHint
    {
        public readonly Type ConsumerTypeDefinition;
        public readonly Func<Type,Type,MethodInfo> Lookup;
        public readonly Type MessageInterface;

        public MethodInvokerHint(Type consumerTypeDefinition, Type messageinterface, Func<Type, Type, MethodInfo> lookup)
        {
            ConsumerTypeDefinition = consumerTypeDefinition;
            Lookup = lookup;
            
            MessageInterface = messageinterface;
        }

        public static MethodInvokerHint FromConsumerSample<THandler>(Expression<Action<THandler>> expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            if (false == typeof(THandler).IsGenericType)
                throw new InvalidOperationException("Type should be a generic like 'IConsumeMessage<IMessage>'");

            var arguments = typeof(THandler).GetGenericArguments();

            if (arguments.Length != 1)
                throw new InvalidOperationException("Expected one generic argument");

            if (!arguments[0].IsInterface)
                throw new InvalidOperationException("Expected interface message base");

            var messageInterface = arguments[0];


            var interfaceTypedMethod = ((MethodCallExpression)expression.Body).Method;
            var parameters = interfaceTypedMethod.GetParameters();
            if (parameters.Length != 1)
                throw new InvalidOperationException("Expression should consume object like: 'i => i.Consume(null)'");


            var declaringGenericInterface = typeof(THandler).GetGenericTypeDefinition();
            var matches = declaringGenericInterface
                .GetMethods()
                .Where(mi => mi.Name == interfaceTypedMethod.Name)
                .Where(mi => mi.ContainsGenericParameters)
                .Where(mi =>
                    {
                        var length = mi.GetParameters().Length;
                        return length == parameters.Length;
                    })
                    .ToArray();

            if (matches.Length == 0)
            {
                throw new InvalidOperationException("Can't find generic method definition");
            }
            var method = matches[0];
            var name = method.Name;

            Func<Type, Type, MethodInfo> lookup = (c, msg) =>
                {
                    var consume = c.GetMethod(name, new[] {msg});
                    if (null == consume)
                    {
                        throw new InvalidOperationException(
                            string.Format("Unable to find consuming method {0}.{1}({2}).",
                                c.Name,
                                name,
                                msg.Name));
                    }
                    return consume;

                };

            
            return new MethodInvokerHint(declaringGenericInterface, messageInterface, lookup);
        }
    }
}