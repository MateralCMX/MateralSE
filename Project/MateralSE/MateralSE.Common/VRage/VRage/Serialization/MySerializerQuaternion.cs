namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRageMath;

    public class MySerializerQuaternion : MySerializer<Quaternion>
    {
        public override void Clone(ref Quaternion value)
        {
        }

        public override bool Equals(ref Quaternion a, ref Quaternion b) => 
            (a == b);

        public override void Read(BitStream stream, out Quaternion value, MySerializeInfo info)
        {
            if (!info.IsNormalized)
            {
                value.X = stream.ReadFloat();
                value.Y = stream.ReadFloat();
                value.Z = stream.ReadFloat();
                value.W = stream.ReadFloat();
            }
            else
            {
                bool flag = stream.ReadBool();
                bool flag2 = stream.ReadBool();
                ushort num = stream.ReadUInt16(0x10);
                ushort num2 = stream.ReadUInt16(0x10);
                ushort num3 = stream.ReadUInt16(0x10);
                value.X = (float) (((double) num) / 65535.0);
                value.Y = (float) (((double) num2) / 65535.0);
                value.Z = (float) (((double) num3) / 65535.0);
                if (stream.ReadBool())
                {
                    value.X = -value.X;
                }
                if (flag)
                {
                    value.Y = -value.Y;
                }
                if (flag2)
                {
                    value.Z = -value.Z;
                }
                float num4 = ((1f - (value.X * value.X)) - (value.Y * value.Y)) - (value.Z * value.Z);
                if (num4 < 0f)
                {
                    num4 = 0f;
                }
                value.W = (float) Math.Sqrt((double) num4);
                if (stream.ReadBool())
                {
                    value.W = -value.W;
                }
            }
        }

        public override void Write(BitStream stream, ref Quaternion value, MySerializeInfo info)
        {
            if (!info.IsNormalized)
            {
                stream.WriteFloat(value.X);
                stream.WriteFloat(value.Y);
                stream.WriteFloat(value.Z);
                stream.WriteFloat(value.W);
            }
            else
            {
                stream.WriteBool(value.W < 0f);
                stream.WriteBool(value.X < 0f);
                stream.WriteBool(value.Y < 0f);
                stream.WriteBool(value.Z < 0f);
                stream.WriteUInt16((ushort) (Math.Abs(value.X) * 65535.0), 0x10);
                stream.WriteUInt16((ushort) (Math.Abs(value.Y) * 65535.0), 0x10);
                stream.WriteUInt16((ushort) (Math.Abs(value.Z) * 65535.0), 0x10);
            }
        }
    }
}

