#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Settings provider built on top of the Windows Azure
	/// </summary>
	[UsedImplicitly]
	public sealed class CloudSettingsProvider : ISettingsProvider
	{
		static readonly bool HasCloudEnvironment;

		static CloudSettingsProvider()
		{
			try
			{
				if (RoleEnvironment.IsAvailable)
					HasCloudEnvironment = true;
			}
			catch (RoleEnvironmentException)
			{
				// no environment
			}
		}

		public Maybe<string> GetString(string key)
		{
			string result = null;
			if (HasCloudEnvironment)
			{
				try
				{
					result = RoleEnvironment.GetConfigurationSettingValue(key);
				}
				catch (RoleEnvironmentException)
				{
					// no setting in dev?
				}
			}
			if (string.IsNullOrEmpty(result))
			{
				result = ConfigurationManager.AppSettings[key];
			}
			return string.IsNullOrEmpty(result) ? Maybe<string>.Empty : result;
		}

		Maybe<string> ISettingsProvider.GetValue(string name)
		{
			return GetString(name);
		}
	}

	/// <summary>
	/// Settings provider based on a simple dictionary
	/// </summary>
	public sealed class DictionarySettingsProvider : ISettingsProvider
	{
		readonly IDictionary<string, string> _dictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionarySettingsProvider"/> class.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		public DictionarySettingsProvider([NotNull] IDictionary<string, string> dictionary)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			_dictionary = dictionary;
		}

		Maybe<string> ISettingsProvider.GetValue([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return _dictionary.GetValue(name);
		}
	}
}