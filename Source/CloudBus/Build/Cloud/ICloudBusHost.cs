namespace Bus2.Build.Cloud
{
	public interface ICloudBusHost
	{
		void Start();
		void Initialize();
		void Stop();
	}
}