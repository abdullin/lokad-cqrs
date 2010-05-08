using Lokad;

namespace Bus2
{
	public interface IProvideBusSettings
	{
		Maybe<string> GetString(string key);
	}
}