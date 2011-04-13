using System;

namespace Lokad.Cqrs.Feature.ConcurrentState
{
	/// <summary>
	/// View writer interface, used by the event handlers
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TEntity">The type of the view.</typeparam>
	public interface IAtomicEntityWriter<TKey, TEntity> where TEntity : IAtomicEntity<TKey>
	{
		/// <summary>
		/// Adds the new view instance or updates it, if the instance already exists.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="addFactory">The add factory.</param>
		/// <param name="update">The update function.</param>
		void AddOrUpdate(TKey key, Func<TEntity> addFactory, Action<TEntity> update);
		/// <summary>
		/// Adds the new view instance or updates it, if the instance already exists.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="newView">The new view.</param>
		/// <param name="update">The update function.</param>
		void AddOrUpdate(TKey key, TEntity newView, Action<TEntity> update);
		/// <summary>
		/// Updates the view or throws exception, if it does not exist.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="change">The change.</param>
		void UpdateOrThrow(TKey key, Action<TEntity> change);
		/// <summary>
		/// Tries to update the view.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="change">The change.</param>
		/// <returns></returns>
		bool TryUpdate(TKey key, Action<TEntity> change);
		/// <summary>
		/// Deletes the view with the specified key. No exception is thrown, if the view does not exist.
		/// </summary>
		/// <param name="key">The key.</param>
		void Delete(TKey key);
	}
}