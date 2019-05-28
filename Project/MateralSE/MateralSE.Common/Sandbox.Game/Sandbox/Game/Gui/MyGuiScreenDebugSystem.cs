namespace Sandbox.Game.Gui
{
    using Havok;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner, MyDebugScreen("VRage", "System")]
    internal class MyGuiScreenDebugSystem : MyGuiScreenDebugBase
    {
        private MyGuiControlMultilineText m_havokStatsMultiline;
        private static StringBuilder m_buffer = new StringBuilder();
        private static string m_statsFromServer = string.Empty;

        public MyGuiScreenDebugSystem() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override bool Draw()
        {
            this.m_havokStatsMultiline.Clear();
            this.m_havokStatsMultiline.AppendText(GetHavokMemoryStats());
            return base.Draw();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugSystem";

        private static string GetHavokMemoryStats()
        {
            if (Sync.IsServer || (MySession.Static == null))
            {
                m_buffer.Append("Out of mem: ").Append(HkBaseSystem.IsOutOfMemory).AppendLine();
                HkBaseSystem.GetMemoryStatistics(m_buffer);
                m_buffer.Clear();
                return m_buffer.ToString();
            }
            if ((MySession.Static.GameplayFrameCounter % 100) == 0)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent(x => new Action(MyGuiScreenDebugSystem.HavokMemoryStatsRequest), targetEndpoint, position);
            }
            return m_statsFromServer;
        }

        [Event(null, 0x74), Reliable, Client]
        private static void HavokMemoryStatsReply(string stats)
        {
            m_statsFromServer = stats;
        }

        [Event(null, 0x68), Reliable, Server]
        private static void HavokMemoryStatsRequest()
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string>(x => new Action<string>(MyGuiScreenDebugSystem.HavokMemoryStatsReply), GetHavokMemoryStats(), MyEventContext.Current.Sender, position);
            }
        }

        private void OnClick_ForceGC(MyGuiControlButton button)
        {
            GC.Collect();
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("System debug", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.AddLabel("System", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Simulate slow update", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.SIMULATE_SLOW_UPDATE)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Force GC"), new Action<MyGuiControlButton>(this.OnClick_ForceGC), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Pause physics", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.PAUSE_PHYSICS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddButton(new StringBuilder("Step physics"), button => MyFakes.STEP_PHYSICS = true, null, color, captionOffset, true, true);
            color = null;
            base.AddSlider("Simulation speed", 0.001f, 3f, null, MemberHelper.GetMember<float>(Expression.Lambda<Func<float>>(Expression.Field(null, fieldof(MyFakes.SIMULATION_SPEED)), Array.Empty<ParameterExpression>())), color);
            color = null;
            this.AddSlider("Statistics Logging Frequency [s]", (float) MyGeneralStats.Static.LogInterval.Seconds, 0f, 120f, (Action<MyGuiControlSlider>) (slider => (MyGeneralStats.Static.LogInterval = MyTimeSpan.FromSeconds((double) slider.Value))), color);
            if ((MySession.Static != null) && (MySession.Static.Settings != null))
            {
                color = null;
                captionOffset = null;
                base.AddCheckBox("Enable save", MySession.Static.Settings, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(Expression.Field(Expression.Property(null, (MethodInfo) methodof(MySession.get_Static)), fieldof(MySession.Settings)), fieldof(MyObjectBuilder_SessionSettings.EnableSaving)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            }
            color = null;
            captionOffset = null;
            base.AddCheckBox("Optimize grid update", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.OPTIMIZE_GRID_UPDATES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddButton(new StringBuilder("Clear achievements and stats"), delegate (MyGuiControlButton button) {
                MyGameService.ResetAllStats(true);
                MyGameService.StoreStats();
            }, null, color, captionOffset, true, true);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            captionOffset = null;
            captionOffset = null;
            this.m_havokStatsMultiline = base.AddMultilineText(captionOffset, captionOffset, 0.8f, false);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugSystem.<>c <>9 = new MyGuiScreenDebugSystem.<>c();
            public static Action<MyGuiControlButton> <>9__3_2;
            public static Action<MyGuiControlSlider> <>9__3_4;
            public static Action<MyGuiControlButton> <>9__3_7;
            public static Func<IMyEventOwner, Action> <>9__6_0;
            public static Func<IMyEventOwner, Action<string>> <>9__7_0;

            internal Action <GetHavokMemoryStats>b__6_0(IMyEventOwner x) => 
                new Action(MyGuiScreenDebugSystem.HavokMemoryStatsRequest);

            internal Action<string> <HavokMemoryStatsRequest>b__7_0(IMyEventOwner x) => 
                new Action<string>(MyGuiScreenDebugSystem.HavokMemoryStatsReply);

            internal void <RecreateControls>b__3_2(MyGuiControlButton button)
            {
                MyFakes.STEP_PHYSICS = true;
            }

            internal void <RecreateControls>b__3_4(MyGuiControlSlider slider)
            {
                MyGeneralStats.Static.LogInterval = MyTimeSpan.FromSeconds((double) slider.Value);
            }

            internal void <RecreateControls>b__3_7(MyGuiControlButton button)
            {
                MyGameService.ResetAllStats(true);
                MyGameService.StoreStats();
            }
        }
    }
}

