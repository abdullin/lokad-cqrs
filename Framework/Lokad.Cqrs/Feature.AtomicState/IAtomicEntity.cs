namespace Lokad.Cqrs.Feature.AtomicState
{
	/// <summary>
	/// View entity that has an identity (there can be many views
	/// of this type)
	/// </summary>
// ReSharper disable UnusedTypeParameter
	public interface IAtomicEntity<TKey> : IAtomicStateBase
// ReSharper restore UnusedTypeParameter
	{
		
	}
}