using System;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Client;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsClientBuilder
    {
        public static void Azure(this CqrsClientBuilder @this, Action<AzureClientModule> config)
        {
            var module = new AzureClientModule();
            config(module);
            @this.Advanced.RegisterModule(module);
        }
    }
}