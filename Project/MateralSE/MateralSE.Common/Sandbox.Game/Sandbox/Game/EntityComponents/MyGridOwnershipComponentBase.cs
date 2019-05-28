namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRage.Game.Components;

    public abstract class MyGridOwnershipComponentBase : MyEntityComponentBase
    {
        protected MyGridOwnershipComponentBase()
        {
        }

        public abstract long GetBlockOwnerId(MySlimBlock block);

        public override string ComponentTypeDebugString =>
            "Ownership";
    }
}

