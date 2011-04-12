using System;
using System.IO;
using Lokad.Cqrs.Core.Durability;

namespace Lokad.Cqrs.Feature.Consume
{
	public delegate void PrintMessageErrorDelegate(MessageEnvelope message, Exception ex, TextWriter writer);
}