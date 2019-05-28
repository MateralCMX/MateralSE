namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyFourEdgeInfo
    {
        private static readonly int DirectionMax = (MyUtils.GetMaxValueFromEnum<Base27Directions.Direction>() + 1);
        public const int MaxInfoCount = 4;
        private Data m_data;

        public MyFourEdgeInfo(Vector4 localOrthoMatrix, MyCubeEdgeType edgeType)
        {
            this.m_data.LocalOrthoMatrix = localOrthoMatrix;
            this.m_data.EdgeType = edgeType;
        }

        public bool AddInstance(Vector3 blockPos, Color color, MyStringHash skinSubtype, MyStringHash edgeModel, Base27Directions.Direction normal0, Base27Directions.Direction normal1) => 
            this.m_data.Set(this.GetIndex(ref blockPos), color, skinSubtype, edgeModel, normal0, normal1);

        private int GetIndex(ref Vector3 blockPos)
        {
            Vector3 vector = blockPos - new Vector3(this.LocalOrthoMatrix);
            return ((Math.Abs(vector.X) >= 1E-05f) ? ((Math.Abs(vector.Y) >= 1E-05f) ? (((vector.X > 0f) ? 1 : 0) + ((vector.Y > 0f) ? 2 : 0)) : (((vector.X > 0f) ? 1 : 0) + ((vector.Z > 0f) ? 2 : 0))) : (((vector.Y > 0f) ? 1 : 0) + ((vector.Z > 0f) ? 2 : 0)));
        }

        public bool GetNormalInfo(int index, out Color color, out MyStringHash skinSubtypeId, out MyStringHash edgeModel, out Base27Directions.Direction normal0, out Base27Directions.Direction normal1)
        {
            this.m_data.Get(index, out color, out skinSubtypeId, out edgeModel, out normal0, out normal1);
            color.A = 0;
            return (((int) normal0) != 0);
        }

        public bool RemoveInstance(Vector3 blockPos) => 
            this.m_data.Reset(this.GetIndex(ref blockPos));

        public Vector4 LocalOrthoMatrix =>
            this.m_data.LocalOrthoMatrix;

        public MyCubeEdgeType EdgeType =>
            this.m_data.EdgeType;

        public bool Empty =>
            this.m_data.Empty;

        public bool Full =>
            this.m_data.Full;

        public int DebugCount =>
            this.m_data.Count;

        [StructLayout(LayoutKind.Sequential)]
        private struct Data
        {
            public Vector4 LocalOrthoMatrix;
            public MyCubeEdgeType EdgeType;
            [FixedBuffer(typeof(uint), 4)]
            private <m_data>e__FixedBuffer m_data;
            [FixedBuffer(typeof(byte), 4)]
            private <m_data2>e__FixedBuffer m_data2;
            [FixedBuffer(typeof(int), 4)]
            private <m_edgeModels>e__FixedBuffer m_edgeModels;
            [FixedBuffer(typeof(int), 4)]
            private <m_skinSubtypes>e__FixedBuffer m_skinSubtypes;
            public bool Full
            {
                get
                {
                    uint* numPtr = &this.m_data.FixedElementField;
                    return ((((numPtr[0] != 0) & (numPtr[1] != 0)) & (numPtr[2] != 0)) & (numPtr[3] != 0));
                }
            }
            public bool Empty
            {
                get
                {
                    uint* numPtr = &this.m_data.FixedElementField;
                    return ((*(((long*) numPtr)) == 0L) & (*(((long*) (numPtr + 2))) == 0L));
                }
            }
            public int Count
            {
                get
                {
                    uint* numPtr = &this.m_data.FixedElementField;
                    return (((((numPtr[0] != 0) ? 1 : 0) + ((numPtr[1] != 0) ? 1 : 0)) + ((numPtr[2] != 0) ? 1 : 0)) + ((numPtr[3] != 0) ? 1 : 0));
                }
            }
            public unsafe uint Get(int index) => 
                &this.m_data.FixedElementField[index];

            public unsafe void Get(int index, out Color color, out MyStringHash skinSubtypeId, out MyStringHash edgeModel, out Base27Directions.Direction normal0, out Base27Directions.Direction normal1)
            {
                uint* numPtr = &this.m_data.FixedElementField;
                byte* numPtr2 = &this.m_data2.FixedElementField;
                int* numPtr3 = &this.m_edgeModels.FixedElementField;
                int* numPtr4 = &this.m_skinSubtypes.FixedElementField;
                color = new Color(numPtr[index]);
                normal0 = (Base27Directions.Direction) color.A;
                normal1 = (Base27Directions.Direction) numPtr2[index];
                edgeModel = MyStringHash.TryGet(numPtr3[index]);
                skinSubtypeId = MyStringHash.TryGet(numPtr4[index]);
                fixed (int* numRef4 = null)
                {
                    fixed (int* numRef3 = null)
                    {
                        fixed (byte* numRef2 = null)
                        {
                            fixed (uint* numRef = null)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            public unsafe bool Set(int index, Color value, MyStringHash skinSubtype, MyStringHash edgeModel, Base27Directions.Direction normal0, Base27Directions.Direction normal1)
            {
                uint* numPtr = &this.m_data.FixedElementField;
                byte* numPtr2 = &this.m_data2.FixedElementField;
                int* numPtr3 = &this.m_skinSubtypes.FixedElementField;
                value.A = (byte) normal0;
                uint packedValue = value.PackedValue;
                bool flag = false;
                if (numPtr[index] != packedValue)
                {
                    flag = true;
                    numPtr[index] = packedValue;
                }
                numPtr2[index] = (byte) normal1;
                &this.m_edgeModels.FixedElementField[index] = (int) edgeModel;
                if (numPtr3[index] != ((int) skinSubtype))
                {
                    flag = true;
                    numPtr3[index] = (int) skinSubtype;
                }
                return flag;
            }

            public unsafe bool Reset(int index)
            {
                int* numPtr = &this.m_edgeModels.FixedElementField;
                int* numPtr2 = &this.m_skinSubtypes.FixedElementField;
                IntPtr ptr1 = (IntPtr) &this.m_data.FixedElementField;
                bool flag = ptr1[(int) (((IntPtr) index) * 4)] != IntPtr.Zero;
                ptr1[(int) (((IntPtr) index) * 4)] = IntPtr.Zero;
                numPtr[index] = 0;
                numPtr2[index] = 0;
                return flag;
            }
            [StructLayout(LayoutKind.Sequential, Size=0x10), CompilerGenerated, UnsafeValueType]
            public struct <m_data>e__FixedBuffer
            {
                public uint FixedElementField;
            }

            [StructLayout(LayoutKind.Sequential, Size=4), CompilerGenerated, UnsafeValueType]
            public struct <m_data2>e__FixedBuffer
            {
                public byte FixedElementField;
            }

            [StructLayout(LayoutKind.Sequential, Size=0x10), CompilerGenerated, UnsafeValueType]
            public struct <m_edgeModels>e__FixedBuffer
            {
                public int FixedElementField;
            }

            [StructLayout(LayoutKind.Sequential, Size=0x10), CompilerGenerated, UnsafeValueType]
            public struct <m_skinSubtypes>e__FixedBuffer
            {
                public int FixedElementField;
            }
        }
    }
}

