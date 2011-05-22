#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Core.Reactive
{
    public sealed class ImmediateEventsObserver : IObserver<ISystemEvent>
    {
        public event Action<ISystemEvent> Event = @event => { };

        public void OnNext(ISystemEvent value)
        {
            Event(value);
        }

        public void OnError(Exception error)
        {
            //throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            Event = @event => { };
            //throw new NotImplementedException();
        }
    }
}