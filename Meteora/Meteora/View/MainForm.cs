using System.Windows.Forms;

namespace MeteoraDesktop.View
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

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
    }
}