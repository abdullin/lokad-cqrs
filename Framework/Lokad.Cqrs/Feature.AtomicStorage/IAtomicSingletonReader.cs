using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
	/// <summary>
	/// Strongly-typed reader for the view singletons.
	/// </summary>
	/// <typeparam name="TSingleton">The type of the view.</typeparam>
	public interface IAtomicSingletonReader<TSingleton> where TSingleton : IAtomicSingleton
	{
		/// <summary>
		/// Gets view singleton (if it's available).
		/// </summary>
		/// <returns>View singleton (if it's available)</returns>
		Maybe<TSingleton> Get();
	}
}