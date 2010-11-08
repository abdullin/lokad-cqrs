namespace Lokad.Cqrs
{
	/// <summary>
	/// Represents storage container regefence
	/// </summary>
	public interface IStorageContainer
	{
		/// <summary>
		/// Gets the child container nested within the current container reference.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		IStorageContainer GetContainer(string name);
		/// <summary>
		/// Gets the storage item reference within the current container.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		IStorageItem GetItem(string name);

		/// <summary>
		/// Ensures that the current reference represents valid container
		/// </summary>
		/// <returns></returns>
		IStorageContainer Create();
		/// <summary>
		/// Deletes container
		/// </summary>
		void Remove();
		/// <summary>
		/// Checks if the underlying container exists
		/// </summary>
		/// <returns></returns>
		bool Exists();

		/// <summary>
		/// Gets the full path.
		/// </summary>
		/// <value>The full path.</value>
		string FullPath { get; }
	}
}