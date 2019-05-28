namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Network;

    public class MyDedicatedServer : MyDedicatedServerBase
    {
        private float m_inventoryMultiplier;
        private float m_blocksInventoryMultiplier;
        private float m_assemblerMultiplier;
        private float m_refineryMultiplier;
        private float m_welderMultiplier;
        private float m_grinderMultiplier;
        private List<MyChatMessage> m_globalChatHistory;

        internal MyDedicatedServer(IPEndPoint serverEndpoint) : base(new MySyncLayer(new MyTransportLayer(2)))
        {
            this.m_globalChatHistory = new List<MyChatMessage>();
            base.Initialize(serverEndpoint);
        }

        public override void OnChatMessage(ref ChatMsg msg)
        {
            MyMultiplayerBase.MyConnectedClientData data;
            bool flag = false;
            if (base.MemberDataGet(msg.Author, out data) && (data.IsAdmin | flag))
            {
                MyServerDebugCommands.Process(msg.Text, msg.Author);
            }
            string str = Sync.Players.TryGetIdentityNameFromSteamId(msg.Author);
            if (string.IsNullOrEmpty(str) && (msg.Author == Sync.MyId))
            {
                str = MyTexts.GetString(MySpaceTexts.ChatBotName);
            }
            if (!string.IsNullOrEmpty(str))
            {
                MyChatMessage message1 = new MyChatMessage();
                message1.SteamId = msg.Author;
                message1.AuthorName = string.IsNullOrEmpty(msg.CustomAuthorName) ? str : msg.CustomAuthorName;
                MyChatMessage item = message1;
                item.Text = msg.Text;
                item.Timestamp = DateTime.UtcNow;
                this.m_globalChatHistory.Add(item);
            }
            this.RaiseChatMessageReceived(msg.Author, msg.Text, (ChatChannel) msg.Channel, msg.TargetId, string.IsNullOrEmpty(msg.CustomAuthorName) ? null : msg.CustomAuthorName);
        }

        internal override void SendGameTagsToSteam()
        {
            if (MyGameService.GameServer != null)
            {
                StringBuilder builder = new StringBuilder();
                MyGameModeEnum gameMode = this.GameMode;
                if (gameMode == MyGameModeEnum.Creative)
                {
                    builder.Append("C");
                }
                else if (gameMode == MyGameModeEnum.Survival)
                {
                    builder.Append($"S{(int) this.InventoryMultiplier}-{(int) this.BlocksInventoryMultiplier}-{(int) this.AssemblerMultiplier}-{(int) this.RefineryMultiplier}");
                }
                object[] objArray2 = new object[12];
                objArray2[0] = "groupId";
                objArray2[1] = base.m_groupId;
                objArray2[2] = " version";
                objArray2[3] = MyFinalBuildConstants.APP_VERSION;
                objArray2[4] = " datahash";
                objArray2[5] = MyDataIntegrityChecker.GetHashBase64();
                objArray2[6] = " mods";
                objArray2[7] = this.ModCount;
                objArray2[8] = " gamemode";
                objArray2[9] = builder;
                objArray2[10] = " view";
                objArray2[11] = this.SyncDistance;
                string tags = string.Concat(objArray2);
                MyGameService.GameServer.SetGameTags(tags);
                MyGameService.GameServer.SetGameData(MyFinalBuildConstants.APP_VERSION.ToString());
            }
        }

        protected override void SendServerData()
        {
            ServerDataMsg msg = new ServerDataMsg {
                WorldName = base.m_worldName,
                GameMode = base.m_gameMode,
                InventoryMultiplier = this.m_inventoryMultiplier,
                BlocksInventoryMultiplier = this.m_blocksInventoryMultiplier,
                AssemblerMultiplier = this.m_assemblerMultiplier,
                RefineryMultiplier = this.m_refineryMultiplier,
                WelderMultiplier = this.m_welderMultiplier,
                GrinderMultiplier = this.m_grinderMultiplier,
                HostName = base.m_hostName,
                WorldSize = base.m_worldSize,
                AppVersion = base.m_appVersion,
                MembersLimit = base.m_membersLimit,
                DataHash = base.m_dataHash,
                ServerPasswordSalt = MySandboxGame.ConfigDedicated.ServerPasswordSalt
            };
            base.ReplicationLayer.SendWorldData(ref msg);
        }

        public List<MyChatMessage> GlobalChatHistory =>
            this.m_globalChatHistory;

        public override float InventoryMultiplier
        {
            get => 
                this.m_inventoryMultiplier;
            set => 
                (this.m_inventoryMultiplier = value);
        }

        public override float BlocksInventoryMultiplier
        {
            get => 
                this.m_blocksInventoryMultiplier;
            set => 
                (this.m_blocksInventoryMultiplier = value);
        }

        public override float AssemblerMultiplier
        {
            get => 
                this.m_assemblerMultiplier;
            set => 
                (this.m_assemblerMultiplier = value);
        }

        public override float RefineryMultiplier
        {
            get => 
                this.m_refineryMultiplier;
            set => 
                (this.m_refineryMultiplier = value);
        }

        public override float WelderMultiplier
        {
            get => 
                this.m_welderMultiplier;
            set => 
                (this.m_welderMultiplier = value);
        }

        public override float GrinderMultiplier
        {
            get => 
                this.m_grinderMultiplier;
            set => 
                (this.m_grinderMultiplier = value);
        }

        public override bool Scenario { get; set; }

        public override string ScenarioBriefing { get; set; }

        public override DateTime ScenarioStartTime { get; set; }

        public override bool ExperimentalMode { get; set; }
    }
}

