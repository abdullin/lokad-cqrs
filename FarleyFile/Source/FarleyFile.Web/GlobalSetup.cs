using System.Diagnostics;

using Lokad.Cqrs;
using Lokad.Cqrs.Build.Client;
using Lokad.Cqrs.Feature.AtomicStorage;


namespace FarleyFile.Web
{
    public static class GlobalSetup
    {
        public static readonly CqrsClient BusInstance;

        static GlobalSetup()
        {
            BusInstance = Build();
        }

        static CqrsClient Build()
        {
            
            var data = AzureStorage.CreateConfigurationForDev();
            var builder = new CqrsClientBuilder();
            builder.Domain(d => d.HandlerSample<Farley.IFarleyHandler<Farley.IMessage>>(m => m.Consume(null)));
            builder.UseProtoBufSerialization();
            builder.Azure(c => c.AddAzureSender(data, "farley-publish"));
            builder.Storage(s =>
                {
                    s.AtomicIsInAzure(data);
                    s.StreamingIsInAzure(data);
                });

            return builder.Build();
        }

        internal static void InitIfNeeded()
        {
            Trace.WriteLine("Bus running: " + BusInstance.GetHashCode());
        }
    }

    public static class FarleyClient
    {
        public static void Send(params Farley.Command[] commands)
        {
            GlobalSetup.BusInstance
                .Resolve<IMessageSender>()
                .SendBatch(commands);
        }

        public static NuclearStorage Storage
        {
            get { return GlobalSetup.BusInstance.Resolve<NuclearStorage>(); }
        }
    }
}