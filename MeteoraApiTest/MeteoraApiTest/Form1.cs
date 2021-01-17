using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeteoraApiTest
{
    public partial class Form1 : Form
    {
        private const string BaseAddress = "http://192.168.0.101/";
        private readonly Timer timer = new Timer();
        List<string> routings = new List<string>()
        {
            "/temperature",
            "/humidity",
            "/altitude",
            "/pressure",
            "/battery_status"
        };
        List<string> data = new List<string>();
        public Form1()
        {
            InitializeComponent();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += OnTimerTick;
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            var dto = await GetData();
            temperature.Text = dto[0];
            humidity.Text = dto[1];
            altitude.Text = dto[2];
            pressure.Text = dto[3];
            batteryLevel.Text = dto[4];
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

            return null;
        }
    }
}