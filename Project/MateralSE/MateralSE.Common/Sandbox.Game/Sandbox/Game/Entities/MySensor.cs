namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Components;
    using System;
    using VRage.ObjectBuilders;

    internal class MySensor : MySensorBase
    {
        public MySensor()
        {
            base.Render = new MyRenderComponentSensor();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
        }

        public void InitPhysics()
        {
        }
    }
}

