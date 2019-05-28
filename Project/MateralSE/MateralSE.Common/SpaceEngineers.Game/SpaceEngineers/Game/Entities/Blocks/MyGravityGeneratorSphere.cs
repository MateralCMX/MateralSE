namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.EntityComponents.DebugRenders;
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

    [MyCubeBlockType(typeof(MyObjectBuilder_GravityGeneratorSphere)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyGravityGeneratorSphere), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorSphere) })]
    public class MyGravityGeneratorSphere : MyGravityGeneratorBase, SpaceEngineers.Game.ModAPI.IMyGravityGeneratorSphere, SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase, IMyGravityProvider, SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorSphere
    {
        private const float DEFAULT_RADIUS = 100f;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_radius;
        private float m_defaultVolume;

        public MyGravityGeneratorSphere()
        {
            this.CreateTerminalControls();
            this.m_radius.ValueChanged += x => base.UpdateFieldShape();
        }

        protected override float CalculateRequiredPowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return this.CalculateRequiredPowerInputForRadius((float) this.m_radius);
        }

        private float CalculateRequiredPowerInputForRadius(float radius) => 
            (((((float) ((Math.Pow((double) radius, (double) this.BlockDefinition.ConsumptionPower) * 3.1415926535897931) * 0.75)) / this.m_defaultVolume) * this.BlockDefinition.BasePowerInput) * (Math.Abs((float) base.m_gravityAcceleration) / 9.81f));

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyGravityGeneratorSphere>())
            {
                base.CreateTerminalControls();
                if (MyFakes.ENABLE_GRAVITY_GENERATOR_SPHERE)
                {
                    MyTerminalControlSlider<MyGravityGeneratorSphere> slider1 = new MyTerminalControlSlider<MyGravityGeneratorSphere>("Radius", MySpaceTexts.BlockPropertyTitle_GravityFieldRadius, MySpaceTexts.BlockPropertyDescription_GravityFieldRadius);
                    slider1.DefaultValue = 100f;
                    slider1.Getter = x => x.Radius;
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local24 = slider1;
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local25 = slider1;
                    local25.Setter = delegate (MyGravityGeneratorSphere x, float v) {
                        if (v < x.BlockDefinition.MinRadius)
                        {
                            v = x.BlockDefinition.MinRadius;
                        }
                        x.Radius = v;
                    };
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local22 = local25;
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local23 = local25;
                    local23.Normalizer = (x, v) => (v != 0f) ? ((MyTerminalControlSlider<MyGravityGeneratorSphere>.FloatFunc) ((v - x.BlockDefinition.MinRadius) / (x.BlockDefinition.MaxRadius - x.BlockDefinition.MinRadius))) : ((MyTerminalControlSlider<MyGravityGeneratorSphere>.FloatFunc) 0f);
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local20 = local23;
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local21 = local23;
                    local21.Denormalizer = (x, v) => (v != 0f) ? ((MyTerminalControlSlider<MyGravityGeneratorSphere>.FloatFunc) ((v * (x.BlockDefinition.MaxRadius - x.BlockDefinition.MinRadius)) + x.BlockDefinition.MinRadius)) : ((MyTerminalControlSlider<MyGravityGeneratorSphere>.FloatFunc) 0f);
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local18 = local21;
                    MyTerminalControlSlider<MyGravityGeneratorSphere> local19 = local21;
                    local19.Writer = (x, result) => result.AppendInt32(((int) ((float) x.m_radius))).Append(" m");
                    MyTerminalControlSlider<MyGravityGeneratorSphere> slider = local19;
                    slider.EnableActions<MyGravityGeneratorSphere>(0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MyGravityGeneratorSphere>(slider);
                    MyTerminalControlSlider<MyGravityGeneratorSphere> slider2 = new MyTerminalControlSlider<MyGravityGeneratorSphere>("Gravity", MySpaceTexts.BlockPropertyTitle_GravityAcceleration, MySpaceTexts.BlockPropertyDescription_GravityAcceleration);
                    MyTerminalControlSlider<MyGravityGeneratorSphere> slider3 = new MyTerminalControlSlider<MyGravityGeneratorSphere>("Gravity", MySpaceTexts.BlockPropertyTitle_GravityAcceleration, MySpaceTexts.BlockPropertyDescription_GravityAcceleration);
                    slider3.SetLimits(x => x.BlockDefinition.MinGravityAcceleration, x => x.BlockDefinition.MaxGravityAcceleration);
                    MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate local9 = (MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate) slider3;
                    local9.DefaultValue = 9.81f;
                    local9.Getter = x => x.GravityAcceleration;
                    MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate local16 = local9;
                    MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate local17 = local9;
                    local17.Setter = delegate (MyGravityGeneratorSphere x, float v) {
                        if (float.IsNaN(v) || float.IsInfinity(v))
                        {
                            v = 0f;
                        }
                        x.GravityAcceleration = v;
                    };
                    MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate local14 = local17;
                    MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate local15 = local17;
                    local15.Writer = (x, result) => result.AppendFormat("{0:F1} m/s\x00b2 ({1:F2} g)", x.GravityAcceleration, x.GravityAcceleration / 9.81f);
                    MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate local13 = local15;
                    ((MyTerminalControlSlider<MyGravityGeneratorSphere>) local13).EnableActions<MyGravityGeneratorSphere>(0.05f, null, null);
                    MyTerminalControlFactory.AddControl<MyGravityGeneratorSphere>((MyTerminalControl<MyGravityGeneratorSphere>) local13);
                }
            }
        }

        protected override HkShape GetHkShape() => 
            ((HkShape) new HkSphereShape((float) this.m_radius));

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_GravityGeneratorSphere objectBuilderCubeBlock = (MyObjectBuilder_GravityGeneratorSphere) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Radius = (float) this.m_radius;
            objectBuilderCubeBlock.GravityAcceleration = (float) base.m_gravityAcceleration;
            return objectBuilderCubeBlock;
        }

        public override void GetProxyAABB(out BoundingBoxD aabb)
        {
            BoundingSphereD sphere = new BoundingSphereD(base.PositionComp.GetPosition(), (double) this.m_radius);
            BoundingBoxD.CreateFromSphere(ref sphere, out aabb);
        }

        public override float GetRadius() => 
            ((float) this.m_radius);

        public override Vector3 GetWorldGravity(Vector3D worldPoint)
        {
            Vector3D vectord = base.WorldMatrix.Translation - worldPoint;
            vectord.Normalize();
            return (Vector3) (vectord * base.GravityAcceleration);
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_GravityGeneratorSphere sphere = (MyObjectBuilder_GravityGeneratorSphere) objectBuilder;
            this.m_radius.SetLocalValue(MathHelper.Clamp(sphere.Radius, this.BlockDefinition.MinRadius, this.BlockDefinition.MaxRadius));
            base.m_gravityAcceleration.SetLocalValue(MathHelper.Clamp(sphere.GravityAcceleration, this.BlockDefinition.MinGravityAcceleration, this.BlockDefinition.MaxGravityAcceleration));
            this.m_defaultVolume = (float) ((Math.Pow(100.0, (double) this.BlockDefinition.ConsumptionPower) * 3.1415926535897931) * 0.75);
            if (base.CubeGrid.CreatePhysics)
            {
                base.AddDebugRenderComponent(new MyDebugRenderComponentGravityGeneratorSphere(this));
            }
        }

        protected override void InitializeSinkComponent()
        {
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.MaxInput, new Func<float>(this.CalculateRequiredPowerInput));
            base.ResourceSink = component;
            if (base.CubeGrid.CreatePhysics)
            {
                base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
                base.ResourceSink.RequiredInputChanged += new MyRequiredResourceChangeDelegate(this.Receiver_RequiredInputChanged);
            }
        }

        public override bool IsPositionInRange(Vector3D worldPoint) => 
            ((base.WorldMatrix.Translation - worldPoint).LengthSquared() < ((double) (this.m_radius * this.m_radius)));

        public override void UpdateBeforeSimulation()
        {
            if (MyFakes.ENABLE_GRAVITY_GENERATOR_SPHERE)
            {
                base.UpdateBeforeSimulation();
            }
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

        private MyGravityGeneratorSphereDefinition BlockDefinition =>
            ((MyGravityGeneratorSphereDefinition) base.BlockDefinition);

        public float Radius
        {
            get => 
                ((float) this.m_radius);
            set => 
                (this.m_radius.Value = value);
        }

        private float MaxInput =>
            ((float) ((Math.Pow((double) this.BlockDefinition.MaxRadius, (double) this.BlockDefinition.ConsumptionPower) / ((double) ((float) Math.Pow(100.0, (double) this.BlockDefinition.ConsumptionPower)))) * this.BlockDefinition.BasePowerInput));

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase.Gravity =>
            base.GravityAcceleration;

        float SpaceEngineers.Game.ModAPI.IMyGravityGeneratorSphere.Radius
        {
            get => 
                this.Radius;
            set => 
                (this.Radius = MathHelper.Clamp(value, this.BlockDefinition.MinRadius, this.BlockDefinition.MaxRadius));
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorSphere.Radius
        {
            get => 
                this.Radius;
            set => 
                (this.Radius = MathHelper.Clamp(value, this.BlockDefinition.MinRadius, this.BlockDefinition.MaxRadius));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGravityGeneratorSphere.<>c <>9 = new MyGravityGeneratorSphere.<>c();
            public static MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate <>9__12_0;
            public static MyTerminalValueControl<MyGravityGeneratorSphere, float>.SetterDelegate <>9__12_1;
            public static MyTerminalControlSlider<MyGravityGeneratorSphere>.FloatFunc <>9__12_2;
            public static MyTerminalControlSlider<MyGravityGeneratorSphere>.FloatFunc <>9__12_3;
            public static MyTerminalControl<MyGravityGeneratorSphere>.WriterDelegate <>9__12_4;
            public static MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate <>9__12_5;
            public static MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate <>9__12_6;
            public static MyTerminalValueControl<MyGravityGeneratorSphere, float>.GetterDelegate <>9__12_7;
            public static MyTerminalValueControl<MyGravityGeneratorSphere, float>.SetterDelegate <>9__12_8;
            public static MyTerminalControl<MyGravityGeneratorSphere>.WriterDelegate <>9__12_9;

            internal float <CreateTerminalControls>b__12_0(MyGravityGeneratorSphere x) => 
                x.Radius;

            internal void <CreateTerminalControls>b__12_1(MyGravityGeneratorSphere x, float v)
            {
                if (v < x.BlockDefinition.MinRadius)
                {
                    v = x.BlockDefinition.MinRadius;
                }
                x.Radius = v;
            }

            internal float <CreateTerminalControls>b__12_2(MyGravityGeneratorSphere x, float v) => 
                ((v != 0f) ? ((v - x.BlockDefinition.MinRadius) / (x.BlockDefinition.MaxRadius - x.BlockDefinition.MinRadius)) : 0f);

            internal float <CreateTerminalControls>b__12_3(MyGravityGeneratorSphere x, float v) => 
                ((v != 0f) ? ((v * (x.BlockDefinition.MaxRadius - x.BlockDefinition.MinRadius)) + x.BlockDefinition.MinRadius) : 0f);

            internal void <CreateTerminalControls>b__12_4(MyGravityGeneratorSphere x, StringBuilder result)
            {
                result.AppendInt32(((int) ((float) x.m_radius))).Append(" m");
            }

            internal float <CreateTerminalControls>b__12_5(MyGravityGeneratorSphere x) => 
                x.BlockDefinition.MinGravityAcceleration;

            internal float <CreateTerminalControls>b__12_6(MyGravityGeneratorSphere x) => 
                x.BlockDefinition.MaxGravityAcceleration;

            internal float <CreateTerminalControls>b__12_7(MyGravityGeneratorSphere x) => 
                x.GravityAcceleration;

            internal void <CreateTerminalControls>b__12_8(MyGravityGeneratorSphere x, float v)
            {
                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    v = 0f;
                }
                x.GravityAcceleration = v;
            }

            internal void <CreateTerminalControls>b__12_9(MyGravityGeneratorSphere x, StringBuilder result)
            {
                result.AppendFormat("{0:F1} m/s\x00b2 ({1:F2} g)", x.GravityAcceleration, x.GravityAcceleration / 9.81f);
            }
        }
    }
}

