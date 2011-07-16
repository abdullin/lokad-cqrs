#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Evil
{
    /// <summary>
    /// Helper to build decay functions
    /// </summary>
    public static class DecayEvil
    {
        /// <summary>
        /// Builds the exponential decay function that grows to <paramref name="maxInterval"/> as the argument grows
        /// </summary>
        /// <param name="maxInterval">The max interval.</param>
        /// <returns>constructed function</returns>
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