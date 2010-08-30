#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using Lokad.Quality;

namespace Lokad
{
	/// <summary>
	/// Class that allows action to be executed, when it is disposed
	/// </summary>
	[Serializable]
	public sealed class DisposableAction : IDisposable
	{
		readonly Action _action;

		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableAction"/> class.
		/// </summary>
		/// <param name="action">The action.</param>
		public DisposableAction(Action action)
		{
			_action = action;
		}

		/// <summary>
		/// Executes the action
		/// </summary>
		public void Dispose()
		{
			_action();
		}
	}
}