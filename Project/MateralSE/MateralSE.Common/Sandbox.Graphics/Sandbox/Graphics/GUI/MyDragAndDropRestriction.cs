namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class MyDragAndDropRestriction
    {
        public MyDragAndDropRestriction()
        {
            this.ObjectBuilders = new List<ushort>();
            this.ObjectBuilderTypes = new List<ushort>();
        }

        public List<ushort> ObjectBuilders { get; private set; }

        public List<ushort> ObjectBuilderTypes { get; private set; }
    }
}

