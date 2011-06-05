using System;
using Lokad.Cqrs.Build.Engine;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsEngineBuilder
    {
        public static void Azure(this CqrsEngineBuilder @this, Action<AzureEngineModule> config)
        {
            var module = new AzureEngineModule();
            config(module);
            @this.Advanced.RegisterModule(module);
        }

    }
}