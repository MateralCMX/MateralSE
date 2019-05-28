namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_MotorAdvancedStator)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyMotorAdvancedStator), typeof(Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator) })]
    public class MyMotorAdvancedStator : MyMotorStator, Sandbox.ModAPI.IMyMotorAdvancedStator, Sandbox.ModAPI.IMyMotorStator, Sandbox.ModAPI.Ingame.IMyMotorStator, Sandbox.ModAPI.Ingame.IMyMotorBase, Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyMotorBase, Sandbox.ModAPI.IMyMechanicalConnectionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyMotorAdvancedStator
    {
        public MyMotorAdvancedStator()
        {
            base.m_canBeDetached = true;
        }

        protected override bool Attach(MyAttachableTopBlockBase rotor, bool updateGroup = true)
        {
            if (!(rotor is MyMotorRotor))
            {
                return false;
            }
            bool flag1 = base.Attach(rotor, updateGroup);
            if ((flag1 & updateGroup) && (base.TopBlock is MyMotorAdvancedRotor))
            {
                base.m_conveyorEndpoint.Attach((base.TopBlock as MyMotorAdvancedRotor).ConveyorEndpoint as MyAttachableConveyorEndpoint);
            }
            return flag1;
        }

        public override unsafe void ComputeTopQueryBox(out Vector3D pos, out Vector3 halfExtents, out Quaternion orientation)
        {
            base.ComputeTopQueryBox(out pos, out halfExtents, out orientation);
            if (base.CubeGrid.GridSizeEnum == MyCubeSize.Small)
            {
                float* singlePtr1 = (float*) ref halfExtents.Y;
                singlePtr1[0] *= 2f;
            }
        }

        protected override void Detach(MyCubeGrid topGrid, bool updateGroup = true)
        {
            if ((base.TopBlock != null) & updateGroup)
            {
                MyAttachableTopBlockBase topBlock = base.TopBlock;
                if (topBlock is MyMotorAdvancedRotor)
                {
                    base.m_conveyorEndpoint.Detach((topBlock as MyMotorAdvancedRotor).ConveyorEndpoint as MyAttachableConveyorEndpoint);
                }
            }
            base.Detach(topGrid, updateGroup);
        }
    }
}

