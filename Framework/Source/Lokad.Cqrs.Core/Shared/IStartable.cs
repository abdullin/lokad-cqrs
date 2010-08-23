#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary>
	/// Generic interface for marking startable classes. Opposite for the <see cref="IDisposable"/>
	/// </summary>
	public interface IStartable
	{
		/// <summary>
		/// Starts this instance up.
		/// </summary>
		void StartUp();
	}
}