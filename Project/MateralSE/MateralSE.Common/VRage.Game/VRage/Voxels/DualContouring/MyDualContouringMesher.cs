namespace VRage.Voxels.DualContouring
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Entities.Components;
    using VRage.Game.Voxels;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyDualContouringMesher : IMyIsoMesher
    {
        [ThreadStatic]
        private static MyDualContouringMesher m_threadInstance;
        public static bool Postprocess = true;
        private const int AFFECTED_RANGE_OFFSET = -1;
        private const int AFFECTED_RANGE_SIZE_CHANGE = 5;
        private MyStorageData m_storageData = new MyStorageData(MyStorageDataTypeFlags.All);
        private List<VrPostprocessing> m_postprocessing = new List<VrPostprocessing>();
        private int m_lastLod;
        private bool m_lastPhysics;
        private MyVoxelMesherComponent m_lastMesher;
        public static readonly int[] EdgeTable = new int[] { 
            0, 0x109, 0x203, 0x30a, 0x80c, 0x905, 0xa0f, 0xb06, 0x406, 0x50f, 0x605, 0x70c, 0xc0a, 0xd03, 0xe09, 0xf00,
            400, 0x99, 0x393, 0x29a, 0x99c, 0x895, 0xb9f, 0xa96, 0x596, 0x49f, 0x795, 0x69c, 0xd9a, 0xc93, 0xf99, 0xe90,
            560, 0x339, 0x33, 0x13a, 0xa3c, 0xb35, 0x83f, 0x936, 0x636, 0x73f, 0x435, 0x53c, 0xe3a, 0xf33, 0xc39, 0xd30,
            0x3a0, 0x2a9, 0x1a3, 170, 0xbac, 0xaa5, 0x9af, 0x8a6, 0x7a6, 0x6af, 0x5a5, 0x4ac, 0xfaa, 0xea3, 0xda9, 0xca0,
            0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc, 0x1c5, 0x2cf, 0x3c6, 0xcc6, 0xdcf, 0xec5, 0xfcc, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
            0x950, 0x859, 0xb53, 0xa5a, 0x15c, 0x55, 0x35f, 0x256, 0xd56, 0xc5f, 0xf55, 0xe5c, 0x55a, 0x453, 0x759, 0x650,
            0xaf0, 0xbf9, 0x8f3, 0x9fa, 0x2fc, 0x3f5, 0xff, 0x1f6, 0xef6, 0xfff, 0xcf5, 0xdfc, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
            0xb60, 0xa69, 0x963, 0x86a, 0x36c, 0x265, 0x16f, 0x66, 0xf66, 0xe6f, 0xd65, 0xc6c, 0x76a, 0x663, 0x569, 0x460,
            0x460, 0x569, 0x663, 0x76a, 0xc6c, 0xd65, 0xe6f, 0xf66, 0x66, 0x16f, 0x265, 0x36c, 0x86a, 0x963, 0xa69, 0xb60,
            0x5f0, 0x4f9, 0x7f3, 0x6fa, 0xdfc, 0xcf5, 0xfff, 0xef6, 0x1f6, 0xff, 0x3f5, 0x2fc, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
            0x650, 0x759, 0x453, 0x55a, 0xe5c, 0xf55, 0xc5f, 0xd56, 0x256, 0x35f, 0x55, 0x15c, 0xa5a, 0xb53, 0x859, 0x950,
            0x7c0, 0x6c9, 0x5c3, 0x4ca, 0xfcc, 0xec5, 0xdcf, 0xcc6, 0x3c6, 0x2cf, 0x1c5, 0xcc, 0xbca, 0xac3, 0x9c9, 0x8c0,
            0xca0, 0xda9, 0xea3, 0xfaa, 0x4ac, 0x5a5, 0x6af, 0x7a6, 0x8a6, 0x9af, 0xaa5, 0xbac, 170, 0x1a3, 0x2a9, 0x3a0,
            0xd30, 0xc39, 0xf33, 0xe3a, 0x53c, 0x435, 0x73f, 0x636, 0x936, 0x83f, 0xb35, 0xa3c, 0x13a, 0x33, 0x339, 560,
            0xe90, 0xf99, 0xc93, 0xd9a, 0x69c, 0x795, 0x49f, 0x596, 0xa96, 0xb9f, 0x895, 0x99c, 0x29a, 0x393, 0x99, 400,
            0xf00, 0xe09, 0xd03, 0xc0a, 0x70c, 0x605, 0x50f, 0x406, 0xb06, 0xa0f, 0x905, 0x80c, 0x30a, 0x203, 0x109, 0
        };

        public MyMesherResult Calculate(MyVoxelMesherComponent mesherComponent, int lod, Vector3I voxelStart, Vector3I voxelEnd, MyStorageDataTypeFlags properties = 3, MyVoxelRequestFlags flags = 0, VrVoxelMesh target = null)
        {
            bool physics = flags.HasFlags(MyVoxelRequestFlags.ForPhysics);
            if ((!ReferenceEquals(this.m_lastMesher, mesherComponent) || (this.m_lastLod != lod)) || (this.m_lastPhysics != physics))
            {
                this.m_lastLod = lod;
                this.m_lastMesher = mesherComponent;
                this.m_lastPhysics = physics;
                this.PreparePostprocessing(this.m_lastMesher.PostprocessingSteps, lod, physics);
            }
            return this.Calculate(mesherComponent.Storage, lod, voxelStart, voxelEnd, properties, flags | MyVoxelRequestFlags.Postprocess, target);
        }

        public unsafe MyMesherResult Calculate(IMyStorage storage, int lod, Vector3I voxelStart, Vector3I voxelEnd, MyStorageDataTypeFlags properties = 3, MyVoxelRequestFlags flags = 0, VrVoxelMesh target = null)
        {
            MyMesherResult empty;
            if (storage == null)
            {
                return MyMesherResult.Empty;
            }
            using (StoragePin pin = storage.Pin())
            {
                if (pin.Valid)
                {
                    MyVoxelRequestFlags requestFlags = flags | MyVoxelRequestFlags.UseNativeProvider;
                    this.m_storageData.Resize(voxelStart, voxelEnd);
                    storage.ReadRange(this.m_storageData, MyStorageDataTypeFlags.Content, lod, voxelStart, voxelEnd, ref requestFlags);
                    if (!requestFlags.HasFlags(MyVoxelRequestFlags.EmptyData))
                    {
                        if (!requestFlags.HasFlags(MyVoxelRequestFlags.FullContent))
                        {
                            VrVoxelMesh mesh;
                            byte[] pinned buffer;
                            byte[] pinned buffer2;
                            if (!requestFlags.HasFlags(MyVoxelRequestFlags.ContentChecked))
                            {
                                MyVoxelContentConstitution constitution = this.m_storageData.ComputeContentConstitution();
                                if (constitution != MyVoxelContentConstitution.Mixed)
                                {
                                    return new MyMesherResult(constitution);
                                }
                            }
                            if (properties.Requests(MyStorageDataTypeEnum.Material))
                            {
                                this.m_storageData.ClearMaterials(0xff);
                            }
                            if (target == null)
                            {
                                mesh = new VrVoxelMesh(voxelStart, voxelEnd, lod);
                            }
                            else
                            {
                                target.Clear();
                                mesh = target;
                            }
                            IsoMesher mesher = new IsoMesher(mesh);
                            try
                            {
                                byte* numPtr;
                                if (((buffer = this.m_storageData[MyStorageDataTypeEnum.Content]) == null) || (buffer.Length == 0))
                                {
                                    numPtr = null;
                                }
                                else
                                {
                                    numPtr = buffer;
                                }
                                try
                                {
                                    byte* numPtr2;
                                    if (((buffer2 = this.m_storageData[MyStorageDataTypeEnum.Material]) == null) || (buffer2.Length == 0))
                                    {
                                        numPtr2 = null;
                                    }
                                    else
                                    {
                                        numPtr2 = buffer2;
                                    }
                                    mesher.Calculate(this.m_storageData.Size3D.X, numPtr, numPtr2);
                                }
                                finally
                                {
                                    buffer2 = null;
                                }
                            }
                            finally
                            {
                                buffer = null;
                            }
                            if (properties.Requests(MyStorageDataTypeEnum.Material))
                            {
                                requestFlags = (flags & ~MyVoxelRequestFlags.SurfaceMaterial) | MyVoxelRequestFlags.ConsiderContent;
                                MyStorageDataTypeFlags dataToRead = properties.Without(MyStorageDataTypeEnum.Content);
                                storage.ReadRange(this.m_storageData, dataToRead, lod, voxelStart, voxelEnd, ref requestFlags);
                                try
                                {
                                    byte* numPtr3;
                                    if (((buffer = this.m_storageData[MyStorageDataTypeEnum.Content]) == null) || (buffer.Length == 0))
                                    {
                                        numPtr3 = null;
                                    }
                                    else
                                    {
                                        numPtr3 = buffer;
                                    }
                                    try
                                    {
                                        byte* numPtr4;
                                        if (((buffer2 = this.m_storageData[MyStorageDataTypeEnum.Material]) == null) || (buffer2.Length == 0))
                                        {
                                            numPtr4 = null;
                                        }
                                        else
                                        {
                                            numPtr4 = buffer2;
                                        }
                                        mesher.CalculateMaterials(this.m_storageData.Size3D.X, numPtr3, numPtr4, requestFlags.HasFlags(MyVoxelRequestFlags.OneMaterial) ? this.m_storageData.Material(0) : -1);
                                    }
                                    finally
                                    {
                                        buffer2 = null;
                                    }
                                }
                                finally
                                {
                                    buffer = null;
                                }
                            }
                            if (mesh.VertexCount == 0)
                            {
                                if (!ReferenceEquals(mesh, target))
                                {
                                    mesh.Dispose();
                                }
                                empty = MyMesherResult.Empty;
                            }
                            else
                            {
                                if (Postprocess && flags.HasFlags(MyVoxelRequestFlags.Postprocess))
                                {
                                    mesher.PostProcess(this.m_postprocessing);
                                }
                                if (Postprocess && (storage.DataProvider != null))
                                {
                                    storage.DataProvider.PostProcess(mesh, properties);
                                }
                                empty = new MyMesherResult(mesh);
                            }
                        }
                        else
                        {
                            empty = new MyMesherResult(MyVoxelContentConstitution.Full);
                        }
                    }
                    else
                    {
                        empty = new MyMesherResult(MyVoxelContentConstitution.Empty);
                    }
                }
                else
                {
                    empty = MyMesherResult.Empty;
                }
            }
            return empty;
        }

        public static void GenerateQuads(byte cubeMask, ushort[] corners, List<MyVoxelQuad> outQuads)
        {
            int num = EdgeTable[cubeMask];
            if (EdgeTable[cubeMask] != 0)
            {
                ushort num2 = corners[0];
                int num6 = 0;
                while (true)
                {
                    while (true)
                    {
                        ushort num3;
                        ushort num4;
                        ushort num5;
                        bool flag;
                        if (num6 >= 3)
                        {
                            return;
                        }
                        if ((num6 == 0) && ((num & 0x400) != 0))
                        {
                            num3 = corners[1];
                            num4 = corners[3];
                            num5 = corners[2];
                            flag = (cubeMask & 0x80) != 0;
                        }
                        else if ((num6 == 1) && ((num & 0x40) != 0))
                        {
                            num3 = corners[4];
                            num4 = corners[6];
                            num5 = corners[2];
                            flag = (cubeMask & 0x40) != 0;
                        }
                        else
                        {
                            if (num6 != 2)
                            {
                                break;
                            }
                            if ((num & 0x20) == 0)
                            {
                                break;
                            }
                            num3 = corners[1];
                            num4 = corners[5];
                            num5 = corners[4];
                            flag = (cubeMask & 0x20) != 0;
                        }
                        if (flag)
                        {
                            outQuads.Add(new MyVoxelQuad(num2, num3, num4, num5));
                        }
                        else
                        {
                            outQuads.Add(new MyVoxelQuad(num4, num3, num2, num5));
                        }
                        break;
                    }
                    num6++;
                }
            }
        }

        private void PreparePostprocessing(ListReader<MyVoxelPostprocessing> steps, int lod, bool physics)
        {
            this.m_postprocessing.Clear();
            foreach (MyVoxelPostprocessing postprocessing in steps)
            {
                VrPostprocessing postprocessing2;
                if ((postprocessing.UseForPhysics || !physics) && postprocessing.Get(lod, out postprocessing2))
                {
                    this.m_postprocessing.Add(postprocessing2);
                }
            }
        }

        MyIsoMesh IMyIsoMesher.Precalc(IMyStorage storage, int lod, Vector3I voxelStart, Vector3I voxelEnd, MyStorageDataTypeFlags properties, MyVoxelRequestFlags flags)
        {
            VrVoxelMesh nativeMesh = this.Calculate(storage, lod, voxelStart, voxelEnd, properties, flags, null).Mesh;
            MyIsoMesh mesh2 = MyIsoMesh.FromNative(nativeMesh);
            if (nativeMesh != null)
            {
                nativeMesh.Dispose();
            }
            return mesh2;
        }

        public static MyDualContouringMesher Static
        {
            get
            {
                if (m_threadInstance == null)
                {
                    VRageNative.SetDebugMode(true);
                    VRageNative.Log = delegate (string x) {
                        MyLog.Default.WriteLine(x);
                    };
                    m_threadInstance = new MyDualContouringMesher();
                }
                return m_threadInstance;
            }
        }

        public int AffectedRangeOffset =>
            -1;

        public int AffectedRangeSizeChange =>
            5;

        public int InvalidatedRangeInflate =>
            4;

        public MyStorageData StorageData =>
            this.m_storageData;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDualContouringMesher.<>c <>9 = new MyDualContouringMesher.<>c();
            public static Action<string> <>9__2_0;

            internal void <get_Static>b__2_0(string x)
            {
                MyLog.Default.WriteLine(x);
            }
        }
    }
}

