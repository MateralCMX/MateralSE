namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_Gyro)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyGyro), typeof(Sandbox.ModAPI.Ingame.IMyGyro) })]
    public class MyGyro : MyFunctionalBlock, Sandbox.ModAPI.IMyGyro, Sandbox.ModAPI.Ingame.IMyGyro, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity
    {
        private MyGyroDefinition m_gyroDefinition;
        private int m_oldEmissiveState = -1;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_gyroPower;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_gyroOverride;
        private VRage.Sync.Sync<Vector3, SyncDirection.BothWays> m_gyroOverrideVelocity;
        private float m_gyroMultiplier = 1f;
        private float m_powerConsumptionMultiplier = 1f;

        public MyGyro()
        {
            this.CreateTerminalControls();
            this.m_gyroPower.ValueChanged += x => this.GyroPowerChanged();
            this.m_gyroOverride.ValueChanged += x => this.GyroOverrideChanged();
        }

        protected override bool CheckIsWorking() => 
            (this.IsPowered && base.CheckIsWorking());

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyGyro>())
            {
                base.CreateTerminalControls();
                MyTerminalControlSlider<MyGyro> slider7 = new MyTerminalControlSlider<MyGyro>("Power", MySpaceTexts.BlockPropertyTitle_GyroPower, MySpaceTexts.BlockPropertyDescription_GyroPower);
                MyTerminalControlSlider<MyGyro> slider8 = new MyTerminalControlSlider<MyGyro>("Power", MySpaceTexts.BlockPropertyTitle_GyroPower, MySpaceTexts.BlockPropertyDescription_GyroPower);
                slider8.Getter = x => x.GyroPower;
                MyTerminalControlSlider<MyGyro> local51 = slider8;
                MyTerminalControlSlider<MyGyro> local52 = slider8;
                local52.Setter = (x, v) => x.GyroPower = v;
                MyTerminalControlSlider<MyGyro> local49 = local52;
                MyTerminalControlSlider<MyGyro> local50 = local52;
                local50.Writer = (x, result) => result.AppendInt32(((int) (x.GyroPower * 100f))).Append(" %");
                MyTerminalControlSlider<MyGyro> slider = local50;
                slider.DefaultValue = new float?((float) 1);
                slider.EnableActions<MyGyro>(MyTerminalActionIcons.INCREASE, MyTerminalActionIcons.DECREASE, 0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyGyro>(slider);
                if (MyFakes.ENABLE_GYRO_OVERRIDE)
                {
                    MyStringId? on = null;
                    on = null;
                    MyTerminalControlCheckbox<MyGyro> checkbox1 = new MyTerminalControlCheckbox<MyGyro>("Override", MySpaceTexts.BlockPropertyTitle_GyroOverride, MySpaceTexts.BlockPropertyDescription_GyroOverride, on, on);
                    MyTerminalControlCheckbox<MyGyro> checkbox2 = new MyTerminalControlCheckbox<MyGyro>("Override", MySpaceTexts.BlockPropertyTitle_GyroOverride, MySpaceTexts.BlockPropertyDescription_GyroOverride, on, on);
                    checkbox2.Getter = x => x.GyroOverride;
                    MyTerminalControlCheckbox<MyGyro> local47 = checkbox2;
                    MyTerminalControlCheckbox<MyGyro> local48 = checkbox2;
                    local48.Setter = (x, v) => x.GyroOverride = v;
                    MyTerminalControlCheckbox<MyGyro> checkbox = local48;
                    checkbox.EnableAction<MyGyro>(null);
                    MyTerminalControlFactory.AddControl<MyGyro>(checkbox);
                    MyTerminalControlSlider<MyGyro> slider5 = new MyTerminalControlSlider<MyGyro>("Yaw", MySpaceTexts.BlockPropertyTitle_GyroYawOverride, MySpaceTexts.BlockPropertyDescription_GyroYawOverride);
                    MyTerminalControlSlider<MyGyro> slider6 = new MyTerminalControlSlider<MyGyro>("Yaw", MySpaceTexts.BlockPropertyTitle_GyroYawOverride, MySpaceTexts.BlockPropertyDescription_GyroYawOverride);
                    slider6.Getter = x => -x.m_gyroOverrideVelocity.Value.Y * 9.549296f;
                    MyTerminalControlSlider<MyGyro> local45 = slider6;
                    MyTerminalControlSlider<MyGyro> local46 = slider6;
                    local46.Setter = (x, v) => SetGyroTorqueYaw(x, -v * 0.1047198f);
                    MyTerminalControlSlider<MyGyro> local43 = local46;
                    MyTerminalControlSlider<MyGyro> local44 = local46;
                    local44.Writer = (x, result) => result.AppendDecimal(((float) (-x.m_gyroOverrideVelocity.Value.Y * 9.549296f)), 2).Append(" RPM");
                    MyTerminalControlSlider<MyGyro> local41 = local44;
                    MyTerminalControlSlider<MyGyro> local42 = local44;
                    local42.Enabled = x => x.GyroOverride;
                    MyTerminalControlSlider<MyGyro> local12 = local42;
                    local12.DefaultValue = 0f;
                    local12.SetDualLogLimits(x => 0.01f, new MyTerminalValueControl<MyGyro, float>.GetterDelegate(MyGyro.MaxAngularRPM), 0.05f);
                    MyTerminalControlSlider<MyGyro> local14 = local12;
                    local14.EnableActions<MyGyro>(MyTerminalActionIcons.INCREASE, MyTerminalActionIcons.DECREASE, 0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MyGyro>(local14);
                    MyTerminalControlSlider<MyGyro> slider3 = new MyTerminalControlSlider<MyGyro>("Pitch", MySpaceTexts.BlockPropertyTitle_GyroPitchOverride, MySpaceTexts.BlockPropertyDescription_GyroPitchOverride);
                    MyTerminalControlSlider<MyGyro> slider4 = new MyTerminalControlSlider<MyGyro>("Pitch", MySpaceTexts.BlockPropertyTitle_GyroPitchOverride, MySpaceTexts.BlockPropertyDescription_GyroPitchOverride);
                    slider4.Getter = x => x.m_gyroOverrideVelocity.Value.X * 9.549296f;
                    MyTerminalControlSlider<MyGyro> local39 = slider4;
                    MyTerminalControlSlider<MyGyro> local40 = slider4;
                    local40.Setter = (x, v) => SetGyroTorquePitch(x, v * 0.1047198f);
                    MyTerminalControlSlider<MyGyro> local37 = local40;
                    MyTerminalControlSlider<MyGyro> local38 = local40;
                    local38.Writer = (x, result) => result.AppendDecimal(((float) (x.m_gyroOverrideVelocity.Value.X * 9.549296f)), 2).Append(" RPM");
                    MyTerminalControlSlider<MyGyro> local35 = local38;
                    MyTerminalControlSlider<MyGyro> local36 = local38;
                    local36.Enabled = x => x.GyroOverride;
                    MyTerminalControlSlider<MyGyro> local19 = local36;
                    local19.DefaultValue = 0f;
                    local19.SetDualLogLimits(x => 0.01f, new MyTerminalValueControl<MyGyro, float>.GetterDelegate(MyGyro.MaxAngularRPM), 0.05f);
                    MyTerminalControlSlider<MyGyro> local21 = local19;
                    local21.EnableActions<MyGyro>(MyTerminalActionIcons.INCREASE, MyTerminalActionIcons.DECREASE, 0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MyGyro>(local21);
                    MyTerminalControlSlider<MyGyro> slider1 = new MyTerminalControlSlider<MyGyro>("Roll", MySpaceTexts.BlockPropertyTitle_GyroRollOverride, MySpaceTexts.BlockPropertyDescription_GyroRollOverride);
                    MyTerminalControlSlider<MyGyro> slider2 = new MyTerminalControlSlider<MyGyro>("Roll", MySpaceTexts.BlockPropertyTitle_GyroRollOverride, MySpaceTexts.BlockPropertyDescription_GyroRollOverride);
                    slider2.Getter = x => -x.m_gyroOverrideVelocity.Value.Z * 9.549296f;
                    MyTerminalControlSlider<MyGyro> local33 = slider2;
                    MyTerminalControlSlider<MyGyro> local34 = slider2;
                    local34.Setter = (x, v) => SetGyroTorqueRoll(x, -v * 0.1047198f);
                    MyTerminalControlSlider<MyGyro> local31 = local34;
                    MyTerminalControlSlider<MyGyro> local32 = local34;
                    local32.Writer = (x, result) => result.AppendDecimal(((float) (-x.m_gyroOverrideVelocity.Value.Z * 9.549296f)), 2).Append(" RPM");
                    MyTerminalControlSlider<MyGyro> local29 = local32;
                    MyTerminalControlSlider<MyGyro> local30 = local32;
                    local30.Enabled = x => x.GyroOverride;
                    MyTerminalControlSlider<MyGyro> local26 = local30;
                    local26.DefaultValue = 0f;
                    local26.SetDualLogLimits(x => 0.01f, new MyTerminalValueControl<MyGyro, float>.GetterDelegate(MyGyro.MaxAngularRPM), 0.05f);
                    MyTerminalControlSlider<MyGyro> local28 = local26;
                    local28.EnableActions<MyGyro>(MyTerminalActionIcons.INCREASE, MyTerminalActionIcons.DECREASE, 0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MyGyro>(local28);
                }
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Gyro objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_Gyro;
            objectBuilderCubeBlock.GyroPower = (float) this.m_gyroPower;
            objectBuilderCubeBlock.GyroOverride = this.GyroOverride;
            objectBuilderCubeBlock.TargetAngularVelocity = this.m_gyroOverrideVelocity.Value;
            return objectBuilderCubeBlock;
        }

        private void GyroOverrideChanged()
        {
            this.SetGyroOverride(this.m_gyroOverride.Value);
        }

        private void GyroPowerChanged()
        {
            this.SetEmissiveStateWorking();
            this.UpdateText();
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            this.m_gyroDefinition = (MyGyroDefinition) base.BlockDefinition;
            MyObjectBuilder_Gyro gyro = objectBuilder as MyObjectBuilder_Gyro;
            this.m_gyroPower.SetLocalValue(MathHelper.Clamp(gyro.GyroPower, 0f, 1f));
            if (MyFakes.ENABLE_GYRO_OVERRIDE)
            {
                this.GyroOverride = gyro.GyroOverride;
                float max = MaxAngularRadiansPerSecond(this);
                SerializableVector3 vector = new SerializableVector3(MathHelper.Clamp(gyro.TargetAngularVelocity.x, -max, max), MathHelper.Clamp(gyro.TargetAngularVelocity.y, -max, max), MathHelper.Clamp(gyro.TargetAngularVelocity.z, -max, max));
                this.m_gyroOverrideVelocity.Value = (Vector3) vector;
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.UpdateText();
        }

        private static float MaxAngularRadiansPerSecond(MyGyro gyro) => 
            ((gyro.m_gyroDefinition.CubeSize != MyCubeSize.Small) ? MyGridPhysics.GetLargeShipMaxAngularVelocity() : MyGridPhysics.GetSmallShipMaxAngularVelocity());

        private static float MaxAngularRPM(MyGyro gyro) => 
            ((gyro.m_gyroDefinition.CubeSize != MyCubeSize.Small) ? (MyGridPhysics.GetLargeShipMaxAngularVelocity() * 9.549296f) : (MyGridPhysics.GetSmallShipMaxAngularVelocity() * 9.549296f));

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
            this.m_oldEmissiveState = -1;
        }

        public override void OnModelChange()
        {
            this.m_oldEmissiveState = -1;
            base.OnModelChange();
        }

        public override bool SetEmissiveStateWorking() => 
            (!this.GyroOverride ? ((this.GyroPower > 1E-05f) ? base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null)) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Alternative, base.Render.RenderObjectIDs[0], null));

        public void SetGyroOverride(bool value)
        {
            base.CubeGrid.GridSystems.GyroSystem.MarkDirty();
            if (this.CheckIsWorking())
            {
                this.SetEmissiveStateWorking();
            }
        }

        public void SetGyroTorque(Vector3 torque)
        {
            this.m_gyroOverrideVelocity.Value = torque;
            base.CubeGrid.GridSystems.GyroSystem.MarkDirty();
        }

        private static void SetGyroTorquePitch(MyGyro gyro, float pitchValue)
        {
            Vector3 torque = gyro.m_gyroOverrideVelocity.Value;
            torque.X = pitchValue;
            gyro.SetGyroTorque(torque);
        }

        private static void SetGyroTorqueRoll(MyGyro gyro, float rollValue)
        {
            Vector3 torque = gyro.m_gyroOverrideVelocity.Value;
            torque.Z = rollValue;
            gyro.SetGyroTorque(torque);
        }

        private static void SetGyroTorqueYaw(MyGyro gyro, float yawValue)
        {
            Vector3 torque = gyro.m_gyroOverrideVelocity.Value;
            torque.Y = yawValue;
            gyro.SetGyroTorque(torque);
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(base.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(this.RequiredPowerInput, base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        public bool IsPowered =>
            base.CubeGrid.GridSystems.GyroSystem.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId);

        public float MaxGyroForce =>
            ((this.m_gyroDefinition.ForceMagnitude * this.m_gyroPower) * this.m_gyroMultiplier);

        public float RequiredPowerInput =>
            ((this.m_gyroDefinition.RequiredPowerInput * this.m_gyroPower) * this.m_powerConsumptionMultiplier);

        public float GyroPower
        {
            get => 
                ((float) this.m_gyroPower);
            set
            {
                float single1 = MathHelper.Clamp(value, 0f, 1f);
                value = single1;
                if (value != this.m_gyroPower)
                {
                    this.m_gyroPower.Value = value;
                }
            }
        }

        public bool GyroOverride
        {
            get => 
                ((bool) this.m_gyroOverride);
            set => 
                (this.m_gyroOverride.Value = value);
        }

        public Vector3 GyroOverrideVelocityGrid =>
            Vector3.TransformNormal((Vector3) this.m_gyroOverrideVelocity, base.Orientation);

        float Sandbox.ModAPI.Ingame.IMyGyro.Yaw
        {
            get => 
                -this.m_gyroOverrideVelocity.Value.Y;
            set
            {
                if (this.GyroOverride)
                {
                    float max = MaxAngularRadiansPerSecond(this);
                    float single1 = MathHelper.Clamp(value, -max, max);
                    value = single1;
                    SetGyroTorqueYaw(this, -value);
                }
            }
        }

        float Sandbox.ModAPI.Ingame.IMyGyro.Pitch
        {
            get => 
                -this.m_gyroOverrideVelocity.Value.X;
            set
            {
                if (this.GyroOverride)
                {
                    float max = MaxAngularRadiansPerSecond(this);
                    float single1 = MathHelper.Clamp(value, -max, max);
                    value = single1;
                    SetGyroTorquePitch(this, -value);
                }
            }
        }

        float Sandbox.ModAPI.Ingame.IMyGyro.Roll
        {
            get => 
                -this.m_gyroOverrideVelocity.Value.Z;
            set
            {
                if (this.GyroOverride)
                {
                    float max = MaxAngularRadiansPerSecond(this);
                    float single1 = MathHelper.Clamp(value, -max, max);
                    value = single1;
                    SetGyroTorqueRoll(this, -value);
                }
            }
        }

        float Sandbox.ModAPI.IMyGyro.GyroStrengthMultiplier
        {
            get => 
                this.m_gyroMultiplier;
            set
            {
                this.m_gyroMultiplier = value;
                if (this.m_gyroMultiplier < 0.01f)
                {
                    this.m_gyroMultiplier = 0.01f;
                }
                if (base.CubeGrid.GridSystems.GyroSystem != null)
                {
                    base.CubeGrid.GridSystems.GyroSystem.MarkDirty();
                }
            }
        }

        float Sandbox.ModAPI.IMyGyro.PowerConsumptionMultiplier
        {
            get => 
                this.m_powerConsumptionMultiplier;
            set
            {
                this.m_powerConsumptionMultiplier = value;
                if (this.m_powerConsumptionMultiplier < 0.01f)
                {
                    this.m_powerConsumptionMultiplier = 0.01f;
                }
                if (base.CubeGrid.GridSystems.GyroSystem != null)
                {
                    base.CubeGrid.GridSystems.GyroSystem.MarkDirty();
                }
                this.UpdateText();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGyro.<>c <>9 = new MyGyro.<>c();
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_0;
            public static MyTerminalValueControl<MyGyro, float>.SetterDelegate <>9__23_1;
            public static MyTerminalControl<MyGyro>.WriterDelegate <>9__23_2;
            public static MyTerminalValueControl<MyGyro, bool>.GetterDelegate <>9__23_3;
            public static MyTerminalValueControl<MyGyro, bool>.SetterDelegate <>9__23_4;
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_5;
            public static MyTerminalValueControl<MyGyro, float>.SetterDelegate <>9__23_6;
            public static MyTerminalControl<MyGyro>.WriterDelegate <>9__23_7;
            public static Func<MyGyro, bool> <>9__23_8;
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_9;
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_10;
            public static MyTerminalValueControl<MyGyro, float>.SetterDelegate <>9__23_11;
            public static MyTerminalControl<MyGyro>.WriterDelegate <>9__23_12;
            public static Func<MyGyro, bool> <>9__23_13;
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_14;
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_15;
            public static MyTerminalValueControl<MyGyro, float>.SetterDelegate <>9__23_16;
            public static MyTerminalControl<MyGyro>.WriterDelegate <>9__23_17;
            public static Func<MyGyro, bool> <>9__23_18;
            public static MyTerminalValueControl<MyGyro, float>.GetterDelegate <>9__23_19;

            internal float <CreateTerminalControls>b__23_0(MyGyro x) => 
                x.GyroPower;

            internal void <CreateTerminalControls>b__23_1(MyGyro x, float v)
            {
                x.GyroPower = v;
            }

            internal float <CreateTerminalControls>b__23_10(MyGyro x) => 
                (x.m_gyroOverrideVelocity.Value.X * 9.549296f);

            internal void <CreateTerminalControls>b__23_11(MyGyro x, float v)
            {
                MyGyro.SetGyroTorquePitch(x, v * 0.1047198f);
            }

            internal void <CreateTerminalControls>b__23_12(MyGyro x, StringBuilder result)
            {
                result.AppendDecimal(((float) (x.m_gyroOverrideVelocity.Value.X * 9.549296f)), 2).Append(" RPM");
            }

            internal bool <CreateTerminalControls>b__23_13(MyGyro x) => 
                x.GyroOverride;

            internal float <CreateTerminalControls>b__23_14(MyGyro x) => 
                0.01f;

            internal float <CreateTerminalControls>b__23_15(MyGyro x) => 
                (-x.m_gyroOverrideVelocity.Value.Z * 9.549296f);

            internal void <CreateTerminalControls>b__23_16(MyGyro x, float v)
            {
                MyGyro.SetGyroTorqueRoll(x, -v * 0.1047198f);
            }

            internal void <CreateTerminalControls>b__23_17(MyGyro x, StringBuilder result)
            {
                result.AppendDecimal(((float) (-x.m_gyroOverrideVelocity.Value.Z * 9.549296f)), 2).Append(" RPM");
            }

            internal bool <CreateTerminalControls>b__23_18(MyGyro x) => 
                x.GyroOverride;

            internal float <CreateTerminalControls>b__23_19(MyGyro x) => 
                0.01f;

            internal void <CreateTerminalControls>b__23_2(MyGyro x, StringBuilder result)
            {
                result.AppendInt32(((int) (x.GyroPower * 100f))).Append(" %");
            }

            internal bool <CreateTerminalControls>b__23_3(MyGyro x) => 
                x.GyroOverride;

            internal void <CreateTerminalControls>b__23_4(MyGyro x, bool v)
            {
                x.GyroOverride = v;
            }

            internal float <CreateTerminalControls>b__23_5(MyGyro x) => 
                (-x.m_gyroOverrideVelocity.Value.Y * 9.549296f);

            internal void <CreateTerminalControls>b__23_6(MyGyro x, float v)
            {
                MyGyro.SetGyroTorqueYaw(x, -v * 0.1047198f);
            }

            internal void <CreateTerminalControls>b__23_7(MyGyro x, StringBuilder result)
            {
                result.AppendDecimal(((float) (-x.m_gyroOverrideVelocity.Value.Y * 9.549296f)), 2).Append(" RPM");
            }

            internal bool <CreateTerminalControls>b__23_8(MyGyro x) => 
                x.GyroOverride;

            internal float <CreateTerminalControls>b__23_9(MyGyro x) => 
                0.01f;
        }
    }
}

