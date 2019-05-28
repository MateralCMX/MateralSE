namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;

    public static class MyExternalDebugStructures
    {
        public static readonly int MsgHeaderSize = Marshal.SizeOf(typeof(CommonMsgHeader));

        public static bool ReadMessageFromPtr<TMessage>(ref CommonMsgHeader header, IntPtr data, out TMessage outMsg) where TMessage: IExternalDebugMsg
        {
            outMsg = default(TMessage);
            if (((data == IntPtr.Zero) || (header.MsgSize != Marshal.SizeOf(typeof(TMessage)))) || (header.MsgType != outMsg.GetTypeStr()))
            {
                return false;
            }
            outMsg = (TMessage) Marshal.PtrToStructure(data, typeof(TMessage));
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ACConnectToEditorMsg : MyExternalDebugStructures.IExternalDebugMsg
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=40)]
            public string ACName;
            string MyExternalDebugStructures.IExternalDebugMsg.GetTypeStr() => 
                "AC_CON";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ACReloadInGameMsg : MyExternalDebugStructures.IExternalDebugMsg
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=40)]
            public string ACName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x400)]
            public string ACAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x400)]
            public string ACContentAddress;
            string MyExternalDebugStructures.IExternalDebugMsg.GetTypeStr() => 
                "AC_LOAD";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ACSendStateToEditorMsg : MyExternalDebugStructures.IExternalDebugMsg
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=240)]
            public string CurrentNodeAddress;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40)]
            public int[] VisitedTreeNodesPath;
            public static MyExternalDebugStructures.ACSendStateToEditorMsg Create(string currentNodeAddress, int[] visitedTreeNodesPath)
            {
                MyExternalDebugStructures.ACSendStateToEditorMsg msg = new MyExternalDebugStructures.ACSendStateToEditorMsg {
                    CurrentNodeAddress = currentNodeAddress,
                    VisitedTreeNodesPath = new int[0x40]
                };
                if (visitedTreeNodesPath != null)
                {
                    Array.Copy(visitedTreeNodesPath, msg.VisitedTreeNodesPath, Math.Min(visitedTreeNodesPath.Length, 0x40));
                }
                return msg;
            }

            string MyExternalDebugStructures.IExternalDebugMsg.GetTypeStr() => 
                "AC_STA";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CommonMsgHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=8)]
            public string MsgHeader;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=8)]
            public string MsgType;
            [MarshalAs(UnmanagedType.I4)]
            public int MsgSize;
            public static MyExternalDebugStructures.CommonMsgHeader Create(string msgType, int msgSize = 0) => 
                new MyExternalDebugStructures.CommonMsgHeader { 
                    MsgHeader = "VRAGEMS",
                    MsgType = msgType,
                    MsgSize = msgSize
                };

            public bool IsValid =>
                ((this.MsgHeader == "VRAGEMS") && (this.MsgSize > 0));
        }

        public interface IExternalDebugMsg
        {
            string GetTypeStr();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SelectedTreeMsg : MyExternalDebugStructures.IExternalDebugMsg
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=40)]
            public string BehaviorTreeName;
            string MyExternalDebugStructures.IExternalDebugMsg.GetTypeStr() => 
                "SELTREE";
        }
    }
}

