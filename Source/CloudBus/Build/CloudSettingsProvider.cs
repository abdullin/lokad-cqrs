using System.Configuration;
using Lokad;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Bus2.Build
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