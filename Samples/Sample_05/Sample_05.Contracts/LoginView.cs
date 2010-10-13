#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Security.Cryptography;
using System.Text;
using Lokad.Default;
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
}