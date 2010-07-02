#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.ProtoBuf;
using Lokad.Cqrs.Serialization;

// ReSharper disable CheckNamespace
namespace Lokad.Cqrs
// ReSharper restore CheckNamespace
{
	public static class ExtendSyntaxWithProtoBuf
	{
		/// <summary>
		/// Configures Protocol Buffers Serializer to be used by the container (protobuf-net implementation by Marc Gravell).
		/// </summary>
		/// <remarks>http://code.google.com/p/protobuf-net/</remarks>
		/// <typeparam name="TSyntax">The type of the syntax to extend.</typeparam>
		/// <param name="syntax">The syntax.</param>
		/// <returns>Same configurer instance for inlining multiple config statements.</returns>
		public static TSyntax UseProtocolBuffers<TSyntax>(this TSyntax syntax)
			where TSyntax : DomainBuildModule
		{
			syntax.Target
				.RegisterType<ProtoBufMessageSerializer>()
				.As<IMessageSerializer>()
				.SingleInstance();
			return syntax;
		}
	}
}