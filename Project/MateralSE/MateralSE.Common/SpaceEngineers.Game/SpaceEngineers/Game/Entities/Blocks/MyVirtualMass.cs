namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_VirtualMass)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyArtificialMassBlock), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyArtificialMassBlock) })]
    public class MyVirtualMass : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyArtificialMassBlock, SpaceEngineers.Game.ModAPI.IMyVirtualMass, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyVirtualMass, SpaceEngineers.Game.ModAPI.Ingame.IMyArtificialMassBlock
    {
        private float CalculateRequiredPowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return this.BlockDefinition.RequiredPowerInput;
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            int enabled;
            base.ResourceSink = new MyResourceSinkComponent(1);
            base.ResourceSink.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, new Func<float>(this.CalculateRequiredPowerInput));
            base.Init(objectBuilder, cubeGrid);
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink.Update();
            if (base.Physics != null)
            {
                base.Physics.Close();
            }
            HkBoxShape shape = new HkBoxShape(new Vector3(cubeGrid.GridSize / 3f));
            HkMassProperties properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(shape.HalfExtents, this.BlockDefinition.VirtualMass);
            base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_UNLOCKED_SPEEDS);
            base.Physics.IsPhantom = false;
            base.Physics.CreateFromCollisionObject((HkShape) shape, Vector3.Zero, base.WorldMatrix, new HkMassProperties?(properties), 0x19);
            if (!base.IsWorking || (cubeGrid.Physics == null))
            {
                enabled = 0;
            }
            else
            {
                enabled = (int) cubeGrid.Physics.Enabled;
            }
            base.Physics.Enabled = (bool) enabled;
            base.Physics.RigidBody.Activate();
            shape.Base.RemoveReference();
            this.UpdateText();
            base.NeedsWorldMatrix = true;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.ResourceSink.Update();
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.ResourceSink.Update();
            base.OnBuildSuccess(builtBy, instantBuild);
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
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
                if (base.IsWorking)
                {
                    base.Physics.RigidBody.Activate();
                }
                this.UpdateText();
            }
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentMass));
            base.DetailedInfo.Append(base.IsWorking ? this.BlockDefinition.VirtualMass.ToString() : "0");
            base.DetailedInfo.Append(" kg\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_RequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        private MyVirtualMassDefinition BlockDefinition =>
            ((MyVirtualMassDefinition) base.BlockDefinition);

        float SpaceEngineers.Game.ModAPI.Ingame.IMyVirtualMass.VirtualMass =>
            this.BlockDefinition.VirtualMass;
    }
}

