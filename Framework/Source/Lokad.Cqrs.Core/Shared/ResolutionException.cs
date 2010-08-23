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
	/// Exception that is thrown by <see cref="IResolver"/> or <see cref="IProvider{TKey,TValue}"/>
	/// </summary>
	[Serializable]
	[NoCodeCoverage]
	public class ResolutionException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResolutionException"/> class.
		/// </summary>
		public ResolutionException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolutionException"/> class.
		/// </summary>
		/// <param name="message">The message related to this exception.</param>
		public ResolutionException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolutionException"/> class.
		/// </summary>
		/// <param name="message">The message related to this exception.</param>
		/// <param name="inner">The inner exception.</param>
		public ResolutionException(string message, Exception inner) : base(message, inner)
		{
		}

#if !SILVERLIGHT2
		/// <summary>
		/// Initializes a new instance of the <see cref="ResolutionException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
		/// </exception>
		protected ResolutionException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}