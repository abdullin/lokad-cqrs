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
	public sealed class CloudSettingsProvider 
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
	}
}