namespace Sandbox.Game.Weapons
{
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;

    [MyEntityType(typeof(MyObjectBuilder_AmmoMagazine), true)]
    public class MyAmmoMagazine : MyBaseInventoryItemEntity
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
        }
    }
}

