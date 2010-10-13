using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Storage;
using Lokad.Serialization;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForSerialization : Syntax, ISupportSyntaxForSerialization
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
				.As<IMessageSerializer, IDataContractMapper, IDataSerializer>().SingleInstance();
		}
	}

	public sealed class AutofacBuilderForDomain : Syntax
	{
		ContainerBuilder _builder;

		public AutofacBuilderForDomain(ContainerBuilder builder)
		{
			_builder = builder;
		}
	}
}