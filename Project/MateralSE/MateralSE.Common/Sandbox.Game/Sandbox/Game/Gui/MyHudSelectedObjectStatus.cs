namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity.UseObject;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyHudSelectedObjectStatus
    {
        public IMyUseObject Instance;
        public string[] SectionNames;
        public int InstanceId;
        public uint[] SubpartIndices;
        public MyHudObjectHighlightStyle Style;
        public void Reset()
        {
            this.Instance = null;
            this.SectionNames = null;
            this.InstanceId = -1;
            this.SubpartIndices = null;
            this.Style = MyHudObjectHighlightStyle.None;
        }
    }
}

