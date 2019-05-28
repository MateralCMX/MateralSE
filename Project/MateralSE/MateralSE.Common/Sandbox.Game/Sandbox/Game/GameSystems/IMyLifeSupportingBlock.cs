namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities.Character;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyLifeSupportingBlock : VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity
    {
        void BroadcastSupportRequest(MyCharacter user);
        void ShowTerminal(MyCharacter user);

        bool RefuelAllowed { get; }

        bool HealingAllowed { get; }

        MyLifeSupportingBlockType BlockType { get; }
    }
}

