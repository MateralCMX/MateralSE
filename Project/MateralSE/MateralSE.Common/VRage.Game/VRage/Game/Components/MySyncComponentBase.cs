namespace VRage.Game.Components
{
    using System;

    public abstract class MySyncComponentBase : MyEntityComponentBase
    {
        protected MySyncComponentBase()
        {
        }

        public override string ComponentTypeDebugString =>
            "Sync";
    }
}

