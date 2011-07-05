using System;

namespace Lokad.Cqrs.Evil
{
    public static class DecayEvil
    {
        public static Func<uint, TimeSpan> BuildExponentialDecay(TimeSpan maxInterval)
        {
            var seconds = maxInterval.TotalSeconds;
            return l =>
                {
                    if (l >= 31)
                    {
                        return maxInterval;
                    }

                    if (l == 0)
                    {
                        l += 1;
                    }

                    var foo = Math.Pow(2, (l - 1) / 5.0) / 64d * seconds;

                    return TimeSpan.FromSeconds(foo);
                };
        }
    }
}