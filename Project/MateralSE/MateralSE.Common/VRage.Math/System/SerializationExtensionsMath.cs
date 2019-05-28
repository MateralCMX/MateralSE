namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Library.Collections;
    using VRageMath;
    using VRageMath.PackedVector;

    public static class SerializationExtensionsMath
    {
        public static HalfVector3 ReadHalfVector3(this BitStream stream)
        {
            HalfVector3 vector;
            vector.X = stream.ReadUInt16(0x10);
            vector.Y = stream.ReadUInt16(0x10);
            vector.Z = stream.ReadUInt16(0x10);
            return vector;
        }

        public static HalfVector4 ReadHalfVector4(this BitStream stream)
        {
            HalfVector4 vector;
            vector.PackedValue = stream.ReadUInt64(0x40);
            return vector;
        }

        public static Vector3 ReadNormalizedSignedVector3(this BitStream stream, int bitCount)
        {
            Vector3 vector;
            vector.X = stream.ReadNormalizedSignedFloat(bitCount);
            vector.Y = stream.ReadNormalizedSignedFloat(bitCount);
            vector.Z = stream.ReadNormalizedSignedFloat(bitCount);
            return vector;
        }

        public static Quaternion ReadQuaternion(this BitStream stream)
        {
            Quaternion quaternion;
            quaternion.X = stream.ReadFloat();
            quaternion.Y = stream.ReadFloat();
            quaternion.Z = stream.ReadFloat();
            quaternion.W = stream.ReadFloat();
            return quaternion;
        }

        public static unsafe Quaternion ReadQuaternionNorm(this BitStream stream)
        {
            Quaternion quaternion;
            bool flag = stream.ReadBool();
            bool flag2 = stream.ReadBool();
            quaternion.X = (float) (((double) stream.ReadUInt16(0x10)) / 65535.0);
            quaternion.Y = (float) (((double) stream.ReadUInt16(0x10)) / 65535.0);
            quaternion.Z = (float) (((double) stream.ReadUInt16(0x10)) / 65535.0);
            if (stream.ReadBool())
            {
                Quaternion* quaternionPtr1 = (Quaternion*) ref quaternion;
                quaternionPtr1->X = -quaternion.X;
            }
            if (flag)
            {
                Quaternion* quaternionPtr2 = (Quaternion*) ref quaternion;
                quaternionPtr2->Y = -quaternion.Y;
            }
            if (flag2)
            {
                Quaternion* quaternionPtr3 = (Quaternion*) ref quaternion;
                quaternionPtr3->Z = -quaternion.Z;
            }
            float num4 = ((1f - (quaternion.X * quaternion.X)) - (quaternion.Y * quaternion.Y)) - (quaternion.Z * quaternion.Z);
            if (num4 < 0f)
            {
                num4 = 0f;
            }
            quaternion.W = (float) Math.Sqrt((double) num4);
            if (stream.ReadBool())
            {
                Quaternion* quaternionPtr4 = (Quaternion*) ref quaternion;
                quaternionPtr4->W = -quaternion.W;
            }
            return quaternion;
        }

        public static Vector3 ReadVector3(this BitStream stream)
        {
            Vector3 vector;
            vector.X = stream.ReadFloat();
            vector.Y = stream.ReadFloat();
            vector.Z = stream.ReadFloat();
            return vector;
        }

        public static Vector3D ReadVector3D(this BitStream stream)
        {
            Vector3D vectord;
            vectord.X = stream.ReadDouble();
            vectord.Y = stream.ReadDouble();
            vectord.Z = stream.ReadDouble();
            return vectord;
        }

        public static Vector3I ReadVector3I(this BitStream stream)
        {
            Vector3I vectori;
            vectori.X = stream.ReadInt32(0x20);
            vectori.Y = stream.ReadInt32(0x20);
            vectori.Z = stream.ReadInt32(0x20);
            return vectori;
        }

        public static Vector3I ReadVector3IVariant(this BitStream stream)
        {
            Vector3I vectori;
            vectori.X = stream.ReadInt32Variant();
            vectori.Y = stream.ReadInt32Variant();
            vectori.Z = stream.ReadInt32Variant();
            return vectori;
        }

        public static Vector4 ReadVector4(this BitStream stream)
        {
            Vector4 vector;
            vector.X = stream.ReadFloat();
            vector.Y = stream.ReadFloat();
            vector.Z = stream.ReadFloat();
            vector.W = stream.ReadFloat();
            return vector;
        }

        public static Vector4D ReadVector4D(this BitStream stream)
        {
            Vector4D vectord;
            vectord.X = stream.ReadDouble();
            vectord.Y = stream.ReadDouble();
            vectord.Z = stream.ReadDouble();
            vectord.W = stream.ReadDouble();
            return vectord;
        }

        public static void Serialize(this BitStream stream, ref BoundingBox bb)
        {
            stream.Serialize(ref bb.Min);
            stream.Serialize(ref bb.Max);
        }

        public static void Serialize(this BitStream stream, ref BoundingBoxD bb)
        {
            stream.Serialize(ref bb.Min);
            stream.Serialize(ref bb.Max);
        }

        public static unsafe void Serialize(this BitStream stream, ref MyBlockOrientation orientation)
        {
            MyBlockOrientation orientation2 = orientation;
            stream.SerializeMemory((void*) &orientation2, sizeof(MyBlockOrientation) * 8);
            orientation = orientation2;
        }

        public static void Serialize(this BitStream stream, ref HalfVector3 vec)
        {
            stream.Serialize(ref vec.X, 0x10);
            stream.Serialize(ref vec.Y, 0x10);
            stream.Serialize(ref vec.Z, 0x10);
        }

        public static void Serialize(this BitStream stream, ref HalfVector4 vec)
        {
            stream.Serialize(ref vec.PackedValue, 0x40);
        }

        public static void Serialize(this BitStream stream, ref Quaternion quat)
        {
            stream.Serialize(ref quat.X);
            stream.Serialize(ref quat.Y);
            stream.Serialize(ref quat.Z);
            stream.Serialize(ref quat.W);
        }

        public static void Serialize(this BitStream stream, ref Vector2 vec)
        {
            stream.Serialize(ref vec.X);
            stream.Serialize(ref vec.Y);
        }

        public static void Serialize(this BitStream stream, ref Vector3 vec)
        {
            stream.Serialize(ref vec.X);
            stream.Serialize(ref vec.Y);
            stream.Serialize(ref vec.Z);
        }

        public static void Serialize(this BitStream stream, ref Vector3D vec)
        {
            stream.Serialize(ref vec.X);
            stream.Serialize(ref vec.Y);
            stream.Serialize(ref vec.Z);
        }

        public static void Serialize(this BitStream stream, ref Vector3I vec)
        {
            stream.Serialize(ref vec.X, 0x20);
            stream.Serialize(ref vec.Y, 0x20);
            stream.Serialize(ref vec.Z, 0x20);
        }

        public static void Serialize(this BitStream stream, ref Vector4 vec)
        {
            stream.Serialize(ref vec.X);
            stream.Serialize(ref vec.Y);
            stream.Serialize(ref vec.Z);
            stream.Serialize(ref vec.W);
        }

        public static void Serialize(this BitStream stream, ref Vector4D vec)
        {
            stream.Serialize(ref vec.X);
            stream.Serialize(ref vec.Y);
            stream.Serialize(ref vec.Z);
            stream.Serialize(ref vec.W);
        }

        public static void SerializeList(this BitStream stream, ref List<Vector3D> list)
        {
            stream.SerializeList<Vector3D>(ref list, (bs, vec) => bs.Serialize(ref vec));
        }

        public static void SerializeNorm(this BitStream stream, ref Quaternion quat)
        {
            if (stream.Reading)
            {
                quat = stream.ReadQuaternionNorm();
            }
            else
            {
                stream.WriteQuaternionNorm(quat);
            }
        }

        public static void SerializePositionOrientation(this BitStream stream, ref Matrix m)
        {
            if (stream.Writing)
            {
                Quaternion quaternion;
                Vector3 translation = m.Translation;
                Quaternion.CreateFromRotationMatrix(ref m, out quaternion);
                stream.Serialize(ref translation);
                stream.SerializeNorm(ref quaternion);
            }
            else
            {
                Vector3 vec = new Vector3();
                Quaternion quat = new Quaternion();
                stream.Serialize(ref vec);
                stream.SerializeNorm(ref quat);
                Matrix.CreateFromQuaternion(ref quat, out m);
                m.Translation = vec;
            }
        }

        public static void SerializePositionOrientation(this BitStream stream, ref MatrixD m)
        {
            if (stream.Writing)
            {
                Quaternion quaternion;
                Vector3D translation = m.Translation;
                Quaternion.CreateFromRotationMatrix(ref m, out quaternion);
                stream.Serialize(ref translation);
                stream.SerializeNorm(ref quaternion);
            }
            else
            {
                Vector3D vec = new Vector3D();
                Quaternion quat = new Quaternion();
                stream.Serialize(ref vec);
                stream.SerializeNorm(ref quat);
                MatrixD.CreateFromQuaternion(ref quat, out m);
                m.Translation = vec;
            }
        }

        public static void SerializeVariant(this BitStream stream, ref Vector3I vec)
        {
            stream.SerializeVariant(ref vec.X);
            stream.SerializeVariant(ref vec.Y);
            stream.SerializeVariant(ref vec.Z);
        }

        public static void Write(this BitStream stream, HalfVector3 vec)
        {
            stream.WriteUInt16(vec.X, 0x10);
            stream.WriteUInt16(vec.Y, 0x10);
            stream.WriteUInt16(vec.Z, 0x10);
        }

        public static void Write(this BitStream stream, HalfVector4 vec)
        {
            stream.WriteUInt64(vec.PackedValue, 0x40);
        }

        public static void Write(this BitStream stream, Vector3 vec)
        {
            stream.WriteFloat(vec.X);
            stream.WriteFloat(vec.Y);
            stream.WriteFloat(vec.Z);
        }

        public static void Write(this BitStream stream, Vector3D vec)
        {
            stream.WriteDouble(vec.X);
            stream.WriteDouble(vec.Y);
            stream.WriteDouble(vec.Z);
        }

        public static void Write(this BitStream stream, Vector3I vec)
        {
            stream.WriteInt32(vec.X, 0x20);
            stream.WriteInt32(vec.Y, 0x20);
            stream.WriteInt32(vec.Z, 0x20);
        }

        public static void Write(this BitStream stream, Vector4 vec)
        {
            stream.WriteFloat(vec.X);
            stream.WriteFloat(vec.Y);
            stream.WriteFloat(vec.Z);
            stream.WriteFloat(vec.W);
        }

        public static void Write(this BitStream stream, Vector4D vec)
        {
            stream.WriteDouble(vec.X);
            stream.WriteDouble(vec.Y);
            stream.WriteDouble(vec.Z);
            stream.WriteDouble(vec.W);
        }

        public static void WriteNormalizedSignedVector3(this BitStream stream, Vector3 vec, int bitCount)
        {
            Vector3 vector1 = Vector3.Clamp(vec, Vector3.MinusOne, Vector3.One);
            vec = vector1;
            stream.WriteNormalizedSignedFloat(vec.X, bitCount);
            stream.WriteNormalizedSignedFloat(vec.Y, bitCount);
            stream.WriteNormalizedSignedFloat(vec.Z, bitCount);
        }

        public static void WriteQuaternion(this BitStream stream, Quaternion q)
        {
            stream.WriteFloat(q.X);
            stream.WriteFloat(q.Y);
            stream.WriteFloat(q.Z);
            stream.WriteFloat(q.W);
        }

        public static void WriteQuaternionNorm(this BitStream stream, Quaternion value)
        {
            stream.WriteBool(value.W < 0f);
            stream.WriteBool(value.X < 0f);
            stream.WriteBool(value.Y < 0f);
            stream.WriteBool(value.Z < 0f);
            stream.WriteUInt16((ushort) (Math.Abs(value.X) * 65535.0), 0x10);
            stream.WriteUInt16((ushort) (Math.Abs(value.Y) * 65535.0), 0x10);
            stream.WriteUInt16((ushort) (Math.Abs(value.Z) * 65535.0), 0x10);
        }

        public static void WriteVariant(this BitStream stream, Vector3I vec)
        {
            stream.WriteVariantSigned(vec.X);
            stream.WriteVariantSigned(vec.Y);
            stream.WriteVariantSigned(vec.Z);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SerializationExtensionsMath.<>c <>9 = new SerializationExtensionsMath.<>c();
            public static BitStreamExtensions.SerializeCallback<Vector3D> <>9__36_0;

            internal void <SerializeList>b__36_0(BitStream bs, ref Vector3D vec)
            {
                bs.Serialize(ref vec);
            }
        }
    }
}

