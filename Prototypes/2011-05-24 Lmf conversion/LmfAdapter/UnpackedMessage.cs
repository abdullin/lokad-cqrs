#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace LmfAdapter
{
	/// <summary>
	/// Deserialized message representation
	/// </summary>
	public class UnpackedMessage
	{
		/// <summary>
		/// Type of the contract behind the message
		/// </summary>
		public readonly Type ContractType;
		/// <summary>
		/// Message Header information
		/// </summary>
		public readonly MessageHeader Header;
		/// <summary>
		/// Available message attributes
		/// </summary>
		public readonly MessageAttributesContract Attributes;
		/// <summary>
		/// Message content
		/// </summary>
		public readonly object Content;
		
		readonly IDictionary<string, object> _dynamicState = new Dictionary<string, object>();

		public UnpackedMessage(MessageHeader header, MessageAttributesContract attributes, object content, Type contractType)
		{
			Header = header;
			ContractType = contractType;
			Attributes = attributes;
			Content = content;
		}
	
		public UnpackedMessage WithState<TValue>(TValue value)
		{
			_dynamicState.Add(typeof(TValue).Name, value);
			return this;
		}

		public UnpackedMessage WithState<TValue>(string key, TValue value)
		{
			_dynamicState.Add(key, value);
			return this;
		}
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return Content == null ? "NULL" : Content.ToString();
		}
	}
}