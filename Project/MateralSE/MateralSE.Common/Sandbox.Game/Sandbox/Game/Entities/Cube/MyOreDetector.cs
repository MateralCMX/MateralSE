namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Groups;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_OreDetector)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyOreDetector), typeof(Sandbox.ModAPI.Ingame.IMyOreDetector) })]
    public class MyOreDetector : MyFunctionalBlock, IMyComponentOwner<MyOreDetectorComponent>, Sandbox.ModAPI.IMyOreDetector, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyOreDetector
    {
        private MyOreDetectorDefinition m_definition;
        private readonly MyOreDetectorComponent m_oreDetectorComponent = new MyOreDetectorComponent();
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_broadcastUsingAntennas;
        private VRage.Sync.Sync<float, SyncDirection.BothWays> m_range;
        private static readonly short UPDATE_HUD_TIMEOUT = 200;

        public MyOreDetector()
        {
            this.CreateTerminalControls();
            this.m_broadcastUsingAntennas.ValueChanged += entity => this.BroadcastChanged();
            this.m_range.ValueChanged += entity => this.UpdateRange();
        }

        private void BroadcastChanged()
        {
            this.BroadcastUsingAntennas = (bool) this.m_broadcastUsingAntennas;
        }

        public override void CheckEmissiveState(bool force = false)
        {
            base.CheckEmissiveState(force);
            if (base.IsWorking)
            {
                if (this.m_oreDetectorComponent.DetectionRadius < 1E-05f)
                {
                    this.SetEmissiveStateDisabled();
                }
                else
                {
                    this.SetEmissiveStateWorking();
                }
            }
            else if (base.IsFunctional)
            {
                this.SetEmissiveStateDisabled();
            }
            else
            {
                this.SetEmissiveStateDamaged();
            }
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyOreDetector>())
            {
                base.CreateTerminalControls();
                MyTerminalControlSlider<MyOreDetector> slider1 = new MyTerminalControlSlider<MyOreDetector>("Range", MySpaceTexts.BlockPropertyTitle_OreDetectorRange, MySpaceTexts.BlockPropertyDescription_OreDetectorRange);
                MyTerminalControlSlider<MyOreDetector> slider2 = new MyTerminalControlSlider<MyOreDetector>("Range", MySpaceTexts.BlockPropertyTitle_OreDetectorRange, MySpaceTexts.BlockPropertyDescription_OreDetectorRange);
                slider2.SetLimits(x => 0f, x => x.m_definition.MaximumRange);
                MyTerminalValueControl<MyOreDetector, float>.GetterDelegate local3 = (MyTerminalValueControl<MyOreDetector, float>.GetterDelegate) slider2;
                local3.DefaultValue = new float?((float) 100);
                local3.Getter = x => (x.Range * x.m_definition.MaximumRange) * 0.01f;
                MyTerminalValueControl<MyOreDetector, float>.GetterDelegate local14 = local3;
                MyTerminalValueControl<MyOreDetector, float>.GetterDelegate local15 = local3;
                local15.Setter = (x, v) => x.Range = (v / x.m_definition.MaximumRange) * 100f;
                MyTerminalValueControl<MyOreDetector, float>.GetterDelegate local12 = local15;
                MyTerminalValueControl<MyOreDetector, float>.GetterDelegate local13 = local15;
                local13.Writer = (x, result) => result.AppendInt32(((int) x.m_oreDetectorComponent.DetectionRadius)).Append(" m");
                MyTerminalControlFactory.AddControl<MyOreDetector>((MyTerminalControl<MyOreDetector>) local13);
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MyOreDetector> checkbox1 = new MyTerminalControlCheckbox<MyOreDetector>("BroadcastUsingAntennas", MySpaceTexts.BlockPropertyDescription_BroadcastUsingAntennas, MySpaceTexts.BlockPropertyDescription_BroadcastUsingAntennas, on, on);
                MyTerminalControlCheckbox<MyOreDetector> checkbox2 = new MyTerminalControlCheckbox<MyOreDetector>("BroadcastUsingAntennas", MySpaceTexts.BlockPropertyDescription_BroadcastUsingAntennas, MySpaceTexts.BlockPropertyDescription_BroadcastUsingAntennas, on, on);
                checkbox2.Getter = x => x.m_oreDetectorComponent.BroadcastUsingAntennas;
                MyTerminalControlCheckbox<MyOreDetector> local10 = checkbox2;
                MyTerminalControlCheckbox<MyOreDetector> local11 = checkbox2;
                local11.Setter = (x, v) => x.m_broadcastUsingAntennas.Value = v;
                MyTerminalControlCheckbox<MyOreDetector> checkbox = local11;
                checkbox.EnableAction<MyOreDetector>(null);
                MyTerminalControlFactory.AddControl<MyOreDetector>(checkbox);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_OreDetector objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_OreDetector;
            objectBuilderCubeBlock.DetectionRadius = this.m_oreDetectorComponent.DetectionRadius;
            objectBuilderCubeBlock.BroadcastUsingAntennas = this.m_oreDetectorComponent.BroadcastUsingAntennas;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            this.m_definition = base.BlockDefinition as MyOreDetectorDefinition;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.m_definition.ResourceSinkGroup, 0.002f, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            base.ResourceSink = component;
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_OreDetector detector = objectBuilder as MyObjectBuilder_OreDetector;
            this.m_oreDetectorComponent.DetectionRadius = (detector.DetectionRadius == 0f) ? MathHelper.Clamp(0.5f * this.m_definition.MaximumRange, 1f, this.m_definition.MaximumRange) : MathHelper.Clamp(detector.DetectionRadius, 1f, this.m_definition.MaximumRange);
            this.m_oreDetectorComponent.BroadcastUsingAntennas = detector.BroadcastUsingAntennas;
            this.m_broadcastUsingAntennas.SetLocalValue(this.m_oreDetectorComponent.BroadcastUsingAntennas);
            this.m_oreDetectorComponent.OnCheckControl = (MyOreDetectorComponent.CheckControlDelegate) Delegate.Combine(this.m_oreDetectorComponent.OnCheckControl, new MyOreDetectorComponent.CheckControlDelegate(this.OnCheckControl));
            base.ResourceSink.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            base.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.MyOreDetector_OnClose);
        }

        private void MyOreDetector_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            if (this.m_oreDetectorComponent != null)
            {
                this.m_oreDetectorComponent.DiscardNextQuery();
            }
        }

        private bool OnCheckControl()
        {
            MyCubeGrid objA = null;
            if (MySession.Static.ControlledEntity != null)
            {
                objA = MySession.Static.ControlledEntity.Entity.Parent as MyCubeGrid;
            }
            if (objA == null)
            {
                return false;
            }
            bool flag = ReferenceEquals(objA, base.CubeGrid) || MyCubeGridGroups.Static.Logical.HasSameGroup(objA, base.CubeGrid);
            return (base.IsWorking & flag);
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
            if (!base.Enabled)
            {
                this.m_oreDetectorComponent.Clear();
            }
        }

        public override void OnUnregisteredFromGridSystems()
        {
            this.m_oreDetectorComponent.Clear();
            base.OnUnregisteredFromGridSystems();
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            if (!base.IsWorking)
            {
                this.m_oreDetectorComponent.Clear();
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            bool flag = base.HasLocalPlayerAccess();
            if (base.IsWorking)
            {
                bool flag2 = false;
                if (flag && (MySession.Static.LocalCharacter != null))
                {
                    if (this.m_oreDetectorComponent.BroadcastUsingAntennas)
                    {
                        MyCharacter localCharacter = MySession.Static.LocalCharacter;
                        MyCubeGrid topMostParent = base.GetTopMostParent(null) as MyCubeGrid;
                        if (topMostParent != null)
                        {
                            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(topMostParent);
                            if ((group != null) && localCharacter.HasAccessToLogicalGroup(group.GroupData))
                            {
                                flag2 = true;
                            }
                        }
                    }
                    else
                    {
                        IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
                        if ((controlledEntity != null) && (controlledEntity.Entity != null))
                        {
                            MyCubeGrid topMostParent = controlledEntity.Entity.GetTopMostParent(null) as MyCubeGrid;
                            if (topMostParent != null)
                            {
                                MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(topMostParent);
                                MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group3 = MyCubeGridGroups.Static.Logical.GetGroup(base.CubeGrid);
                                if (ReferenceEquals(topMostParent, base.CubeGrid) || (((group != null) && (group3 != null)) && (group.GroupData == group3.GroupData)))
                                {
                                    flag2 = true;
                                }
                            }
                        }
                    }
                }
                if (flag2)
                {
                    this.m_oreDetectorComponent.Update(base.PositionComp.GetPosition(), base.EntityId, false);
                    this.m_oreDetectorComponent.SetRelayedRequest = true;
                }
                else
                {
                    this.m_oreDetectorComponent.Clear();
                }
            }
        }

        private void UpdateRange()
        {
            this.m_oreDetectorComponent.DetectionRadius = (float) ((this.m_range / 100f) * this.m_definition.MaximumRange);
            base.RaisePropertiesChanged();
            this.CheckEmissiveState(false);
        }

        bool IMyComponentOwner<MyOreDetectorComponent>.GetComponent(out MyOreDetectorComponent component)
        {
            component = this.m_oreDetectorComponent;
            return base.IsWorking;
        }

        public float Range
        {
            get => 
                ((this.m_oreDetectorComponent.DetectionRadius / this.m_definition.MaximumRange) * 100f);
            set => 
                (this.m_range.Value = value);
        }

        public bool BroadcastUsingAntennas
        {
            get => 
                this.m_oreDetectorComponent.BroadcastUsingAntennas;
            set
            {
                this.m_oreDetectorComponent.BroadcastUsingAntennas = value;
                base.RaisePropertiesChanged();
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyOreDetector.BroadcastUsingAntennas
        {
            get => 
                this.BroadcastUsingAntennas;
            set => 
                (this.BroadcastUsingAntennas = value);
        }

        float Sandbox.ModAPI.Ingame.IMyOreDetector.Range =>
            this.Range;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyOreDetector.<>c <>9 = new MyOreDetector.<>c();
            public static MyTerminalValueControl<MyOreDetector, float>.GetterDelegate <>9__6_0;
            public static MyTerminalValueControl<MyOreDetector, float>.GetterDelegate <>9__6_1;
            public static MyTerminalValueControl<MyOreDetector, float>.GetterDelegate <>9__6_2;
            public static MyTerminalValueControl<MyOreDetector, float>.SetterDelegate <>9__6_3;
            public static MyTerminalControl<MyOreDetector>.WriterDelegate <>9__6_4;
            public static MyTerminalValueControl<MyOreDetector, bool>.GetterDelegate <>9__6_5;
            public static MyTerminalValueControl<MyOreDetector, bool>.SetterDelegate <>9__6_6;

            internal float <CreateTerminalControls>b__6_0(MyOreDetector x) => 
                0f;

            internal float <CreateTerminalControls>b__6_1(MyOreDetector x) => 
                x.m_definition.MaximumRange;

            internal float <CreateTerminalControls>b__6_2(MyOreDetector x) => 
                ((x.Range * x.m_definition.MaximumRange) * 0.01f);

            internal void <CreateTerminalControls>b__6_3(MyOreDetector x, float v)
            {
                x.Range = (v / x.m_definition.MaximumRange) * 100f;
            }

            internal void <CreateTerminalControls>b__6_4(MyOreDetector x, StringBuilder result)
            {
                result.AppendInt32(((int) x.m_oreDetectorComponent.DetectionRadius)).Append(" m");
            }

            internal bool <CreateTerminalControls>b__6_5(MyOreDetector x) => 
                x.m_oreDetectorComponent.BroadcastUsingAntennas;

            internal void <CreateTerminalControls>b__6_6(MyOreDetector x, bool v)
            {
                x.m_broadcastUsingAntennas.Value = v;
            }
        }
    }
}

