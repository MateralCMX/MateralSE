namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
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

    [MyCubeBlockType(typeof(MyObjectBuilder_GravityGenerator)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyGravityGenerator), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGenerator) })]
    public class MyGravityGenerator : MyGravityGeneratorBase, SpaceEngineers.Game.ModAPI.IMyGravityGenerator, SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase, IMyGravityProvider, SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGenerator
    {
        private const int NUM_DECIMALS = 0;
        private BoundingBox m_gizmoBoundingBox;
        private readonly VRage.Sync.Sync<Vector3, SyncDirection.BothWays> m_fieldSize;

        public MyGravityGenerator()
        {
            this.CreateTerminalControls();
            this.m_fieldSize.ValueChanged += x => base.UpdateFieldShape();
        }

        protected override float CalculateRequiredPowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return ((0.0003f * Math.Abs((float) base.m_gravityAcceleration)) * ((float) Math.Pow((double) this.m_fieldSize.Value.Volume, 0.35)));
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyGravityGenerator>())
            {
                base.CreateTerminalControls();
                MyTerminalControlSlider<MyGravityGenerator> slider7 = new MyTerminalControlSlider<MyGravityGenerator>("Width", MySpaceTexts.BlockPropertyTitle_GravityFieldWidth, MySpaceTexts.BlockPropertyDescription_GravityFieldWidth);
                MyTerminalControlSlider<MyGravityGenerator> slider8 = new MyTerminalControlSlider<MyGravityGenerator>("Width", MySpaceTexts.BlockPropertyTitle_GravityFieldWidth, MySpaceTexts.BlockPropertyDescription_GravityFieldWidth);
                slider8.SetLimits(x => x.BlockDefinition.MinFieldSize.X, x => x.BlockDefinition.MaxFieldSize.X);
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local3 = (MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate) slider8;
                local3.DefaultValue = new float?((float) 150);
                local3.Getter = x => x.m_fieldSize.Value.X;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local43 = local3;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local44 = local3;
                local44.Setter = delegate (MyGravityGenerator x, float v) {
                    Vector3 fieldSize = (Vector3) x.m_fieldSize;
                    fieldSize.X = v;
                    x.m_fieldSize.Value = fieldSize;
                };
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local41 = local44;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local42 = local44;
                local42.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.X, 0)).Append(" m");
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local7 = local42;
                ((MyTerminalControlSlider<MyGravityGenerator>) local7).EnableActions<MyGravityGenerator>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyGravityGenerator>((MyTerminalControl<MyGravityGenerator>) local7);
                MyTerminalControlSlider<MyGravityGenerator> slider5 = new MyTerminalControlSlider<MyGravityGenerator>("Height", MySpaceTexts.BlockPropertyTitle_GravityFieldHeight, MySpaceTexts.BlockPropertyDescription_GravityFieldHeight);
                MyTerminalControlSlider<MyGravityGenerator> slider6 = new MyTerminalControlSlider<MyGravityGenerator>("Height", MySpaceTexts.BlockPropertyTitle_GravityFieldHeight, MySpaceTexts.BlockPropertyDescription_GravityFieldHeight);
                slider6.SetLimits(x => x.BlockDefinition.MinFieldSize.Y, x => x.BlockDefinition.MaxFieldSize.Y);
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local10 = (MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate) slider6;
                local10.DefaultValue = new float?((float) 150);
                local10.Getter = x => x.m_fieldSize.Value.Y;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local39 = local10;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local40 = local10;
                local40.Setter = delegate (MyGravityGenerator x, float v) {
                    Vector3 fieldSize = (Vector3) x.m_fieldSize;
                    fieldSize.Y = v;
                    x.m_fieldSize.Value = fieldSize;
                };
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local37 = local40;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local38 = local40;
                local38.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.Y, 0)).Append(" m");
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local14 = local38;
                ((MyTerminalControlSlider<MyGravityGenerator>) local14).EnableActions<MyGravityGenerator>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyGravityGenerator>((MyTerminalControl<MyGravityGenerator>) local14);
                MyTerminalControlSlider<MyGravityGenerator> slider3 = new MyTerminalControlSlider<MyGravityGenerator>("Depth", MySpaceTexts.BlockPropertyTitle_GravityFieldDepth, MySpaceTexts.BlockPropertyDescription_GravityFieldDepth);
                MyTerminalControlSlider<MyGravityGenerator> slider4 = new MyTerminalControlSlider<MyGravityGenerator>("Depth", MySpaceTexts.BlockPropertyTitle_GravityFieldDepth, MySpaceTexts.BlockPropertyDescription_GravityFieldDepth);
                slider4.SetLimits(x => x.BlockDefinition.MinFieldSize.Z, x => x.BlockDefinition.MaxFieldSize.Z);
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local17 = (MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate) slider4;
                local17.DefaultValue = new float?((float) 150);
                local17.Getter = x => x.m_fieldSize.Value.Z;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local35 = local17;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local36 = local17;
                local36.Setter = delegate (MyGravityGenerator x, float v) {
                    Vector3 fieldSize = (Vector3) x.m_fieldSize;
                    fieldSize.Z = v;
                    x.m_fieldSize.Value = fieldSize;
                };
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local33 = local36;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local34 = local36;
                local34.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.Z, 0)).Append(" m");
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local21 = local34;
                ((MyTerminalControlSlider<MyGravityGenerator>) local21).EnableActions<MyGravityGenerator>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyGravityGenerator>((MyTerminalControl<MyGravityGenerator>) local21);
                MyTerminalControlSlider<MyGravityGenerator> slider1 = new MyTerminalControlSlider<MyGravityGenerator>("Gravity", MySpaceTexts.BlockPropertyTitle_GravityAcceleration, MySpaceTexts.BlockPropertyDescription_GravityAcceleration);
                MyTerminalControlSlider<MyGravityGenerator> slider2 = new MyTerminalControlSlider<MyGravityGenerator>("Gravity", MySpaceTexts.BlockPropertyTitle_GravityAcceleration, MySpaceTexts.BlockPropertyDescription_GravityAcceleration);
                slider2.SetLimits(x => x.BlockDefinition.MinGravityAcceleration, x => x.BlockDefinition.MaxGravityAcceleration);
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local24 = (MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate) slider2;
                local24.DefaultValue = 9.81f;
                local24.Getter = x => x.GravityAcceleration;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local31 = local24;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local32 = local24;
                local32.Setter = delegate (MyGravityGenerator x, float v) {
                    if (float.IsNaN(v) || float.IsInfinity(v))
                    {
                        v = 0f;
                    }
                    x.GravityAcceleration = v;
                };
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local29 = local32;
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local30 = local32;
                local30.Writer = (x, result) => result.AppendFormat("{0:F1} m/s\x00b2 ({1:F2} g)", x.GravityAcceleration, x.GravityAcceleration / 9.81f);
                MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate local28 = local30;
                ((MyTerminalControlSlider<MyGravityGenerator>) local28).EnableActions<MyGravityGenerator>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyGravityGenerator>((MyTerminalControl<MyGravityGenerator>) local28);
            }
        }

        public override BoundingBox? GetBoundingBox()
        {
            this.m_gizmoBoundingBox.Min = base.PositionComp.LocalVolume.Center - (this.FieldSize / 2f);
            this.m_gizmoBoundingBox.Max = base.PositionComp.LocalVolume.Center + (this.FieldSize / 2f);
            return new BoundingBox?(this.m_gizmoBoundingBox);
        }

        protected override HkShape GetHkShape() => 
            ((HkShape) new HkBoxShape(this.m_fieldSize.Value * 0.5f));

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_GravityGenerator objectBuilderCubeBlock = (MyObjectBuilder_GravityGenerator) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.FieldSize = this.m_fieldSize.Value;
            objectBuilderCubeBlock.GravityAcceleration = (float) base.m_gravityAcceleration;
            return objectBuilderCubeBlock;
        }

        public override void GetProxyAABB(out BoundingBoxD aabb)
        {
            Vector3 halfExtents = this.m_fieldSize.Value * 0.5f;
            aabb = new MyOrientedBoundingBoxD(base.WorldMatrix.Translation, halfExtents, Quaternion.CreateFromRotationMatrix(base.WorldMatrix)).GetAABB();
        }

        public override Vector3 GetWorldGravity(Vector3D worldPoint) => 
            Vector3.TransformNormal(Vector3.Down * base.GravityAcceleration, base.WorldMatrix);

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_GravityGenerator generator = (MyObjectBuilder_GravityGenerator) objectBuilder;
            this.FieldSize = (Vector3) generator.FieldSize;
            this.m_fieldSize.SetLocalValue(this.FieldSize);
            base.m_gravityAcceleration.SetLocalValue(MathHelper.Clamp(generator.GravityAcceleration, this.BlockDefinition.MinGravityAcceleration, this.BlockDefinition.MaxGravityAcceleration));
            if (this.BlockDefinition.EmissiveColorPreset == MyStringHash.NullOrEmpty)
            {
                this.BlockDefinition.EmissiveColorPreset = MyStringHash.GetOrCompute("GravityBlock");
            }
        }

        protected override void InitializeSinkComponent()
        {
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, new Func<float>(this.CalculateRequiredPowerInput));
            base.ResourceSink = component;
            if (base.CubeGrid.CreatePhysics)
            {
                base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
                base.ResourceSink.RequiredInputChanged += new MyRequiredResourceChangeDelegate(this.Receiver_RequiredInputChanged);
            }
        }

        public override bool IsPositionInRange(Vector3D worldPoint)
        {
            Vector3 halfExtents = this.m_fieldSize.Value * 0.5f;
            MyOrientedBoundingBoxD xd = new MyOrientedBoundingBoxD(base.WorldMatrix.Translation, halfExtents, Quaternion.CreateFromRotationMatrix(base.WorldMatrix));
            return xd.Contains(ref worldPoint);
        }

        protected override void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f, base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        private MyGravityGeneratorDefinition BlockDefinition =>
            ((MyGravityGeneratorDefinition) base.BlockDefinition);

        public Vector3 FieldSize
        {
            get => 
                ((Vector3) this.m_fieldSize);
            set
            {
                if (this.m_fieldSize.Value != value)
                {
                    Vector3 vector = value;
                    Vector3* vectorPtr1 = (Vector3*) ref vector;
                    vectorPtr1->X = MathHelper.Clamp(vector.X, this.BlockDefinition.MinFieldSize.X, this.BlockDefinition.MaxFieldSize.X);
                    Vector3* vectorPtr2 = (Vector3*) ref vector;
                    vectorPtr2->Y = MathHelper.Clamp(vector.Y, this.BlockDefinition.MinFieldSize.Y, this.BlockDefinition.MaxFieldSize.Y);
                    Vector3* vectorPtr3 = (Vector3*) ref vector;
                    vectorPtr3->Z = MathHelper.Clamp(vector.Z, this.BlockDefinition.MinFieldSize.Z, this.BlockDefinition.MaxFieldSize.Z);
                    this.m_fieldSize.Value = vector;
                }
            }
        }

        Vector3 SpaceEngineers.Game.ModAPI.IMyGravityGenerator.FieldSize
        {
            get => 
                this.FieldSize;
            set => 
                (this.FieldSize = value);
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGenerator.FieldWidth =>
            this.m_fieldSize.Value.X;

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGenerator.FieldHeight =>
            this.m_fieldSize.Value.Y;

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGenerator.FieldDepth =>
            this.m_fieldSize.Value.Z;

        Vector3 SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGenerator.FieldSize
        {
            get => 
                this.FieldSize;
            set => 
                (this.FieldSize = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGravityGenerator.<>c <>9 = new MyGravityGenerator.<>c();
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_0;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_1;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_2;
            public static MyTerminalValueControl<MyGravityGenerator, float>.SetterDelegate <>9__10_3;
            public static MyTerminalControl<MyGravityGenerator>.WriterDelegate <>9__10_4;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_5;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_6;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_7;
            public static MyTerminalValueControl<MyGravityGenerator, float>.SetterDelegate <>9__10_8;
            public static MyTerminalControl<MyGravityGenerator>.WriterDelegate <>9__10_9;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_10;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_11;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_12;
            public static MyTerminalValueControl<MyGravityGenerator, float>.SetterDelegate <>9__10_13;
            public static MyTerminalControl<MyGravityGenerator>.WriterDelegate <>9__10_14;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_15;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_16;
            public static MyTerminalValueControl<MyGravityGenerator, float>.GetterDelegate <>9__10_17;
            public static MyTerminalValueControl<MyGravityGenerator, float>.SetterDelegate <>9__10_18;
            public static MyTerminalControl<MyGravityGenerator>.WriterDelegate <>9__10_19;

            internal float <CreateTerminalControls>b__10_0(MyGravityGenerator x) => 
                x.BlockDefinition.MinFieldSize.X;

            internal float <CreateTerminalControls>b__10_1(MyGravityGenerator x) => 
                x.BlockDefinition.MaxFieldSize.X;

            internal float <CreateTerminalControls>b__10_10(MyGravityGenerator x) => 
                x.BlockDefinition.MinFieldSize.Z;

            internal float <CreateTerminalControls>b__10_11(MyGravityGenerator x) => 
                x.BlockDefinition.MaxFieldSize.Z;

            internal float <CreateTerminalControls>b__10_12(MyGravityGenerator x) => 
                x.m_fieldSize.Value.Z;

            internal void <CreateTerminalControls>b__10_13(MyGravityGenerator x, float v)
            {
                Vector3 fieldSize = (Vector3) x.m_fieldSize;
                fieldSize.Z = v;
                x.m_fieldSize.Value = fieldSize;
            }

            internal void <CreateTerminalControls>b__10_14(MyGravityGenerator x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.Z, 0)).Append(" m");
            }

            internal float <CreateTerminalControls>b__10_15(MyGravityGenerator x) => 
                x.BlockDefinition.MinGravityAcceleration;

            internal float <CreateTerminalControls>b__10_16(MyGravityGenerator x) => 
                x.BlockDefinition.MaxGravityAcceleration;

            internal float <CreateTerminalControls>b__10_17(MyGravityGenerator x) => 
                x.GravityAcceleration;

            internal void <CreateTerminalControls>b__10_18(MyGravityGenerator x, float v)
            {
                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    v = 0f;
                }
                x.GravityAcceleration = v;
            }

            internal void <CreateTerminalControls>b__10_19(MyGravityGenerator x, StringBuilder result)
            {
                result.AppendFormat("{0:F1} m/s\x00b2 ({1:F2} g)", x.GravityAcceleration, x.GravityAcceleration / 9.81f);
            }

            internal float <CreateTerminalControls>b__10_2(MyGravityGenerator x) => 
                x.m_fieldSize.Value.X;

            internal void <CreateTerminalControls>b__10_3(MyGravityGenerator x, float v)
            {
                Vector3 fieldSize = (Vector3) x.m_fieldSize;
                fieldSize.X = v;
                x.m_fieldSize.Value = fieldSize;
            }

            internal void <CreateTerminalControls>b__10_4(MyGravityGenerator x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.X, 0)).Append(" m");
            }

            internal float <CreateTerminalControls>b__10_5(MyGravityGenerator x) => 
                x.BlockDefinition.MinFieldSize.Y;

            internal float <CreateTerminalControls>b__10_6(MyGravityGenerator x) => 
                x.BlockDefinition.MaxFieldSize.Y;

            internal float <CreateTerminalControls>b__10_7(MyGravityGenerator x) => 
                x.m_fieldSize.Value.Y;

            internal void <CreateTerminalControls>b__10_8(MyGravityGenerator x, float v)
            {
                Vector3 fieldSize = (Vector3) x.m_fieldSize;
                fieldSize.Y = v;
                x.m_fieldSize.Value = fieldSize;
            }

            internal void <CreateTerminalControls>b__10_9(MyGravityGenerator x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.m_fieldSize.Value.Y, 0)).Append(" m");
            }
        }
    }
}

