namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Network;

    [StaticEventOwner, PreloadRequired]
    public static class MySyncScenario
    {
        [CompilerGenerated]
        private static Action<bool, bool> InfoAnswer;
        [CompilerGenerated]
        private static Action<long> PrepareScenario;
        [CompilerGenerated]
        private static Action<ulong> PlayerReadyToStartScenario;
        [CompilerGenerated]
        private static Action ClientWorldLoaded;
        [CompilerGenerated]
        private static Action<long> StartScenario;
        [CompilerGenerated]
        private static Action<int> TimeoutReceived;
        [CompilerGenerated]
        private static Action<bool> CanJoinRunningReceived;

        internal static  event Action<bool> CanJoinRunningReceived
        {
            [CompilerGenerated] add
            {
                Action<bool> canJoinRunningReceived = CanJoinRunningReceived;
                while (true)
                {
                    Action<bool> a = canJoinRunningReceived;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    canJoinRunningReceived = Interlocked.CompareExchange<Action<bool>>(ref CanJoinRunningReceived, action3, a);
                    if (ReferenceEquals(canJoinRunningReceived, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> canJoinRunningReceived = CanJoinRunningReceived;
                while (true)
                {
                    Action<bool> source = canJoinRunningReceived;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    canJoinRunningReceived = Interlocked.CompareExchange<Action<bool>>(ref CanJoinRunningReceived, action3, source);
                    if (ReferenceEquals(canJoinRunningReceived, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static  event Action ClientWorldLoaded
        {
            [CompilerGenerated] add
            {
                Action clientWorldLoaded = ClientWorldLoaded;
                while (true)
                {
                    Action a = clientWorldLoaded;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    clientWorldLoaded = Interlocked.CompareExchange<Action>(ref ClientWorldLoaded, action3, a);
                    if (ReferenceEquals(clientWorldLoaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action clientWorldLoaded = ClientWorldLoaded;
                while (true)
                {
                    Action source = clientWorldLoaded;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    clientWorldLoaded = Interlocked.CompareExchange<Action>(ref ClientWorldLoaded, action3, source);
                    if (ReferenceEquals(clientWorldLoaded, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static  event Action<bool, bool> InfoAnswer
        {
            [CompilerGenerated] add
            {
                Action<bool, bool> infoAnswer = InfoAnswer;
                while (true)
                {
                    Action<bool, bool> a = infoAnswer;
                    Action<bool, bool> action3 = (Action<bool, bool>) Delegate.Combine(a, value);
                    infoAnswer = Interlocked.CompareExchange<Action<bool, bool>>(ref InfoAnswer, action3, a);
                    if (ReferenceEquals(infoAnswer, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool, bool> infoAnswer = InfoAnswer;
                while (true)
                {
                    Action<bool, bool> source = infoAnswer;
                    Action<bool, bool> action3 = (Action<bool, bool>) Delegate.Remove(source, value);
                    infoAnswer = Interlocked.CompareExchange<Action<bool, bool>>(ref InfoAnswer, action3, source);
                    if (ReferenceEquals(infoAnswer, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static  event Action<ulong> PlayerReadyToStartScenario
        {
            [CompilerGenerated] add
            {
                Action<ulong> playerReadyToStartScenario = PlayerReadyToStartScenario;
                while (true)
                {
                    Action<ulong> a = playerReadyToStartScenario;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Combine(a, value);
                    playerReadyToStartScenario = Interlocked.CompareExchange<Action<ulong>>(ref PlayerReadyToStartScenario, action3, a);
                    if (ReferenceEquals(playerReadyToStartScenario, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong> playerReadyToStartScenario = PlayerReadyToStartScenario;
                while (true)
                {
                    Action<ulong> source = playerReadyToStartScenario;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Remove(source, value);
                    playerReadyToStartScenario = Interlocked.CompareExchange<Action<ulong>>(ref PlayerReadyToStartScenario, action3, source);
                    if (ReferenceEquals(playerReadyToStartScenario, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static  event Action<long> PrepareScenario
        {
            [CompilerGenerated] add
            {
                Action<long> prepareScenario = PrepareScenario;
                while (true)
                {
                    Action<long> a = prepareScenario;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    prepareScenario = Interlocked.CompareExchange<Action<long>>(ref PrepareScenario, action3, a);
                    if (ReferenceEquals(prepareScenario, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> prepareScenario = PrepareScenario;
                while (true)
                {
                    Action<long> source = prepareScenario;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    prepareScenario = Interlocked.CompareExchange<Action<long>>(ref PrepareScenario, action3, source);
                    if (ReferenceEquals(prepareScenario, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static  event Action<long> StartScenario
        {
            [CompilerGenerated] add
            {
                Action<long> startScenario = StartScenario;
                while (true)
                {
                    Action<long> a = startScenario;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    startScenario = Interlocked.CompareExchange<Action<long>>(ref StartScenario, action3, a);
                    if (ReferenceEquals(startScenario, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> startScenario = StartScenario;
                while (true)
                {
                    Action<long> source = startScenario;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    startScenario = Interlocked.CompareExchange<Action<long>>(ref StartScenario, action3, source);
                    if (ReferenceEquals(startScenario, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static  event Action<int> TimeoutReceived
        {
            [CompilerGenerated] add
            {
                Action<int> timeoutReceived = TimeoutReceived;
                while (true)
                {
                    Action<int> a = timeoutReceived;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    timeoutReceived = Interlocked.CompareExchange<Action<int>>(ref TimeoutReceived, action3, a);
                    if (ReferenceEquals(timeoutReceived, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> timeoutReceived = TimeoutReceived;
                while (true)
                {
                    Action<int> source = timeoutReceived;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    timeoutReceived = Interlocked.CompareExchange<Action<int>>(ref TimeoutReceived, action3, source);
                    if (ReferenceEquals(timeoutReceived, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static void AskInfo()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MySyncScenario.OnAskInfo), targetEndpoint, position);
        }

        private static void MyGuiScreenLoadSandbox_ScenarioWorldLoaded()
        {
            MySessionLoader.ScenarioWorldLoaded -= new Action(MySyncScenario.MyGuiScreenLoadSandbox_ScenarioWorldLoaded);
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ulong>(s => new Action<ulong>(MySyncScenario.OnPlayerReadyToStartScenario), Sync.MyId, targetEndpoint, position);
            if (ClientWorldLoaded != null)
            {
                ClientWorldLoaded();
            }
        }

        [Event(null, 0x37), Reliable, Client]
        private static void OnAnswerInfo(bool isRunning, bool canJoin)
        {
            if (InfoAnswer != null)
            {
                InfoAnswer(isRunning, canJoin);
            }
        }

        [Event(null, 0x23), Reliable, Server]
        private static void OnAskInfo()
        {
            EndpointId sender;
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                sender = new EndpointId(Sync.MyId);
            }
            else
            {
                sender = MyEventContext.Current.Sender;
            }
            bool flag = MyMultiplayer.Static.ScenarioStartTime > DateTime.MinValue;
            bool flag2 = !flag || MySession.Static.Settings.CanJoinRunning;
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<bool, bool>(s => new Action<bool, bool>(MySyncScenario.OnAnswerInfo), flag, flag2, sender, position);
            int selectedIndex = MyGuiScreenScenarioMpBase.Static.TimeoutCombo.GetSelectedIndex();
            position = null;
            MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(MySyncScenario.OnSetTimeoutClient), selectedIndex, sender, position);
            bool canJoinRunning = MySession.Static.Settings.CanJoinRunning;
            position = null;
            MyMultiplayer.RaiseStaticEvent<bool>(s => new Action<bool>(MySyncScenario.OnSetJoinRunningClient), canJoinRunning, sender, position);
        }

        [Event(null, 0x6f), Reliable, Server]
        private static void OnPlayerReadyToStartScenario(ulong playerSteamId)
        {
            if (PlayerReadyToStartScenario != null)
            {
                PlayerReadyToStartScenario(playerSteamId);
            }
        }

        [Event(null, 0x57), Reliable, Broadcast]
        public static void OnPrepareScenarioFromLobby(long PrepStartTime)
        {
            if (PrepareScenario != null)
            {
                PrepareScenario(PrepStartTime);
            }
            MySessionLoader.ScenarioWorldLoaded += new Action(MySyncScenario.MyGuiScreenLoadSandbox_ScenarioWorldLoaded);
        }

        private static void OnSetJoinRunning(bool canJoin)
        {
            if (CanJoinRunningReceived != null)
            {
                CanJoinRunningReceived(canJoin);
            }
        }

        [Event(null, 170), Reliable, Broadcast]
        private static void OnSetJoinRunningBroadcast(bool canJoin)
        {
            OnSetJoinRunning(canJoin);
        }

        [Event(null, 0x44), Reliable, Client]
        private static void OnSetJoinRunningClient(bool canJoin)
        {
            OnSetJoinRunning(canJoin);
        }

        private static void OnSetTimeout(int index)
        {
            if (TimeoutReceived != null)
            {
                TimeoutReceived(index);
            }
        }

        [Event(null, 0x98), Reliable, Broadcast]
        private static void OnSetTimeoutBroadcast(int index)
        {
            OnSetTimeout(index);
        }

        [Event(null, 0x3e), Reliable, Client]
        private static void OnSetTimeoutClient(int index)
        {
            OnSetTimeout(index);
        }

        [Event(null, 0x8b), Reliable, Client]
        private static void OnStartScenario(long gameStartTime)
        {
            if (StartScenario != null)
            {
                StartScenario(gameStartTime);
            }
        }

        internal static void PrepareScenarioFromLobby(long preparationStartTime)
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySyncScenario.OnPrepareScenarioFromLobby), preparationStartTime, targetEndpoint, position);
            }
        }

        public static void SetJoinRunning(bool canJoin)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<bool>(s => new Action<bool>(MySyncScenario.OnSetJoinRunningBroadcast), canJoin, targetEndpoint, position);
        }

        public static void SetTimeout(int index)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(MySyncScenario.OnSetTimeoutBroadcast), index, targetEndpoint, position);
        }

        internal static void StartScenarioRequest(ulong playerSteamId, long gameStartTime)
        {
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySyncScenario.OnStartScenario), gameStartTime, new EndpointId(playerSteamId), position);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySyncScenario.<>c <>9 = new MySyncScenario.<>c();
            public static Func<IMyEventOwner, Action> <>9__21_0;
            public static Func<IMyEventOwner, Action<bool, bool>> <>9__22_0;
            public static Func<IMyEventOwner, Action<int>> <>9__22_1;
            public static Func<IMyEventOwner, Action<bool>> <>9__22_2;
            public static Func<IMyEventOwner, Action<long>> <>9__26_0;
            public static Func<IMyEventOwner, Action<ulong>> <>9__28_0;
            public static Func<IMyEventOwner, Action<long>> <>9__30_0;
            public static Func<IMyEventOwner, Action<int>> <>9__32_0;
            public static Func<IMyEventOwner, Action<bool>> <>9__35_0;

            internal Action <AskInfo>b__21_0(IMyEventOwner s) => 
                new Action(MySyncScenario.OnAskInfo);

            internal Action<ulong> <MyGuiScreenLoadSandbox_ScenarioWorldLoaded>b__28_0(IMyEventOwner s) => 
                new Action<ulong>(MySyncScenario.OnPlayerReadyToStartScenario);

            internal Action<bool, bool> <OnAskInfo>b__22_0(IMyEventOwner s) => 
                new Action<bool, bool>(MySyncScenario.OnAnswerInfo);

            internal Action<int> <OnAskInfo>b__22_1(IMyEventOwner s) => 
                new Action<int>(MySyncScenario.OnSetTimeoutClient);

            internal Action<bool> <OnAskInfo>b__22_2(IMyEventOwner s) => 
                new Action<bool>(MySyncScenario.OnSetJoinRunningClient);

            internal Action<long> <PrepareScenarioFromLobby>b__26_0(IMyEventOwner s) => 
                new Action<long>(MySyncScenario.OnPrepareScenarioFromLobby);

            internal Action<bool> <SetJoinRunning>b__35_0(IMyEventOwner s) => 
                new Action<bool>(MySyncScenario.OnSetJoinRunningBroadcast);

            internal Action<int> <SetTimeout>b__32_0(IMyEventOwner s) => 
                new Action<int>(MySyncScenario.OnSetTimeoutBroadcast);

            internal Action<long> <StartScenarioRequest>b__30_0(IMyEventOwner s) => 
                new Action<long>(MySyncScenario.OnStartScenario);
        }
    }
}

