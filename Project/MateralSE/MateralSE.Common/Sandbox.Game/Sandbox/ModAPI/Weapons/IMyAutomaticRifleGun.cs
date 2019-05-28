namespace Sandbox.ModAPI.Weapons
{
    using Sandbox.Game.Entities;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyAutomaticRifleGun : VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyHandheldGunObject<MyGunBase>, IMyGunObject<MyGunBase>, IMyGunBaseUser
    {
    }
}

