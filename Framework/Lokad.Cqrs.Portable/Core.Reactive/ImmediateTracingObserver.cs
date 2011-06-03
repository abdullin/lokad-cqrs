using System;
using System.Diagnostics;

namespace Lokad.Cqrs.Core.Reactive
{
    public sealed class ImmediateTracingObserver : IObserver<ISystemEvent>, ISystemObserver
    {
        readonly DateTime _started = DateTime.UtcNow;
        public void OnNext(ISystemEvent value)
        {
            var diff = (DateTime.UtcNow - _started).TotalMilliseconds;
            Trace.WriteLine(string.Format("[{0:########}] {1}", diff, value));
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

        public void Notify(ISystemEvent @event)
        {
            OnNext(@event);
        }
    }
}