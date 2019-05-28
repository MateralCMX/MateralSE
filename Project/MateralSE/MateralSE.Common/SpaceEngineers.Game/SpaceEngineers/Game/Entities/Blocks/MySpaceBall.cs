namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_SpaceBall)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMySpaceBall), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall) })]
    public class MySpaceBall : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMySpaceBall, SpaceEngineers.Game.ModAPI.IMyVirtualMass, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyVirtualMass, SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall
    {
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_friction;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_virtualMass;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_restitution;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_broadcastSync;
        private bool m_savedBroadcast;
        public const float DEFAULT_RESTITUTION = 0.4f;
        public const float DEFAULT_MASS = 100f;
        public const float DEFAULT_FRICTION = 0.5f;
        public const float REAL_MAXIMUM_RESTITUTION = 0.9f;
        public const float REAL_MINIMUM_MASS = 0.01f;

        public MySpaceBall()
        {
            this.CreateTerminalControls();
            base.m_baseIdleSound.Init("BlockArtMass", true);
            this.m_virtualMass.ValueChanged += x => this.RefreshPhysicsBody();
            this.m_broadcastSync.ValueChanged += x => this.BroadcastChanged();
        }

        private void BroadcastChanged()
        {
            this.RadioBroadcaster.Enabled = (bool) this.m_broadcastSync;
            base.RaisePropertiesChanged();
        }

        public override void ContactPointCallback(ref MyGridContactInfo value)
        {
            HkContactPointProperties contactProperties = value.Event.ContactProperties;
            value.EnableDeformation = false;
            value.EnableParticles = false;
            value.RubberDeformation = false;
            if (MyPerGameSettings.BallFriendlyPhysics)
            {
                contactProperties.Friction = this.Friction;
                contactProperties.Restitution = (this.Restitution > 0.9f) ? 0.9f : this.Restitution;
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySpaceBall>())
            {
                base.CreateTerminalControls();
                MyTerminalControlSlider<MySpaceBall> slider5 = new MyTerminalControlSlider<MySpaceBall>("VirtualMass", MySpaceTexts.BlockPropertyDescription_SpaceBallVirtualMass, MySpaceTexts.BlockPropertyDescription_SpaceBallVirtualMass);
                MyTerminalControlSlider<MySpaceBall> slider6 = new MyTerminalControlSlider<MySpaceBall>("VirtualMass", MySpaceTexts.BlockPropertyDescription_SpaceBallVirtualMass, MySpaceTexts.BlockPropertyDescription_SpaceBallVirtualMass);
                slider6.Getter = x => x.VirtualMass;
                MyTerminalControlSlider<MySpaceBall> local39 = slider6;
                MyTerminalControlSlider<MySpaceBall> local40 = slider6;
                local40.Setter = (x, v) => x.VirtualMass = v;
                MyTerminalControlSlider<MySpaceBall> local37 = local40;
                MyTerminalControlSlider<MySpaceBall> local38 = local40;
                local38.DefaultValueGetter = x => 100f;
                MyTerminalControlSlider<MySpaceBall> local35 = local38;
                MyTerminalControlSlider<MySpaceBall> local36 = local38;
                local36.SetLimits(x => 0f, x => x.BlockDefinition.MaxVirtualMass);
                MyTerminalValueControl<MySpaceBall, float>.GetterDelegate local33 = (MyTerminalValueControl<MySpaceBall, float>.GetterDelegate) local36;
                MyTerminalValueControl<MySpaceBall, float>.GetterDelegate local34 = (MyTerminalValueControl<MySpaceBall, float>.GetterDelegate) local36;
                local34.Writer = (x, result) => MyValueFormatter.AppendWeightInBestUnit(x.VirtualMass, result);
                MyTerminalValueControl<MySpaceBall, float>.GetterDelegate local7 = local34;
                ((MyTerminalControlSlider<MySpaceBall>) local7).EnableActions<MySpaceBall>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySpaceBall>((MyTerminalControl<MySpaceBall>) local7);
                if (MyPerGameSettings.BallFriendlyPhysics)
                {
                    MyTerminalControlSlider<MySpaceBall> slider3 = new MyTerminalControlSlider<MySpaceBall>("Friction", MySpaceTexts.BlockPropertyDescription_SpaceBallFriction, MySpaceTexts.BlockPropertyDescription_SpaceBallFriction);
                    MyTerminalControlSlider<MySpaceBall> slider4 = new MyTerminalControlSlider<MySpaceBall>("Friction", MySpaceTexts.BlockPropertyDescription_SpaceBallFriction, MySpaceTexts.BlockPropertyDescription_SpaceBallFriction);
                    slider4.Getter = x => x.Friction;
                    MyTerminalControlSlider<MySpaceBall> local31 = slider4;
                    MyTerminalControlSlider<MySpaceBall> local32 = slider4;
                    local32.Setter = (x, v) => x.Friction = v;
                    MyTerminalControlSlider<MySpaceBall> local29 = local32;
                    MyTerminalControlSlider<MySpaceBall> local30 = local32;
                    local30.DefaultValueGetter = x => 0.5f;
                    MyTerminalControlSlider<MySpaceBall> local11 = local30;
                    local11.SetLimits((float) 0f, (float) 1f);
                    local11.Writer = (x, result) => result.AppendInt32(((int) (x.Friction * 100f))).Append("%");
                    MyTerminalControlSlider<MySpaceBall> slider = local11;
                    slider.EnableActions<MySpaceBall>(0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MySpaceBall>(slider);
                    MyTerminalControlSlider<MySpaceBall> slider1 = new MyTerminalControlSlider<MySpaceBall>("Restitution", MySpaceTexts.BlockPropertyDescription_SpaceBallRestitution, MySpaceTexts.BlockPropertyDescription_SpaceBallRestitution);
                    MyTerminalControlSlider<MySpaceBall> slider2 = new MyTerminalControlSlider<MySpaceBall>("Restitution", MySpaceTexts.BlockPropertyDescription_SpaceBallRestitution, MySpaceTexts.BlockPropertyDescription_SpaceBallRestitution);
                    slider2.Getter = x => x.Restitution;
                    MyTerminalControlSlider<MySpaceBall> local27 = slider2;
                    MyTerminalControlSlider<MySpaceBall> local28 = slider2;
                    local28.Setter = (x, v) => x.Restitution = v;
                    MyTerminalControlSlider<MySpaceBall> local25 = local28;
                    MyTerminalControlSlider<MySpaceBall> local26 = local28;
                    local26.DefaultValueGetter = x => 0.4f;
                    MyTerminalControlSlider<MySpaceBall> local17 = local26;
                    local17.SetLimits((float) 0f, (float) 1f);
                    local17.Writer = (x, result) => result.AppendInt32(((int) (x.Restitution * 100f))).Append("%");
                    MyTerminalControlSlider<MySpaceBall> local19 = local17;
                    local19.EnableActions<MySpaceBall>(0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MySpaceBall>(local19);
                }
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MySpaceBall> checkbox1 = new MyTerminalControlCheckbox<MySpaceBall>("EnableBroadCast", MySpaceTexts.Antenna_EnableBroadcast, MySpaceTexts.Antenna_EnableBroadcast, on, on);
                MyTerminalControlCheckbox<MySpaceBall> checkbox2 = new MyTerminalControlCheckbox<MySpaceBall>("EnableBroadCast", MySpaceTexts.Antenna_EnableBroadcast, MySpaceTexts.Antenna_EnableBroadcast, on, on);
                checkbox2.Getter = x => x.Broadcast;
                MyTerminalControlCheckbox<MySpaceBall> local23 = checkbox2;
                MyTerminalControlCheckbox<MySpaceBall> local24 = checkbox2;
                local24.Setter = (x, v) => x.Broadcast = v;
                MyTerminalControlCheckbox<MySpaceBall> checkbox = local24;
                checkbox.EnableAction<MySpaceBall>(null);
                MyTerminalControlFactory.AddControl<MySpaceBall>(checkbox);
            }
        }

        private void CubeGrid_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            this.UpdatePhysics();
        }

        public override List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            if (base.ShowOnHUD || (base.IsBeingHacked & allowBlink))
            {
                return base.GetHudParams(allowBlink);
            }
            base.m_hudParams.Clear();
            return base.m_hudParams;
        }

        public override float GetMass() => 
            ((this.VirtualMass > 0f) ? this.VirtualMass : 0.01f);

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_SpaceBall objectBuilderCubeBlock = (MyObjectBuilder_SpaceBall) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.VirtualMass = (float) this.m_virtualMass;
            objectBuilderCubeBlock.Restitution = this.Restitution;
            objectBuilderCubeBlock.Friction = this.Friction;
            objectBuilderCubeBlock.EnableBroadcast = this.RadioBroadcaster.Enabled;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            base.NeedsWorldMatrix = true;
            this.RadioReceiver = new MyRadioReceiver();
            this.RadioBroadcaster = new MyRadioBroadcaster(50f);
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_SpaceBall ball = (MyObjectBuilder_SpaceBall) objectBuilder;
            this.m_virtualMass.SetLocalValue(MathHelper.Clamp(ball.VirtualMass, 0f, this.BlockDefinition.MaxVirtualMass));
            this.m_restitution.SetLocalValue(MathHelper.Clamp(ball.Restitution, 0f, 1f));
            this.m_friction.SetLocalValue(MathHelper.Clamp(ball.Friction, 0f, 1f));
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MySpaceBall_IsWorkingChanged);
            base.UpdateIsWorking();
            this.RefreshPhysicsBody();
            this.m_savedBroadcast = ball.EnableBroadcast;
            base.ShowOnHUD = false;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        private void MySpaceBall_IsWorkingChanged(MyCubeBlock obj)
        {
            this.UpdateRadios(base.IsWorking);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.RadioReceiver.UpdateBroadcastersInRange();
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            base.CubeGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
        }

        private void RefreshPhysicsBody()
        {
            if (base.CubeGrid.CreatePhysics)
            {
                int enabled;
                if (base.Physics != null)
                {
                    base.Physics.Close();
                }
                HkSphereShape shape = new HkSphereShape(base.CubeGrid.GridSize * 0.5f);
                HkMassProperties properties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(shape.Radius, (this.VirtualMass != 0f) ? this.VirtualMass : 0.01f);
                base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_KEYFRAMED_REPORTING);
                base.Physics.IsPhantom = false;
                base.Physics.CreateFromCollisionObject((HkShape) shape, Vector3.Zero, base.WorldMatrix, new HkMassProperties?(properties), 0x19);
                base.UpdateIsWorking();
                if (!base.IsWorking || (base.CubeGrid.Physics == null))
                {
                    enabled = 0;
                }
                else
                {
                    enabled = (int) base.CubeGrid.Physics.Enabled;
                }
                base.Physics.Enabled = (bool) enabled;
                base.Physics.RigidBody.Activate();
                shape.Base.RemoveReference();
                if (((base.CubeGrid != null) && (base.CubeGrid.Physics != null)) && !base.CubeGrid.IsStatic)
                {
                    base.CubeGrid.Physics.UpdateMass();
                }
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.RadioReceiver.UpdateBroadcastersInRange();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if ((base.CubeGrid.Physics != null) && !base.CubeGrid.IsStatic)
            {
                base.CubeGrid.Physics.UpdateMass();
            }
            if (base.Physics != null)
            {
                this.UpdatePhysics();
            }
            this.UpdateRadios(this.m_savedBroadcast);
        }

        private void UpdatePhysics()
        {
            if (base.Physics != null)
            {
                int enabled;
                if (!base.IsWorking || (base.CubeGrid.Physics == null))
                {
                    enabled = 0;
                }
                else
                {
                    enabled = (int) base.CubeGrid.Physics.Enabled;
                }
                base.Physics.Enabled = (bool) enabled;
            }
        }

        public void UpdateRadios(bool isTrue)
        {
            if ((this.RadioBroadcaster != null) && (this.RadioReceiver != null))
            {
                this.RadioBroadcaster.WantsToBeEnabled = isTrue;
                this.RadioReceiver.Enabled = isTrue & base.Enabled;
            }
        }

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.MoveBroadcaster();
            }
        }

        public float Friction
        {
            get => 
                ((float) this.m_friction);
            set => 
                (this.m_friction.Value = value);
        }

        public float VirtualMass
        {
            get => 
                ((float) this.m_virtualMass);
            set => 
                (this.m_virtualMass.Value = value);
        }

        public float Restitution
        {
            get => 
                ((float) this.m_restitution);
            set => 
                (this.m_restitution.Value = value);
        }

        public bool Broadcast
        {
            get => 
                ((this.RadioBroadcaster != null) && this.RadioBroadcaster.Enabled);
            set => 
                (this.m_broadcastSync.Value = value);
        }

        private MySpaceBallDefinition BlockDefinition =>
            ((MySpaceBallDefinition) base.BlockDefinition);

        internal MyRadioBroadcaster RadioBroadcaster
        {
            get => 
                ((MyRadioBroadcaster) base.Components.Get<MyDataBroadcaster>());
            private set => 
                base.Components.Add<MyDataBroadcaster>(value);
        }

        internal MyRadioReceiver RadioReceiver
        {
            get => 
                ((MyRadioReceiver) base.Components.Get<MyDataReceiver>());
            set => 
                base.Components.Add<MyDataReceiver>(value);
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall.VirtualMass
        {
            get => 
                this.GetMass();
            set => 
                (this.VirtualMass = MathHelper.Clamp(value, 0.01f, this.BlockDefinition.MaxVirtualMass));
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall.Friction
        {
            get => 
                this.Friction;
            set => 
                (this.Friction = MathHelper.Clamp(value, 0f, 1f));
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall.Restitution
        {
            get => 
                this.Restitution;
            set => 
                (this.Restitution = MathHelper.Clamp(value, 0f, 1f));
        }

        bool SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall.IsBroadcasting =>
            this.Broadcast;

        bool SpaceEngineers.Game.ModAPI.Ingame.IMySpaceBall.Broadcasting
        {
            get => 
                this.Broadcast;
            set => 
                (this.Broadcast = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySpaceBall.<>c <>9 = new MySpaceBall.<>c();
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_0;
            public static MyTerminalValueControl<MySpaceBall, float>.SetterDelegate <>9__31_1;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_2;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_3;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_4;
            public static MyTerminalControl<MySpaceBall>.WriterDelegate <>9__31_5;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_8;
            public static MyTerminalValueControl<MySpaceBall, float>.SetterDelegate <>9__31_9;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_10;
            public static MyTerminalControl<MySpaceBall>.WriterDelegate <>9__31_11;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_12;
            public static MyTerminalValueControl<MySpaceBall, float>.SetterDelegate <>9__31_13;
            public static MyTerminalValueControl<MySpaceBall, float>.GetterDelegate <>9__31_14;
            public static MyTerminalControl<MySpaceBall>.WriterDelegate <>9__31_15;
            public static MyTerminalValueControl<MySpaceBall, bool>.GetterDelegate <>9__31_6;
            public static MyTerminalValueControl<MySpaceBall, bool>.SetterDelegate <>9__31_7;

            internal float <CreateTerminalControls>b__31_0(MySpaceBall x) => 
                x.VirtualMass;

            internal void <CreateTerminalControls>b__31_1(MySpaceBall x, float v)
            {
                x.VirtualMass = v;
            }

            internal float <CreateTerminalControls>b__31_10(MySpaceBall x) => 
                0.5f;

            internal void <CreateTerminalControls>b__31_11(MySpaceBall x, StringBuilder result)
            {
                result.AppendInt32(((int) (x.Friction * 100f))).Append("%");
            }

            internal float <CreateTerminalControls>b__31_12(MySpaceBall x) => 
                x.Restitution;

            internal void <CreateTerminalControls>b__31_13(MySpaceBall x, float v)
            {
                x.Restitution = v;
            }

            internal float <CreateTerminalControls>b__31_14(MySpaceBall x) => 
                0.4f;

            internal void <CreateTerminalControls>b__31_15(MySpaceBall x, StringBuilder result)
            {
                result.AppendInt32(((int) (x.Restitution * 100f))).Append("%");
            }

            internal float <CreateTerminalControls>b__31_2(MySpaceBall x) => 
                100f;

            internal float <CreateTerminalControls>b__31_3(MySpaceBall x) => 
                0f;

            internal float <CreateTerminalControls>b__31_4(MySpaceBall x) => 
                x.BlockDefinition.MaxVirtualMass;

            internal void <CreateTerminalControls>b__31_5(MySpaceBall x, StringBuilder result)
            {
                MyValueFormatter.AppendWeightInBestUnit(x.VirtualMass, result);
            }

            internal bool <CreateTerminalControls>b__31_6(MySpaceBall x) => 
                x.Broadcast;

            internal void <CreateTerminalControls>b__31_7(MySpaceBall x, bool v)
            {
                x.Broadcast = v;
            }

            internal float <CreateTerminalControls>b__31_8(MySpaceBall x) => 
                x.Friction;

            internal void <CreateTerminalControls>b__31_9(MySpaceBall x, float v)
            {
                x.Friction = v;
            }
        }
    }
}

