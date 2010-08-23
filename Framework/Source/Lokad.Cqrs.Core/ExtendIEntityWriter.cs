#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using Lokad.Quality;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Extensions for the <see cref="IEntityWriter"/>
	/// </summary>
	[UsedImplicitly]
	public static class ExtendIEntityWriter
	{
		/// <summary>
		/// Updates an entity if the key already exists, or throws <see cref="EntityNotFoundException"/> otherwise.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity to update.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="key">The key of the entity to update.</param>
		/// <param name="updateFunction">The update function.</param>
		/// <exception cref="OptimisticConcurrencyException">when entity being updated had been changed concurrently</exception>
		/// <exception cref="EntityNotFoundException">when entity to update was not found</exception>
		public static void Update<TEntity>(this IEntityWriter store, object key, Action<TEntity> updateFunction)
		{
			store.Write(typeof (TEntity), key,
				k => { throw CqrsErrors.EntityNotFound(typeof (TEntity), k); },
				(k, value) =>
					{
						updateFunction((TEntity) value);
						return value;
					});
		}

		/// <summary>
		/// Creates an entity if the key does not already exist, or throws <see cref="EntityAlreadyExistsException"/> otherwise.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity to create.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="key">The key of the entity to create.</param>
		/// <param name="newEntityInit">The initialization function.</param>
		/// <exception cref="EntityAlreadyExistsException">when entity with the same key already exists</exception>
		public static void Add<TEntity>(this IEntityWriter store, object key, Action<TEntity> newEntityInit)
			where TEntity : new()
		{
			store.Write(typeof(TEntity), key,
				k =>
					{
						var entity = new TEntity();
						newEntityInit(entity);
						return entity;
					},
				(k, value) => { throw CqrsErrors.EntityAlreadyExists(typeof (TEntity), k); });
		}
		/// <summary>
		/// Creates an entity if the key does not already exist, or throws <see cref="EntityAlreadyExistsException"/> otherwise.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity to create.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="key">The key of the entity to create.</param>
		/// <param name="entity">The entity to add.</param>
		/// <exception cref="EntityAlreadyExistsException">when entity with the same key already exists</exception>
		public static void Add<TEntity>(this IEntityWriter store, object key, TEntity entity)
		{
			store.Write(typeof(TEntity), key,
				k => entity,
				(k, value) => { throw CqrsErrors.EntityAlreadyExists(typeof(TEntity), k); });
		}

		/// <summary>
		/// Update the entity with a given key, creating a new one before that, if needed.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="key">The key.</param>
		/// <param name="updateEntity">The function used to update entity.</param>
		/// <param name="entityFactory">The function used to create new entity before updating, if it does not exist.</param>
		public static void UpdateOrAdd<TEntity>(this IEntityWriter store, Func<TEntity> entityFactory, object key, Action<TEntity> updateEntity)
		{
			store.Write(typeof(TEntity), key, o =>
				{
					var entity = entityFactory();
					updateEntity(entity);
					return entity;
				}, (key1, value) =>
					{
						updateEntity((TEntity) value);
						return value;
					});
		}

		/// <summary>
		/// Update the entity with a given key, creating a new one before that, if needed.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="key">The key.</param>
		/// <param name="updateEntity">The function used to update entity.</param>
		/// <param name="entityFactory">The function used to create new entity before updating, if it does not exist.</param>
		public static void UpdateOrAdd<TEntity, TKey>(this IEntityWriter store, Func<TKey,TEntity> entityFactory, TKey key, Action<TEntity> updateEntity)
		{
			store.Write(typeof(TEntity), key, o =>
			{
				var entity = entityFactory(key);
				updateEntity(entity);
				return entity;
			}, (key1, value) =>
			{
				updateEntity((TEntity)value);
				return value;
			});
		}

		/// <summary>
		/// Update the entity with a given key, creating a new one before that, if needed.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="key">The key.</param>
		/// <param name="updateValue">The function used to update entity.</param>
		public static void UpdateOrAdd<TEntity>(this IEntityWriter store, object key, Action<TEntity> updateValue)
			where TEntity : new()
		{
			store.UpdateOrAdd(() => new TEntity(), key, updateValue);
		}


		

		/// <summary>
		/// Deletes the specified entity, given it's type and identity
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="identity">The identity.</param>
		public static void Remove<TEntity>(this IEntityWriter store, string identity)
		{
			store.Remove(typeof (TEntity), identity);
		}
	}
}