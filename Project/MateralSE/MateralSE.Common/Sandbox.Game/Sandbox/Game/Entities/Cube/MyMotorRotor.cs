namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_MotorRotor))]
    public class MyMotorRotor : MyAttachableTopBlockBase, Sandbox.ModAPI.IMyMotorRotor, Sandbox.ModAPI.IMyAttachableTopBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyAttachableTopBlock, Sandbox.ModAPI.Ingame.IMyMotorRotor
    {
        [Obsolete("Use MyAttachableTopBlockBase.Base")]
        Sandbox.ModAPI.IMyMotorBase Sandbox.ModAPI.IMyMotorRotor.Stator =>
            (base.Stator as MyMotorStator);

        Sandbox.ModAPI.IMyMotorBase Sandbox.ModAPI.IMyMotorRotor.Base =>
            ((Sandbox.ModAPI.IMyMotorBase) base.Stator);
    }
}

