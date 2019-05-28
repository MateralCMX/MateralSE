namespace VRage
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCubeInstanceData
    {
        [FixedBuffer(typeof(byte), 0x20)]
        private <m_bones>e__FixedBuffer m_bones;
        public Vector4 m_translationAndRot;
        public Vector4 ColorMaskHSV;
        public Matrix ConstructDeformedCubeInstanceMatrix(ref Vector4UByte boneIndices, ref Vector4 boneWeights, out Matrix localMatrix)
        {
            localMatrix = this.LocalMatrix;
            Matrix matrix = localMatrix;
            if (this.EnableSkinning)
            {
                matrix.Translation += this.ComputeBoneOffset(ref boneIndices, ref boneWeights);
            }
            return matrix;
        }

        public Vector3 ComputeBoneOffset(ref Vector4UByte boneIndices, ref Vector4 boneWeights)
        {
            Matrix matrix = new Matrix();
            matrix.SetRow(0, this.GetNormalizedBone(boneIndices[0]));
            matrix.SetRow(1, this.GetNormalizedBone(boneIndices[1]));
            matrix.SetRow(2, this.GetNormalizedBone(boneIndices[2]));
            matrix.SetRow(3, this.GetNormalizedBone(boneIndices[3]));
            return this.Denormalize(Vector4.Transform(boneWeights, matrix), this.BoneRange);
        }

        public unsafe void RetrieveBones(byte* bones)
        {
            byte* numPtr = &this.m_bones.FixedElementField;
            for (int i = 0; i < 0x20; i++)
            {
                bones[i] = numPtr[i];
            }
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public Matrix LocalMatrix
        {
            get
            {
                Matrix matrix;
                Vector4.UnpackOrthoMatrix(ref this.m_translationAndRot, out matrix);
                return matrix;
            }
            set => 
                (this.m_translationAndRot = Vector4.PackOrthoMatrix(ref value));
        }
        public Vector3 Translation =>
            new Vector3(this.m_translationAndRot);
        public Vector4 PackedOrthoMatrix
        {
            get => 
                this.m_translationAndRot;
            set => 
                (this.m_translationAndRot = value);
        }
        public unsafe void ResetBones()
        {
            ulong* numPtr = (ulong*) &this.m_bones.FixedElementField;
            numPtr[0] = 9259542123273814144UL;
            numPtr[1] = 0x80808080808080L;
            numPtr[2] = 9259542123273814144UL;
            numPtr[3] = 0x80808080808080L;
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public unsafe void SetTextureOffset(Vector4UByte patternOffset)
        {
            IntPtr ptr1 = (IntPtr) &this.m_bones.FixedElementField;
            (ptr1 + (((IntPtr) 5) * sizeof(Vector4UByte))).W = patternOffset.X;
            (ptr1 + (((IntPtr) 6) * sizeof(Vector4UByte))).W = patternOffset.Y;
            (ptr1 + (((IntPtr) 7) * sizeof(Vector4UByte))).W = (byte) ((patternOffset.W - 1) | ((patternOffset.Z - 1) << 4));
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public unsafe float GetTextureOffset(int index)
        {
            IntPtr ptr1 = (IntPtr) &this.m_bones.FixedElementField;
            int num = (ptr1 + (((IntPtr) (5 + index)) * sizeof(Vector4UByte))).W & 15;
            int num2 = ((ptr1 + (((IntPtr) (5 + index)) * sizeof(Vector4UByte))).W >> 4) & 0x10;
            return ((num2 == 0) ? ((float) 0) : ((float) (num / num2)));
        }

        public float BoneRange
        {
            get => 
                (((float) &this.m_bones.FixedElementField[4 * sizeof(Vector4UByte)].W) / 10f);
            set
            {
                &this.m_bones.FixedElementField[4 * sizeof(Vector4UByte)].W = (byte) (value * 10f);
                fixed (byte* numRef = null)
                {
                    return;
                }
            }
        }
        public bool EnableSkinning
        {
            get => 
                ((&this.m_bones.FixedElementField[3 * sizeof(Vector4UByte)].W & 1) > 0);
            set
            {
                byte* numPtr = &this.m_bones.FixedElementField;
                if (value)
                {
                    byte* numPtr1 = (byte*) ref numPtr[3 * sizeof(Vector4UByte)].W;
                    numPtr1[0] = (byte) (numPtr1[0] | 1);
                }
                else
                {
                    byte* numPtr2 = (byte*) ref numPtr[3 * sizeof(Vector4UByte)].W;
                    numPtr2[0] = (byte) (numPtr2[0] & 0xfe);
                }
                fixed (byte* numRef = null)
                {
                    return;
                }
            }
        }
        public unsafe void SetColorMaskHSV(Vector4 colorMaskHSV)
        {
            this.ColorMaskHSV = colorMaskHSV;
            byte* numPtr = &this.m_bones.FixedElementField;
            if (colorMaskHSV.W < 0f)
            {
                byte* numPtr1 = (byte*) ref numPtr[3 * sizeof(Vector4UByte)].W;
                numPtr1[0] = (byte) (numPtr1[0] | 2);
            }
            else
            {
                byte* numPtr2 = (byte*) ref numPtr[3 * sizeof(Vector4UByte)].W;
                numPtr2[0] = (byte) (numPtr2[0] & 0xfd);
            }
            fixed (byte* numRef = null)
            {
                this.ColorMaskHSV.W = Math.Abs(this.ColorMaskHSV.W);
                return;
            }
        }

        public Vector3UByte this[int index]
        {
            get
            {
                byte* numPtr = &this.m_bones.FixedElementField;
                return ((index != 8) ? ((Vector3UByte) numPtr[index * sizeof(Vector4UByte)]) : new Vector3UByte(numPtr->W, numPtr[sizeof(Vector4UByte)].W, numPtr[2 * sizeof(Vector4UByte)].W));
            }
            set
            {
                byte* numPtr = &this.m_bones.FixedElementField;
                if (index != 8)
                {
                    numPtr[index * sizeof(Vector4UByte)] = (byte) value;
                }
                else
                {
                    numPtr->W = value.X;
                    numPtr[sizeof(Vector4UByte)].W = value.Y;
                    numPtr[2 * sizeof(Vector4UByte)].W = value.Z;
                }
                fixed (byte* numRef = null)
                {
                    return;
                }
            }
        }
        public Vector3 GetDenormalizedBone(int index) => 
            this.Denormalize(this.GetNormalizedBone(index), this.BoneRange);

        public unsafe Vector4UByte GetPackedBone(int index)
        {
            byte* numPtr = &this.m_bones.FixedElementField;
            return ((index != 8) ? ((Vector4UByte) numPtr[index * sizeof(Vector4UByte)]) : new Vector4UByte(numPtr->W, numPtr[sizeof(Vector4UByte)].W, numPtr[2 * sizeof(Vector4UByte)].W, 0));
        }

        private Vector4 GetNormalizedBone(int index)
        {
            Vector4UByte packedBone = this.GetPackedBone(index);
            return (new Vector4((float) packedBone.X, (float) packedBone.Y, (float) packedBone.Z, (float) packedBone.W) / 255f);
        }

        private Vector3 Denormalize(Vector4 position, float range) => 
            ((((new Vector3(position) + 0.001960784f) - 0.5f) * range) * 2f);
        [StructLayout(LayoutKind.Sequential, Size=0x20), CompilerGenerated, UnsafeValueType]
        public struct <m_bones>e__FixedBuffer
        {
            public byte FixedElementField;
        }
    }
}

