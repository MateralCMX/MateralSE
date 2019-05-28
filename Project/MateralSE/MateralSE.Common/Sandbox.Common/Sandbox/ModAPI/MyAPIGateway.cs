namespace Sandbox.ModAPI
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;

    public static class MyAPIGateway
    {
        [Obsolete("Use IMyGui.GuiControlCreated")]
        public static Action<object> GuiControlCreated;
        public static IMyPlayerCollection Players;
        public static IMyCubeBuilder CubeBuilder;
        public static IMyTerminalActionsHelper TerminalActionsHelper;
        public static IMyTerminalControls TerminalControls;
        public static IMyUtilities Utilities;
        public static IMyMultiplayer Multiplayer;
        public static IMyParallelTask Parallel;
        public static IMyPhysics Physics;
        public static IMyGui Gui;
        public static IMyPrefabManager PrefabManager;
        public static IMyIngameScripting IngameScripting;
        public static IMyInput Input;
        private static IMyEntities m_entitiesStorage;
        private static IMySession m_sessionStorage;
        public static IMyGridGroups GridGroups;

        [Obsolete]
        public static void Clean()
        {
            Session = null;
            Entities = null;
            Players = null;
            CubeBuilder = null;
            if (IngameScripting != null)
            {
                IngameScripting.Clean();
            }
            IngameScripting = null;
            TerminalActionsHelper = null;
            Utilities = null;
            Parallel = null;
            Physics = null;
            Multiplayer = null;
            PrefabManager = null;
            Input = null;
            TerminalControls = null;
            GridGroups = null;
        }

        [Obsolete]
        public static StringBuilder DoorBase(string name)
        {
            StringBuilder builder = new StringBuilder();
            string str = name;
            int num = 0;
            while (num < str.Length)
            {
                char ch = str[num];
                if (ch == ' ')
                {
                    builder.Append(ch);
                }
                byte num2 = (byte) ch;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= 8)
                    {
                        num++;
                        break;
                    }
                    builder.Append(((num2 & 0x80) != 0) ? "Door" : "Base");
                    num2 = (byte) (num2 << 1);
                    num3++;
                }
            }
            return builder;
        }

        [Conditional("DEBUG"), Obsolete]
        public static void GetMessageBoxPointer(ref IntPtr pointer)
        {
            IntPtr hModule = LoadLibrary("user32.dll");
            pointer = GetProcAddress(hModule, "MessageBoxW");
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procname);
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllname);

        public static IMySession Session
        {
            get => 
                m_sessionStorage;
            set => 
                (m_sessionStorage = value);
        }

        public static IMyEntities Entities
        {
            get => 
                m_entitiesStorage;
            set
            {
                m_entitiesStorage = value;
                if (Entities == null)
                {
                    MyAPIGatewayShortcuts.RegisterEntityUpdate = null;
                    MyAPIGatewayShortcuts.UnregisterEntityUpdate = null;
                }
                else
                {
                    IMyEntities entities = Entities;
                    MyAPIGatewayShortcuts.RegisterEntityUpdate = new Action<IMyEntity>(entities.RegisterForUpdate);
                    IMyEntities entities2 = Entities;
                    MyAPIGatewayShortcuts.UnregisterEntityUpdate = new Action<IMyEntity, bool>(entities2.UnregisterForUpdate);
                }
            }
        }
    }
}

