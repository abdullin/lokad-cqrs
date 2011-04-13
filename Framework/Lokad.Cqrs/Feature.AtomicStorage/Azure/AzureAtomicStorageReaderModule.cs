using Autofac;

namespace Lokad.Cqrs.Feature.AtomicStorage.Azure
{
	public sealed class AzureAtomicStorageReaderModule : Module
	{
		readonly IAzureAtomicStorageStrategy _strategy;

		public AzureAtomicStorageReaderModule(IAzureAtomicStorageStrategy strategy)
		{
			_strategy = strategy;
		}


		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterGeneric(typeof(AzureAtomicEntityReader<,>))
				.As(typeof(IAtomicEntityReader<,>))
				.SingleInstance();
			builder
				.RegisterGeneric(typeof(AzureAtomicSingletonReader<>))
				.As(typeof(IAtomicSingletonReader<>))
				.SingleInstance();

			builder.RegisterInstance(_strategy);
		}
	}
}