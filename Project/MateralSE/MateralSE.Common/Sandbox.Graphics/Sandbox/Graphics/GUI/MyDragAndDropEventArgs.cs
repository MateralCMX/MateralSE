namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Input;

    public class MyDragAndDropEventArgs
    {
        public MySharedButtonsEnum DragButton;

        public MyDragAndDropInfo DragFrom { get; set; }

        public MyDragAndDropInfo DropTo { get; set; }

        public MyGuiGridItem Item { get; set; }
    }
}

