#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class MessageItemToSave
    {
        internal readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();
        public readonly Type MappedType;
        public readonly object Content;

        public MessageItemToSave(Type mappedType, object content)
        {
            MappedType = mappedType;
            Content = content;
        }

        public void AddNumber(string key, long number)
        {
            Attributes.Add(key, number);
        }
        public void AddString(string key, string value)
        {
            Attributes.Add(key, value);
        }
    }
}