#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Directory.Default;

namespace Sample_02.Worker
{
    #region SendPaymentMessage
    [DataContract]
    public sealed class SendPaymentMessage : IMessage
    {
        [DataMember(Order = 1)]
        public double Amount { get; private set; }

        public SendPaymentMessage(double amount)
        {
            Amount = amount;
        }

        public SendPaymentMessage()
        {
        }

        public override string ToString()
        {
            // makes the message more readable in the debug logs
            return string.Format("Send Payment ({0})", Amount);
        }
    } 
    #endregion

    #region Message consumers
    public sealed class SendPaymentHandler : IConsume<SendPaymentMessage>
    {
        public void Consume(SendPaymentMessage message)
        {
            Trace.WriteLine("There is an incoming payment! " + message.Amount);
        }
    }

    public sealed class ListenToEverythingHandler : IConsume<IMessage>
    {
        public void Consume(IMessage message)
        {
            Trace.WriteLine("Got message of type: " + message.GetType());
        }
    } 
    #endregion

    #region Task
    public sealed class SendPaymentsTask : IEngineProcess
    {
        readonly IMessageSender _sender;

        public SendPaymentsTask(IMessageSender sender)
        {
            // sender will be injected by the IoC
            _sender = sender;
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var wait = ExecuteTask();
                        if (wait == TimeSpan.MaxValue)
                        {
                            // quit task
                            return;
                        }
                        token.WaitHandle.WaitOne(wait);

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                    }
                }
            });
        }

        TimeSpan ExecuteTask()
        {
            var amount = Math.Round(new Random().NextDouble() * 100, 4);

            // send new message
            _sender.SendOne(new SendPaymentMessage(amount));

            return TimeSpan.FromSeconds(3);
        }
    } 
    #endregion
}