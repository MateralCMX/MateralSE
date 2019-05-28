namespace VRage.Render.Scene
{
    using System;

    public class MyChildCullTreeData
    {
        public bool FarCull;
        public Action<MyCullResultsBase, bool> Add;
        public Action<MyCullResultsBase> Remove;
        public Func<Color> DebugColor;
    }
}

