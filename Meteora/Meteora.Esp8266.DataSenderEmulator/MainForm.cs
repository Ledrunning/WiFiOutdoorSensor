using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Meteora.Esp8266.DataSenderEmulator.Enums;
using Meteora.Esp8266.DataSenderEmulator.Helpers;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            var adapterNames = Enum.GetNames(typeof(NetworkAdapters));
            networkAdapters.DataSource = adapterNames;

            var sendTimeoutsMs = new List<short>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };

            sendTimeouts.DataSource = sendTimeoutsMs;
        }

        private bool _isRun;

        private void OnRunClick(object sender, EventArgs e)
        {
            var currentIpl = GetCurrentIpAddress();
            currentIpLabel.Text = currentIpl;
            var serverService = new TcpServerService(currentIpl);

            _isRun = !_isRun;
            runButton.Text = _isRun ? "Run" : "Stop";

            if (_isRun)
            {
                serverService.Start();
            }
            else
            {
                serverService.Stop();
            }
        }

        private string GetCurrentIpAddress()
        {
            var selectedAdapter = networkAdapters.SelectedItem.ToString();

            var selectedEnum = (NetworkAdapters)Enum.Parse(typeof(NetworkAdapters), selectedAdapter);

            switch (selectedEnum)
            {
                case NetworkAdapters.Lan:
                    return LocalNetwork.GetLocalIpAddress();
                case NetworkAdapters.Wlan:
                    return LocalNetwork.GetWirelessIpAddress();
                default:
                    return LocalNetwork.GetLocalIpAddress();
            }
        }
    }
}