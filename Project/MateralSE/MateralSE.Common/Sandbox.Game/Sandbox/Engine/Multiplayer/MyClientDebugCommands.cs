namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using VRage.Network;

    [PreloadRequired]
    public class MyClientDebugCommands
    {
        private static readonly char[] m_separators = new char[] { ' ', '\r', '\n' };
        private static readonly Dictionary<string, Action<string[]>> m_commands = new Dictionary<string, Action<string[]>>(StringComparer.InvariantCultureIgnoreCase);
        private static ulong m_commandAuthor;

        static MyClientDebugCommands()
        {
            foreach (MethodInfo info in typeof(MyClientDebugCommands).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                DisplayNameAttribute customAttribute = info.GetCustomAttribute<DisplayNameAttribute>();
                ParameterInfo[] parameters = info.GetParameters();
                if (((customAttribute != null) && ((info.ReturnType == typeof(void)) && (parameters.Length == 1))) && (parameters[0].ParameterType == typeof(string[])))
                {
                    m_commands[customAttribute.DisplayName] = info.CreateDelegate<Action<string[]>>();
                }
            }
        }

        public static bool Process(string msg, ulong author)
        {
            Action<string[]> action;
            m_commandAuthor = author;
            string[] source = msg.Split(m_separators, StringSplitOptions.RemoveEmptyEntries);
            if ((source.Length == 0) || !m_commands.TryGetValue(source[0], out action))
            {
                return false;
            }
            action(source.Skip<string>(1).ToArray<string>());
            return true;
        }

        [DisplayName("+stress")]
        private static void StressTest(string[] args)
        {
            if (args.Length <= 1)
            {
                MyReplicationClient.StressSleep.X = 0;
                MyReplicationClient.StressSleep.Y = 0;
            }
            else if (((args[0] == MySession.Static.LocalHumanPlayer.DisplayName) || (args[0] == "all")) || (args[0] == "clients"))
            {
                if (args.Length > 3)
                {
                    MyReplicationClient.StressSleep.X = Convert.ToInt32(args[1]);
                    MyReplicationClient.StressSleep.Y = Convert.ToInt32(args[2]);
                    MyReplicationClient.StressSleep.Z = Convert.ToInt32(args[3]);
                }
                else if (args.Length > 2)
                {
                    MyReplicationClient.StressSleep.X = Convert.ToInt32(args[1]);
                    MyReplicationClient.StressSleep.Y = Convert.ToInt32(args[2]);
                    MyReplicationClient.StressSleep.Z = 0;
                }
                else
                {
                    MyReplicationClient.StressSleep.Y = Convert.ToInt32(args[1]);
                    MyReplicationClient.StressSleep.X = MyReplicationClient.StressSleep.Y;
                    MyReplicationClient.StressSleep.Z = 0;
                }
            }
        }

        [DisplayName("+vcadd")]
        private static void VirtualClientAdd(string[] args)
        {
            int num = 1;
            if (args.Length == 1)
            {
                num = int.Parse(args[0]);
            }
            for (int i = 0; num > 0; i++)
            {
                MySession.Static.VirtualClients.Add(i);
                num--;
            }
        }
    }
}

