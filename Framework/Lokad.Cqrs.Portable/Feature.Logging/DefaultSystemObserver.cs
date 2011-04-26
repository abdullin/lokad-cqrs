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
    public sealed class DefaultSystemObserver : ISystemObserver, IDisposable
    {
        readonly IObserver<ISystemEvent> _observer;

        public DefaultSystemObserver(IEnumerable<IObserver<ISystemEvent>> observers)
        {
            var array = observers.ToArray();
            if (array.Length > 1)
                throw new InvalidOperationException("There should be 0 or 1 system observers");
            _observer = array.FirstOrDefault();
        }

        

        public void Notify(ISystemEvent @event)
        {
            if (null != _observer)
            {
                _observer.OnNext(@event);
            }
            Trace.WriteLine(@event);
            Trace.Flush();
        }

        public void Dispose()
        {
            if (_observer != null)
            {
                _observer.OnCompleted();
            }
        }
    }
}