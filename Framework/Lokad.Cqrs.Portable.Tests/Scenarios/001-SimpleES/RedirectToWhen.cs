using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    public static class RedirectToWhen<T>
    {
        static readonly IDictionary<Type, MethodInfo> _dict = typeof (T)
            .GetMethods()
            .Where(m => m.Name == "When")
            .Where(m => m.GetParameters().Length == 1)
            .ToDictionary(m => m.GetParameters().First().ParameterType, m => m);

        public static void Invoke(T instance, object argument)
        {
            _dict[argument.GetType()].Invoke(instance, new[] {argument});
        }
    }
}