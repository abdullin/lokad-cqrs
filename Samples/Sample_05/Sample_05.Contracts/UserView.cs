using System;
using Lokad.Default;
using ProtoBuf;

namespace Sample_05.Contracts
{
	[ProtoContract]
	public class UserView : IEntity
	{
		[ProtoMember(1)]
		public Guid UserId { get; set; }
		[ProtoMember(2)]
		public string Username { get; set; }
		[ProtoMember(3)]
		public string Email { get; set; }
		[ProtoMember(4)]
		public string LoginIdentity { get; set; }
		[ProtoMember(5)]
		public string RegistrationToken { get; set; }
	}
}