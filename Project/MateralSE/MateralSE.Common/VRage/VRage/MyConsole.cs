namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class MyConsole
    {
        private static StringBuilder m_displayScreen = new StringBuilder();
        private static MyCommandHandler m_handler = new MyCommandHandler();
        private static LinkedList<string> m_commandHistory = new LinkedList<string>();
        private static LinkedListNode<string> m_position = null;

        public static void AddCommand(MyCommand command)
        {
            m_handler.AddCommand(command);
        }

        public static void Clear()
        {
            m_displayScreen.Clear();
        }

        public static string GetLine() => 
            ((m_position != null) ? m_position.Value : "");

        public static void NextLine()
        {
            if (m_position != null)
            {
                m_position = m_position.Next;
            }
        }

        public static void ParseCommand(string command)
        {
            if (m_position == null)
            {
                m_commandHistory.AddLast(command);
            }
            else
            {
                m_commandHistory.AddAfter(m_position, command);
                m_position = m_position.Next;
            }
            m_displayScreen.Append(m_handler.Handle(command)).AppendLine();
        }

        public static void PreviousLine()
        {
            if (m_position == null)
            {
                m_position = m_commandHistory.Last;
            }
            else if (!ReferenceEquals(m_position, m_commandHistory.First))
            {
                m_position = m_position.Previous;
            }
        }

        public static bool TryGetCommand(string commandName, out MyCommand command) => 
            m_handler.TryGetCommand(commandName, out command);

        public static StringBuilder DisplayScreen =>
            m_displayScreen;
    }
}

