#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Quality
{
	/// <summary>
	/// Attribute used to inform code coverage tool to ignore marked code block
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct,
		Inherited = false, AllowMultiple = false)]
	[NoCodeCoverage]
	public sealed class NoCodeCoverageAttribute : Attribute
	{
		/// <summary> Gets or sets the justification for removing 
		/// the member from the unit test code coverage. </summary>
		/// <value>The justification.</value>
		public string Justification { get; set; }
	}
}