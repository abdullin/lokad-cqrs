using System;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Core
{
    public static class FunqContainer
    {
        public static void Register<T>(this IComponentRegistry registry, Func<IComponentContext,T> reg)
        {
            var builder = new ContainerBuilder();
            builder.Register(reg).SingleInstance();
            builder.Update(registry);
        }

        public static void Register<T>(this IComponentRegistry registry, T instance)
            where T : class
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance);
            builder.Update(registry);
        }

        
    }
}