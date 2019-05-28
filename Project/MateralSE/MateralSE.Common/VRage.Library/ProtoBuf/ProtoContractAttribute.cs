namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class ProtoContractAttribute : Attribute
    {
        private string name;
        private int implicitFirstTag;
        private ProtoBuf.ImplicitFields implicitFields;
        private int dataMemberOffset;
        private byte flags;
        private const byte OPTIONS_InferTagFromName = 1;
        private const byte OPTIONS_InferTagFromNameHasValue = 2;
        private const byte OPTIONS_UseProtoMembersOnly = 4;
        private const byte OPTIONS_SkipConstructor = 8;
        private const byte OPTIONS_IgnoreListHandling = 0x10;
        private const byte OPTIONS_AsReferenceDefault = 0x20;

        private bool HasFlag(byte flag) => 
            ((this.flags & flag) == flag);

        private void SetFlag(byte flag, bool value)
        {
            if (value)
            {
                this.flags = (byte) (this.flags | flag);
            }
            else
            {
                this.flags = (byte) (this.flags & ~flag);
            }
        }

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public int ImplicitFirstTag
        {
            get => 
                this.implicitFirstTag;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("ImplicitFirstTag");
                }
                this.implicitFirstTag = value;
            }
        }

        public bool UseProtoMembersOnly
        {
            get => 
                this.HasFlag(4);
            set => 
                this.SetFlag(4, value);
        }

        public bool IgnoreListHandling
        {
            get => 
                this.HasFlag(0x10);
            set => 
                this.SetFlag(0x10, value);
        }

        public ProtoBuf.ImplicitFields ImplicitFields
        {
            get => 
                this.implicitFields;
            set => 
                (this.implicitFields = value);
        }

        public bool InferTagFromName
        {
            get => 
                this.HasFlag(1);
            set
            {
                this.SetFlag(1, value);
                this.SetFlag(2, true);
            }
        }

        internal bool InferTagFromNameHasValue =>
            this.HasFlag(2);

        public int DataMemberOffset
        {
            get => 
                this.dataMemberOffset;
            set => 
                (this.dataMemberOffset = value);
        }

        public bool SkipConstructor
        {
            get => 
                this.HasFlag(8);
            set => 
                this.SetFlag(8, value);
        }

        public bool AsReferenceDefault
        {
            get => 
                this.HasFlag(0x20);
            set => 
                this.SetFlag(0x20, value);
        }
    }
}

