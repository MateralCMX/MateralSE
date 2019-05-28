namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public sealed class ProtoIncludeAttribute : Attribute
    {
        private readonly int tag;
        private readonly string knownTypeName;
        private ProtoBuf.DataFormat dataFormat;

        public ProtoIncludeAttribute(int tag, string knownTypeName)
        {
            if (tag <= 0)
            {
                throw new ArgumentOutOfRangeException("tag", "Tags must be positive integers");
            }
            if (Helpers.IsNullOrEmpty(knownTypeName))
            {
                throw new ArgumentNullException("knownTypeName", "Known type cannot be blank");
            }
            this.tag = tag;
            this.knownTypeName = knownTypeName;
        }

        public ProtoIncludeAttribute(int tag, Type knownType) : this(tag, (knownType == null) ? "" : knownType.AssemblyQualifiedName)
        {
        }

        public int Tag =>
            this.tag;

        public string KnownTypeName =>
            this.knownTypeName;

        public Type KnownType =>
            TypeModel.ResolveKnownType(this.KnownTypeName, null, null);

        [DefaultValue(0)]
        public ProtoBuf.DataFormat DataFormat
        {
            get => 
                this.dataFormat;
            set => 
                (this.dataFormat = value);
        }
    }
}

