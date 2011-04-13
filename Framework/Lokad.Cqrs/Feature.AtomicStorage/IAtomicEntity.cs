namespace Lokad.Cqrs.Feature.AtomicStorage
{
	/// <summary>
	/// View entity that has an identity (there can be many views
	/// of this type)
	/// </summary>
// ReSharper disable UnusedTypeParameter
	public interface IAtomicEntity<TKey> : IAtomicEntity
// ReSharper restore UnusedTypeParameter
	{
		
	}

	/// <summary>
	/// Base marker interface for the views, used internally to
	/// simplify assembly-based lookups
	/// </summary>
	public interface IAtomicEntity
	{
	}
}