namespace VRage
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    public class MyTryConnectHelper
    {
        private static bool Initialized;
        private static FieldInfo m_Buffer;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int closesocket([In] IntPtr socketHandle);
        public static bool TryConnect(string ipString, int port)
        {
            IPAddress address;
            if (!Initialized)
            {
                WSAData lpWSAData = new WSAData();
                if (WSAStartup(0x202, out lpWSAData) != 0)
                {
                    return false;
                }
                m_Buffer = typeof(SocketAddress).GetField("m_Buffer", BindingFlags.NonPublic | BindingFlags.Instance);
                Initialized = true;
            }
            if (!IPAddress.TryParse(ipString, out address))
            {
                return false;
            }
            if ((port < 0) || (port > 0xffff))
            {
                return false;
            }
            IPEndPoint point = new IPEndPoint(address, port);
            SocketAddress address2 = point.Serialize();
            IntPtr socketHandle = WSASocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, IntPtr.Zero, 0, 1);
            if (socketHandle == new IntPtr(-1))
            {
                return false;
            }
            new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, point.Address.ToString(), point.Port).Demand();
            closesocket(socketHandle);
            return (WSAConnect(socketHandle, (byte[]) m_Buffer.GetValue(address2), address2.Size, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == 0);
        }

        [DllImport("ws2_32.dll", SetLastError=true)]
        internal static extern int WSAConnect([In] IntPtr socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr inBuffer, [In] IntPtr outBuffer, [In] IntPtr sQOS, [In] IntPtr gQOS);
        [DllImport("ws2_32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern IntPtr WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] IntPtr protocolInfo, [In] uint group, [In] int flags);
        [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        internal static extern int WSAStartup([In] short wVersionRequested, out WSAData lpWSAData);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSAData
        {
            internal short wVersion;
            internal short wHighVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x101)]
            internal string szDescription;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x81)]
            internal string szSystemStatus;
            internal short iMaxSockets;
            internal short iMaxUdpDg;
            internal IntPtr lpVendorInfo;
        }
    }
}

