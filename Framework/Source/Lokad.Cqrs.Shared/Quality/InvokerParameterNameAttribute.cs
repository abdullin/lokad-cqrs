#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Quality
{
	/// <summary>
	/// Indicates that the function argument should be string literal and match one  of the parameters of the caller function.
	/// For example, <see cref="ArgumentNullException"/> has such parameter.
	/// </summary>
	/// <remarks>This attribute helps R# in code analysis</remarks>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	[NoCodeCoverage]
	public sealed class InvokerParameterNameAttribute : Attribute
	{
	}
}