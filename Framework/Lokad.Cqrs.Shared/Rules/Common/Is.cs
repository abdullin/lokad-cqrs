#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Linq.Expressions;

namespace Lokad.Rules
{
	/// <summary>
	/// Generic rules 
	/// </summary>
	public static class Is
	{
		/// <summary>
		/// Composes the validator ensuring that the provided value does not equal to <paramref name="item"/>
		/// </summary>
		/// <typeparam name="T">type of the item to compare</typeparam>
		/// <param name="item">The item.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> NotEqual<T>(IEquatable<T> item)
		{
			return (t, scope) =>
				{
					if (item.Equals(t))
						scope.Error(RuleResources.Value_cant_be_equal_to_X, item);
				};
		}

		/// <summary>
		/// Composes the validator ensuring that the provided value equals to <paramref name="item"/>
		/// </summary>
		/// <typeparam name="T">type of the item to compare</typeparam>
		/// <param name="item">The item.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> Equal<T>(IEquatable<T> item)
		{
			return (t, scope) =>
				{
					if (!item.Equals(t))
						scope.Error(RuleResources.Value_must_be_equal_to_X, item);
				};
		}

		/// <summary>
		/// Composes the validator ensuring that the provided value equals to <paramref name="item"/>
		/// </summary>
		/// <typeparam name="T">type of the item to compare</typeparam>
		/// <param name="item">The item.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> Value<T>(T item) where T : struct
		{
			return (t, scope) =>
				{
					if (!item.Equals(t))
						scope.Error(RuleResources.Value_must_be_equal_to_X, item);
				};
		}

		/// <summary>
		/// Composes the validator ensuring that the provided object is same as <paramref name="item"/>
		/// </summary>
		/// <typeparam name="T">type of the item to compare</typeparam>
		/// <param name="item">The item.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> SameAs<T>(T item) where T : class
		{
			return (t, scope) =>
				{
					if (!ReferenceEquals(item, t))
						scope.Error(RuleResources.Object_must_be_same_as_reference);
				};
		}

		/// <summary>
		/// Returns error if the provided value type has default value
		/// </summary>
		/// <typeparam name="T">value type to check</typeparam>
		/// <param name="item">The item.</param>
		/// <param name="scope">The scope.</param>
		public static void NotDefault<T>(T item, IScope scope) where T : struct
		{
			if (default(T).Equals(item))
			{
				scope.Error(RuleResources.Value_cant_be_equal_to_X, default(T));
			}
		}

		/// <summary>
		/// Returns error if provided value type has been initialized
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item">The item.</param>
		/// <param name="scope">The scope.</param>
		public static void Default<T>(T item, IScope scope) where T : struct
		{
			if (!default(T).Equals(item))
			{
				scope.Error(RuleResources.Value_must_be_equal_to_X, default(T));
			}
		}

		/// <summary>
		/// Composes the range validator that ensures that the supplied value belongs
		/// to the interval from <paramref name="minValue"/> to <paramref name="maxValue"/>
		/// (inclusive).
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="minValue">The min value.</param>
		/// <param name="maxValue">The max value.</param>
		/// <returns>new validator instance</returns>
		public static Rule<T> Within<T>(IComparable<T> minValue, IComparable<T> maxValue)
		{
			return (value, scope) =>
				{
					if (minValue.CompareTo(value) > 0)
						scope.Error(RuleResources.Value_cant_be_less_than_X, minValue);
					if (maxValue.CompareTo(value) < 0)
						scope.Error(RuleResources.Value_cant_be_greater_than_X, maxValue);
				};
		}

		/// <summary>
		/// Composes the range validator that ensures that the supplied value belongs
		/// to the interval between <paramref name="lowerBound"/> and <paramref name="upperBound"/>
		/// (exclusive)
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="lowerBound">The lower bound.</param>
		/// <param name="upperBound">The upper bound.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> Between<T>(IComparable<T> lowerBound, IComparable<T> upperBound)
		{
			return (value, scope) =>
				{
					if (lowerBound.CompareTo(value) >= 0)
						scope.Error(RuleResources.Value_must_be_greater_than_X, lowerBound);
					if (upperBound.CompareTo(value) <= 0)
						scope.Error(RuleResources.Value_must_be_less_than_X, upperBound);
				};
		}

		/// <summary>
		/// Creates the rule to ensure that the validated value is greater than
		/// the specified <paramref name="comparable"/>
		/// </summary>
		/// <typeparam name="T">type of the item to run rule against</typeparam>
		/// <param name="comparable">The comparable.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> GreaterThan<T>(IComparable<T> comparable)
		{
			return (t, scope) =>
				{
					if (comparable.CompareTo(t) >= 0)
						scope.Error(RuleResources.Value_must_be_greater_than_X, comparable);
				};
		}

		/// <summary>
		/// Creates the rule to ensure that the validated value is greater than
		/// or equal to the specified <paramref name="comparable"/>
		/// </summary>
		/// <typeparam name="T">type of the item to run rule against</typeparam>
		/// <param name="comparable">The comparable.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> AtLeast<T>(IComparable<T> comparable)
		{
			return (t, scope) =>
				{
					if (comparable.CompareTo(t) > 0)
						scope.Error(RuleResources.Value_cant_be_less_than_X, comparable);
				};
		}

		/// <summary>
		/// Creates the rule to ensure that the validated value is less than
		/// or equal to the specified <paramref name="comparable"/>
		/// </summary>
		/// <typeparam name="T">type of the item to run rule against</typeparam>
		/// <param name="comparable">The comparable.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> AtMost<T>(IComparable<T> comparable)
		{
			return (t, scope) =>
				{
					if (comparable.CompareTo(t) < 0)
						scope.Error(RuleResources.Value_cant_be_greater_than_X, comparable);
				};
		}

		/// <summary>
		/// Creates the rule to ensure that the validated value is less than
		/// the specified <paramref name="comparable"/>
		/// </summary>
		/// <typeparam name="T">type of the item to run rule against</typeparam>
		/// <param name="comparable">The comparable.</param>
		/// <returns>new rule instance</returns>
		public static Rule<T> LessThan<T>(IComparable<T> comparable)
		{
			return (t, scope) =>
				{
					if (comparable.CompareTo(t) <= 0)
						scope.Error(RuleResources.Value_must_be_less_than_X, comparable);
				};
		}

		/// <summary> 
		/// <para>Compiles the rule out of the specified expression.</para> 
		/// </summary>
		/// <typeparam name="TTarget">The type of the target.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>compiled rule instance</returns>
		public static Rule<TTarget> True<TTarget>(Expression<Predicate<TTarget>> expression)
		{
			var stringRepresentation = expression.ToString();
			var check = expression.Compile();

			return (t, scope) =>
				{
					if (!check(t))
						scope.Error(RuleResources.Expression_X_must_be_true, stringRepresentation);
				};
		}
	}
}