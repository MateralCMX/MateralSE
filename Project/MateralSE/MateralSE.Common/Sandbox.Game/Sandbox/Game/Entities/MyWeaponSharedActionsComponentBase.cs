namespace Sandbox.Game.Entities
{
    using System;
    using VRage.Game.Components;

    [MyComponentType(typeof(MyWeaponSharedActionsComponentBase))]
    public abstract class MyWeaponSharedActionsComponentBase : MyEntityComponentBase
    {
        protected MyWeaponSharedActionsComponentBase()
        {
        }

        public abstract void EndShoot(MyShootActionEnum action);
        public abstract void Shoot(MyShootActionEnum action);
        public abstract void Update();

        public override string ComponentTypeDebugString =>
            "WeaponSharedActionsComponentBase";
    }
}

