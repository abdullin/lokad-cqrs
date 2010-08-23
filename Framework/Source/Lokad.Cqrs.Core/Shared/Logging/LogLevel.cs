#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary>
	/// Degines the importance level associated with the log
	/// entry in <see cref="ILog"/>
	/// </summary>
	[CLSCompliant(true)]
	public enum LogLevel
	{
		/// <summary> Message is intended for debugging </summary>
		Debug,
		/// <summary> Informatory message </summary>
		Info,
		/// <summary> The message is about potential problem in the system </summary>
		Warn,
		/// <summary> Some error has occured </summary>
		Error,
		/// <summary> Message is associated with the critical problem </summary>
		Fatal,

		/// <summary>
		/// Highest possible level
		/// </summary>
		Max = int.MaxValue,
		/// <summary> Smallest logging level</summary>
		Min = int.MinValue
	}
}