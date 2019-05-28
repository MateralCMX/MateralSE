namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using VRage.Win32;

    public static class MyMessageLoop
    {
        private static Dictionary<uint, ActionRef<Message>> m_messageDictionary = new Dictionary<uint, ActionRef<Message>>();
        private static Queue<Message> m_messageQueue = new Queue<Message>(0x40);
        private static Queue<WinApi.MyCopyData> m_messageCopyDataQueue = new Queue<WinApi.MyCopyData>(0x40);
        private static List<Message> m_tmpMessages = new List<Message>(0x40);
        private static List<WinApi.MyCopyData> m_tmpCopyData = new List<WinApi.MyCopyData>(0x40);

        public static void AddMessage(ref Message message)
        {
            Queue<Message> messageQueue = m_messageQueue;
            lock (messageQueue)
            {
                if (message.Msg == 0x4a)
                {
                    WinApi.MyCopyData lParam = (WinApi.MyCopyData) message.GetLParam(typeof(WinApi.MyCopyData));
                    IntPtr dest = Marshal.AllocHGlobal(lParam.DataSize);
                    CopyMemory(dest, lParam.DataPointer, (uint) lParam.DataSize);
                    lParam.DataPointer = dest;
                    m_messageCopyDataQueue.Enqueue(lParam);
                }
                m_messageQueue.Enqueue(message);
            }
        }

        public static void AddMessageHandler(uint wmCode, ActionRef<Message> messageHandler)
        {
            if (!m_messageDictionary.ContainsKey(wmCode))
            {
                m_messageDictionary.Add(wmCode, messageHandler);
            }
            else
            {
                Dictionary<uint, ActionRef<Message>> messageDictionary = m_messageDictionary;
                uint num = wmCode;
                messageDictionary[num] = (ActionRef<Message>) Delegate.Combine(messageDictionary[num], messageHandler);
            }
        }

        public static void AddMessageHandler(WinApi.WM wmCode, ActionRef<Message> messageHandler)
        {
            AddMessageHandler((uint) wmCode, messageHandler);
        }

        public static void ClearMessageQueue()
        {
            Queue<Message> messageQueue = m_messageQueue;
            lock (messageQueue)
            {
                m_messageQueue.Clear();
            }
        }

        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        public static unsafe void Process()
        {
            Queue<Message> messageQueue = m_messageQueue;
            lock (messageQueue)
            {
                m_tmpMessages.AddRange(m_messageQueue);
                m_tmpCopyData.AddRange(m_messageCopyDataQueue);
                m_messageQueue.Clear();
                m_messageCopyDataQueue.Clear();
            }
            int num = 0;
            for (int i = 0; i < m_tmpMessages.Count; i++)
            {
                Message message = m_tmpMessages[i];
                if (message.Msg != 0x4a)
                {
                    ProcessMessage(ref message);
                }
                else if (num < m_tmpCopyData.Count)
                {
                    num++;
                    WinApi.MyCopyData data = m_tmpCopyData[num];
                    message.LParam = (IntPtr) &data;
                    ProcessMessage(ref message);
                    Marshal.FreeHGlobal(data.DataPointer);
                }
            }
            m_tmpMessages.Clear();
            m_tmpCopyData.Clear();
        }

        private static void ProcessMessage(ref Message message)
        {
            ActionRef<Message> ref2 = null;
            m_messageDictionary.TryGetValue((uint) message.Msg, out ref2);
            if (ref2 != null)
            {
                ref2(ref message);
            }
        }

        public static void RemoveMessageHandler(uint wmCode, ActionRef<Message> messageHandler)
        {
            if (m_messageDictionary.ContainsKey(wmCode))
            {
                Dictionary<uint, ActionRef<Message>> messageDictionary = m_messageDictionary;
                uint num = wmCode;
                messageDictionary[num] = (ActionRef<Message>) Delegate.Remove(messageDictionary[num], messageHandler);
            }
        }

        public static void RemoveMessageHandler(WinApi.WM wmCode, ActionRef<Message> messageHandler)
        {
            RemoveMessageHandler((uint) wmCode, messageHandler);
        }
    }
}

