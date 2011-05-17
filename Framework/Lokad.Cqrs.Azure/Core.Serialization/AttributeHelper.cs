#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;

namespace Lokad.Cqrs.Core.Serialization
{
    sealed class AttributeHelper
    {
        readonly object[] _attributes;

        public AttributeHelper(object[] attributes)
        {
            _attributes = attributes;
        }

        public Optional<string> GetString<TAttribute>(Func<TAttribute, string> retriever)
            where TAttribute : Attribute
        {
            var result = "";
            foreach (var attribute in _attributes.OfType<TAttribute>())
            {
                result = retriever(attribute);
            }
            
            if (String.IsNullOrEmpty(result))
                return Optional<string>.Empty;

            return result;
        }
    }
}