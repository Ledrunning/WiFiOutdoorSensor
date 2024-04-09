using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Meteora.Esp8266.DataSenderEmulator.Contracts;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public class TcpServerService : IHttpServerService
    {
        private readonly string _ipAddress;
        private readonly int _port;
        private const string WebPage = "index.html";
        private readonly TcpListener _listener;
        private int _intervalInSeconds;
        private Timer _timer;

        public TcpServerService(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            var localAddress = IPAddress.Parse(_ipAddress);
            _listener = new TcpListener(localAddress, _port);
        }

        public void ChangeInterval(int newIntervalInSeconds)
        {
            _intervalInSeconds = newIntervalInSeconds;
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(_intervalInSeconds));
        }

        public void Stop()
        {
            _timer?.Dispose();
            _listener?.Stop();
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream);
                var request = reader.ReadLine();

                if (request != null && request.StartsWith("GET"))
                {
                    // Construct the HTTP response with HTML content
                    var htmlContent = GetHtmlContent();
                    var response = "HTTP/1.1 200 OK\r\n" +
                                   "Content-Type: text/html\r\n" +
                                   $"Content-Length: {htmlContent.Length}\r\n" +
                                   "\r\n" +
                                   htmlContent;

                    var responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
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

        public void Start()
        {
            try
            {
                _listener.Start();
                Debug.WriteLine($"Server started on {_ipAddress}:{_port}");

                // Accept incoming client connections asynchronously
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting server: {ex.Message}");
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