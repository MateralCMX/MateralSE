namespace Sandbox.Game.World
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.ModAPI;

    public class MyNetworkClient : IMyNetworkClient
    {
        private readonly ulong m_steamUserId;
        public ushort ClientFrameId;
        private int m_controlledPlayerSerialId;
        [CompilerGenerated]
        private Action ClientLeft;

        public event Action ClientLeft
        {
            [CompilerGenerated] add
            {
                Action clientLeft = this.ClientLeft;
                while (true)
                {
                    Action a = clientLeft;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    clientLeft = Interlocked.CompareExchange<Action>(ref this.ClientLeft, action3, a);
                    if (ReferenceEquals(clientLeft, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action clientLeft = this.ClientLeft;
                while (true)
                {
                    Action source = clientLeft;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    clientLeft = Interlocked.CompareExchange<Action>(ref this.ClientLeft, action3, source);
                    if (ReferenceEquals(clientLeft, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyNetworkClient(ulong steamId)
        {
            this.m_steamUserId = steamId;
            this.IsLocal = Sync.MyId == steamId;
            this.DisplayName = MyGameService.IsActive ? MyGameService.GetPersonaName(steamId) : "Client";
        }

        public MyPlayer GetPlayer(int serialId)
        {
            MyPlayer.PlayerId id = new MyPlayer.PlayerId {
                SteamId = this.m_steamUserId,
                SerialId = serialId
            };
            return Sync.Players.GetPlayerById(id);
        }

        public ulong SteamUserId =>
            this.m_steamUserId;

        public bool IsLocal { get; private set; }

        public string DisplayName { get; private set; }

        public int ControlledPlayerSerialId
        {
            private get => 
                this.m_controlledPlayerSerialId;
            set
            {
                if (this.ControlledPlayerSerialId != value)
                {
                    this.FirstPlayer.ReleaseControls();
                    this.m_controlledPlayerSerialId = value;
                    this.FirstPlayer.AcquireControls();
                }
            }
        }

        public MyPlayer FirstPlayer =>
            this.GetPlayer(this.ControlledPlayerSerialId);
    }
}

