namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using ProtoBuf.Serializers;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class SubType
    {
        private readonly int fieldNumber;
        private readonly MetaType derivedType;
        private readonly DataFormat dataFormat;
        private IProtoSerializer serializer;

        public SubType(int fieldNumber, MetaType derivedType, DataFormat format)
        {
            if (derivedType == null)
            {
                throw new ArgumentNullException("derivedType");
            }
            if (fieldNumber <= 0)
            {
                throw new ArgumentOutOfRangeException("fieldNumber");
            }
            this.fieldNumber = fieldNumber;
            this.derivedType = derivedType;
            this.dataFormat = format;
        }

        private IProtoSerializer BuildSerializer()
        {
            WireType wireType = WireType.String;
            if (this.dataFormat == DataFormat.Group)
            {
                wireType = WireType.StartGroup;
            }
            return new TagDecorator(this.fieldNumber, wireType, false, new SubItemSerializer(this.derivedType.Type, this.derivedType.GetKey(false, false), this.derivedType, false));
        }

        public int FieldNumber =>
            this.fieldNumber;

        public MetaType DerivedType =>
            this.derivedType;

        internal IProtoSerializer Serializer
        {
            get
            {
                if (this.serializer == null)
                {
                    this.serializer = this.BuildSerializer();
                }
                return this.serializer;
            }
        }

        internal class Comparer : IComparer, IComparer<SubType>
        {
            public static readonly SubType.Comparer Default = new SubType.Comparer();

            public int Compare(SubType x, SubType y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                return x.FieldNumber.CompareTo(y.FieldNumber);
            }

            public int Compare(object x, object y) => 
                this.Compare(x as SubType, y as SubType);
        }
    }
}

