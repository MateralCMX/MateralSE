namespace Sandbox.Game.Entities
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyStockpileItem
    {
        [ProtoMember(20)]
        public int Amount;
        [ProtoMember(0x17), Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))]
        public MyObjectBuilder_PhysicalObject Content;
        public override string ToString() => 
            $"{this.Amount}x {this.Content.SubtypeName}";
    }
}

