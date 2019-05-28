namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemEmpty))]
    internal class MyToolbarItemEmpty : MyToolbarItem
    {
        public static MyToolbarItemEmpty Default = new MyToolbarItemEmpty();

        public MyToolbarItemEmpty()
        {
            base.SetEnabled(true);
            base.ActivateOnClick = false;
            base.WantsToBeSelected = true;
        }

        public override bool Activate() => 
            false;

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            true;

        public override bool Equals(object obj) => 
            false;

        public override int GetHashCode() => 
            -1;

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder() => 
            null;

        public override bool Init(MyObjectBuilder_ToolbarItem data) => 
            true;

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L) => 
            MyToolbarItem.ChangeInfo.None;
    }
}

