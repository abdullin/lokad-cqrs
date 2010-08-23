#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lokad.Rules
{
	/// <summary>
	/// Collection of messages with the associated highest level
	/// </summary>
	public sealed class RuleMessages : ReadOnlyCollection<RuleMessage>
	{
		readonly RuleLevel _level;

		internal static readonly RuleMessages Empty = new RuleMessages(new List<RuleMessage>(0), RuleLevel.None);

		internal RuleMessages(IList<RuleMessage> list, RuleLevel level) : base(list)
		{
			_level = level;
		}

		/// <summary>
		/// The highest level within the collection
		/// </summary>
		/// <value>The level.</value>
		public RuleLevel Level
		{
			get { return _level; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance has a message 
		/// of  <see cref="RuleLevel.Error"/> or higher.
		/// </summary>
		/// <value><c>true</c> if this instance has message of 
		/// <see cref="RuleLevel.Error"/> or higher ; otherwise, <c>false</c>.</value>
		public bool IsError
		{
			get { return _level >= RuleLevel.Error; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance has a message 
		/// of <see cref="RuleLevel.Warn"/> or higher.
		/// </summary>
		/// <value><c>true</c> if this instance has message of 
		/// <see cref="RuleLevel.Warn"/> or higher ; otherwise, <c>false</c>.</value>
		public bool IsWarn
		{
			get { return _level >= RuleLevel.Warn; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance does not have any messages
		/// of <see cref="RuleLevel.Warn"/> or higher
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance does not have any messages
		/// of <see cref="RuleLevel.Warn"/> or higher; otherwise, <c>false</c>.
		/// </value>
		public bool IsSuccess
		{
			get { return _level < RuleLevel.Warn; }
		}
	}
}