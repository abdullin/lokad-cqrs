using Autofac;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForLogging : Syntax, ISupportSyntaxForLogging
	{
		readonly ContainerBuilder _builder;

		public AutofacBuilderForLogging(ContainerBuilder builder)
		{
			_builder = builder;
		}

		public void RegisterLogProvider(ILogProvider provider)
		{
			_builder.RegisterInstance(provider);
		}
	}
}