#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Quality
{
	/// <summary>
	/// Indicates the condition parameter of the assertion method. 
	/// The method itself should be marked by <see cref="AssertionMethodAttribute"/> attribute.
	/// The mandatory argument of the attribute is the assertion type.
	/// </summary>
	/// <seealso cref="AssertionConditionType"/>
	/// <remarks>This attribute helps R# in code analysis</remarks>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	[NoCodeCoverage]
	public sealed class AssertionConditionAttribute : Attribute
	{
		readonly AssertionConditionType _conditionType;

		/// <summary>
		/// Initializes new instance of AssertionConditionAttribute
		/// </summary>
		/// <param name="conditionType">Specifies condition type</param>
		public AssertionConditionAttribute(AssertionConditionType conditionType)
		{
			_conditionType = conditionType;
		}

		/// <summary>
		/// Gets condition type
		/// </summary>
		public AssertionConditionType ConditionType
		{
			get { return _conditionType; }
		}
	}
}