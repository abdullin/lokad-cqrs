#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Build.Engine.Events
{
    public sealed class HostStarted : ISystemEvent
    {
        public readonly string[] EngineProcesses;

        public HostStarted(string[] engineProcesses)
        {
            EngineProcesses = engineProcesses;
        }

        public override string ToString()
        {
            return string.Format("Host started: {0}", string.Join(",", EngineProcesses));
        }
    }
}