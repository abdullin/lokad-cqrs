using System;
using System.IO;
using Lokad.Cqrs.Durability;

namespace Lokad.Cqrs.Consume
{
	public delegate void PrintMessageErrorDelegate(MessageEnvelope message, Exception ex, TextWriter writer);
}