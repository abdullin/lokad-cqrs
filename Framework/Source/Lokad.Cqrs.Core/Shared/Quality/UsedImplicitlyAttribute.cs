#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Quality
{
	/// <summary>
	/// Indicates that the marked symbol is used implicitly (ex. reflection, external library), 
	/// so this symbol will not be marked as unused (as well as by other usage inspections)
	/// </summary>
	/// <remarks>This attribute helps R# in code analysis</remarks>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	[MeansImplicitUse(ImplicitUseFlags.ALL_MEMBERS_USED)]
	public sealed class UsedImplicitlyAttribute : Attribute
	{
	}
}