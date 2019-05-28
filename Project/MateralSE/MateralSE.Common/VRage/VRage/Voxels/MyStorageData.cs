namespace VRage.Voxels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using VRage.Utils;
    using VRageMath;

    [Serializable]
    public class MyStorageData
    {
        private byte[][] m_dataByType;
        private int m_sZ;
        private int m_sY;
        private Vector3I m_size3d;
        private int m_sizeLinear;
        private int m_dataSizeLinear;
        private MyStorageDataTypeFlags m_storedTypes;

        public MyStorageData(MyStorageDataTypeFlags typesToStore = 3)
        {
            this.m_dataSizeLinear = -1;
            this.m_storedTypes = typesToStore;
            this.m_dataByType = new byte[2][];
        }

        public MyStorageData(Vector3I size, byte[] content = null, byte[] material = null)
        {
            this.m_dataSizeLinear = -1;
            this.m_dataByType = new byte[2][];
            this.Resize(size);
            if (content != null)
            {
                this.m_storedTypes |= MyStorageDataTypeFlags.Content;
                this[MyStorageDataTypeEnum.Content] = content;
            }
            if (material != null)
            {
                this.m_storedTypes |= MyStorageDataTypeFlags.Material;
                this[MyStorageDataTypeEnum.Material] = material;
            }
        }

        [Conditional("DEBUG")]
        private void AssertPosition(ref Vector3I position)
        {
        }

        [Conditional("DEBUG")]
        private void AssertPosition(int x, int y, int z)
        {
        }

        public unsafe void BlockFill(MyStorageDataTypeEnum type, Vector3I min, Vector3I max, byte content)
        {
            Vector3I vectori;
            int* numPtr1 = (int*) ref min.Z;
            numPtr1[0] *= this.m_sZ;
            int* numPtr2 = (int*) ref max.Z;
            numPtr2[0] *= this.m_sZ;
            int* numPtr3 = (int*) ref min.Y;
            numPtr3[0] *= this.m_sY;
            int* numPtr4 = (int*) ref max.Y;
            numPtr4[0] *= this.m_sY;
            int* numPtr5 = (int*) ref min.X;
            numPtr5[0] = numPtr5[0];
            int* numPtr6 = (int*) ref max.X;
            numPtr6[0] = numPtr6[0];
            byte* numPtr = this[type];
            vectori.Z = min.Z;
            while (vectori.Z <= max.Z)
            {
                int z = vectori.Z;
                vectori.Y = min.Y;
                while (true)
                {
                    if (vectori.Y > max.Y)
                    {
                        int* numPtr9 = (int*) ref vectori.Z;
                        numPtr9[0] += this.m_sZ;
                        break;
                    }
                    int num2 = z + vectori.Y;
                    vectori.X = min.X;
                    while (true)
                    {
                        if (vectori.X > max.X)
                        {
                            int* numPtr8 = (int*) ref vectori.Y;
                            numPtr8[0] += this.m_sY;
                            break;
                        }
                        numPtr[vectori.X + num2] = content;
                        int* numPtr7 = (int*) ref vectori.X;
                        numPtr7[0]++;
                    }
                }
            }
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public void BlockFillContent(Vector3I min, Vector3I max, byte content)
        {
            this.BlockFill(MyStorageDataTypeEnum.Content, min, max, content);
        }

        public void BlockFillMaterial(Vector3I min, Vector3I max, byte materialIdx)
        {
            this.BlockFill(MyStorageDataTypeEnum.Material, min, max, materialIdx);
        }

        public unsafe void BlockFillMaterialConsiderContent(Vector3I min, Vector3I max, byte materialIdx)
        {
            Vector3I vectori;
            int* numPtr1 = (int*) ref min.Z;
            numPtr1[0] *= this.m_sZ;
            int* numPtr3 = (int*) ref max.Z;
            numPtr3[0] *= this.m_sZ;
            int* numPtr4 = (int*) ref min.Y;
            numPtr4[0] *= this.m_sY;
            int* numPtr5 = (int*) ref max.Y;
            numPtr5[0] *= this.m_sY;
            int* numPtr6 = (int*) ref min.X;
            numPtr6[0] = numPtr6[0];
            int* numPtr7 = (int*) ref max.X;
            numPtr7[0] = numPtr7[0];
            byte* numPtr = this[MyStorageDataTypeEnum.Content];
            byte* numPtr2 = this[MyStorageDataTypeEnum.Material];
            vectori.Z = min.Z;
            while (vectori.Z <= max.Z)
            {
                int z = vectori.Z;
                vectori.Y = min.Y;
                while (true)
                {
                    if (vectori.Y > max.Y)
                    {
                        int* numPtr10 = (int*) ref vectori.Z;
                        numPtr10[0] += this.m_sZ;
                        break;
                    }
                    int num2 = z + vectori.Y;
                    vectori.X = min.X;
                    while (true)
                    {
                        if (vectori.X > max.X)
                        {
                            int* numPtr9 = (int*) ref vectori.Y;
                            numPtr9[0] += this.m_sY;
                            break;
                        }
                        numPtr2[vectori.X + num2] = (numPtr[vectori.X + num2] != 0) ? materialIdx : ((byte) 0xff);
                        int* numPtr8 = (int*) ref vectori.X;
                        numPtr8[0]++;
                    }
                }
            }
            fixed (byte* numRef2 = null)
            {
                fixed (byte* numRef = null)
                {
                    return;
                }
            }
        }

        public unsafe void Clear(MyStorageDataTypeEnum type, byte p)
        {
            byte* numPtr;
            byte[] pinned buffer;
            if (((buffer = this[type]) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            for (int i = 0; i < this.m_sizeLinear; i++)
            {
                numPtr[i] = p;
            }
            buffer = null;
        }

        public void ClearContent(byte p)
        {
            this.Clear(MyStorageDataTypeEnum.Content, p);
        }

        public void ClearMaterials(byte p)
        {
            this.Clear(MyStorageDataTypeEnum.Material, p);
        }

        public MyVoxelContentConstitution ComputeContentConstitution()
        {
            MyVoxelContentConstitution mixed;
            try
            {
                byte[] buffer = this[MyStorageDataTypeEnum.Content];
                bool flag = buffer[0] < 0x7f;
                int index = 1;
                while (true)
                {
                    if (index >= this.m_sizeLinear)
                    {
                        mixed = flag ? MyVoxelContentConstitution.Empty : MyVoxelContentConstitution.Full;
                    }
                    else
                    {
                        bool flag2 = buffer[index] < 0x7f;
                        if (flag == flag2)
                        {
                            index += this.StepLinear;
                            continue;
                        }
                        mixed = MyVoxelContentConstitution.Mixed;
                    }
                    break;
                }
            }
            finally
            {
            }
            return mixed;
        }

        public int ComputeLinear(ref Vector3I p) => 
            ((p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ));

        public void ComputePosition(int linear, out Vector3I p)
        {
            int x = linear % this.m_sY;
            int y = ((linear - x) % this.m_sZ) / this.m_sY;
            p = new Vector3I(x, y, ((linear - x) - (y * this.m_sY)) / this.m_sZ);
        }

        public bool ContainsIsoSurface()
        {
            bool flag3;
            try
            {
                byte[] buffer = this[MyStorageDataTypeEnum.Content];
                bool flag = buffer[0] < 0x7f;
                int index = 1;
                while (true)
                {
                    if (index >= this.m_sizeLinear)
                    {
                        flag3 = false;
                    }
                    else
                    {
                        bool flag2 = buffer[index] < 0x7f;
                        if (flag == flag2)
                        {
                            index += this.StepLinear;
                            continue;
                        }
                        flag3 = true;
                    }
                    break;
                }
            }
            finally
            {
            }
            return flag3;
        }

        public bool ContainsVoxelsAboveIsoLevel()
        {
            bool flag;
            byte[] buffer = this[MyStorageDataTypeEnum.Content];
            try
            {
                int index = 0;
                while (true)
                {
                    if (index >= this.m_sizeLinear)
                    {
                        flag = false;
                    }
                    else
                    {
                        if (buffer[index] <= 0x7f)
                        {
                            index += this.StepLinear;
                            continue;
                        }
                        flag = true;
                    }
                    break;
                }
            }
            finally
            {
            }
            return flag;
        }

        public byte Content(ref Vector3I p) => 
            this[MyStorageDataTypeEnum.Content][(p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ)];

        public byte Content(int linearIdx) => 
            this[MyStorageDataTypeEnum.Content][linearIdx];

        public void Content(ref Vector3I p, byte content)
        {
            this[MyStorageDataTypeEnum.Content][(p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ)] = content;
        }

        public void Content(int linearIdx, byte content)
        {
            this[MyStorageDataTypeEnum.Content][linearIdx] = content;
        }

        public byte Content(int x, int y, int z) => 
            this[MyStorageDataTypeEnum.Content][(x + (y * this.m_sY)) + (z * this.m_sZ)];

        public void CopyRange(MyStorageData src, Vector3I min, Vector3I max, Vector3I offset, MyStorageDataTypeEnum dataType)
        {
            this.OpRange<CopyOperator>(src, min, max, offset, dataType);
        }

        public static MyStorageData FromBase64(string str)
        {
            MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(str));
            return (MyStorageData) new BinaryFormatter().Deserialize(serializationStream);
        }

        public byte Get(MyStorageDataTypeEnum type, ref Vector3I p) => 
            this[type][(p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ)];

        public byte Get(MyStorageDataTypeEnum type, int linearIdx) => 
            this[type][linearIdx];

        public byte Get(MyStorageDataTypeEnum type, int x, int y, int z) => 
            this[type][(x + (y * this.m_sY)) + (z * this.m_sZ)];

        public byte Material(ref Vector3I p) => 
            this[MyStorageDataTypeEnum.Material][(p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ)];

        public byte Material(int linearIdx) => 
            this[MyStorageDataTypeEnum.Material][linearIdx];

        public void Material(ref Vector3I p, byte materialIdx)
        {
            this[MyStorageDataTypeEnum.Material][(p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ)] = materialIdx;
        }

        public void Material(int linearIdx, byte materialIdx)
        {
            this[MyStorageDataTypeEnum.Material][linearIdx] = materialIdx;
        }

        public void OpRange<Op>(MyStorageData src, Vector3I min, Vector3I max, Vector3I offset, MyStorageDataTypeEnum dataType) where Op: struct, IOperator
        {
            byte[] buffer = this[dataType];
            Vector3I step = this.Step;
            Vector3I vectori2 = src.Step;
            byte[] buffer2 = src[dataType];
            min *= vectori2;
            max *= vectori2;
            offset *= step;
            Op local = default(Op);
            int z = min.Z;
            int x = offset.X;
            while (z <= max.Z)
            {
                int y = min.Y;
                int num5 = offset.Y;
                while (true)
                {
                    if (y > max.Y)
                    {
                        z += vectori2.Z;
                        x += step.Z;
                        break;
                    }
                    int num7 = y + z;
                    int num8 = num5 + x;
                    int num = min.X;
                    int num4 = offset.Z;
                    while (true)
                    {
                        if (num > max.X)
                        {
                            y += vectori2.Y;
                            num5 += step.Y;
                            break;
                        }
                        local.Op(ref buffer[num4 + num8], buffer2[num + num7]);
                        num += vectori2.X;
                        num4 += step.X;
                    }
                }
            }
        }

        public void Resize(Vector3I size3D)
        {
            this.m_size3d = size3D;
            int size = size3D.Size;
            this.m_sY = size3D.X;
            this.m_sZ = size3D.Y * this.m_sY;
            this.m_sizeLinear = size * this.StepLinear;
            for (int i = 0; i < this.m_dataByType.Length; i++)
            {
                if (((this.m_dataByType[i] == null) || (this.m_dataByType[i].Length < this.m_sizeLinear)) && this.m_storedTypes.Requests(((MyStorageDataTypeEnum) i)))
                {
                    this.m_dataByType[i] = new byte[this.m_sizeLinear];
                }
            }
        }

        public void Resize(Vector3I start, Vector3I end)
        {
            this.Resize((Vector3I) ((end - start) + 1));
        }

        public void Set(MyStorageDataTypeEnum type, ref Vector3I p, byte value)
        {
            this[type][(p.X + (p.Y * this.m_sY)) + (p.Z * this.m_sZ)] = value;
        }

        public string ToBase64()
        {
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, this);
            return Convert.ToBase64String(serializationStream.GetBuffer());
        }

        public int ValueWhenAllEqual(MyStorageDataTypeEnum dataType)
        {
            byte[] buffer = this[dataType];
            byte num = buffer[0];
            for (int i = 1; i < this.m_sizeLinear; i += this.StepLinear)
            {
                if (num != buffer[i])
                {
                    return -1;
                }
            }
            return num;
        }

        public bool WrinkleVoxelContent(ref Vector3I p, float wrinkleWeightAdd, float wrinkleWeightRemove)
        {
            int num = -2147483648;
            int num2 = 0x7fffffff;
            int num3 = (int) (wrinkleWeightAdd * 255f);
            int num4 = (int) (wrinkleWeightRemove * 255f);
            for (int i = -1; i <= 1; i++)
            {
                Vector3I vectori;
                vectori.Z = i + p.Z;
                if (vectori.Z < this.m_size3d.Z)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        vectori.Y = j + p.Y;
                        if (vectori.Y < this.m_size3d.Y)
                        {
                            for (int k = -1; k <= 1; k++)
                            {
                                vectori.X = k + p.X;
                                if (vectori.X < this.m_size3d.X)
                                {
                                    byte num10 = this.Content(ref vectori);
                                    num = Math.Max(num, num10);
                                    num2 = Math.Min(num2, num10);
                                }
                            }
                        }
                    }
                }
            }
            if (num2 == num)
            {
                return false;
            }
            int num5 = this.Content(ref p);
            byte content = (byte) MyUtils.GetClampInt((num5 + MyUtils.GetRandomInt(num3 + num4)) - num4, num2, num);
            if (content == num5)
            {
                return false;
            }
            this.Content(ref p, content);
            return true;
        }

        public byte[] this[MyStorageDataTypeEnum type]
        {
            get => 
                this.m_dataByType[(int) type];
            set
            {
                if (this.m_dataSizeLinear == -1)
                {
                    this.m_dataSizeLinear = value.Length;
                }
                this.m_dataByType[(int) type] = value;
            }
        }

        public int SizeLinear =>
            this.m_sizeLinear;

        public int StepLinear =>
            1;

        public int StepX =>
            1;

        public int StepY =>
            this.m_sY;

        public int StepZ =>
            this.m_sZ;

        public Vector3I Step =>
            new Vector3I(1, this.m_sY, this.m_sZ);

        public Vector3I Size3D =>
            this.m_size3d;

        [StructLayout(LayoutKind.Sequential, Size=1)]
        public struct CopyOperator : MyStorageData.IOperator
        {
            public void Op(ref byte target, byte source)
            {
                target = source;
            }
        }

        public interface IOperator
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void Op(ref byte target, byte source);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MortonEnumerator : IEnumerator<byte>, IDisposable, IEnumerator
        {
            private MyStorageDataTypeEnum m_type;
            private MyStorageData m_source;
            private int m_maxMortonCode;
            private int m_mortonCode;
            private Vector3I m_pos;
            private byte m_current;
            public MortonEnumerator(MyStorageData source, MyStorageDataTypeEnum type)
            {
                this.m_type = type;
                this.m_source = source;
                this.m_maxMortonCode = source.Size3D.Size;
                this.m_mortonCode = -1;
                this.m_pos = new Vector3I();
                this.m_current = 0;
            }

            public byte Current =>
                this.m_current;
            public void Dispose()
            {
            }

            object IEnumerator.Current =>
                this.m_current;
            public bool MoveNext()
            {
                this.m_mortonCode++;
                if (this.m_mortonCode >= this.m_maxMortonCode)
                {
                    return false;
                }
                MyMortonCode3D.Decode(this.m_mortonCode, out this.m_pos);
                this.m_current = this.m_source.Get(this.m_type, ref this.m_pos);
                return true;
            }

            public void Reset()
            {
                this.m_mortonCode = -1;
                this.m_current = 0;
            }
        }
    }
}

