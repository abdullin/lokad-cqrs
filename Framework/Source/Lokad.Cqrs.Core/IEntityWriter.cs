using System;

namespace Lokad.Cqrs
{
	/// <summary>
	/// The function used to generate a value for an absent key
	/// </summary>
	public delegate object AddEntityDelegate(object key);

	/// <summary>
	/// The function used to generate a new value for an existing key based on the key's existing value
	/// </summary>
	public delegate object UpdateEntityDelegate(object key, object oldValue);

	/// <summary>
	/// Handles write operations for the state storage
	/// </summary>
	public interface IEntityWriter
	{
		/// <summary>
		/// Updates an entity if the key already exists or creates a new entity otherwise.
		/// </summary>
		/// <param name="type">The type of the entity to patch.</param>
		/// <param name="key">The identity of the entity to patch.</param>
		/// <param name="addEntityDelegate">The function used to generate an entity for an absent key.</param>
		/// <param name="updateEntityDelegate">The function used to generate a new entity for an existing key based on the existing entity.</param>
		/// <exception cref="OptimisticConcurrencyException">when updated entity had been changed concurrently</exception>
		void Write(Type type, object key, AddEntityDelegate addEntityDelegate, UpdateEntityDelegate updateEntityDelegate);

		/// <summary>
		/// Deletes the specified entity, given it's type and key
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="key">The identity.</param>
		void Remove(Type type, object key);
	}
}