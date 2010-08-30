namespace Lokad.Cqrs
{
	public enum StorageConditionType
	{
		None,
		IfUnmodifiedSince,
		/// <summary>
		/// Only perform the action if the client supplied entity matches the same entity on the server. 
		/// This is mainly for methods like PUT to only update a resource if it has not been modified since 
		/// the user last updated it.
		/// </summary>
		IfMatch,
		IfModifiedSince,
		IfNoneMatch
	}
}