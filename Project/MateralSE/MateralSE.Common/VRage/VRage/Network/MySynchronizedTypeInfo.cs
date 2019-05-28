namespace VRage.Network
{
    using System;
    using VRage.Utils;

    public class MySynchronizedTypeInfo
    {
        public readonly System.Type Type;
        public readonly VRage.Network.TypeId TypeId;
        public readonly int TypeHash;
        public readonly string TypeName;
        public readonly string FullTypeName;
        public readonly bool IsReplicated;
        public readonly MySynchronizedTypeInfo BaseType;
        public readonly MyEventTable EventTable;

        public MySynchronizedTypeInfo(System.Type type, VRage.Network.TypeId id, MySynchronizedTypeInfo baseType, bool isReplicated)
        {
            this.Type = type;
            this.TypeId = id;
            this.TypeHash = GetHashFromType(type);
            this.TypeName = type.Name;
            this.FullTypeName = type.FullName;
            this.BaseType = baseType;
            this.IsReplicated = isReplicated;
            this.EventTable = new MyEventTable(this);
        }

        public static int GetHashFromType(System.Type type) => 
            MyStringHash.GetOrCompute(type.ToString()).GetHashCode();
    }
}

