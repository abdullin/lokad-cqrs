#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Lokad.Cqrs.Serialization;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForSerialization : Syntax
	{
		public AutofacBuilderForSerialization(ContainerBuilder builder) : base(builder)
		{
		}

		public void RegisterSerializer<TSerializer>() where TSerializer : IMessageSerializer
		{
			Builder
				.RegisterType<TSerializer>()
				.As<IMessageSerializer>().SingleInstance();
		}

		public void UseDataContractSerializer()
		{
			RegisterSerializer<DataContractMessageSerializer>();
		}

		public void UseProtoBufMessageSerializer()
		{
			RegisterSerializer<ProtoBufMessageSerializer>();
		}
	}
}