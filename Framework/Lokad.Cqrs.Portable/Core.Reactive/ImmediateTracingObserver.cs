using System;
using System.Diagnostics;

namespace Lokad.Cqrs.Core.Reactive
{
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