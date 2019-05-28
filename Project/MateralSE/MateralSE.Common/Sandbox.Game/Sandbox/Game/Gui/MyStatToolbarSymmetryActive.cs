namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Utils;

    public class MyStatToolbarSymmetryActive : MyStatBase
    {
        public MyStatToolbarSymmetryActive()
        {
            base.Id = MyStringHash.GetOrCompute("toolbar_symmetry");
        }

        public override void Update()
        {
            this.CurrentValue = MyCubeBuilder.Static.UseSymmetry ? ((float) 1) : ((float) 0);
        }
    }
}

