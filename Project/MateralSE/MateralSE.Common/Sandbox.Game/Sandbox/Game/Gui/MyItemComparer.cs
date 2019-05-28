namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;

    public class MyItemComparer : IComparer<MyGuiControlListbox.Item>
    {
        private Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> comparator;

        public MyItemComparer(Func<MyGuiControlListbox.Item, MyGuiControlListbox.Item, int> comp)
        {
            this.comparator = comp;
        }

        public int Compare(MyGuiControlListbox.Item x, MyGuiControlListbox.Item y) => 
            ((this.comparator == null) ? 0 : this.comparator(x, y));
    }
}

