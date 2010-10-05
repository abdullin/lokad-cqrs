#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Security.Cryptography;
using System.Text;
using Lokad.Cqrs.Default;
using Lokad.Quality;
using ProtoBuf;

namespace Sample_05.Contracts
{
	[ProtoContract]
	public class LoginView : IEntity
	{
		[ProtoMember(1)]
		public Guid UserId { get; private set; }
		[ProtoMember(2)]
		public string Username { get; private set; }
		[ProtoMember(3)]
		public string Email { get; private set; }
		[ProtoMember(4)]
		public string LoginIdentity { get; private set; }

		public void Init(Guid userId, string username, string email, string identity)
		{
			UserId = userId;
			Username = username;
			Email = email;
			LoginIdentity = identity;
		}



		

		public static string CalculateSHA1(string text)
		{
			byte[] buffer = Encoding.Unicode.GetBytes(text);
			var cryptoTransformSha1 =
			new SHA1CryptoServiceProvider();
			string hash = BitConverter.ToString(
				cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");

			return hash;
		}
	}

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
	

	public interface IDomainCommand : IMessage
	{
		
	}


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