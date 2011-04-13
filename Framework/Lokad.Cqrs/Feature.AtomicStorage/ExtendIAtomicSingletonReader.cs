namespace Lokad.Cqrs.Feature.AtomicStorage
{
	public static class ExtendIAtomicSingletonReader
	{
		public static TView GetOrNew<TView>(this IAtomicSingletonReader<TView> reader) where TView : IAtomicSingleton, new()
		{
			return reader.Get().GetValue(() => new TView());
		}
	}
}