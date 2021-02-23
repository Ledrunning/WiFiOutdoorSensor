using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeteoraDesktop.View;

namespace MeteoraDesktop.Presenter
{
    public class MainFormPresenter
    {
        private const string BaseAddress = "http://192.168.0.101/";
        private const int TemperatureIndex = 0;
        private const int HumidityIndex = 1;
        private const int AltitudeIndex = 2;
        private const int PressureIndex = 3;
        private const int BatteryLevelIndex = 4;

        private readonly List<string> data = new List<string>();

        private readonly List<string> routings = new List<string>
        {
            "/temperature",
            "/humidity",
            "/altitude",
            "/pressure",
            "/battery_status"
        };

        private readonly Timer timer = new Timer();
        private readonly IMainForm view;

        public MainFormPresenter(IMainForm view)
        {
            this.view = view;
            view.Presenter = this;

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += OnTimerTick;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            var dto = await GetData();
            view.Temperature = dto[TemperatureIndex];
            view.Humidity = dto[HumidityIndex];
            view.Altitude = dto[AltitudeIndex];
            view.Pressure = dto[PressureIndex];
            view.BatteryLevel = dto[BatteryLevelIndex];
        }

        public async Task<List<string>> GetData()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
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