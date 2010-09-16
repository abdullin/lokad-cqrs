using System;
using Lokad;

namespace Sample_05.Web
{
	public sealed class AuthInfo
	{
		public readonly Guid UserId;

		public AuthInfo(Guid userId)
		{
			UserId = userId;
		}

		const string Prefix = "cqrs-v1";

		public static Maybe<AuthInfo> Parse(string source)
		{
			if (string.IsNullOrEmpty(source))
				return Maybe<AuthInfo>.Empty;
			
			if (!source.StartsWith(Prefix + '|'))
				return Maybe<AuthInfo>.Empty;

			var strings = source.Split('|');
			
			var user = Guid.Parse(strings[1]);
			return new AuthInfo(user);
		}
		public string ToCookieString()
		{
			return string.Format( Prefix + "|{0}", UserId);
		}
	}
}