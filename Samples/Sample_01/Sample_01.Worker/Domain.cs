#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Directory.Default;

namespace Sample_01.Worker
{
    [DataContract]
    public sealed class PingPongCommand : IMessage
    {
        [DataMember(Order = 1)]
        public int Ball { get; private set; }

        [DataMember(Order = 2)]
        public string Game { get; private set; }

        public PingPongCommand(int ball, string game)
        {
            Ball = ball;
            Game = game;
        }

        public PingPongCommand()
        {
        }

        public PingPongCommand Pong()
        {
            return new PingPongCommand(Ball + 1, Game);
        }
    }

    public sealed class PingPongHandler : IConsume<PingPongCommand>
    {
        readonly IMessageSender _sender;

        public PingPongHandler(IMessageSender sender)
        {
            _sender = sender;
        }

        public void Consume(PingPongCommand message)
        {
            const string format = "Ping #{0} in game '{1}'.";
            Trace.WriteLine(string.Format(format, message.Ball, message.Game));
            Thread.Sleep(1000);

            _sender.SendOne(message.Pong());
        }
    }
}