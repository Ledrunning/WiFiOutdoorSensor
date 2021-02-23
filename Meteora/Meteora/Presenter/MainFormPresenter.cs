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

        private readonly List<string> Routings = new List<string>
        {
            "/temperature",
            "/humidity",
            "/altitude",
            "/pressure",
            "/battery_status"
        };

        private readonly Timer timer = new Timer();

        private readonly List<string> Data = new List<string>();
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
            view.Temperature = dto[0];
            view.Humidity = dto[1];
            view.Altitude = dto[2];
            view.Pressure = dto[3];
            view.BatteryLevel = dto[4];
        }

        public async Task<List<string>> GetData()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var page in Routings)
                {
                    var response = await client.GetAsync(page);

                    if (response.IsSuccessStatusCode)
                    {
                        var telemetry = await response.Content.ReadAsStringAsync(); // без await - Task.All Wait
                        Data.Add(telemetry);
                    }
                }

                return Data;
            }

            return null;
        }
    }
}