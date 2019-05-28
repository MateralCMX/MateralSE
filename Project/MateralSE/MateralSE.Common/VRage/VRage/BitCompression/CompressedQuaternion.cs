namespace VRage.BitCompression
{
    using System;
    using VRage.Library.Collections;
    using VRageMath;

    public static class CompressedQuaternion
    {
        private const float MIN_QF_LENGTH = -0.7071065f;
        private const float MAX_QF_LENGTH = 0.7071065f;
        private const int QF_BITS = 9;
        private const int QF_VALUE = 0x1ff;
        private const float QF_SCALE = 511f;
        private const float QF_SCALE_INV = 0.001956947f;

        public static bool CompressedQuaternionUnitTest()
        {
            BitStream stream = new BitStream(0x600);
            stream.ResetWrite();
            Quaternion identity = Quaternion.Identity;
            stream.WriteQuaternionNormCompressed(identity);
            stream.ResetRead();
            stream.ResetWrite();
            identity = Quaternion.CreateFromAxisAngle(Vector3.Forward, 1.047198f);
            stream.WriteQuaternionNormCompressed(identity);
            stream.ResetRead();
            stream.ResetWrite();
            Vector3 axis = new Vector3(1f, -1f, 3f);
            axis.Normalize();
            identity = Quaternion.CreateFromAxisAngle(axis, 1.047198f);
            stream.WriteQuaternionNormCompressed(identity);
            stream.ResetRead();
            return ((!identity.Equals(stream.ReadQuaternionNormCompressed(), 0.001956947f) | !identity.Equals(stream.ReadQuaternionNormCompressed(), 0.001956947f)) | !identity.Equals(stream.ReadQuaternionNormCompressed(), 0.001956947f));
        }

        public static Quaternion Read(BitStream stream)
        {
            Quaternion identity = Quaternion.Identity;
            int index = stream.ReadInt32(2);
            float num2 = 0f;
            for (int i = 0; i < 4; i++)
            {
                if (i != index)
                {
                    float num4 = ((stream.ReadInt32(9) * 0.001956947f) * 1.414213f) + -0.7071065f;
                    identity.SetComponent(i, num4);
                    num2 += num4 * num4;
                }
            }
            identity.SetComponent(index, (float) Math.Sqrt((double) (1f - num2)));
            identity.Normalize();
            return identity;
        }

        public static void Write(BitStream stream, Quaternion value)
        {
            value.Normalize();
            int index = value.FindLargestIndex();
            if (value.GetComponent(index) < 0f)
            {
                value = -value;
            }
            stream.WriteInt32(index, 2);
            for (int i = 0; i < 4; i++)
            {
                if (i != index)
                {
                    uint num3 = (uint) Math.Floor((double) ((((value.GetComponent(i) - -0.7071065f) / 1.414213f) * 511f) + 0.5f));
                    stream.WriteUInt32(num3, 9);
                }
            }
        }
    }
}

