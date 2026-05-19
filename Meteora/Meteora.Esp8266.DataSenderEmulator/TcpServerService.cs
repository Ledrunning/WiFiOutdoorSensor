using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Meteora.Esp8266.DataSenderEmulator.Contracts;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
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
        private readonly HtmlDocument _htmlDocument;
        private bool _isRunning;
        private TcpListener _listener;
        private readonly Random _randomizer;

        public TcpServerService(string ipAddress, int port, int intervalInSecond)
        {
            _ipAddress = ipAddress;
            _port = port;
            _randomizer = new Random();
            GetHtmlContent();
            _htmlDocument = new HtmlDocument();

            _timer = new Timer(async _ => await BroadcastHtmlContentAsync(), null, 0, intervalInSecond * 1000);
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

        private string UpdateHtml()
        {
            _htmlDocument.LoadHtml(_htmlContent);

            _htmlDocument.GetElementbyId("temperature").InnerHtml = _randomizer.Next(-10, 40).ToString();
            _htmlDocument.GetElementbyId("humidity").InnerHtml = _randomizer.Next(20, 90).ToString();
            _htmlDocument.GetElementbyId("pressure").InnerHtml = _randomizer.Next(700, 800).ToString();
            _htmlDocument.GetElementbyId("altitude").InnerHtml = _randomizer.Next(50, 500).ToString();
            _htmlDocument.GetElementbyId("bmpTemperature").InnerHtml = _randomizer.Next(-10, 40).ToString();
            _htmlDocument.GetElementbyId("chargeLevel").InnerHtml = _randomizer.Next(0, 100).ToString();

            return _htmlDocument.DocumentNode.OuterHtml;
        }

        private async Task BroadcastHtmlContentAsync()
        {
            var updatedHtmlContent = UpdateHtml();

            try
            {
                if (_isRunning)
                {
                    using (var client = await _listener.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        var requestBuffer = new byte[1024];
                        var bytesRead = await stream.ReadAsync(requestBuffer, 0, requestBuffer.Length);

                        var request = Encoding.UTF8.GetString(requestBuffer, 0, bytesRead);

                        if (request.Contains("GET / HTTP"))
                        {
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/html\r\n" +
                                           $"Content-Length: {updatedHtmlContent.Length}\r\n" +
                                           "\r\n" +
                                           $"{updatedHtmlContent}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        else if (request.Contains("GET /temperature"))
                        {
                            var temperature = _randomizer.Next(-10, 40).ToString();
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/plain\r\n" +
                                           $"Content-Length: {temperature.Length}\r\n" +
                                           "\r\n" +
                                           $"{temperature}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        else if (request.Contains("GET /humidity"))
                        {
                            var humidity = _randomizer.Next(20, 90).ToString();
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/plain\r\n" +
                                           $"Content-Length: {humidity.Length}\r\n" +
                                           "\r\n" +
                                           $"{humidity}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        else if (request.Contains("GET /pressure"))
                        {
                            var pressure = _randomizer.Next(700, 800).ToString();
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/plain\r\n" +
                                           $"Content-Length: {pressure.Length}\r\n" +
                                           "\r\n" +
                                           $"{pressure}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        else if (request.Contains("GET /altitude"))
                        {
                            var altitude = _randomizer.Next(100, 2000).ToString();
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/plain\r\n" +
                                           $"Content-Length: {altitude.Length}\r\n" +
                                           "\r\n" +
                                           $"{altitude}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        else if (request.Contains("GET /bmpTemperature"))
                        {
                            var bmpTemperature = _randomizer.Next(500, 800).ToString();
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/plain\r\n" +
                                           $"Content-Length: {bmpTemperature.Length}\r\n" +
                                           "\r\n" +
                                           $"{bmpTemperature}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                        else if (request.Contains("GET /chargeLevel"))
                        {
                            var chargeLevel = _randomizer.Next(80, 100).ToString();
                            var response = "HTTP/1.1 200 OK\r\n" +
                                           "Content-Type: text/plain\r\n" +
                                           $"Content-Length: {chargeLevel.Length}\r\n" +
                                           "\r\n" +
                                           $"{chargeLevel}";
                            var responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
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