#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using Lokad;
using Lokad.Cqrs;
using Lokad.Default;
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
			_writer.UpdateOrAdd<LoginView>(key,
				view => view.Init(message.UserId, message.Username, message.Email, message.Identity));

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