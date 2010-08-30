namespace Lokad.Reflection
{
	static class ReflectCache<T>
	{
		public static string ReferenceName = "{" + typeof (T).Name + "}";
		public static string SequenceName = "{" + typeof(T).Name + "}[]";
	}
}
