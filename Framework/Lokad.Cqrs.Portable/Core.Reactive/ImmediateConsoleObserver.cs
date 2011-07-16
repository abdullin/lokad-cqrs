#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;

namespace Lokad.Cqrs.Core.Reactive
{
    public sealed class ImmediateConsoleObserver : IObserver<ISystemEvent>
    {
        readonly Stopwatch _watch = Stopwatch.StartNew();

        public void OnNext(ISystemEvent value)
        {
            Console.WriteLine("[{0:0000000}]: {1}", _watch.ElapsedMilliseconds, value);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            Console.WriteLine("Completed");
        }
    }
}