using System;

namespace Lokad.Cqrs.Build.Client
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