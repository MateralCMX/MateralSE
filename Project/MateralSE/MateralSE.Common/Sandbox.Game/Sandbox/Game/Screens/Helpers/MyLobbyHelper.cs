namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;
    using VRage.GameServices;

    public class MyLobbyHelper
    {
        private IMyLobby m_lobby;
        private MyLobbyDataUpdated m_dataUpdateHandler;
        [CompilerGenerated]
        private Action<IMyLobby> OnSuccess;

        public event Action<IMyLobby> OnSuccess
        {
            [CompilerGenerated] add
            {
                Action<IMyLobby> onSuccess = this.OnSuccess;
                while (true)
                {
                    Action<IMyLobby> a = onSuccess;
                    Action<IMyLobby> action3 = (Action<IMyLobby>) Delegate.Combine(a, value);
                    onSuccess = Interlocked.CompareExchange<Action<IMyLobby>>(ref this.OnSuccess, action3, a);
                    if (ReferenceEquals(onSuccess, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyLobby> onSuccess = this.OnSuccess;
                while (true)
                {
                    Action<IMyLobby> source = onSuccess;
                    Action<IMyLobby> action3 = (Action<IMyLobby>) Delegate.Remove(source, value);
                    onSuccess = Interlocked.CompareExchange<Action<IMyLobby>>(ref this.OnSuccess, action3, source);
                    if (ReferenceEquals(onSuccess, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyLobbyHelper(IMyLobby lobby)
        {
            this.m_lobby = lobby;
            this.m_dataUpdateHandler = new MyLobbyDataUpdated(this.JoinGame_LobbyUpdate);
        }

        public void Cancel()
        {
            this.m_lobby.OnDataReceived -= this.m_dataUpdateHandler;
        }

        private void JoinGame_LobbyUpdate(bool success, IMyLobby lobby, ulong memberOrLobby)
        {
            if (lobby.LobbyId == this.m_lobby.LobbyId)
            {
                this.m_lobby.OnDataReceived -= this.m_dataUpdateHandler;
                Action<IMyLobby> onSuccess = this.OnSuccess;
                if (onSuccess != null)
                {
                    onSuccess(lobby);
                }
            }
        }

        public bool RequestData()
        {
            this.m_lobby.OnDataReceived += this.m_dataUpdateHandler;
            if (this.m_lobby.RequestData())
            {
                return true;
            }
            this.m_lobby.OnDataReceived -= this.m_dataUpdateHandler;
            return false;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.m_lobby.OnDataReceived -= this.m_dataUpdateHandler;
        }
    }
}

