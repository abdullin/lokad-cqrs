using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sample_05.Contracts;

namespace Sample_05.Domain
{
	public sealed class RegisterUser : IHandle<RegisterUserCommand>
	{

		public void Handle(RegisterUserCommand command)
		{
			
		}
	}
}
