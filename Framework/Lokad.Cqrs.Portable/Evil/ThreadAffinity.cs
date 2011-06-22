using System;
using System.Threading;

namespace Lokad.Cqrs.Evil
{
    public sealed class ThreadAffinity
    {
        private readonly int _threadId;

        public ThreadAffinity()
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void Check()
        {
            if (Thread.CurrentThread.ManagedThreadId != _threadId)
            {
                var msg = String.Format(
                    "Call to class with affinity to thread {0} detected from thread {1}.",
                    _threadId,
                    Thread.CurrentThread.ManagedThreadId);
                throw new InvalidOperationException(msg);
            }
        }
    }
}