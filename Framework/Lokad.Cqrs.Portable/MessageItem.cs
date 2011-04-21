using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Transport
{
	public sealed class MessageItem
	{
		public readonly Type MappedType;
		public readonly object Content;
		readonly IDictionary<string, object> _attributes;

		public ICollection<KeyValuePair<string, object>> GetAllAttributes()
		{
			return _attributes;
		}

		public MessageItem(Type mappedType, object content, IDictionary<string,object > attributes)
		{
			MappedType = mappedType;
			Content = content;
			_attributes = attributes;
		}
	}
}