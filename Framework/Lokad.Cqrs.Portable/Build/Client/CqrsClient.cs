using System;
using Autofac;

namespace Lokad.Cqrs.Build.Client
{
    public sealed class CqrsClient 
    {
        public ILifetimeScope Scope { get; private set; }

        public CqrsClient(ILifetimeScope scope)
        {
            Scope = scope;
        }

        public IMessageSender Sender
        {
            get
            {
                IMessageSender sender;
                if(Scope.TryResolve(out sender))
                {
                    return sender;
                }
                var message = string.Format("Failed to discover default {0}, have you added it to the system?", typeof(IMessageSender).Name);
                throw new InvalidOperationException(message);
            }
        }

        public TService Resolve<TService>()
        {
            return Scope.Resolve<TService>();
        }
    }
}