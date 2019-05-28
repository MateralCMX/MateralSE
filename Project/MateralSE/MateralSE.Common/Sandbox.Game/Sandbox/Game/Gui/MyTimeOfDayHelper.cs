namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Network;

    [StaticEventOwner]
    internal static class MyTimeOfDayHelper
    {
        private static float timeOfDay;
        private static TimeSpan? OriginalTime;

        public static void Reset()
        {
            timeOfDay = 0f;
            OriginalTime = null;
        }

        internal static void UpdateTimeOfDay(float time)
        {
            timeOfDay = time;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyTimeOfDayHelper.UpdateTimeOfDayServer), time, targetEndpoint, position);
        }

        [Event(null, 0x35), Reliable, Broadcast]
        private static void UpdateTimeOfDayClient(long ticks, float time)
        {
            timeOfDay = time;
            MySession.Static.ElapsedGameTime = new TimeSpan(ticks);
        }

        [Event(null, 30), Reliable, Server]
        private static void UpdateTimeOfDayServer(float time)
        {
            if (MySession.Static != null)
            {
                if (!MySession.Static.IsUserAdmin(MyEventContext.Current.IsLocallyInvoked ? Sync.MyId : MyEventContext.Current.Sender.Value))
                {
                    (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                }
                else
                {
                    if (OriginalTime == null)
                    {
                        OriginalTime = new TimeSpan?(MySession.Static.ElapsedGameTime);
                    }
                    MySession.Static.ElapsedGameTime = OriginalTime.Value.Add(new TimeSpan(0, 0, (int) (60f * time)));
                    timeOfDay = time;
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, float>(x => new Action<long, float>(MyTimeOfDayHelper.UpdateTimeOfDayClient), MySession.Static.ElapsedGameTime.Ticks, time, targetEndpoint, position);
                }
            }
        }

        internal static float TimeOfDay =>
            timeOfDay;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTimeOfDayHelper.<>c <>9 = new MyTimeOfDayHelper.<>c();
            public static Func<IMyEventOwner, Action<float>> <>9__4_0;
            public static Func<IMyEventOwner, Action<long, float>> <>9__6_0;

            internal Action<float> <UpdateTimeOfDay>b__4_0(IMyEventOwner x) => 
                new Action<float>(MyTimeOfDayHelper.UpdateTimeOfDayServer);

            internal Action<long, float> <UpdateTimeOfDayServer>b__6_0(IMyEventOwner x) => 
                new Action<long, float>(MyTimeOfDayHelper.UpdateTimeOfDayClient);
        }
    }
}

