namespace VRage.Game.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyInventoryItem : IComparable<MyInventoryItem>, IEquatable<MyInventoryItem>
    {
        public readonly uint ItemId;
        public readonly MyFixedPoint Amount;
        public readonly MyItemType Type;
        public MyInventoryItem(MyItemType type, uint itemId, MyFixedPoint amount)
        {
            this.Type = type;
            this.ItemId = itemId;
            this.Amount = amount;
        }

        public static bool operator ==(MyInventoryItem a, MyInventoryItem b) => 
            ((a.ItemId == b.ItemId) && ((a.Amount == b.Amount) && (a.Type == b.Type)));

        public static bool operator !=(MyInventoryItem a, MyInventoryItem b) => 
            !(a == b);

        public bool Equals(MyInventoryItem other) => 
            (this == other);

        public override bool Equals(object obj) => 
            ((obj is MyInventoryItem) ? this.Equals((MyInventoryItem) obj) : false);

        public override int GetHashCode() => 
            MyTuple.CombineHashCodes(this.ItemId.GetHashCode(), this.Amount.GetHashCode(), this.Type.GetHashCode());

        public int CompareTo(MyInventoryItem other)
        {
            int num = this.ItemId.CompareTo(other.ItemId);
            if (num != 0)
            {
                return num;
            }
            int num2 = ((double) this.Amount).CompareTo((double) other.Amount);
            if (num2 != 0)
            {
                return num2;
            }
            return this.Type.CompareTo(other.Type);
        }

        public override string ToString() => 
            $"{this.Amount.ToString()}x {this.Type.ToString()}";
    }
}

