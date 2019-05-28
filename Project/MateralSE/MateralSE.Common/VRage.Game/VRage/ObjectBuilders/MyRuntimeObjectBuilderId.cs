namespace VRage.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyRuntimeObjectBuilderId
    {
        public static readonly MyRuntimeObjectBuilderIdComparer Comparer;
        [ProtoMember(8)]
        internal readonly ushort Value;
        public MyRuntimeObjectBuilderId(ushort value)
        {
            this.Value = value;
        }

        public bool IsValid =>
            (this.Value != 0);
        public override string ToString() => 
            $"{this.Value}: {((MyObjectBuilderType) this)}";

        static MyRuntimeObjectBuilderId()
        {
            Comparer = new MyRuntimeObjectBuilderIdComparer();
        }
    }
}

