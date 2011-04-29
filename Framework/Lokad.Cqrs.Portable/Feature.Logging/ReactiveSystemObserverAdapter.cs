#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Lokad.Cqrs.Feature.Logging
{
    /// <summary>
    /// Simple <see cref="ISystemObserver"/> that writes to the <see cref="Trace.Listeners"/>
    /// </summary>
    /// <remarks>Use Logging stack, if more flexibility is needed</remarks>
    [Serializable]
    public sealed class ReactiveSystemObserverAdapter : ISystemObserver, IDisposable
    {
        readonly IObserver<ISystemEvent>[] _observers;

        public ReactiveSystemObserverAdapter(IEnumerable<IObserver<ISystemEvent>> observers)
        {
            _observers = observers.ToArray();
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

    public sealed class ImmediateTracingObserver : IObserver<ISystemEvent>
    {
        public void OnNext(ISystemEvent value)
        {
            Trace.WriteLine(value);
            Trace.Flush();
        }

        public void OnError(Exception error)
        {
            Trace.WriteLine("!" + error.Message);
        }

        public void OnCompleted()
        {
            Trace.WriteLine("Observing completed");
        }
    }
}