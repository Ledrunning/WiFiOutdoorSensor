using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Meteora.Esp8266.DataSenderEmulator.Helpers
{
    public class LocalNetwork
    {
        public static string GetActiveIPv4(NetworkInterfaceType type)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType != type)
                    continue;

                if (ni.Description.ToLower().Contains("virtual") ||
                    ni.Description.ToLower().Contains("docker") ||
                    ni.Description.ToLower().Contains("vmware") ||
                    ni.Description.ToLower().Contains("hyper-v"))
                    continue;

                var props = ni.GetIPProperties();

                if (props.GatewayAddresses.All(g => g.Address.AddressFamily != AddressFamily.InterNetwork))
                    continue;

                foreach (var ip in props.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        return ip.Address.ToString();
                }
            }

            throw new Exception($"No active IPv4 for {type}");
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