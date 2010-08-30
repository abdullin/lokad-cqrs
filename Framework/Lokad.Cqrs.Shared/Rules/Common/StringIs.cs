#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System.Linq;
using System.Text.RegularExpressions;

namespace Lokad.Rules
{
	/// <summary>
	/// Common string rules
	/// </summary>
	public static class StringIs
	{
		/// <summary>
		/// Originally http://fightingforalostcause.net/misc/2006/compare-email-regex.php,
		/// but modified to have less negative results 
		/// </summary> 
		static readonly Regex EmailRegex = new Regex(
			@"^[a-z0-9](([_\.\-\'\+]?[a-z0-9]+)*)" +
				@"@(([a-z0-9]{1,64})(([\.\-][a-z0-9]{1,64})*)\.([a-z]{2,})" +
					@"|(\d{1,3}(\.\d{1,3}){3}))$",
#if !SILVERLIGHT2
			RegexOptions.Compiled |
#endif
				RegexOptions.Singleline | RegexOptions.IgnoreCase);


		static readonly Regex ServerConnectionRegex = new Regex(
			@"^(([a-z0-9]{1,64})(([\.\-][a-z0-9]{1,64})*)(\:\d{1,5})?)$",
			RegexOptions.Singleline |
#if !SILVERLIGHT2
				RegexOptions.Compiled |
#endif
					RegexOptions.IgnoreCase);

		/// <summary>
		/// Determines whether the string is valid email address
		/// </summary>
		/// <param name="email">string to validate</param>
		/// <param name="scope">validation scope.</param>
		public static void ValidEmail(string email, IScope scope)
		{
			if (!EmailRegex.IsMatch(email))
				scope.Error(RuleResources.String_must_be_valid_email);
		}

		/// <summary>
		/// Determines whether the string is valid server connection (with optional port)
		/// </summary>
		/// <param name="host">The host name to validate.</param>
		/// <param name="scope">The validation scope.</param>
		public static void ValidServerConnection(string host, IScope scope)
		{
			if (!ServerConnectionRegex.IsMatch(host))
				scope.Error(RuleResources.String_must_be_valid_host);
		}

		/// <summary>
		/// Composes the string validator ensuring string length is within the supplied rangs
		/// </summary>
		/// <param name="minLength">Min string length.</param>
		/// <param name="maxLength">Max string length.</param>
		/// <returns>new validator instance</returns>
		public static Rule<string> Limited(int minLength, int maxLength)
		{
			Enforce.Argument(() => maxLength, Is.GreaterThan(0));
			Enforce.Argument(() => minLength, Is.Within(0, maxLength));

			return (s, scope) =>
				{
					if (s.Length < minLength)
						scope.Error(RuleResources.String_cant_be_shorter_than_X, minLength);
					if (s.Length > maxLength)
						scope.Error(RuleResources.String_cant_be_longer_than_X, maxLength);
				};
		}


		/// <summary>
		/// Composes the string validator ensuring string length is shorter than
		/// <paramref name="maxLength"/>
		/// </summary>
		/// <param name="maxLength">Max string length.</param>
		/// <returns>new validator instance</returns>
		public static Rule<string> Limited(int maxLength)
		{
			Enforce.Argument(() => maxLength, Is.GreaterThan(0));

			return (s, scope) =>
				{
					if (s.Length > maxLength)
						scope.Error(RuleResources.String_cant_be_longer_than_X, maxLength);
				};
		}

		/// <summary>
		/// Reports error if the associated string is empty
		/// </summary>
		public static readonly Rule<string> NotEmpty = (s, scope) =>
			{
				if (string.IsNullOrEmpty(s))
					scope.Error(RuleResources.String_cant_be_empty);
			};

		/// <summary>
		/// String validator that ensures absence of any illegal characters
		/// </summary>
		/// <param name="illegalCharacters">The illegal characters.</param>
		/// <returns>new validator instance</returns>
		public static Rule<string> Without(params char[] illegalCharacters)
		{
			var joined = illegalCharacters.Select(c => "'" + c + "'").Join(", ");
			return (s, scope) =>
				{
					if (s.IndexOfAny(illegalCharacters) >= 0)
						scope.Error(RuleResources.String_cant_contain_X_characters, joined);
				};
		}

		/// <summary>
		/// String validator that detects possible issues 
		/// for passing strings through the ASP.NET Web services
		/// </summary>
		public static readonly Rule<string> ValidForXmlSerialization = (s, scope) =>
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (char.IsControl(s[i]))
						scope.Error(RuleResources.String_cant_contain_unicode_control_characters);
				}
			};

		/// <summary> String validator checking for presence of 
		/// white-space characters in the beginning of string </summary>
		public static readonly Rule<string> WithoutLeadingWhiteSpace = (s, scope) =>
			{
				if (s.Length > 0 && char.IsWhiteSpace(s[0]))
					scope.Error(RuleResources.String_cant_start_with_whitespace);
			};

		/// <summary> String validator checking for presence of 
		/// white-space characters in the end of string </summary>
		public static readonly Rule<string> WithoutTrailingWhiteSpace = (s, scope) =>
			{
				if (s.Length > 0 && char.IsWhiteSpace(s.Last()))
					scope.Error(RuleResources.String_cant_end_with_whitespace);
			};


		/// <summary> String validator checking for presence of uppercase 
		/// characters </summary>
		public static readonly Rule<string> WithoutUppercase = (s, scope) =>
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (char.IsUpper(s, i))
				{
					scope.Error(RuleResources.String_cant_contain_uppercase);
					return;
				}
			}
		};
	}
}