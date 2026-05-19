using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Meteora.Esp8266.DataSenderEmulator.Enums;
using Meteora.Esp8266.DataSenderEmulator.Helpers;

namespace Meteora.Esp8266.DataSenderEmulator;

public partial class MainForm : Form
{
    private bool _isRun;

    private TcpServerService _serverService;

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

    private void OnRunClick(object sender, EventArgs e)
    {
        if (!int.TryParse(port.Text, out var currentPort))
        {
            MessageBox.Show(@"Enter a valid Port!", @"Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _isRun = !_isRun;

        if (_isRun)
        {
            if (!CreateServer(currentPort))
            {
                _isRun = false;
                runButton.Text = @"Run";
                return;
            }

            try
            {
                _serverService?.Start();
                runButton.Text = @"Stop";
            }
            catch (Exception ex)
            {
                _isRun = false;
                runButton.Text = "Run";
                currentIpLabel.Text = @"-/-";
                MessageBox.Show($@"Failed to start server: {ex.Message}",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            _serverService?.Stop();
            currentIpLabel.Text = @"-/-";
            runButton.Text = @"Run";
        }
    }

    private bool CreateServer(int currentPort)
    {
        var currentIp = GetCurrentIpAddress();
        if (string.IsNullOrWhiteSpace(currentIp))
        {
            return false;
        }

        if (sendTimeouts.SelectedItem == null ||
            !int.TryParse(sendTimeouts.SelectedItem.ToString(), out var timeout))
        {
            MessageBox.Show(@"Select a timeout value!", @"Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        // Stop the old server before creating a new one
        _serverService?.Stop();
        _serverService = new TcpServerService(currentIp, currentPort, timeout);
        currentIpLabel.Text = currentIp;
        return true;
    }

    private string GetCurrentIpAddress()
    {
        try
        {
            var selectedAdapter = networkAdapters.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedAdapter))
            {
                MessageBox.Show(@"Select a network adapter!", @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            var selectedEnum = (NetworkAdapters)Enum.Parse(typeof(NetworkAdapters), selectedAdapter);
            return selectedEnum switch
            {
                NetworkAdapters.Lan => LocalNetwork.GetActiveIPv4(NetworkInterfaceType.Ethernet),
                NetworkAdapters.Wlan => LocalNetwork.GetActiveIPv4(NetworkInterfaceType.Wireless80211),
                _ => LocalNetwork.GetActiveIPv4(NetworkInterfaceType.Ethernet)
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error getting IP address: {ex.Message}",
                @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }
    }

    private void OnSendTimeoutsChanged(object sender, EventArgs e)
    {
        if (sender is not ComboBox comboBox)
        {
            return;
        }

        var selectedValue = comboBox.SelectedItem;
        _serverService?.ChangeInterval((int)selectedValue);
    }
}