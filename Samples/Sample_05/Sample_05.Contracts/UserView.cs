#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

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