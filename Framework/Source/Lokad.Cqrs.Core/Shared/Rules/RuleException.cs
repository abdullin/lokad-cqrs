#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Lokad.Rules
{
	/// <summary>
	/// Exception that is thrown when some validation error is encountered
	/// </summary>
	/// <remarks>
	/// TODO: add proper implementation
	/// </remarks>
	[Serializable]
	public sealed class RuleException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RuleException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="reference">The reference.</param>
		public RuleException(string message, string reference)
			: base(string.Format("{0}: {1}", reference, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public RuleException(string message) : base(message)
		{
		}


		/// <summary> Initializes a new instance of the <see cref="RuleException"/> class. </summary>
		/// <param name="messages">The messages.</param>
		public RuleException(IEnumerable<RuleMessage> messages) : base(FromMessages(messages))
		{
		}

		static string FromMessages(IEnumerable<RuleMessage> messages)
		{
			var collection = messages.ToArray();

			if (collection.Length == 1)
				return collection[0].ToString();

			var builder = new StringBuilder(RuleResources.RuleException_header);
			foreach (var message in messages)
			{
				builder
					.AppendLine()
					.Append(message);
			}
			return builder.ToString();
		}

#if !SILVERLIGHT2


		/// <summary>
		/// Initializes a new instance of the <see cref="RuleException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
		/// </exception>
		RuleException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}