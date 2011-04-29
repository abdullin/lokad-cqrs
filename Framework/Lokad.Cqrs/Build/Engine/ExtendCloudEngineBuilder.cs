using System;

namespace Lokad.Cqrs.Build.Engine
{
    public static class ExtendCloudEngineBuilder
    {
        public static void Azure(this CloudEngineBuilder @this, Action<AzureModule> config)
        {
            var module = new AzureModule();
            config(module);
            @this.EnlistModule(module);
        }
    }
}