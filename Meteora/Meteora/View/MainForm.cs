using System;
using System.Windows.Forms;
using MeteoraDesktop.Presenter;

namespace MeteoraDesktop.View
{
    public partial class MainForm : Form, IMainForm
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public MainFormPresenter Presenter { get; set; }

        public string Temperature
        {
            get => temperature.Text;
            set => temperature.Text = value;
        }

        public string Humidity
        {
            get => humidity.Text;
            set => humidity.Text = value;
        }

        public string Altitude
        {
            get => altitude.Text;
            set => altitude.Text = value;
        }

        public string Pressure
        {
            get => pressure.Text;
            set => pressure.Text = value;
        }

        public string BatteryLevel
        {
            get => batteryLevel.Text;
            set => batteryLevel.Text = value;
        }

        public event EventHandler OnLoadForm;

        private void OnLoad(object sender, EventArgs e)
        {
            OnLoadForm?.Invoke(sender, e);
        }
    }
}