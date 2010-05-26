using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Lokad.Quality;

namespace CloudBus.Domain
{
	[UsedImplicitly]
	public sealed class AssemblyMessageScan : IMessageScan
	{
		public void LoadSystemMessages()
		{
			var mappings = GetType()
				.Assembly
				.GetTypes()
				.Where(t => t.IsPublic)
				.Where(t => t.IsDefined(typeof(DataContractAttribute), false))
				.Select(c => new DomainMessageMapping(c, typeof(DomainMessageMapping.BusSystem)));

			_mappings.AddRange(mappings);
		}

		
		readonly List<DomainMessageMapping> _mappings = new List<DomainMessageMapping>();

		static IEnumerable<Type> GetConsumedMessages(Type type, Type consumerTypeDefinition)
		{
			var interfaces = type
				.GetInterfaces()
				.Where(i => i.IsGenericType)
				.Where(t => t.GetGenericTypeDefinition() == consumerTypeDefinition);

			foreach (var consumerInterface in interfaces)
			{
				yield return consumerInterface.GetGenericArguments()[0];
			}
		}

		public MethodInfo ConsumingMethod { get; set; }

		public void LoadDomainMessagesAndConsumers(
			IEnumerable<Assembly> assemblies, 
			Type consumingTypeDefition,
			Func<Type, bool> messageSelector)
		{
			var types = assemblies
				.SelectMany(a => a.GetExportedTypes())
				.ToList();

			var mappings = types
				.Where(t => false == t.IsAbstract)
				.SelectMany(handler =>
					GetConsumedMessages(handler, consumingTypeDefition)
						.Select(c => new DomainMessageMapping(c, handler)))
				.ToList();

			_mappings.AddRange(mappings);

			// add unmapped messages
			var listed = mappings.ToSet(m => m.Message);


			var unmapped = types
				.Where(messageSelector)
				.Where(m => false == listed.Contains(m))
				.Select(c => new DomainMessageMapping(c, typeof(DomainMessageMapping.BusNull)));

			_mappings.AddRange(unmapped);
		}

		public IEnumerable<DomainMessageMapping> GetMappings()
		{
			return _mappings;
		}

		public IMessageDirectory BuildDirectory(Func<DomainMessageMapping, bool> filter)
		{
			var builder = new MessageDirectoryBuilder();
			builder.AddConstraint(filter);
			return builder.BuildDirectory(this.GetMappings(), ConsumingMethod.Name);
		}
	}
}