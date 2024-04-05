using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Meteora.Esp8266.DataSenderEmulator.Helpers
{
    public class LocalNetwork
    {
        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string GetWirelessIpAddress()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                {
                    continue;
                }

                foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }

            throw new Exception("No WiFi network adapters with an IPv4 address in the system!");
        }
    }
}