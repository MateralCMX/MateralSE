namespace Sandbox.Engine.Multiplayer
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.GameServices;

    public class MyMultiplayerJoinResult
    {
        [CompilerGenerated]
        private Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> JoinDone;

        public event Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> JoinDone
        {
            [CompilerGenerated] add
            {
                Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> joinDone = this.JoinDone;
                while (true)
                {
                    Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> a = joinDone;
                    Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> action3 = (Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase>) Delegate.Combine(a, value);
                    joinDone = Interlocked.CompareExchange<Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase>>(ref this.JoinDone, action3, a);
                    if (ReferenceEquals(joinDone, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> joinDone = this.JoinDone;
                while (true)
                {
                    Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> source = joinDone;
                    Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> action3 = (Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase>) Delegate.Remove(source, value);
                    joinDone = Interlocked.CompareExchange<Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase>>(ref this.JoinDone, action3, source);
                    if (ReferenceEquals(joinDone, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Cancel()
        {
            this.Cancelled = true;
        }

        public void RaiseJoined(bool success, IMyLobby lobby, MyLobbyEnterResponseEnum response, MyMultiplayerBase multiplayer)
        {
            Action<bool, IMyLobby, MyLobbyEnterResponseEnum, MyMultiplayerBase> joinDone = this.JoinDone;
            if (joinDone != null)
            {
                joinDone(success, lobby, response, multiplayer);
            }
        }

        public bool Cancelled { get; private set; }
    }
}

