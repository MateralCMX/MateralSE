namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Replication.History;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    [MyDebugScreen("VRage", "Network"), StaticEventOwner]
    internal class MyGuiScreenDebugNetwork : MyGuiScreenDebugBase
    {
        private MyGuiControlLabel m_entityLabel;
        private MyEntity m_currentEntity;
        private MyGuiControlSlider m_up;
        private MyGuiControlSlider m_right;
        private MyGuiControlSlider m_forward;
        private MyGuiControlButton m_kickButton;
        private MyGuiControlLabel m_profileLabel;
        private bool m_profileEntityLocked;
        private const float FORCED_PRIORITY = 1f;
        private readonly MyPredictedSnapshotSyncSetup m_kickSetup;

        public MyGuiScreenDebugNetwork() : base(nullable, false)
        {
            MyPredictedSnapshotSyncSetup setup1 = new MyPredictedSnapshotSyncSetup();
            setup1.AllowForceStop = false;
            setup1.ApplyPhysicsAngular = false;
            setup1.ApplyPhysicsLinear = false;
            setup1.ApplyRotation = false;
            setup1.ApplyPosition = true;
            setup1.ExtrapolationSmoothing = true;
            this.m_kickSetup = setup1;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugNetwork";

        [Event(null, 0x6a), Reliable, Server]
        private static void OnSnapshotsMechanicalPivotsChange(bool state)
        {
            MyFakes.SNAPSHOTS_MECHANICAL_PIVOTS = state;
        }

        [Event(null, 0x70), Reliable, Server]
        private static void OnWorldSnapshotsChange(bool state)
        {
            MyFakes.WORLD_SNAPSHOTS = state;
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            Vector4? nullable2;
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 1f;
            Vector2? captionOffset = null;
            base.AddCaption("Network", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            if (MyMultiplayer.Static != null)
            {
                nullable2 = null;
                this.AddSlider("Priority multiplier", 1f, 0f, 16f, delegate (MyGuiControlSlider slider) {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnSetPriorityMultiplier), slider.Value, targetEndpoint, position);
                }, nullable2);
                float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
                singlePtr2[0] += 0.01f;
                nullable2 = null;
                captionOffset = null;
                this.AddCheckBox("Smooth ping", MyMultiplayer.Static.ReplicationLayer.UseSmoothPing, (Action<MyGuiControlCheckbox>) (x => (MyMultiplayer.Static.ReplicationLayer.UseSmoothPing = x.IsChecked)), true, null, nullable2, captionOffset);
                nullable2 = null;
                this.AddSlider("Ping smooth factor", MyMultiplayer.Static.ReplicationLayer.PingSmoothFactor, 0f, 3f, (Action<MyGuiControlSlider>) (slider => (MyMultiplayer.Static.ReplicationLayer.PingSmoothFactor = slider.Value)), nullable2);
                nullable2 = null;
                this.AddSlider("Timestamp correction minimum", (float) MyMultiplayer.Static.ReplicationLayer.TimestampCorrectionMinimum, 0f, 100f, (Action<MyGuiControlSlider>) (slider => (MyMultiplayer.Static.ReplicationLayer.TimestampCorrectionMinimum = (int) slider.Value)), nullable2);
                nullable2 = null;
                captionOffset = null;
                this.AddCheckBox("Smooth timestamp correction", MyMultiplayer.Static.ReplicationLayer.UseSmoothCorrection, (Action<MyGuiControlCheckbox>) (x => (MyMultiplayer.Static.ReplicationLayer.UseSmoothCorrection = x.IsChecked)), true, null, nullable2, captionOffset);
                nullable2 = null;
                this.AddSlider("Smooth timestamp correction amplitude", MyMultiplayer.Static.ReplicationLayer.SmoothCorrectionAmplitude, 0f, 5f, (Action<MyGuiControlSlider>) (slider => (MyMultiplayer.Static.ReplicationLayer.SmoothCorrectionAmplitude = (int) slider.Value)), nullable2);
            }
            nullable2 = null;
            captionOffset = null;
            this.AddCheckBox("Physics World Locking", MyFakes.WORLD_LOCKING_IN_CLIENTUPDATE, (Action<MyGuiControlCheckbox>) (x => (MyFakes.WORLD_LOCKING_IN_CLIENTUPDATE = x.IsChecked)), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Pause physics", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.PAUSE_PHYSICS)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Client physics constraints", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_CLIENT_CONSTRAINTS)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            this.AddCheckBox("New timing", MyReplicationClient.SynchronizationTimingType == MyReplicationClient.TimingType.LastServerTime, (Action<MyGuiControlCheckbox>) (x => (MyReplicationClient.SynchronizationTimingType = x.IsChecked ? MyReplicationClient.TimingType.LastServerTime : MyReplicationClient.TimingType.ServerTimestep)), true, null, nullable2, captionOffset);
            nullable2 = null;
            this.AddSlider("Animation time shift [ms]", (float) MyAnimatedSnapshotSync.TimeShift.Milliseconds, 0f, 1000f, (Action<MyGuiControlSlider>) (slider => (MyAnimatedSnapshotSync.TimeShift = MyTimeSpan.FromMilliseconds((double) slider.Value))), nullable2);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Prediction in jetpack", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_CHARACTER_IN_JETPACK)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Prediction for grids", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_GRID)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Skip prediction", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_SKIP_PREDICTION)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Skip prediction subgrids", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_SKIP_PREDICTION_SUBGRIDS)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Extrapolation smoothing", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_EXTRAPOLATION_SMOOTHING)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Skip animation", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.MULTIPLAYER_SKIP_ANIMATION)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("SnapshotCache Hierarchy Propagation", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.SNAPSHOTCACHE_HIERARCHY)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            this.AddCheckBox("World snapshots", MyFakes.WORLD_SNAPSHOTS, delegate (MyGuiControlCheckbox x) {
                MyFakes.WORLD_SNAPSHOTS = x.IsChecked;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<bool>(y => new Action<bool>(MyGuiScreenDebugNetwork.OnWorldSnapshotsChange), x.IsChecked, targetEndpoint, position);
            }, true, null, nullable2, captionOffset);
            nullable2 = null;
            captionOffset = null;
            this.AddCheckBox("Mechanical Pivots in Snapshots", MyFakes.SNAPSHOTS_MECHANICAL_PIVOTS, delegate (MyGuiControlCheckbox x) {
                MyFakes.SNAPSHOTS_MECHANICAL_PIVOTS = x.IsChecked;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<bool>(y => new Action<bool>(MyGuiScreenDebugNetwork.OnSnapshotsMechanicalPivotsChange), x.IsChecked, targetEndpoint, position);
            }, true, null, nullable2, captionOffset);
        }

        public override bool Update(bool hasFocus)
        {
            bool flag = base.Update(hasFocus);
            if (((this.m_kickButton != null) && (this.m_entityLabel != null)) && (MySession.Static != null))
            {
                MyEntity objA = null;
                if (MySession.Static != null)
                {
                    LineD line = new LineD(MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * 500.0));
                    MyIntersectionResultLineTriangleEx? nullable = MyEntities.GetIntersectionWithLine(ref line, MySession.Static.LocalCharacter, null, false, true, true, IntersectionFlags.ALL_TRIANGLES, 0f, false);
                    if (nullable != null)
                    {
                        objA = nullable.Value.Entity as MyEntity;
                    }
                }
                if (!ReferenceEquals(objA, this.m_currentEntity) && !this.m_profileEntityLocked)
                {
                    this.m_currentEntity = objA;
                    this.m_kickButton.Enabled = this.m_currentEntity != null;
                    this.m_entityLabel.Text = (this.m_currentEntity != null) ? this.m_currentEntity.DisplayName : "";
                    this.m_profileLabel.Text = this.m_entityLabel.Text;
                    MySnapshotCache.DEBUG_ENTITY_ID = (this.m_currentEntity != null) ? this.m_currentEntity.EntityId : 0L;
                    MyFakes.VDB_ENTITY = this.m_currentEntity;
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MyMultiplayerBase.OnSetDebugEntity), (this.m_currentEntity == null) ? 0L : this.m_currentEntity.EntityId, targetEndpoint, position);
                }
            }
            return flag;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugNetwork.<>c <>9 = new MyGuiScreenDebugNetwork.<>c();
            public static Func<IMyEventOwner, Action<float>> <>9__11_20;
            public static Action<MyGuiControlSlider> <>9__11_0;
            public static Action<MyGuiControlCheckbox> <>9__11_1;
            public static Action<MyGuiControlSlider> <>9__11_2;
            public static Action<MyGuiControlSlider> <>9__11_3;
            public static Action<MyGuiControlCheckbox> <>9__11_4;
            public static Action<MyGuiControlSlider> <>9__11_5;
            public static Action<MyGuiControlCheckbox> <>9__11_6;
            public static Action<MyGuiControlCheckbox> <>9__11_9;
            public static Action<MyGuiControlSlider> <>9__11_10;
            public static Func<IMyEventOwner, Action<bool>> <>9__11_21;
            public static Action<MyGuiControlCheckbox> <>9__11_18;
            public static Func<IMyEventOwner, Action<bool>> <>9__11_22;
            public static Action<MyGuiControlCheckbox> <>9__11_19;
            public static Func<IMyEventOwner, Action<long>> <>9__14_0;

            internal void <RecreateControls>b__11_0(MyGuiControlSlider slider)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnSetPriorityMultiplier), slider.Value, targetEndpoint, position);
            }

            internal void <RecreateControls>b__11_1(MyGuiControlCheckbox x)
            {
                MyMultiplayer.Static.ReplicationLayer.UseSmoothPing = x.IsChecked;
            }

            internal void <RecreateControls>b__11_10(MyGuiControlSlider slider)
            {
                MyAnimatedSnapshotSync.TimeShift = MyTimeSpan.FromMilliseconds((double) slider.Value);
            }

            internal void <RecreateControls>b__11_18(MyGuiControlCheckbox x)
            {
                MyFakes.WORLD_SNAPSHOTS = x.IsChecked;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<bool>(y => new Action<bool>(MyGuiScreenDebugNetwork.OnWorldSnapshotsChange), x.IsChecked, targetEndpoint, position);
            }

            internal void <RecreateControls>b__11_19(MyGuiControlCheckbox x)
            {
                MyFakes.SNAPSHOTS_MECHANICAL_PIVOTS = x.IsChecked;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<bool>(y => new Action<bool>(MyGuiScreenDebugNetwork.OnSnapshotsMechanicalPivotsChange), x.IsChecked, targetEndpoint, position);
            }

            internal void <RecreateControls>b__11_2(MyGuiControlSlider slider)
            {
                MyMultiplayer.Static.ReplicationLayer.PingSmoothFactor = slider.Value;
            }

            internal Action<float> <RecreateControls>b__11_20(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnSetPriorityMultiplier);

            internal Action<bool> <RecreateControls>b__11_21(IMyEventOwner y) => 
                new Action<bool>(MyGuiScreenDebugNetwork.OnWorldSnapshotsChange);

            internal Action<bool> <RecreateControls>b__11_22(IMyEventOwner y) => 
                new Action<bool>(MyGuiScreenDebugNetwork.OnSnapshotsMechanicalPivotsChange);

            internal void <RecreateControls>b__11_3(MyGuiControlSlider slider)
            {
                MyMultiplayer.Static.ReplicationLayer.TimestampCorrectionMinimum = (int) slider.Value;
            }

            internal void <RecreateControls>b__11_4(MyGuiControlCheckbox x)
            {
                MyMultiplayer.Static.ReplicationLayer.UseSmoothCorrection = x.IsChecked;
            }

            internal void <RecreateControls>b__11_5(MyGuiControlSlider slider)
            {
                MyMultiplayer.Static.ReplicationLayer.SmoothCorrectionAmplitude = (int) slider.Value;
            }

            internal void <RecreateControls>b__11_6(MyGuiControlCheckbox x)
            {
                MyFakes.WORLD_LOCKING_IN_CLIENTUPDATE = x.IsChecked;
            }

            internal void <RecreateControls>b__11_9(MyGuiControlCheckbox x)
            {
                MyReplicationClient.SynchronizationTimingType = x.IsChecked ? MyReplicationClient.TimingType.LastServerTime : MyReplicationClient.TimingType.ServerTimestep;
            }

            internal Action<long> <Update>b__14_0(IMyEventOwner x) => 
                new Action<long>(MyMultiplayerBase.OnSetDebugEntity);
        }
    }
}

