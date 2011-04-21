#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Serialization;

namespace Lokad.Cqrs.Build
{
	public sealed class AutofacBuilderForSerialization : AutofacBuilderBase
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

		public void RegisterEnvelopeSerializer<TEnvelopeSerializer>() where TEnvelopeSerializer : IEnvelopeSerializer
		{
			Builder
				.RegisterType<TEnvelopeSerializer>()
				.As<IEnvelopeSerializer>()
				.SingleInstance();
		}

		public void UseDataContractSerializer()
		{
			RegisterSerializer<DataContractMessageSerializer>();
		}

		public void AutoDetectSerializer()
		{
			RegisterSerializer<AutoDetectingMessageSerializer>();
		}

		public void UseProtoBufMessageSerializer()
		{
			RegisterSerializer<ProtoBufMessageSerializer>();
			RegisterEnvelopeSerializer<ProtoBufEnvelopeSerializer>();
		}
	}
}