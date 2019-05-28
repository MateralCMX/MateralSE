namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class MyGameItemsEventArgs : EventArgs
    {
        public MyGameItemsEventArgs()
        {
            this.NewItems = new List<MyGameInventoryItem>();
        }

        public List<MyGameInventoryItem> NewItems { get; set; }

        public byte[] CheckData { get; set; }
    }
}

