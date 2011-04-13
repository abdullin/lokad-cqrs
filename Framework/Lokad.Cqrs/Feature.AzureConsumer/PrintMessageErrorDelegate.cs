using System;
using System.IO;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public delegate void PrintMessageErrorDelegate(MessageEnvelope message, Exception ex, TextWriter writer);
}