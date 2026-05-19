using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MeteoraDesktop.Events;
using MeteoraDesktop.Model;

namespace MeteoraDesktop.Service
{
    public sealed class TelemetryService : ITelemetryService
    {
        private const int TemperatureIndex = 0;
        private const int HumidityIndex = 1;
        private const int AltitudeIndex = 2;
        private const int PressureIndex = 3;
        private const int BatteryLevelIndex = 4;
        private const short ReadingIntervalMs = 1000;
        private readonly List<string> _data = new List<string>();
        private readonly string _ipAddress;

        private readonly List<string> _routs = new List<string>
        {
            "/temperature",
            "/humidity",
            "/altitude",
            "/pressure",
            "/chargeLevel"
        };

        private readonly TelemetryDto _telemetryDto;

        public TelemetryService(string ipAddress)
        {
            _ipAddress = ipAddress;
            _telemetryDto = new TelemetryDto();
        }

        public event TelemetryReceiveEventHandler TelemetryEvent;

        public async Task ReadDataAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var receivedBuffer = await GetData();
                    if (receivedBuffer.Count == 0)
                    {
                        throw new Exception("Empty data");
                    }

                    _telemetryDto.Temperature = receivedBuffer[TemperatureIndex];
                    _telemetryDto.Humidity = receivedBuffer[HumidityIndex];
                    _telemetryDto.Altitude = receivedBuffer[AltitudeIndex];
                    _telemetryDto.Pressure = receivedBuffer[PressureIndex];
                    _telemetryDto.BatteryLevel = receivedBuffer[BatteryLevelIndex];

                    TelemetryEvent?.Invoke(new TelemetryEventArgs(_telemetryDto));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TelemetryService] Error reading data: {ex.Message}");
                }

                await Task.Delay(ReadingIntervalMs, token);
            }
        }

        public async Task<List<string>> GetData()
        {
            var buffer = new List<string>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_ipAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var page in _routs)
                {
                    try
                    {
                        var response = await client.GetAsync(page);

                        if (response.IsSuccessStatusCode)
                        {
                            var telemetry = await response.Content.ReadAsStringAsync();
                            buffer.Add(telemetry);
                        }
                        else
                        {
                            buffer.Add("--");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Request error {page}: {ex.Message}");
                        buffer.Add("--");
                    }
                }
            }

            return buffer;
        }
    }
}