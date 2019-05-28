namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using System;
    using VRage.Game;

    public interface IMyHandheldGunObject<out T> : IMyGunObject<T> where T: MyDeviceBase
    {
        bool CanDoubleClickToStick(MyShootActionEnum action);
        void DoubleClicked(MyShootActionEnum action);
        bool ShouldEndShootOnPause(MyShootActionEnum action);

        MyObjectBuilder_PhysicalGunObject PhysicalObject { get; }

        MyPhysicalItemDefinition PhysicalItemDefinition { get; }

        bool ForceAnimationInsteadOfIK { get; }

        bool IsBlocking { get; }

        int CurrentAmmunition { get; set; }

        int CurrentMagazineAmmunition { get; set; }

        long OwnerId { get; }

        long OwnerIdentityId { get; }
    }
}

