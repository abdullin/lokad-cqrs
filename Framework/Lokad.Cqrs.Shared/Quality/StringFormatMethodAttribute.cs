#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Quality
{
	/// <summary>
	/// Indicates that marked method builds string by format pattern and (optional) arguments. 
	/// Parameter, which contains format string, should be given in constructor.
	/// The format string should be in <see cref="string.Format(IFormatProvider,string,object[])"/> -like form
	/// </summary>
	/// <remarks>
	/// This attribute helps R# in code analysis
	/// </remarks>
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	[NoCodeCoverage]
	public sealed class StringFormatMethodAttribute : Attribute
	{
		readonly string _formatParameterName;

		/// <summary>
		/// Initializes new instance of StringFormatMethodAttribute
		/// </summary>
		/// <param name="formatParameterName">Specifies which parameter of an annotated method should be treated as format-string</param>
		public StringFormatMethodAttribute(string formatParameterName)
		{
			_formatParameterName = formatParameterName;
		}

		/// <summary>
		/// Gets format parameter name
		/// </summary>
		public string FormatParameterName
		{
			get { return _formatParameterName; }
		}
	}
}