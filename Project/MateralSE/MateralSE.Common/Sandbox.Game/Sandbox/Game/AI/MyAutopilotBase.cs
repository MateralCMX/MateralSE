namespace Sandbox.Game.AI
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;

    public abstract class MyAutopilotBase
    {
        protected MyAutopilotBase()
        {
        }

        public virtual void DebugDraw()
        {
        }

        public abstract MyObjectBuilder_AutopilotBase GetObjectBuilder();
        public abstract void Init(MyObjectBuilder_AutopilotBase objectBuilder);
        public virtual void OnAttachedToShipController(MyCockpit newShipController)
        {
            this.ShipController = newShipController;
        }

        public virtual void OnRemovedFromCockpit()
        {
            this.ShipController = null;
        }

        public abstract void Update();

        protected MyCockpit ShipController { get; private set; }

        public virtual bool RemoveOnPlayerControl =>
            true;
    }
}

