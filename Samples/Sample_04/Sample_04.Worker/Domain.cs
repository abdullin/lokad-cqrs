#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Directory.Default;

namespace Sample_04.Worker
{
    #region PingPongCommand
    /// <summary>
    /// That's the recommended way of defining messages:
    /// 1. Immutable
    /// 2. With Properties
    /// 3. Private empty ctor for the ProtoBuf
    /// 4. Public CTOR that sets the properties
    /// </summary>
    [DataContract]
    public sealed class PingPongCommand : IMessage
    {
        [DataMember(Order = 1)]
        public int Ball { get; private set; }

        [DataMember(Order = 2)]
        public string Game { get; private set; }

        public PingPongCommand()
        {
        }

        public PingPongCommand(int ball, string game)
        {
            Ball = ball;
            Game = game;
        }

        public PingPongCommand Pong()
        {
            return new PingPongCommand(Ball + 1, Game);
        }
    } 
    #endregion

    #region PingPongCommand consumer
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
            var title = string.Format(format, message.Ball, message.Game);
            Trace.WriteLine(title);

            if (new Random().Next(6) == 1)
            {
                Trace.WriteLine("Missing!");
                throw new BounceFailedException("Bouncing failed for: " + title);
            }
            Thread.Sleep(1000);

            _sender.SendOne(message.Pong());
        }
    } 
    #endregion

    public class BounceFailedException : Exception
    {
        public BounceFailedException(string message)
            : base(message)
        {
        }
    }
}