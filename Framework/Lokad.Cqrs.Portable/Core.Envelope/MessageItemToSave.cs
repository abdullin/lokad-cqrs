using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Envelope
{
	public sealed class MessageItemToSave
	{
		public readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();
		public readonly Type MappedType;
		public readonly object Content;

		public MessageItemToSave(Type mappedType, object content)
		{
			MappedType = mappedType;
			Content = content;
		}
	}
}