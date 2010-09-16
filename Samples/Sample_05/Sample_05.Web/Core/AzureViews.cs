using Lokad;
using Lokad.Cqrs;
using Lokad.Cqrs.Default;
using Sample_05.Contracts;

namespace Sample_05.Web
{
	public static class AzureViews
	{
		public static Maybe<TView> Get<TView>(object identity)
			where TView : IEntity
		{
			return GlobalSetup.Views.Read<TView>(identity);
		}
	}
}