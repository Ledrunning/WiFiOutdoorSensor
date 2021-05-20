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
        private readonly List<string> data = new List<string>();
        private readonly string ipAddress;

        private readonly List<string> routings = new List<string>
        {
            "/temperature",
            "/humidity",
            "/altitude",
            "/pressure",
            "/battery_status"
        };

        private readonly TelemetryDto telemetryDto;

        private readonly Timer timer = new Timer();

        public TelemetryService(string ipAddress)
        {
            this.ipAddress = ipAddress;
            telemetryDto = new TelemetryDto();
        }

        public event TelemetryReceiveEventHandler TelemetryEvent;


        public async Task ReadDataAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var receivedBuffer = await GetData();
                telemetryDto.Temperature = receivedBuffer[TemperatureIndex];
                telemetryDto.Humidity = receivedBuffer[HumidityIndex];
                telemetryDto.Altitude = receivedBuffer[AltitudeIndex];
                telemetryDto.Pressure = receivedBuffer[PressureIndex];
                telemetryDto.BatteryLevel = receivedBuffer[BatteryLevelIndex];

                TelemetryEvent?.Invoke(new TelemetryEventArgs(telemetryDto));

                await Task.Delay(1000, token);
            }
        }


        public async Task<List<string>> GetData()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ipAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var page in routings)
                {
                    var response = await client.GetAsync(page);

                    if (response.IsSuccessStatusCode)
                    {
                        var telemetry = await response.Content.ReadAsStringAsync();
                        data.Add(telemetry);
                    }
                }

                return data;
            }
        }
    }
}