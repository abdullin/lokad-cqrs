using System;
using System.Linq;
using System.Threading;
using Autofac;

namespace Lokad.Cqrs
{
    public sealed class PartialRegistration<TService>
    {
        readonly Type _implementation;
        readonly int _counter;
        readonly Action<object> _activate;
        readonly string _id;

        PartialRegistration(Type implementation, Action<object> activator)
        {
            _implementation = implementation;
            _activate = activator;
            _id = string.Format("{0}-{1}", implementation.Name, Interlocked.Increment(ref _counter));
        }
        public static PartialRegistration<TService> From<TComponent>(Action<TComponent> init = null) where TComponent : TService
        {
            var type = typeof (TComponent);
            if (init == null) return new PartialRegistration<TService>(type, null);
            return new PartialRegistration<TService>(type, (service => init((TComponent) service)));
        }

        public void Register(ContainerBuilder builder)
        {
            builder
                .RegisterType(_implementation)
                .Named(_id, typeof (TService));
        }
        public TService ResolveWithTypedParams(IComponentContext ctx, params object[] items)
        {
            var typedParameters = items.Select(i => new TypedParameter(i.GetType(), i));
            var service = ctx.ResolveKeyed<TService>(_id, typedParameters);

            if (null != _activate)
            {
                _activate(service);
            }

            return service;
        }
    }
}