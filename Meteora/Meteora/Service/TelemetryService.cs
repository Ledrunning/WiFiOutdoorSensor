using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MeteoraDesktop.Events;
using MeteoraDesktop.Model;
using Timer = System.Windows.Forms.Timer;

namespace MeteoraDesktop.Service
{
    public sealed class TelemetryService : ITelemetryService
    {
        private const int TemperatureIndex = 0;
        private const int HumidityIndex = 1;
        private const int AltitudeIndex = 2;
        private const int PressureIndex = 3;
        private const int BatteryLevelIndex = 4;
        private readonly List<string> _data = new List<string>();
        private readonly string _ipAddress;
        private const short ReadingIntervalMs = 1000;

        private readonly List<string> _routs = new List<string>
        {
            "/temperature",
            "/humidity",
            "/altitude",
            "/pressure",
            "/battery_status"
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
                var receivedBuffer = await GetData();
                _telemetryDto.Temperature = receivedBuffer[TemperatureIndex];
                _telemetryDto.Humidity = receivedBuffer[HumidityIndex];
                _telemetryDto.Altitude = receivedBuffer[AltitudeIndex];
                _telemetryDto.Pressure = receivedBuffer[PressureIndex];
                _telemetryDto.BatteryLevel = receivedBuffer[BatteryLevelIndex];

                TelemetryEvent?.Invoke(new TelemetryEventArgs(_telemetryDto));

                await Task.Delay(ReadingIntervalMs, token);
            }
        }


        public async Task<List<string>> GetData()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_ipAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var page in _routs)
                {
                    var response = await client.GetAsync(page);

                    if (response.IsSuccessStatusCode)
                    {
                        var telemetry = await response.Content.ReadAsStringAsync();
                        _data.Add(telemetry);
                    }
                }

                return _data;
            }
        }
    }
}