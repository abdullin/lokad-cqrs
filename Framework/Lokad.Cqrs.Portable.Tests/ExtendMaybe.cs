#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using NUnit.Framework;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Extends <see cref="Optional{T}"/> for the purposes of testing
    /// </summary>
    public static class ExtendMaybe
    {
        /// <summary>
        /// Checks that optional has value matching to the provided value in tests.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="result">The result.</param>
        /// <returns>same instance for inlining</returns>
        public static void ShouldPass<TValue>(this Optional<TValue> result)
        {
            Assert.IsTrue(result.HasValue, "Maybe should have value");
            return;
        }

        /// <summary>
        /// Checks that the optional does not have any value
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="optional">The maybe.</param>
        /// <returns>same instance for inlining</returns>
        public static void ShouldFail<TValue>(this Optional<TValue> optional)
        {
            Assert.IsFalse(optional.HasValue, "Maybe should not have value");
            return;
        }
    }
}