namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using VRage.Network;
    using VRageMath;

    [PreloadRequired]
    public class MyServerDebugCommands
    {
        private static readonly char[] m_separators = new char[] { ' ', '\r', '\n' };
        private static readonly Dictionary<string, Action<string[]>> m_commands = new Dictionary<string, Action<string[]>>(StringComparer.InvariantCultureIgnoreCase);
        private static ulong m_commandAuthor;

        static MyServerDebugCommands()
        {
            foreach (MethodInfo info in typeof(MyServerDebugCommands).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                DisplayNameAttribute customAttribute = info.GetCustomAttribute<DisplayNameAttribute>();
                ParameterInfo[] parameters = info.GetParameters();
                if (((customAttribute != null) && ((info.ReturnType == typeof(void)) && (parameters.Length == 1))) && (parameters[0].ParameterType == typeof(string[])))
                {
                    m_commands[customAttribute.DisplayName] = info.CreateDelegate<Action<string[]>>();
                }
            }
        }

        [DisplayName("+dump")]
        private static void Dump(string[] args)
        {
            MySession.InitiateDump();
        }

        [DisplayName("+forcereorder")]
        private static void ForceReorder(string[] args)
        {
            MyPhysics.ForceClustersReorder();
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

        [DisplayName("+resetplayers")]
        private static unsafe void ResetPlayers(string[] args)
        {
            Vector3D zero = Vector3D.Zero;
            foreach (MyEntity local1 in MyEntities.GetEntities())
            {
                MatrixD worldMatrix = MatrixD.CreateTranslation(zero);
                local1.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                local1.Physics.LinearVelocity = (Vector3) Vector3D.Forward;
                double* numPtr1 = (double*) ref zero.X;
                numPtr1[0] += 50.0;
            }
        }

        [DisplayName("+save")]
        private static void Save(string[] args)
        {
            MySandboxGame.Log.WriteLineAndConsole("Executing +save command");
            MyAsyncSaving.Start(null, null, false);
        }

        [DisplayName("+stop")]
        private static void Stop(string[] args)
        {
            MySandboxGame.Log.WriteLineAndConsole("Executing +stop command");
            MySandboxGame.ExitThreadSafe();
        }

        [DisplayName("+stress")]
        private static void StressTest(string[] args)
        {
            if (args.Length <= 1)
            {
                MyReplicationServer.StressSleep.X = 0;
                MyReplicationServer.StressSleep.Y = 0;
            }
            else if ((args[0] == "server") || (args[0] == "all"))
            {
                if (args.Length > 3)
                {
                    MyReplicationServer.StressSleep.X = Convert.ToInt32(args[1]);
                    MyReplicationServer.StressSleep.Y = Convert.ToInt32(args[2]);
                    MyReplicationServer.StressSleep.Z = Convert.ToInt32(args[3]);
                }
                else if (args.Length > 2)
                {
                    MyReplicationServer.StressSleep.X = Convert.ToInt32(args[1]);
                    MyReplicationServer.StressSleep.Y = Convert.ToInt32(args[2]);
                    MyReplicationServer.StressSleep.Z = 0;
                }
                else
                {
                    MyReplicationServer.StressSleep.X = Convert.ToInt32(args[1]);
                    MyReplicationServer.StressSleep.Y = MyReplicationServer.StressSleep.X;
                    MyReplicationServer.StressSleep.Z = 0;
                }
            }
        }

        [DisplayName("+unban")]
        private static void Unban(string[] args)
        {
            if (args.Length != 0)
            {
                ulong result = 0UL;
                if (ulong.TryParse(args[0], out result))
                {
                    MyMultiplayer.Static.BanClient(result, false);
                }
            }
        }

        private static MyReplicationServer Replication =>
            ((MyReplicationServer) MyMultiplayer.Static.ReplicationLayer);
    }
}

