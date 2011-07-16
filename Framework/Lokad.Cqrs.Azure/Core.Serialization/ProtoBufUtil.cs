#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;
using System.Linq;

namespace Lokad.Cqrs.Core.Serialization
{
    public static class ProtoBufUtil
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

        public static string GetContractReference(Type type)
        {
            var attribs = type.GetCustomAttributes(false);
            var helper = new AttributeHelper(attribs);


            var s1 = Optional<string>.Empty;
            var name = s1
                .Combine(() => helper.GetString<ProtoContractAttribute>(p => p.Name))
                .Combine(() => helper.GetString<DataContractAttribute>(p => p.Name))
                .Combine(() => helper.GetString<XmlTypeAttribute>(p => p.TypeName))
                .GetValue(type.Name);

            var ns = s1
                .Combine(() => helper.GetString<DataContractAttribute>(p => p.Namespace))
                .Combine(() => helper.GetString<XmlTypeAttribute>(p => p.Namespace))
                .Convert(s => s.Trim() + "/", "");

            ns = AppendNesting(ns, type);

            return ns + name;
        }

        static string AppendNesting(string ns, Type type)
        {
            var list = new List<string>();
            while (type.IsNested)
            {
                list.Insert(0, type.DeclaringType.Name);
                type = type.DeclaringType;
            }
            if (list.Count == 0)
            {
                return ns;
            }
            var suffix = string.Join("/", Enumerable.ToArray(list)) + "/";
            return ns + suffix;
        }
    }
}