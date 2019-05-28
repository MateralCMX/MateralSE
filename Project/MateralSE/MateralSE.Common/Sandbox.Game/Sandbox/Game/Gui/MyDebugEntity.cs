namespace Sandbox.Game.Gui
{
    using System;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.ObjectBuilders;

    internal class MyDebugEntity : MyEntity
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            base.Render.ModelStorage = MyModels.GetModelOnlyData(@"Models\StoneRoundLargeFull.mwm");
        }
    }
}

