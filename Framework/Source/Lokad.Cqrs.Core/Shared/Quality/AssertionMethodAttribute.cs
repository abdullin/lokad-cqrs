#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Quality
{
	/// <summary>
	/// Indicates that the marked method is assertion method, i.e. it halts control flow if one of the conditions is satisfied. 
	/// To set the condition, mark one of the parameters with <see cref="AssertionConditionAttribute"/> attribute
	/// </summary>
	/// <seealso cref="AssertionConditionAttribute"/>
	/// <remarks>This attribute helps R# in code analysis</remarks>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	[NoCodeCoverage]
	public sealed class AssertionMethodAttribute : Attribute
	{
	}
}