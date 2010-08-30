#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Reflection
{
	/// <summary>
	/// Exception thrown, when <see cref="Reflect"/> fails to parse some lambda
	/// </summary>
	[Serializable]
	public sealed class ReflectLambdaException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReflectLambdaException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ReflectLambdaException(string message) : base(message)
		{
		}

#if !SILVERLIGHT2


		ReflectLambdaException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

#endif
	}
}