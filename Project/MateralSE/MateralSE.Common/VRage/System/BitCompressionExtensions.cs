namespace System
{
    using System.Runtime.CompilerServices;
    using VRage.BitCompression;
    using VRage.Library.Collections;
    using VRageMath;

    public static class BitCompressionExtensions
    {
        public static Quaternion ReadQuaternionNormCompressed(this BitStream stream) => 
            CompressedQuaternion.Read(stream);

        public static Quaternion ReadQuaternionNormCompressedIdentity(this BitStream stream) => 
            (!stream.ReadBool() ? CompressedQuaternion.Read(stream) : Quaternion.Identity);

        public static void SerializeNormCompressed(this BitStream stream, ref Quaternion quat)
        {
            if (stream.Reading)
            {
                quat = stream.ReadQuaternionNormCompressed();
            }
            else
            {
                stream.WriteQuaternionNormCompressed(quat);
            }
        }

        public static void SerializeNormCompressedIdentity(this BitStream stream, ref Quaternion quat)
        {
            if (stream.Reading)
            {
                quat = stream.ReadQuaternionNormCompressedIdentity();
            }
            else
            {
                stream.WriteQuaternionNormCompressedIdentity(quat);
            }
        }

        public static void WriteQuaternionNormCompressed(this BitStream stream, Quaternion value)
        {
            CompressedQuaternion.Write(stream, value);
        }

        public static void WriteQuaternionNormCompressedIdentity(this BitStream stream, Quaternion value)
        {
            bool flag = value == Quaternion.Identity;
            stream.WriteBool(flag);
            if (!flag)
            {
                CompressedQuaternion.Write(stream, value);
            }
        }
    }
}

