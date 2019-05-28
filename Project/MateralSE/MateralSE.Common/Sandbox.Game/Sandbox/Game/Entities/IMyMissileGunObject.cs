namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using System;

    public interface IMyMissileGunObject : IMyGunObject<MyGunBase>
    {
        void MissileShootEffect();
        void RemoveMissile(long entityId);
        void ShootMissile(MyObjectBuilder_Missile builder);
    }
}

