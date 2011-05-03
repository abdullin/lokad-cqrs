#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Linq;

namespace Lokad.Cqrs.Build
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

    public sealed class AzureStorageCleaner : IEngineProcess
    {
        IEnumerable<IAzureClientConfiguration> _configs;

        public AzureStorageCleaner(IEnumerable<IAzureClientConfiguration> configs)
        {
            _configs = configs;
        }

        public void Dispose()
        {
            
        }

        public void Initialize()
        {
            _configs
                .AsParallel()
                .Select(c => c.CreateBlobClient())
                .SelectMany(c => c.ListContainers())
                .ForAll(c => c.Delete());

            _configs
                .AsParallel()
                .Select(c => c.CreateQueueClient())
                .SelectMany(c => c.ListQueues())
                .ForAll(c => c.Delete());

        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}