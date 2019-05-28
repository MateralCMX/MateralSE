namespace VRage.Game.Entity
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ObjectBuilders;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPhysicalInventoryItem : VRage.Game.ModAPI.IMyInventoryItem, VRage.Game.ModAPI.Ingame.IMyInventoryItem
    {
        public MyFixedPoint Amount;
        public float Scale;
        [DynamicObjectBuilder(false)]
        public MyObjectBuilder_PhysicalObject Content;
        public uint ItemId;
        public MyPhysicalInventoryItem(MyFixedPoint amount, MyObjectBuilder_PhysicalObject content, float scale = 1f)
        {
            this.ItemId = 0;
            this.Amount = amount;
            this.Scale = scale;
            this.Content = content;
        }

        public MyPhysicalInventoryItem(MyObjectBuilder_InventoryItem item)
        {
            this.ItemId = 0;
            this.Amount = item.Amount;
            this.Scale = item.Scale;
            this.Content = item.PhysicalContent.Clone() as MyObjectBuilder_PhysicalObject;
        }

        public MyObjectBuilder_InventoryItem GetObjectBuilder()
        {
            MyObjectBuilder_InventoryItem local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_InventoryItem>();
            local1.Amount = this.Amount;
            local1.Scale = this.Scale;
            local1.PhysicalContent = this.Content;
            local1.ItemId = this.ItemId;
            return local1;
        }

        public override string ToString() => 
            $"{this.Amount}x {this.Content.GetId()}";

        MyFixedPoint VRage.Game.ModAPI.Ingame.IMyInventoryItem.Amount
        {
            get => 
                this.Amount;
            set => 
                (this.Amount = value);
        }
        float VRage.Game.ModAPI.Ingame.IMyInventoryItem.Scale
        {
            get => 
                this.Scale;
            set => 
                (this.Scale = value);
        }
        MyObjectBuilder_Base VRage.Game.ModAPI.Ingame.IMyInventoryItem.Content
        {
            get => 
                this.Content;
            set => 
                (this.Content = value as MyObjectBuilder_PhysicalObject);
        }
        uint VRage.Game.ModAPI.Ingame.IMyInventoryItem.ItemId
        {
            get => 
                this.ItemId;
            set => 
                (this.ItemId = value);
        }
    }
}

