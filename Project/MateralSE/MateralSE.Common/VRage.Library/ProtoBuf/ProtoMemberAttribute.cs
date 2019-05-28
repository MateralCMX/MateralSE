namespace ProtoBuf
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class ProtoMemberAttribute : Attribute, IComparable, IComparable<ProtoMemberAttribute>
    {
        internal MemberInfo Member;
        internal bool TagIsPinned;
        private string name;
        private ProtoBuf.DataFormat dataFormat;
        private int tag;
        private MemberSerializationOptions options;

        public ProtoMemberAttribute([CallerLineNumber] int tag = 0) : this(false, tag)
        {
        }

        internal ProtoMemberAttribute(bool forced, [CallerLineNumber] int tag = 0)
        {
            if ((tag <= 0) && !forced)
            {
                throw new ArgumentOutOfRangeException("tag");
            }
            this.tag = tag;
        }

        public int CompareTo(ProtoMemberAttribute other)
        {
            if (other == null)
            {
                return -1;
            }
            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            int num = this.tag.CompareTo(other.tag);
            if (num == 0)
            {
                num = string.CompareOrdinal(this.name, other.name);
            }
            return num;
        }

        public int CompareTo(object other) => 
            this.CompareTo(other as ProtoMemberAttribute);

        internal void Rebase(int tag)
        {
            this.tag = tag;
        }

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public ProtoBuf.DataFormat DataFormat
        {
            get => 
                this.dataFormat;
            set => 
                (this.dataFormat = value);
        }

        public int Tag =>
            this.tag;

        public bool IsRequired
        {
            get => 
                ((this.options & MemberSerializationOptions.Required) == MemberSerializationOptions.Required);
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.Required;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.Required;
                }
            }
        }

        public bool IsPacked
        {
            get => 
                ((this.options & MemberSerializationOptions.Packed) == MemberSerializationOptions.Packed);
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.Packed;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.Packed;
                }
            }
        }

        public bool OverwriteList
        {
            get => 
                ((this.options & MemberSerializationOptions.OverwriteList) == MemberSerializationOptions.OverwriteList);
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.OverwriteList;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.OverwriteList;
                }
            }
        }

        public bool AsReference
        {
            get => 
                ((this.options & MemberSerializationOptions.AsReference) == MemberSerializationOptions.AsReference);
            set
            {
                this.options = !value ? (this.options & ~MemberSerializationOptions.AsReference) : (this.options | MemberSerializationOptions.AsReference);
                this.options |= MemberSerializationOptions.AsReferenceHasValue;
            }
        }

        internal bool AsReferenceHasValue
        {
            get => 
                ((this.options & MemberSerializationOptions.AsReferenceHasValue) == MemberSerializationOptions.AsReferenceHasValue);
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.AsReferenceHasValue;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.AsReferenceHasValue;
                }
            }
        }

        public bool DynamicType
        {
            get => 
                ((this.options & MemberSerializationOptions.DynamicType) == MemberSerializationOptions.DynamicType);
            set
            {
                if (value)
                {
                    this.options |= MemberSerializationOptions.DynamicType;
                }
                else
                {
                    this.options &= ~MemberSerializationOptions.DynamicType;
                }
            }
        }

        public MemberSerializationOptions Options
        {
            get => 
                this.options;
            set => 
                (this.options = value);
        }
    }
}

