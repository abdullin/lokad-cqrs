#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Runtime.Serialization;
using Lokad.Quality;

namespace Lokad
{
	/// <summary>
	/// This exception is thrown when the key is not valid (i.e.: not found)
	/// </summary>
	/// <remarks> TODO: add proper implementation.</remarks>
	[Serializable]
	[NoCodeCoverage]
	public class KeyInvalidException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyInvalidException"/> class.
		/// </summary>
		public KeyInvalidException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyInvalidException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public KeyInvalidException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyInvalidException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public KeyInvalidException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if !SILVERLIGHT2
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyInvalidException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
		/// </exception>
		protected KeyInvalidException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}