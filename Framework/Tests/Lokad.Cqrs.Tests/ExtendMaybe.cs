#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Linq.Expressions;
using Lokad.Cqrs;
using NUnit.Framework;

namespace Lokad.Testing
{
	/// <summary>
	/// Extends <see cref="Maybe{T}"/> for the purposes of testing
	/// </summary>
	
	public static class ExtendMaybe
	{
		/// <summary>
		/// Checks that optional has value matching to the provided value in tests.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="result">The result.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>same instance for inlining</returns>
		public static Maybe<TValue> ShouldPassCheck<TValue>(this Maybe<TValue> result,
			Expression<Func<TValue, bool>> expression)
		{
			Assert.IsTrue(result.HasValue, "Maybe should have value");
			var check = expression.Compile();
			Assert.IsTrue(check(result.Value), "Expression should be true: '{0}'.", expression.Body.ToString());
			return result;
		}

		/// <summary>
		/// Checks that optional has value matching to the provided value in tests.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="result">The result.</param>
		/// <param name="value">The value.</param>
		/// <returns>same instance for inlining</returns>
		public static Maybe<TValue> ShouldPassWith<TValue>(this Maybe<TValue> result, TValue value)
		{
			Assert.IsTrue(result.HasValue, "Maybe should have value");
		
			Assert.IsTrue(value.Equals(result.Value), "Value should be equal to: '{0}'.", value);
			return result;
		}


		/// <summary>
		/// Checks that optional has value matching to the provided value in tests.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="result">The result.</param>
		/// <returns>same instance for inlining</returns>
		public static Maybe<TValue> ShouldPass<TValue>(this Maybe<TValue> result)
		{
			Assert.IsTrue(result.HasValue, "Maybe should have value");
			return result;
		}

		/// <summary>
		/// Checks that the optional does not have any value
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="maybe">The maybe.</param>
		/// <returns>same instance for inlining</returns>
		public static Maybe<TValue> ShouldFail<TValue>(this Maybe<TValue> maybe)
		{
			Assert.IsFalse(maybe.HasValue, "Maybe should not have value");
			return maybe;
		}

		/// <summary>
		/// Checks that optional has value matching to the provided value in tests.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="maybe">The maybe.</param>
		/// <param name="value">The value.</param>
		/// <returns>same instance for inlining</returns>
		public static Maybe<TValue> ShouldBe<TValue>(this Maybe<TValue> maybe, TValue value)
		{
			return ShouldPassWith(maybe, value);
		}


		/// <summary>
		/// Checks that optional has value matching to the provided value in tests.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="maybe">The maybe.</param>
		/// <param name="value">The value.</param>
		/// <returns>same instance for inlining</returns>
		public static Maybe<TValue> ShouldBe<TValue>(this Maybe<TValue> maybe, bool value)
		{
			Assert.IsTrue(maybe.HasValue == value, "Value.HasValue should be: {0}", value);
			return maybe;
		}
	}
}