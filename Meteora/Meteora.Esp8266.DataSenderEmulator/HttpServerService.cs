using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Meteora.Esp8266.DataSenderEmulator.Contracts;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public class HttpServerService : IHttpServerService
    {
        private const string WebPage = "index.html";
        private readonly HttpListener _listener;
        private string _htmlContent;
        private int _intervalInSeconds;
        private Timer _timer;

        public HttpServerService(string url)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
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
            _timer.Dispose();
            _listener.Stop();
            _listener.Close();
        }

        private void SendHtmlPage(object state)
        {
            var context = _listener.GetContext();
            var response = context.Response;
            var buffer = Encoding.UTF8.GetBytes(_htmlContent);

            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        private string GetHtmlContent()
        {
            _htmlContent = ReadFile();
            return _htmlContent;
        }

        private string ReadFile()
        {
            var fullPath = Path.Combine(Path.GetTempPath(), WebPage);
            return File.ReadAllText(fullPath);
        }

        private string GetBasePath()
        {
            return Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().GetName().CodeBase);
        }
    }
}