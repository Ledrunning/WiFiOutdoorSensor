using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Meteora.Esp8266.DataSenderEmulator.Enums;
using Meteora.Esp8266.DataSenderEmulator.Helpers;

namespace Meteora.Esp8266.DataSenderEmulator
{
    public partial class MainForm : Form
    {
        private bool _isRun;

        public MainForm()
        {
            InitializeComponent();
            InitializeComboBox();
            port.KeyPress += OnTextBoxKeyPress;
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

        private void OnTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private TcpServerService _serverService;

        private void OnRunClick(object sender, EventArgs e)
        {
            var currentIpl = GetCurrentIpAddress();
            currentIpLabel.Text = currentIpl;
            int.TryParse(port.Text, out var currentPort);
            _serverService = new TcpServerService(currentIpl, currentPort);

            _isRun = !_isRun;
            runButton.Text = _isRun ? "Run" : "Stop";

            if (_isRun)
            {
                _serverService.Start();
            }
            else
            {
                _serverService.Stop();
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

        private void OnSendTimeoutsChanged(object sender, EventArgs e)
        {
            if (!(sender is ComboBox comboBox))
            {
                return;
            }

            var selectedValue = comboBox.SelectedItem;
            _serverService?.ChangeInterval((int)selectedValue);
        }
    }
}