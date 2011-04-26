#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs
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

        public MessageItem(Type mappedType, object content, IDictionary<string, object> attributes)
        {
            MappedType = mappedType;
            Content = content;
            _attributes = attributes;
        }
    }
}