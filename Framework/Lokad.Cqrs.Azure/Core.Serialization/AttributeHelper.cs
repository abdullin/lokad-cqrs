#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
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

        public Maybe<string> GetString<TAttribute>(Func<TAttribute, string> retriever)
            where TAttribute : Attribute
        {
            var v = FirstOrEmpty(_attributes
                .OfType<TAttribute>())
                .Convert(retriever, "");

            if (String.IsNullOrEmpty(v))
                return Maybe<string>.Empty;

            return v;
        }

        static Maybe<TSource> FirstOrEmpty<TSource>(IEnumerable<TSource> sequence)
        {
            foreach (var source in sequence)
            {
                return source;
            }
            return Maybe<TSource>.Empty;
        }
    }
}