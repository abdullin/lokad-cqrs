#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Information about message currently processed by this thread
	/// </summary>
	public static class MessageContext
	{
		// we can't store message as Maybe.Empty, since thread
		// static fields might be uninitialized
		[ThreadStatic] static UnpackedMessage _current;

		/// <summary>
		/// Gets the information about message currently processed by this thread
		/// </summary>
		/// <value>Message currently processed by this thread.</value>
		public static Maybe<UnpackedMessage> Current
		{
			get { return _current ?? Maybe<UnpackedMessage>.Empty; }
		}


		/// <summary>
		/// Gets currently processed message, which could be null
		/// </summary>
		/// <value>The current message reference.</value>
		public static UnpackedMessage CurrentReference
		{
			get { return _current; }
		}

		public static void OverrideContext([NotNull] UnpackedMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");
			_current = message;
		}

		/// <summary>
		/// Clears the context.
		/// </summary>
		public static void ClearContext()
		{
			_current = null;
		}
	}
}