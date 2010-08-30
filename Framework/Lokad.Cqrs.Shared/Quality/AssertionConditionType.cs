#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Quality
{
	/// <summary>
	/// Specifies assertion type. If the assertion method argument satisifes the condition, then the execution continues. 
	/// Otherwise, execution is assumed to be halted
	/// </summary>
	public enum AssertionConditionType
	{
		/// <summary>
		/// Indicates that the marked parameter should be evaluated to true
		/// </summary>
		IS_TRUE = 0,

		/// <summary>
		/// Indicates that the marked parameter should be evaluated to false
		/// </summary>
		IS_FALSE = 1,

		/// <summary>
		/// Indicates that the marked parameter should be evaluated to null value
		/// </summary>
		IS_NULL = 2,

		/// <summary>
		/// Indicates that the marked parameter should be evaluated to not null value
		/// </summary>
		IS_NOT_NULL = 3,
	}
}