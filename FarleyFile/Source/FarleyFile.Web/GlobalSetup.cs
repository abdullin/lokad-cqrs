using System;
using System.Diagnostics;
using System.IO;
using FarleyFile.Views;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Client;
using Lokad.Cqrs.Feature.AtomicStorage;
using ServiceStack.Text;


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
                    s.AtomicIsInAzure(data, b =>
                        {
                            b.CustomSerializer(JsonSerializer.SerializeToStream, JsonSerializer.DeserializeFromStream);
                            b.WhereEntity(t => t.Name.EndsWith("View") && t.IsDefined(typeof(SerializableAttribute),false));
                        });
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

        static readonly NuclearStorage Storage = GlobalSetup.BusInstance.Resolve<NuclearStorage>();

        public static UserDashboardView GetUserDashboard()
        {
            return Storage.GetSingletonOrNew<UserDashboardView>();
        }
    }
}