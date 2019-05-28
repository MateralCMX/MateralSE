namespace Sandbox.Game.Entities.EnvironmentItems
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender;
    using VRageRender.Import;
    using VRageRender.Messages;

    public class MyEnvironmentSector
    {
        private readonly Vector3I m_id;
        private MatrixD m_sectorMatrix;
        private MatrixD m_sectorInvMatrix;
        private FastResourceLock m_instancePartsLock = new FastResourceLock();
        private Dictionary<int, MyModelInstanceData> m_instanceParts = new Dictionary<int, MyModelInstanceData>();
        private List<MyInstanceData> m_tmpInstanceData = new List<MyInstanceData>();
        private BoundingBox m_AABB = BoundingBox.CreateInvalid();
        private bool m_invalidateAABB;
        private int m_sectorItemCount;

        public MyEnvironmentSector(Vector3I id, Vector3D sectorOffset)
        {
            this.m_id = id;
            this.m_sectorMatrix = MatrixD.CreateTranslation(sectorOffset);
            this.m_sectorInvMatrix = MatrixD.Invert(this.m_sectorMatrix);
        }

        public int AddInstance(MyStringHash subtypeId, int modelId, int localId, ref Matrix localMatrix, BoundingBox localAabb, MyInstanceFlagsEnum instanceFlags, float maxViewDistance, Vector3 colorMaskHsv = new Vector3())
        {
            MyModelInstanceData data;
            using (this.m_instancePartsLock.AcquireExclusiveUsing())
            {
                if (!this.m_instanceParts.TryGetValue(modelId, out data))
                {
                    data = new MyModelInstanceData(this, subtypeId, modelId, instanceFlags, maxViewDistance, localAabb);
                    this.m_instanceParts.Add(modelId, data);
                }
            }
            MySectorInstanceData data3 = new MySectorInstanceData {
                LocalId = localId
            };
            MyInstanceData data4 = new MyInstanceData {
                ColorMaskHSV = new HalfVector4(colorMaskHsv.X, colorMaskHsv.Y, colorMaskHsv.Z, 0f),
                LocalMatrix = localMatrix
            };
            data3.InstanceData = data4;
            MySectorInstanceData instanceData = data3;
            int num = data.AddInstanceData(ref instanceData);
            localMatrix = data.InstanceData[num].LocalMatrix;
            this.m_AABB = this.m_AABB.Include(localAabb.Transform((Matrix) localMatrix));
            this.m_sectorItemCount++;
            this.m_invalidateAABB = true;
            return num;
        }

        public void ClearInstanceData()
        {
            this.m_tmpInstanceData.Clear();
            this.m_AABB = BoundingBox.CreateInvalid();
            this.m_sectorItemCount = 0;
            using (this.m_instancePartsLock.AcquireExclusiveUsing())
            {
                foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
                {
                    pair.Value.InstanceData.Clear();
                }
            }
        }

        internal void DebugDraw(Vector3I sectorPos, float sectorSize)
        {
            using (this.m_instancePartsLock.AcquireSharedUsing())
            {
                foreach (MyModelInstanceData data in this.m_instanceParts.Values)
                {
                    using (data.InstanceBufferLock.AcquireSharedUsing())
                    {
                        foreach (KeyValuePair<int, MyInstanceData> pair in data.InstanceData)
                        {
                            MyInstanceData data2 = pair.Value;
                            Matrix localMatrix = data2.LocalMatrix;
                            Vector3D vectord = Vector3D.Transform(localMatrix.Translation, this.m_sectorMatrix);
                            BoundingBox modelBox = data.ModelBox;
                            MyRenderProxy.DebugDrawOBB(Matrix.Rescale(data2.LocalMatrix * this.m_sectorMatrix, modelBox.HalfExtents * 2f), Color.OrangeRed, 0.5f, true, true, true, false);
                            if (Vector3D.Distance(MySector.MainCamera.Position, vectord) < 250.0)
                            {
                                MyRenderProxy.DebugDrawText3D(vectord, data.SubtypeId.ToString(), Color.White, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                            }
                        }
                    }
                }
            }
            MyRenderProxy.DebugDrawAABB(this.SectorWorldBox, Color.OrangeRed, 1f, 1f, true, false, false);
        }

        public bool DisableInstance(int sectorInstanceId, int modelId)
        {
            MyModelInstanceData data = null;
            this.m_instanceParts.TryGetValue(modelId, out data);
            if (data == null)
            {
                return false;
            }
            if (!data.DisableInstance(sectorInstanceId))
            {
                return false;
            }
            this.m_sectorItemCount--;
            this.m_invalidateAABB = true;
            return true;
        }

        internal void GetItems(List<MyEnvironmentItems.ItemInfo> output)
        {
            foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
            {
                MyModelInstanceData data = pair.Value;
                using (data.InstanceBufferLock.AcquireSharedUsing())
                {
                    foreach (KeyValuePair<int, MyInstanceData> pair2 in data.InstanceData)
                    {
                        MyInstanceData data2 = pair2.Value;
                        Matrix localMatrix = data2.LocalMatrix;
                        if (!localMatrix.EqualsFast(ref Matrix.Zero, 0.0001f))
                        {
                            MyEnvironmentItems.ItemInfo item = new MyEnvironmentItems.ItemInfo {
                                LocalId = data.InstanceIds[pair2.Key],
                                SubtypeId = pair.Value.SubtypeId,
                                Transform = new MyTransformD(Vector3.Transform(localMatrix.Translation, this.m_sectorMatrix))
                            };
                            output.Add(item);
                        }
                    }
                }
            }
        }

        internal void GetItems(List<Vector3D> output)
        {
            foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
            {
                MyModelInstanceData data = pair.Value;
                using (data.InstanceBufferLock.AcquireSharedUsing())
                {
                    foreach (KeyValuePair<int, MyInstanceData> pair2 in data.InstanceData)
                    {
                        MyInstanceData data2 = pair2.Value;
                        Matrix localMatrix = data2.LocalMatrix;
                        if (!localMatrix.EqualsFast(ref Matrix.Zero, 0.0001f))
                        {
                            output.Add(Vector3D.Transform(data2.LocalMatrix.Translation, this.m_sectorMatrix));
                        }
                    }
                }
            }
        }

        internal void GetItemsInRadius(Vector3 position, float radius, List<MyEnvironmentItems.ItemInfo> output)
        {
            double num = radius * radius;
            foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
            {
                MyModelInstanceData data = pair.Value;
                using (data.InstanceBufferLock.AcquireSharedUsing())
                {
                    foreach (KeyValuePair<int, MyInstanceData> pair2 in data.InstanceData)
                    {
                        MyInstanceData data2 = pair2.Value;
                        Matrix localMatrix = data2.LocalMatrix;
                        if (!localMatrix.EqualsFast(ref Matrix.Zero, 0.0001f))
                        {
                            Vector3D vectord = Vector3.Transform(data2.LocalMatrix.Translation, this.m_sectorMatrix);
                            if ((vectord - position).LengthSquared() < num)
                            {
                                MyEnvironmentItems.ItemInfo item = new MyEnvironmentItems.ItemInfo {
                                    LocalId = data.InstanceIds[pair2.Key],
                                    SubtypeId = pair.Value.SubtypeId,
                                    Transform = new MyTransformD(vectord)
                                };
                                output.Add(item);
                            }
                        }
                    }
                }
            }
        }

        internal void GetItemsInRadius(Vector3D position, float radius, List<Vector3D> output)
        {
            Vector3D vectord = Vector3D.Transform(position, this.m_sectorInvMatrix);
            foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
            {
                using (pair.Value.InstanceBufferLock.AcquireSharedUsing())
                {
                    foreach (KeyValuePair<int, MyInstanceData> pair2 in pair.Value.InstanceData)
                    {
                        MyInstanceData data = pair2.Value;
                        Matrix localMatrix = data.LocalMatrix;
                        if (Vector3D.DistanceSquared(localMatrix.Translation, vectord) < (radius * radius))
                        {
                            output.Add(Vector3D.Transform(pair2.Value.LocalMatrix.Translation, this.m_sectorMatrix));
                        }
                    }
                }
            }
        }

        private BoundingBox GetSectorBoundingBox()
        {
            if (!this.IsValid)
            {
                return new BoundingBox(Vector3.Zero, Vector3.Zero);
            }
            BoundingBox box = BoundingBox.CreateInvalid();
            using (this.m_instancePartsLock.AcquireSharedUsing())
            {
                foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
                {
                    MyModelInstanceData data = pair.Value;
                    using (data.InstanceBufferLock.AcquireSharedUsing())
                    {
                        BoundingBox modelBox = data.ModelBox;
                        foreach (KeyValuePair<int, MyInstanceData> pair2 in data.InstanceData)
                        {
                            MyInstanceData data2 = pair2.Value;
                            Matrix localMatrix = data2.LocalMatrix;
                            if (!localMatrix.EqualsFast(ref Matrix.Zero, 0.0001f))
                            {
                                box.Include(modelBox.Transform(data2.LocalMatrix));
                            }
                        }
                    }
                }
            }
            return box;
        }

        public static Vector3I GetSectorId(Vector3D position, float sectorSize) => 
            Vector3I.Floor(position / ((double) sectorSize));

        public void UnloadRenderObjects()
        {
            using (this.m_instancePartsLock.AcquireExclusiveUsing())
            {
                foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
                {
                    pair.Value.UnloadRenderObjects();
                }
            }
        }

        public void UpdateRenderEntitiesData(MatrixD worldMatrixD, bool useTransparency = false, float transparency = 0f)
        {
            using (Dictionary<int, MyModelInstanceData>.ValueCollection.Enumerator enumerator = this.m_instanceParts.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateRenderEntitiesData(ref worldMatrixD, useTransparency, transparency);
                }
            }
        }

        public void UpdateRenderInstanceData()
        {
            using (this.m_instancePartsLock.AcquireSharedUsing())
            {
                foreach (KeyValuePair<int, MyModelInstanceData> pair in this.m_instanceParts)
                {
                    pair.Value.UpdateRenderInstanceData();
                }
            }
        }

        public void UpdateRenderInstanceData(int modelId)
        {
            using (this.m_instancePartsLock.AcquireSharedUsing())
            {
                MyModelInstanceData data = null;
                this.m_instanceParts.TryGetValue(modelId, out data);
                if (data != null)
                {
                    data.UpdateRenderInstanceData();
                }
            }
        }

        public Vector3I SectorId =>
            this.m_id;

        public MatrixD SectorMatrix =>
            this.m_sectorMatrix;

        public bool IsValid =>
            (this.m_sectorItemCount > 0);

        public BoundingBox SectorBox
        {
            get
            {
                if (this.m_invalidateAABB)
                {
                    this.m_invalidateAABB = false;
                    this.m_AABB = this.GetSectorBoundingBox();
                }
                return this.m_AABB;
            }
        }

        public BoundingBoxD SectorWorldBox =>
            this.SectorBox.Transform(this.m_sectorMatrix);

        public int SectorItemCount =>
            this.m_sectorItemCount;

        private class MyModelInstanceData
        {
            public MyEnvironmentSector Parent;
            public int Model;
            public readonly MyStringHash SubtypeId;
            public readonly MyInstanceFlagsEnum Flags = (MyInstanceFlagsEnum.CastShadows | MyInstanceFlagsEnum.EnableColorMask | MyInstanceFlagsEnum.ShowLod1);
            public readonly float MaxViewDistance = float.MaxValue;
            public readonly Dictionary<int, MyInstanceData> InstanceData = new Dictionary<int, MyInstanceData>();
            public readonly Dictionary<int, int> InstanceIds = new Dictionary<int, int>();
            private int m_keyIndex;
            public readonly BoundingBox ModelBox;
            public uint InstanceBuffer = uint.MaxValue;
            public uint RenderObjectId = uint.MaxValue;
            public FastResourceLock InstanceBufferLock = new FastResourceLock();
            private bool m_changed;

            public MyModelInstanceData(MyEnvironmentSector parent, MyStringHash subtypeId, int model, MyInstanceFlagsEnum flags, float maxViewDistance, BoundingBox modelBox)
            {
                this.Parent = parent;
                this.SubtypeId = subtypeId;
                this.Flags = flags;
                this.MaxViewDistance = maxViewDistance;
                this.ModelBox = modelBox;
                this.Model = model;
            }

            public int AddInstanceData(ref MyEnvironmentSector.MySectorInstanceData instanceData)
            {
                int keyIndex;
                using (this.InstanceBufferLock.AcquireExclusiveUsing())
                {
                    while (true)
                    {
                        if (!this.InstanceData.ContainsKey(this.m_keyIndex) || (this.InstanceData.Count >= 0x7fffffff))
                        {
                            if (this.InstanceData.ContainsKey(this.m_keyIndex))
                            {
                                throw new Exception("No available keys to add new instance data to sector!");
                            }
                            this.InstanceData.Add(this.m_keyIndex, instanceData.InstanceData);
                            this.InstanceIds.Add(this.m_keyIndex, instanceData.LocalId);
                            keyIndex = this.m_keyIndex;
                            break;
                        }
                        this.m_keyIndex++;
                    }
                }
                return keyIndex;
            }

            public bool DisableInstance(int sectorInstanceId)
            {
                using (this.InstanceBufferLock.AcquireExclusiveUsing())
                {
                    if (this.InstanceData.ContainsKey(sectorInstanceId))
                    {
                        this.InstanceData.Remove(sectorInstanceId);
                        this.InstanceIds.Remove(sectorInstanceId);
                    }
                    else
                    {
                        bool flag1 = MyFakes.ENABLE_FLORA_COMPONENT_DEBUG;
                        return false;
                    }
                }
                return true;
            }

            public void UnloadRenderObjects()
            {
                if (this.InstanceBuffer != uint.MaxValue)
                {
                    MyRenderProxy.RemoveRenderObject(this.InstanceBuffer, MyRenderProxy.ObjectType.InstanceBuffer, false);
                    this.InstanceBuffer = uint.MaxValue;
                }
                if (this.RenderObjectId != uint.MaxValue)
                {
                    MyRenderProxy.RemoveRenderObject(this.RenderObjectId, MyRenderProxy.ObjectType.Entity, true);
                    this.RenderObjectId = uint.MaxValue;
                }
            }

            internal void UpdateRenderEntitiesData(ref MatrixD worldMatrixD, bool useTransparency, float transparency)
            {
                int model = this.Model;
                bool flag = this.RenderObjectId != uint.MaxValue;
                if (this.InstanceCount <= 0)
                {
                    if (flag)
                    {
                        this.UnloadRenderObjects();
                    }
                }
                else
                {
                    RenderFlags flags = RenderFlags.Visible | RenderFlags.CastShadows;
                    if (!flag)
                    {
                        string byId = MyModel.GetById(model);
                        this.RenderObjectId = MyRenderProxy.CreateRenderEntity("Instance parts, part: " + model, byId, this.Parent.SectorMatrix, MyMeshDrawTechnique.MESH, flags, CullingOptions.Default, Vector3.One, Vector3.Zero, useTransparency ? transparency : 0f, this.MaxViewDistance, 0, 1f, true);
                    }
                    MyRenderProxy.SetInstanceBuffer(this.RenderObjectId, this.InstanceBuffer, 0, this.InstanceData.Count, this.Parent.SectorBox, null);
                    MyRenderProxy.UpdateRenderEntity(this.RenderObjectId, new Color?(Vector3.One), new Vector3?(Vector3.Zero), new float?(useTransparency ? transparency : 0f), true);
                    MatrixD sectorMatrix = this.Parent.SectorMatrix;
                    BoundingBox? aabb = null;
                    Matrix? localMatrix = null;
                    MyRenderProxy.UpdateRenderObject(this.RenderObjectId, new MatrixD?(sectorMatrix), aabb, -1, localMatrix);
                }
            }

            public void UpdateRenderInstanceData()
            {
                if (this.InstanceData.Count != 0)
                {
                    if (this.InstanceBuffer == uint.MaxValue)
                    {
                        this.InstanceBuffer = MyRenderProxy.CreateRenderInstanceBuffer($"EnvironmentSector{this.Parent.SectorId} - {this.SubtypeId}", MyRenderInstanceBufferType.Generic, uint.MaxValue);
                    }
                    MyRenderProxy.UpdateRenderInstanceBufferRange(this.InstanceBuffer, this.InstanceData.Values.ToArray<MyInstanceData>(), 0, false);
                }
            }

            public int InstanceCount =>
                this.InstanceData.Count;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MySectorInstanceData
        {
            public int LocalId;
            public MyInstanceData InstanceData;
        }
    }
}

