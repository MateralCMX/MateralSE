namespace Sandbox.ModAPI.Weapons
{
    using Sandbox.Game.Entities;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyHandDrill : VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>, IMyGunBaseUser
    {
    }
}

