namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class IPAddressExtensions
    {
        public static IPAddress FromIPv4NetworkOrder(uint ip) => 
            new IPAddress((long) ((ulong) IPAddress.NetworkToHostOrder((int) ip)));

        public static IPAddress ParseOrAny(string ip)
        {
            IPAddress address;
            return (IPAddress.TryParse(ip, out address) ? address : IPAddress.Any);
        }

        public static uint ToIPv4NetworkOrder(this IPAddress ip) => 
            ((uint) IPAddress.HostToNetworkOrder((int) ((uint) ip.Address)));

        public static bool TryParseEndpoint(string ipAndPort, out IPEndPoint result)
        {
            try
            {
                int num;
                IPAddress address;
                string[] separator = new string[] { ":" };
                string[] strArray = ipAndPort.Replace(" ", string.Empty).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (((strArray.Length == 2) && IPAddress.TryParse(strArray[0], out address)) && int.TryParse(strArray[1], out num))
                {
                    result = new IPEndPoint(address, num);
                    return true;
                }
            }
            catch
            {
            }
            result = null;
            return false;
        }
    }
}

