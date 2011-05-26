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
        //static readonly bool HasCloudEnvironment;


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

        public static string GetString(string key)
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