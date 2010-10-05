#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using Lokad.Cqrs;
using Lokad.Cqrs.Default;
using Lokad.Quality;
using Sample_05.Contracts;

namespace Sample_05.Domain
{
	[UsedImplicitly]
	public sealed class RegisterUser : IConsume<RegisterUserCommand>
	{
		readonly ViewWriter _writer;

		public RegisterUser(ViewWriter writer)
		{
			_writer = writer;
		}

		public void Consume(RegisterUserCommand message)
		{
			var key = LoginView.CalculateSHA1(message.Identity);
			_writer.UpdateOrAdd<LoginView>(key, view => view.Init(message.UserId, message.Username, message.Email, message.Identity));

			_writer.UpdateOrAdd<UserView>(message.UserId, uv =>
				{
					uv.Email = message.Email;
					uv.LoginIdentity = message.Identity;
					uv.UserId = message.UserId;
					uv.Username = message.Username;
				});
		}
	}
}