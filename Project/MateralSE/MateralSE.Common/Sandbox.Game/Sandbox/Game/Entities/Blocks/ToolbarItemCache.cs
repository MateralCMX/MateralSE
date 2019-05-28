namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Game.Screens.Helpers;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential)]
    public struct ToolbarItemCache
    {
        private MyToolbarItem m_cachedItem;
        private ToolbarItem m_item;
        public ToolbarItem Item
        {
            get => 
                this.m_item;
            set
            {
                this.m_item = value;
                this.m_cachedItem = null;
            }
        }
        [NoSerialize]
        public MyToolbarItem CachedItem
        {
            get
            {
                if (this.m_cachedItem == null)
                {
                    this.m_cachedItem = ToolbarItem.ToItem(this.Item);
                }
                return this.m_cachedItem;
            }
        }
        public MyObjectBuilder_ToolbarItem ToObjectBuilder()
        {
            MyToolbarItem cachedItem = this.m_cachedItem;
            return cachedItem?.GetObjectBuilder();
        }

        public void SetToToolbar(MyToolbar toolbar, int index)
        {
            MyToolbarItem cachedItem = this.m_cachedItem;
            if (cachedItem != null)
            {
                toolbar.SetItemAtIndex(index, cachedItem);
            }
        }

        public static implicit operator ToolbarItemCache(ToolbarItem item) => 
            new ToolbarItemCache { m_item = item };
    }
}

