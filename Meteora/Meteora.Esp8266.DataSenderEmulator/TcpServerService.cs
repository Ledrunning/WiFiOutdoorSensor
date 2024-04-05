using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Meteora.Esp8266.DataSenderEmulator.Contracts;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public class HttpServerService : IHttpServerService
    {
        private const string WebPage = "index.html";
        private readonly HttpListener _listener;
        private int _intervalInSeconds;
        private Timer _timer;

        public HttpServerService(string ipAddress)
        {
            var absoluteUri = new Uri($"http://192.168.1.102:{8080}").AbsoluteUri;
            _listener = new HttpListener();
            _listener.Prefixes.Add(absoluteUri);
            _listener.Start();
        }

        public void ChangeInterval(int newIntervalInSeconds)
        {
            _intervalInSeconds = newIntervalInSeconds;
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(_intervalInSeconds));
        }

        public void Start()
        {
            _timer = new Timer(SendHtmlPage, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void Stop()
        {
            _timer?.Dispose();
            _listener?.Stop();
            _listener?.Close();
        }

        private void SendHtmlPage(object state)
        {
            try
            {
                var context = _listener.GetContext();
                var response = context.Response;
                var buffer = Encoding.UTF8.GetBytes(GetHtmlContent());

                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"Error sending HTML page: {ex.Message}");
            }
        }

        private static string GetHtmlContent()
        {
            try
            {
                var fullPath = Path.Combine(Path.GetTempPath(), WebPage);
                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading HTML file: {ex.Message}");
                return string.Empty;
            }
        }
    }
}