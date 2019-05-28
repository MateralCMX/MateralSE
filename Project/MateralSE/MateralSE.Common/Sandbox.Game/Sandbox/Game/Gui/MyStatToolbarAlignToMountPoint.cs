namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Utils;

    public class MyStatToolbarAlignToMountPoint : MyStatBase
    {
        public MyStatToolbarAlignToMountPoint()
        {
            base.Id = MyStringHash.GetOrCompute("toolbar_align_to_mountpoint");
        }

        public override void Update()
        {
            this.CurrentValue = MyCubeBuilder.Static.AlignToDefault ? ((float) 1) : ((float) 0);
        }
    }
}

