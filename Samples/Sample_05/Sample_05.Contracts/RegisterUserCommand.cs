using System;
using Lokad;
using ProtoBuf;

namespace Sample_05.Contracts
{
	[ProtoContract]
	public sealed class RegisterUserCommand : IDomainCommand
	{
		[ProtoMember(1)]
		public readonly string Email;
		[ProtoMember(2)]
		public readonly string Identity;
		[ProtoMember(3)]
		public readonly string IpHost;
		[ProtoMember(4)]
		public readonly Guid UserId;
		[ProtoMember(5)]
		public readonly string Username;


		public RegisterUserCommand(
			Guid userId,
			string identity,
			string username,
			string ipHost,
			string email)
		{
			UserId = userId;
			Identity = identity;
			Username = username;
			IpHost = ipHost;
			Email = email;
		}

		[UsedImplicitly]
		RegisterUserCommand()
		{
		}
	}
}