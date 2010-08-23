#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Rules
{
	///<summary>
	/// Typed delegate for holding the validation logics
	///</summary>
	///<param name="obj">Object to validate</param>
	///<param name="scope">Scope that will hold all validation results</param>
	///<typeparam name="T">type of the item to validate</typeparam>
	public delegate void Rule<T>(T obj, IScope scope);
}