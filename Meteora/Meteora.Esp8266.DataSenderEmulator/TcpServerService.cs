using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Meteora.Esp8266.DataSenderEmulator.Contracts;
using Timer = System.Threading.Timer;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public class TcpServerService : IHttpServerService
    {
        private const string WebPage = "WebPageTemplate\\index.html";
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly Timer _timer;

        private string _htmlContent;
        private bool _isRunning;
        private TcpListener _listener;

        public TcpServerService(string ipAddress, int port, int intervalInMilliseconds)
        {
            _ipAddress = ipAddress;
            _port = port;

            GetHtmlContent();

            _timer = new Timer(async _ => await BroadcastHtmlContentAsync(), null, 0, intervalInMilliseconds);
        }

        public void ChangeInterval(int newIntervalInMilliseconds)
        {
            _timer.Change(0, newIntervalInMilliseconds);
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _timer?.Dispose();
            _listener?.Stop();
            _listener = null;
            _isRunning = false;
            Debug.WriteLine($"Server stopped on {_ipAddress}:{_port}");
        }

        public void Start()
        {
            try
            {
                if (!_isRunning)
                {
                    _listener = new TcpListener(IPAddress.Parse(_ipAddress), _port);
                    _listener.Start();
                    _isRunning = true;
                    Debug.WriteLine($"Server started on {_ipAddress}:{_port}");
                }
                else
                {
                    Debug.WriteLine($"Server is already running on {_ipAddress}:{_port}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting server: {ex.Message}");
            }
        }

        private async Task BroadcastHtmlContentAsync()
        {
            try
            {
                if (_isRunning)
                {
                    using (var client = await _listener.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        var response = "HTTP/1.1 200 OK\r\n" +
                                       "Content-Type: text/html\r\n" +
                                       $"Content-Length: {_htmlContent.Length}\r\n" +
                                       "\r\n" +
                                       $"{_htmlContent}";

                        var responseData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error broadcasting HTML content: {ex.Message}");
            }
        }

        private void GetHtmlContent()
        {
            try
            {
                var fullPath = Path.Combine(Application.StartupPath, WebPage);
                _htmlContent = File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading HTML file: {ex.Message}");
            }
        }
    }
}