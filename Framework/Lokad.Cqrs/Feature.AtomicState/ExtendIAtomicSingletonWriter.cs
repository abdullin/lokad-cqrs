using System;

namespace Lokad.Cqrs.Feature.AtomicState
{
	public static class ExtendIAtomicSingletonWriter
	{
		public static void EnforceAndUpdate<TView>(this IAtomicSingletonWriter<TView> self , Action<TView> update) 
			where TView : IAtomicSingleton, new()
		{
			self.AddOrUpdate(() =>
				{
					var view = new TView();
					update(view);
					return view;
				}, update);
		}

		public static void EnforceAndUpdate<TKey,TView>(this IAtomicEntityWriter<TKey,TView> self, TKey key, Action<TView> update) 
			where TView : IAtomicEntity<TKey>, new()
		{
			self.AddOrUpdate(key, () =>
				{
					var view = new TView();
					update(view);
					return view;

				}, update);
		}
	}
}