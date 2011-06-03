using System;
using System.Diagnostics;

namespace Lokad.Cqrs.Core.Reactive
{
    public sealed class ImmediateConsoleObserver : IObserver<ISystemEvent>
    {
        Stopwatch _watch = Stopwatch.StartNew();

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