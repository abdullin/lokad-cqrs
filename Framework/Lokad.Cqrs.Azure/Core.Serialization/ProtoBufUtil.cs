#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Serialization
{
    public static class ProtoBufUtil
    {
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

            return ns + name;
        }
    }
}