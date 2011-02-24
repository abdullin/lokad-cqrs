using System;
using System.Collections.Generic;

namespace Lokad.Cqrs
{
	public sealed class MessageItem
	{
		public readonly string Contract;
		public readonly Type MappedType;
		public readonly object Content;
		readonly IDictionary<string, object> _attributes;

		public ICollection<KeyValuePair<string, object>> GetAllAttributes()
		{
			return _attributes;
		}

		public MessageItem(string contract, Type mappedType, object content, IDictionary<string,object > attributes)
		{
			Contract = contract;
			MappedType = mappedType;
			Content = content;
			_attributes = attributes;
		}
	}
}