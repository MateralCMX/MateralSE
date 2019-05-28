namespace Sandbox.Game.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Network;
    using VRageMath;

    public static class Sync
    {
        private static bool? m_steamOnline;
        private static float m_serverCPULoad;
        private static float m_serverCPULoadSmooth;
        private static float m_serverThreadLoad;
        private static float m_serverThreadLoadSmooth;

        public static void ClientConnected(ulong sender)
        {
            if (((Layer != null) && (Layer.Clients != null)) && !Layer.Clients.HasClient(sender))
            {
                Layer.Clients.AddClient(sender);
            }
        }

        public static bool IsGameServer(this MyNetworkClient client) => 
            ((client != null) && (client.SteamUserId == ServerId));

        public static bool MultiplayerActive =>
            (MyMultiplayer.Static != null);

        public static bool IsServer =>
            (!MultiplayerActive || MyMultiplayer.Static.IsServer);

        public static bool IsValidEventOnServer
        {
            get
            {
                if ((MyMultiplayer.Static == null) || !MyMultiplayer.Static.IsServer)
                {
                    return false;
                }
                return MyEventContext.Current.IsValid;
            }
        }

        public static bool IsDedicated =>
            Game.IsDedicated;

        public static ulong ServerId =>
            (MultiplayerActive ? MyMultiplayer.Static.ServerId : MyId);

        public static MySyncLayer Layer =>
            MySession.Static?.SyncLayer;

        public static ulong MyId
        {
            get
            {
                if (MyFakes.ENABLE_RUN_WITHOUT_STEAM && !Game.IsDedicated)
                {
                    if (m_steamOnline == null)
                    {
                        m_steamOnline = new bool?(MyGameService.IsOnline);
                    }
                    if (!m_steamOnline.Value)
                    {
                        return 0x11f71fb0843UL;
                    }
                }
                return MyGameService.UserId;
            }
        }

        public static string MyName =>
            MyGameService.UserName;

        public static float ServerSimulationRatio
        {
            get
            {
                MyMultiplayerBase @static = MyMultiplayer.Static;
                if ((@static == null) || @static.IsServer)
                {
                    return MyPhysics.SimulationRatio;
                }
                return @static.ServerSimulationRatio;
            }
            set
            {
                if (MultiplayerActive && !IsServer)
                {
                    MyMultiplayer.Static.ServerSimulationRatio = value;
                }
            }
        }

        public static float ServerCPULoad
        {
            get
            {
                MyMultiplayerBase @static = MyMultiplayer.Static;
                if ((@static == null) || @static.IsServer)
                {
                    return MySandboxGame.Static.CPULoad;
                }
                return m_serverCPULoad;
            }
            set => 
                (m_serverCPULoad = value);
        }

        public static float ServerCPULoadSmooth
        {
            get
            {
                MyMultiplayerBase @static = MyMultiplayer.Static;
                if ((@static == null) || @static.IsServer)
                {
                    return MySandboxGame.Static.CPULoadSmooth;
                }
                return m_serverCPULoadSmooth;
            }
            set => 
                (m_serverCPULoadSmooth = MathHelper.Smooth(value, m_serverCPULoadSmooth));
        }

        public static float ServerThreadLoad
        {
            get
            {
                MyMultiplayerBase @static = MyMultiplayer.Static;
                if ((@static == null) || @static.IsServer)
                {
                    return MySandboxGame.Static.ThreadLoad;
                }
                return m_serverThreadLoad;
            }
            set => 
                (m_serverThreadLoad = value);
        }

        public static float ServerThreadLoadSmooth
        {
            get
            {
                MyMultiplayerBase @static = MyMultiplayer.Static;
                if ((@static == null) || @static.IsServer)
                {
                    return MySandboxGame.Static.ThreadLoadSmooth;
                }
                return m_serverThreadLoadSmooth;
            }
            set => 
                (m_serverThreadLoadSmooth = MathHelper.Smooth(value, m_serverThreadLoadSmooth));
        }

        public static MyClientCollection Clients =>
            ((Layer == null) ? null : Layer.Clients);

        public static MyPlayerCollection Players =>
            MySession.Static.Players;

        public static bool IsProcessingBufferedMessages =>
            Layer.TransportLayer.IsProcessingBuffer;
    }
}

