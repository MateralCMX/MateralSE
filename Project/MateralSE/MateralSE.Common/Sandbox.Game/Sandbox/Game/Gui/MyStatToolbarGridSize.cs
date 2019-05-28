namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;
    using VRage.Utils;

    public class MyStatToolbarGridSize : MyStatBase
    {
        public MyStatToolbarGridSize()
        {
            base.Id = MyStringHash.GetOrCompute("toolbar_grid_size");
        }

        public override void Update()
        {
            if (!MyCubeBuilder.Static.IsActivated || (MyCubeBuilder.Static.ToolbarBlockDefinition == null))
            {
                base.CurrentValue = -1f;
            }
            else
            {
                this.CurrentValue = (MyCubeBuilder.Static.ToolbarBlockDefinition.CubeSize == MyCubeSize.Small) ? ((float) 0) : ((float) 1);
            }
        }
    }
}

