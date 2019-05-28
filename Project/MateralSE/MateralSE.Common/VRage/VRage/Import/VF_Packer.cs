namespace VRage.Import
{
    using System;
    using Unsharper;
    using VRageMath;
    using VRageMath.PackedVector;

    [UnsharperDisableReflection]
    public class VF_Packer
    {
        public static short PackAmbientAndAlpha(float ambient, byte alpha) => 
            ((short) (((short) (ambient * 8191f)) + ((short) (alpha << 13))));

        public static uint PackNormal(Vector3 normal) => 
            PackNormal(ref normal);

        public static unsafe uint PackNormal(ref Vector3 normal)
        {
            Vector3 vector = normal;
            Vector3* vectorPtr1 = (Vector3*) ref vector;
            vectorPtr1->X = 0.5f * (vector.X + 1f);
            Vector3* vectorPtr2 = (Vector3*) ref vector;
            vectorPtr2->Y = 0.5f * (vector.Y + 1f);
            uint num = (ushort) (vector.Y * 32767f);
            return (((uint) (((ushort) (vector.X * 32767f)) | ((ushort) (((ushort) ((vector.Z > 0f) ? 1 : 0)) << 15)))) | (num << 0x10));
        }

        public static Byte4 PackNormalB4(ref Vector3 normal) => 
            new Byte4 { PackedValue = PackNormal(ref normal) };

        public static HalfVector4 PackPosition(Vector3 position) => 
            PositionPacker.PackPosition(ref position);

        public static HalfVector4 PackPosition(ref Vector3 position) => 
            PositionPacker.PackPosition(ref position);

        public static unsafe uint PackTangentSign(ref Vector4 tangent)
        {
            Vector4 vector = tangent;
            Vector4* vectorPtr1 = (Vector4*) ref vector;
            vectorPtr1->X = 0.5f * (vector.X + 1f);
            Vector4* vectorPtr2 = (Vector4*) ref vector;
            vectorPtr2->Y = 0.5f * (vector.Y + 1f);
            uint num = (ushort) (vector.Y * 32767f);
            return (((uint) (((ushort) (vector.X * 32767f)) | ((ushort) (((ushort) ((vector.Z > 0f) ? 1 : 0)) << 15)))) | ((num | ((ushort) (((vector.W > 0f) ? 1 : 0) << 15))) << 0x10));
        }

        public static Byte4 PackTangentSignB4(ref Vector4 tangentW) => 
            new Byte4 { PackedValue = PackTangentSign(ref tangentW) };

        public static Vector3 RepackModelPosition(ref Vector3 position) => 
            UnpackPosition(ref PackPosition(ref position));

        public static byte UnpackAlpha(short packed) => 
            ((byte) Math.Abs((int) (packed / 0x2000)));

        public static byte UnpackAlpha(float packed) => 
            ((byte) Math.Abs((float) (packed / 8192f)));

        public static float UnpackAmbient(short packed) => 
            ((((float) packed) % 8192f) / 8191f);

        public static float UnpackAmbient(float packed) => 
            ((packed % 8192f) / 8191f);

        public static Vector3 UnpackNormal(ref uint packedNormal)
        {
            Byte4 num = new Byte4 {
                PackedValue = packedNormal
            };
            return UnpackNormal(ref num);
        }

        public static Vector3 UnpackNormal(uint packedNormal)
        {
            Byte4 num = new Byte4 {
                PackedValue = packedNormal
            };
            return UnpackNormal(ref num);
        }

        public static Vector3 UnpackNormal(Byte4 packedNormal) => 
            UnpackNormal(ref packedNormal);

        public static unsafe Vector3 UnpackNormal(ref Byte4 packedNormal)
        {
            Vector4 vector = packedNormal.ToVector4();
            float local1 = (vector.Y > 127.5f) ? 1f : -1f;
            if (local1 > 0f)
            {
                float* singlePtr1 = (float*) ref vector.Y;
                singlePtr1[0] -= 128f;
            }
            float x = (2f * ((vector.X + (256f * vector.Y)) / 32767f)) - 1f;
            float y = (2f * ((vector.Z + (256f * vector.W)) / 32767f)) - 1f;
            return new Vector3(x, y, local1 * ((float) Math.Sqrt((double) Math.Max((float) 0f, (float) ((1f - (x * x)) - (y * y))))));
        }

        public static Vector3 UnpackPosition(ref HalfVector4 position) => 
            PositionPacker.UnpackPosition(ref position);

        public static Vector3 UnpackPosition(HalfVector4 position) => 
            PositionPacker.UnpackPosition(ref position);

        public static unsafe Vector4 UnpackTangentSign(ref Byte4 packedTangent)
        {
            Vector4 vector = packedTangent.ToVector4();
            float w = (vector.W > 127.5f) ? 1f : -1f;
            float local1 = (vector.Y > 127.5f) ? 1f : -1f;
            if (local1 > 0f)
            {
                float* singlePtr1 = (float*) ref vector.Y;
                singlePtr1[0] -= 128f;
            }
            if (w > 0f)
            {
                float* singlePtr2 = (float*) ref vector.W;
                singlePtr2[0] -= 128f;
            }
            float x = (2f * ((vector.X + (256f * vector.Y)) / 32767f)) - 1f;
            float y = (2f * ((vector.Z + (256f * vector.W)) / 32767f)) - 1f;
            return new Vector4(x, y, local1 * ((float) Math.Sqrt((double) Math.Max((float) 0f, (float) ((1f - (x * x)) - (y * y))))), w);
        }
    }
}

