#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Core.Reactive
{
    public sealed class SystemObserver : ISystemObserver, IDisposable
    {
        readonly IObserver<ISystemEvent>[] _observers;

        public SystemObserver(IObserver<ISystemEvent>[] observers)
        {
            _observers = observers;
        }

        public void Notify(ISystemEvent @event)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(@event);
            }
        }

        public void Dispose()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}