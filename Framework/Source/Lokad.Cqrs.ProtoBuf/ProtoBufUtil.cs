#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.ProtoBuf
{
	public static class ProtoBufUtil
	{
		public static string GetContractReference(Type type)
		{
			var attribs = type.GetCustomAttributes(false);
			var helper = new AttributeHelper(attribs);


			var name = Maybe.String
				.GetValue(() => helper.GetString<ProtoContractAttribute>(p => p.Name))
				.GetValue(() => helper.GetString<DataContractAttribute>(p => p.Name))
				.GetValue(() => helper.GetString<XmlTypeAttribute>(p => p.TypeName))
				.GetValue(() => type.Name);

			var ns = Maybe.String
				.GetValue(() => helper.GetString<DataContractAttribute>(p => p.Namespace))
				.GetValue(() => helper.GetString<XmlTypeAttribute>(p => p.Namespace))
				.Convert(s => s.Trim() + "/", "");

			return ns + name;
		}


		public static IFormatter CreateFormatter(Type type)
		{
			try
			{
				typeof (Serializer)
					.GetMethod("PrepareSerializer")
					.MakeGenericMethod(type)
					.Invoke(null, null);

				return (IFormatter) typeof (Serializer)
					.GetMethod("CreateFormatter")
					.MakeGenericMethod(type)
					.Invoke(null, null);
			}
			catch (TargetInvocationException tie)
			{
				throw Throw.InnerExceptionWhilePreservingStackTrace(tie);
			}
		}
	}
}