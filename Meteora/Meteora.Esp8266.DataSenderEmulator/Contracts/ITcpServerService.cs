namespace Meteora.Esp8266.DataSenderEmulator.Contracts
{
    public interface IHttpServerService
    {
        void ChangeInterval(int newIntervalInSeconds);
        void Start();
        void Stop();
    }
}