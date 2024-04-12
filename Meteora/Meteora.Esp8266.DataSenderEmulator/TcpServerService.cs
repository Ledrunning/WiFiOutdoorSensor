using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Meteora.Esp8266.DataSenderEmulator.Contracts;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public class TcpServerService : IHttpServerService
    {
        private const string WebPage = "index.html";
        private readonly string _ipAddress;
        private readonly TcpListener _listener;
        private readonly int _port;

        private string _htmlContent;
        private int _intervalInMilliseconds;
        private Timer _timer;

        public TcpServerService(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            var localAddress = IPAddress.Parse(_ipAddress);
            _listener = new TcpListener(localAddress, _port);
            GetHtmlContent();
        }

        public void ChangeInterval(int newIntervalInMilliseconds)
        {
            _intervalInMilliseconds = newIntervalInMilliseconds;
            _timer.Interval = _intervalInMilliseconds;
        }

        public void Stop()
        {
            _timer?.Stop();
            _listener?.Stop();
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                Debug.WriteLine($"Server started on {_ipAddress}:{_port}");

                _timer = new Timer();
                _timer.Interval = _intervalInMilliseconds;
                _timer.Elapsed += async (sender, e) => await HandleClientAsync(await _listener.AcceptTcpClientAsync());
                _timer.AutoReset = true;
                _timer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting server: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream);
                var request = await reader.ReadLineAsync();

                if (request != null && request.StartsWith("GET"))
                {
                    // Construct the HTTP response with HTML content
                    var response = "HTTP/1.1 200 OK\r\n" +
                                   "Content-Type: text/html\r\n" +
                                   $"Content-Length: {_htmlContent.Length}\r\n" +
                                   "\r\n" +
                                   _htmlContent;

                    var responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    stream.Close();
                    client.Close();
                }
                else
                {
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling client request: {ex.Message}");
            }
        }

        private void GetHtmlContent()
        {
            try
            {
                var fullPath = Path.Combine(Path.GetTempPath(), WebPage);
                _htmlContent = File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading HTML file: {ex.Message}");
            }
        }
    }
}