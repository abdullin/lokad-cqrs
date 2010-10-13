#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;

namespace Lokad.Rules
{
	/// <summary> Rule message </summary>
	[Serializable]
	public sealed class RuleMessage : IEquatable<RuleMessage>
	{
		const string Format = "{0}: [{1}] {2}";
		readonly RuleLevel _level;
		readonly string _message;
		readonly string _path;

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleMessage"/> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="level">The level.</param>
		/// <param name="message">The message.</param>
		public RuleMessage(string path, RuleLevel level, string message)
		{
			_path = path;
			_level = level;
			_message = message;
		}

		/// <summary>
		/// Gets the object path for the current message.
		/// </summary>
		/// <value>The path object path.</value>
		public string Path
		{
			get { return _path; }
		}

		/// <summary>
		/// Gets the <see cref="RuleLevel"/> associated with this message.
		/// </summary>
		/// <value>The level.</value>
		public RuleLevel Level
		{
			get { return _level; }
		}

		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <value>The message.</value>
		public string Message
		{
			get { return _message; }
		}

		bool IEquatable<RuleMessage>.Equals(RuleMessage other)
		{
			return _message == other.Message
				&& Level == other.Level
					&& Path == other.Path;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, Format, Path, Level, Message);
		}
	}
}