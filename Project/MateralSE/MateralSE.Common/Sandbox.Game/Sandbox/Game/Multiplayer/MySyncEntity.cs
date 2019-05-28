namespace Sandbox.Game.Multiplayer
{
    using System;
    using VRage.Game.Components;
    using VRage.Game.Entity;

    [PreloadRequired]
    public class MySyncEntity : MySyncComponentBase
    {
        public readonly MyEntity Entity;

        public MySyncEntity(MyEntity entity)
        {
            this.Entity = entity;
        }
    }
}

