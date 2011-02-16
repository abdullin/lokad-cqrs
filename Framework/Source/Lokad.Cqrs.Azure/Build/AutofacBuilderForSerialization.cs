#region (c) 2010-2011 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Serialization;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForSerialization : Syntax
	{
		readonly ContainerBuilder _builder;

		public AutofacBuilderForSerialization(ContainerBuilder builder)
		{
			_builder = builder;
		}

		public void RegisterSerializer<TSerializer>() where TSerializer : IMessageSerializer
		{
			_builder
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