namespace Sandbox.Game.Gui
{
    using System;
    using System.Collections.Generic;

    public class MyItemComparer_Rew : IComparer<MyBlueprintItemInfo>
    {
        private Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> comparator;

        public MyItemComparer_Rew(Func<MyBlueprintItemInfo, MyBlueprintItemInfo, int> comp)
        {
            this.comparator = comp;
        }

        public int Compare(MyBlueprintItemInfo x, MyBlueprintItemInfo y) => 
            ((this.comparator == null) ? 0 : this.comparator(x, y));
    }
}

