#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs
{
    /// <summary>
    /// Settings provider built on top of the Windows Azure
    /// </summary>
    public sealed class AzureSettingsProvider
    {
        static bool DetectCloudEnvironment()
        {
            try
            {
                if (RoleEnvironment.IsAvailable)
                    return true;
            }
            catch (RoleEnvironmentException)
            {
                // no environment
            }
            return false;
        }

        static readonly Lazy<bool> HasCloudEnvironment = new Lazy<bool>(DetectCloudEnvironment, true);

        /// <summary>
        /// Attempts to get the configuration string from cloud environment or app settings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        /// <returns><em>True</em> if configuration value is available, <em>False</em> otherwise</returns>
        public static bool TryGetString(string key, out string result)
        {
            result = null;
            if (HasCloudEnvironment.Value)
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
            if (string.IsNullOrEmpty(result))
                return false;
            return true;
        }

        /// <summary>
        /// Attempts to get the configuration string from cloud environment or app settings. Throws the exception if not available.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>
        /// configuration value
        /// </returns>
        public static string GetStringOrThrow(string key)
        {
            string result;
            if (!TryGetString(key, out result))
            {
                var s = string.Format("Failed to find configuration setting for '{0}'", key);
                throw new InvalidOperationException(s);
            }
            return result;
        }
    }
}