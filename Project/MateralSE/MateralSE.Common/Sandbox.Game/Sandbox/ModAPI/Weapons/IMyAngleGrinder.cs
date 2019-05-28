namespace Sandbox.ModAPI.Weapons
{
    using Sandbox.Game.Entities;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyAngleGrinder : VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEngineerToolBase, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>
    {
    }
}

