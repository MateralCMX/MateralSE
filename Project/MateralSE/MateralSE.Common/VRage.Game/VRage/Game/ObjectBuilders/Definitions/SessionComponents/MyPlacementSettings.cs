namespace VRage.Game.ObjectBuilders.Definitions.SessionComponents
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPlacementSettings
    {
        public MyGridPlacementSettings SmallGrid;
        public MyGridPlacementSettings SmallStaticGrid;
        public MyGridPlacementSettings LargeGrid;
        public MyGridPlacementSettings LargeStaticGrid;
        public bool StaticGridAlignToCenter;
        public MyGridPlacementSettings GetGridPlacementSettings(MyCubeSize cubeSize, bool isStatic) => 
            ((cubeSize == MyCubeSize.Large) ? (isStatic ? this.LargeStaticGrid : this.LargeGrid) : ((cubeSize == MyCubeSize.Small) ? (isStatic ? this.SmallStaticGrid : this.SmallGrid) : this.LargeGrid));

        public MyGridPlacementSettings GetGridPlacementSettings(MyCubeSize cubeSize) => 
            ((cubeSize == MyCubeSize.Large) ? this.LargeGrid : ((cubeSize == MyCubeSize.Small) ? this.SmallGrid : this.LargeGrid));
    }
}

