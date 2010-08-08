namespace Lokad.Cqrs
{
	public enum StorageConditionType
	{
		None,
		IfUnmodifiedSince,
		IfMatch,
		IfModifiedSince,
		IfNoneMatch
	}
}