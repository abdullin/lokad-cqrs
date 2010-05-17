#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Configuration;
using Lokad;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CloudBus.Build
{
	public sealed class CloudSettingsProvider : IProvideBusSettings
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
				result = RoleEnvironment.GetConfigurationSettingValue(key);
			}
			if (string.IsNullOrEmpty(result))
			{
				result = ConfigurationManager.AppSettings[key];
			}
			return string.IsNullOrEmpty(result) ? Maybe<string>.Empty : result;
		}
	}
}