namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.Models;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;
    using VRageRender.Messages;

    public class MyInstancedRenderSector
    {
        private const bool ENABLE_SEPARATE_INSTANCE_LOD = false;
        public MatrixD WorldMatrix;
        private readonly Dictionary<int, InstancedModelBuffer> m_instancedModels = new Dictionary<int, InstancedModelBuffer>();
        private readonly HashSet<int> m_changedBuffers = new HashSet<int>();
        private int m_lod;

        public MyInstancedRenderSector(string name, MatrixD worldMatrix)
        {
            this.Name = name;
            this.WorldMatrix = worldMatrix;
        }

        public short AddInstance(int modelId, ref MyInstanceData data)
        {
            InstancedModelBuffer buffer;
            short num;
            if (!this.m_instancedModels.TryGetValue(modelId, out buffer))
            {
                buffer = new InstancedModelBuffer(this, modelId);
                buffer.SetPerInstanceLod(this.Lod == 0);
                this.m_instancedModels[modelId] = buffer;
            }
            if (buffer.UnusedSlots.TryDequeue<short>(out num))
            {
                buffer.Instances[num] = data;
            }
            else
            {
                int size = (buffer.Instances != null) ? buffer.Instances.Length : 0;
                int expandedSize = this.GetExpandedSize(size);
                Array.Resize<MyInstanceData>(ref buffer.Instances, expandedSize);
                int num4 = expandedSize - size;
                num = (short) size;
                buffer.Instances[num] = data;
                for (int i = 1; i < num4; i++)
                {
                    buffer.UnusedSlots.Enqueue((short) (i + num));
                }
            }
            BoundingBox box = buffer.ModelBb.Transform(data.LocalMatrix);
            buffer.Bounds.Include(ref box);
            this.m_changedBuffers.Add(modelId);
            return num;
        }

        public unsafe int AddInstances(int model, List<MyInstanceData> instances)
        {
            InstancedModelBuffer buffer;
            MyInstanceData* dataPtr;
            MyInstanceData[] pinned dataArray;
            if (!this.m_instancedModels.TryGetValue(model, out buffer))
            {
                buffer = new InstancedModelBuffer(this, model);
                buffer.SetPerInstanceLod(this.Lod == 0);
                this.m_instancedModels[model] = buffer;
            }
            buffer.Instances = instances.GetInternalArray<MyInstanceData>();
            int count = instances.Count;
            if (((dataArray = buffer.Instances) == null) || (dataArray.Length == 0))
            {
                dataPtr = null;
            }
            else
            {
                dataPtr = dataArray;
            }
            for (int i = 0; i < count; i++)
            {
                BoundingBox box = buffer.ModelBb.Transform((dataPtr + i).LocalMatrix);
                buffer.Bounds.Include(ref box);
            }
            dataArray = null;
            buffer.UnusedSlots.Clear();
            for (int j = count; j < instances.Capacity; j++)
            {
                buffer.UnusedSlots.Enqueue((short) j);
            }
            this.m_changedBuffers.Add(model);
            return 0;
        }

        public void Close()
        {
            using (Dictionary<int, InstancedModelBuffer>.ValueCollection.Enumerator enumerator = this.m_instancedModels.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
        }

        public void CommitChangesToRenderer()
        {
            foreach (int num in this.m_changedBuffers)
            {
                this.m_instancedModels[num].UpdateRenderObjects();
            }
            this.m_changedBuffers.Clear();
        }

        public void DetachEnvironment(MyEnvironmentSector myEnvironmentSector)
        {
            this.Close();
        }

        private int GetExpandedSize(int size) => 
            (size + 5);

        public uint GetRenderEntity(int modelId)
        {
            InstancedModelBuffer buffer;
            return (!this.m_instancedModels.TryGetValue(modelId, out buffer) ? uint.MaxValue : buffer.RenderObjectId);
        }

        public bool HasChanges() => 
            (this.m_changedBuffers.Count != 0);

        public void RemoveInstance(int modelId, short index)
        {
            InstancedModelBuffer local1 = this.m_instancedModels[modelId];
            local1.Instances[index] = new MyInstanceData();
            local1.UnusedSlots.Enqueue(index);
            this.m_changedBuffers.Add(modelId);
        }

        public string Name { get; private set; }

        public int Lod
        {
            get => 
                this.m_lod;
            set
            {
                if ((this.m_lod != value) && (value != -1))
                {
                    using (Dictionary<int, InstancedModelBuffer>.ValueCollection.Enumerator enumerator = this.m_instancedModels.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.SetPerInstanceLod(value == 0);
                        }
                    }
                }
                this.m_lod = value;
            }
        }

        private class InstancedModelBuffer
        {
            public MyInstanceData[] Instances = new MyInstanceData[4];
            public uint[] InstanceOIDs;
            public Queue<short> UnusedSlots = new Queue<short>();
            public uint InstanceBufferId = uint.MaxValue;
            public uint RenderObjectId = uint.MaxValue;
            public BoundingBox Bounds = BoundingBox.CreateInvalid();
            public int Model;
            private readonly MyInstancedRenderSector m_parent;
            public readonly BoundingBox ModelBb;

            public InstancedModelBuffer(MyInstancedRenderSector parent, int modelId)
            {
                this.m_parent = parent;
                this.Model = modelId;
                MyModel modelOnlyData = MyModels.GetModelOnlyData(MyModel.GetById(this.Model));
                this.ModelBb = modelOnlyData.BoundingBox;
            }

            public void ClearRenderObjects()
            {
                if (MyFakes.ENABLE_TREES_IN_THE_NEW_PIPE)
                {
                    this.ClearRenderObjectsWithTheNew();
                }
                else
                {
                    this.ClearRenderObjectsWithTheOld();
                }
            }

            private void ClearRenderObjectsWithTheNew()
            {
                if (this.RenderObjectId != uint.MaxValue)
                {
                    MyRenderProxy.RemoveRenderObject(this.RenderObjectId, MyRenderProxy.ObjectType.Entity, false);
                    this.RenderObjectId = uint.MaxValue;
                }
            }

            private void ClearRenderObjectsWithTheOld()
            {
                if (this.InstanceBufferId != uint.MaxValue)
                {
                    MyRenderProxy.RemoveRenderObject(this.InstanceBufferId, MyRenderProxy.ObjectType.InstanceBuffer, false);
                    this.InstanceBufferId = uint.MaxValue;
                }
                if (this.RenderObjectId != uint.MaxValue)
                {
                    MyRenderProxy.RemoveRenderObject(this.RenderObjectId, MyRenderProxy.ObjectType.Entity, true);
                    this.RenderObjectId = uint.MaxValue;
                }
                if (this.InstanceOIDs != null)
                {
                    for (int i = 0; i < this.InstanceOIDs.Length; i++)
                    {
                        if (this.InstanceOIDs[i] != uint.MaxValue)
                        {
                            MyRenderProxy.RemoveRenderObject(this.InstanceOIDs[i], MyRenderProxy.ObjectType.Entity, false);
                            this.InstanceOIDs[i] = uint.MaxValue;
                        }
                    }
                }
            }

            public void Close()
            {
                this.ClearRenderObjects();
                this.Bounds = BoundingBox.CreateInvalid();
                this.Instances = null;
                this.InstanceOIDs = null;
                this.UnusedSlots.Clear();
            }

            private void ResizeActorBuffer()
            {
                Array.Resize<uint>(ref this.InstanceOIDs, this.Instances.Length);
                for (int i = (this.InstanceOIDs != null) ? this.InstanceOIDs.Length : 0; i < this.InstanceOIDs.Length; i++)
                {
                    this.InstanceOIDs[i] = uint.MaxValue;
                }
            }

            public void SetPerInstanceLod(bool value)
            {
                if (value != (this.InstanceOIDs != null))
                {
                    MyInstanceData[] instances = this.Instances;
                }
            }

            public void UpdateRenderObjects()
            {
                if (MyFakes.ENABLE_TREES_IN_THE_NEW_PIPE)
                {
                    this.UpdateRenderObjectsWithTheNew();
                }
                else
                {
                    this.UpdateRenderObjectsWithTheOld();
                }
            }

            private void UpdateRenderObjectsWithTheNew()
            {
                string byId = MyModel.GetById(this.Model);
                if (this.RenderObjectId != uint.MaxValue)
                {
                    MyRenderProxy.RemoveRenderObject(this.RenderObjectId, MyRenderProxy.ObjectType.Entity, false);
                }
                Vector3D translation = this.m_parent.WorldMatrix.Translation;
                Matrix worldMatrix = (Matrix) this.m_parent.WorldMatrix;
                worldMatrix.Translation = Vector3.Zero;
                Matrix[] localMatrices = new Matrix[this.Instances.Length];
                for (int i = 0; i < this.Instances.Length; i++)
                {
                    MyInstanceData data = this.Instances[i];
                    Matrix localMatrix = data.LocalMatrix;
                    localMatrices[i] = localMatrix * worldMatrix;
                }
                this.RenderObjectId = MyRenderProxy.CreateStaticGroup(byId, translation, localMatrices);
            }

            private unsafe void UpdateRenderObjectsWithTheOld()
            {
                Matrix? nullable2;
                string byId = MyModel.GetById(this.Model);
                if (this.InstanceOIDs == null)
                {
                    BoundingBox bounds = this.Bounds;
                    if (this.RenderObjectId == uint.MaxValue)
                    {
                        this.RenderObjectId = MyRenderProxy.CreateRenderEntity($"RO::{this.m_parent.Name}: {byId}", byId, this.m_parent.WorldMatrix, MyMeshDrawTechnique.MESH, RenderFlags.DistanceFade | RenderFlags.ForceOldPipeline | RenderFlags.Visible | RenderFlags.CastShadows, CullingOptions.Default, Vector3.One, Vector3.Zero, 0f, 100000f, 0, 1f, true);
                    }
                    if (this.InstanceBufferId == uint.MaxValue)
                    {
                        this.InstanceBufferId = MyRenderProxy.CreateRenderInstanceBuffer($"IB::{this.m_parent.Name}: {byId}", MyRenderInstanceBufferType.Generic, uint.MaxValue);
                    }
                    MyRenderProxy.UpdateRenderInstanceBufferRange(this.InstanceBufferId, this.Instances, 0, false);
                    MyRenderProxy.SetInstanceBuffer(this.RenderObjectId, this.InstanceBufferId, 0, this.Instances.Length, bounds, null);
                    BoundingBox? aabb = null;
                    nullable2 = null;
                    MyRenderProxy.UpdateRenderObject(this.RenderObjectId, new MatrixD?(this.m_parent.WorldMatrix), aabb, -1, nullable2);
                }
                else
                {
                    MyInstanceData* dataPtr;
                    MyInstanceData[] pinned dataArray;
                    if (this.InstanceOIDs.Length != this.Instances.Length)
                    {
                        this.ResizeActorBuffer();
                    }
                    if (((dataArray = this.Instances) == null) || (dataArray.Length == 0))
                    {
                        dataPtr = null;
                    }
                    else
                    {
                        dataPtr = dataArray;
                    }
                    for (int i = 0; i < this.InstanceOIDs.Length; i++)
                    {
                        if ((this.InstanceOIDs[i] == uint.MaxValue) && (dataPtr[i].m_row0.PackedValue != 0))
                        {
                            MatrixD worldMatrix = (dataPtr + i).LocalMatrix * this.m_parent.WorldMatrix;
                            uint id = MyRenderProxy.CreateRenderEntity($"RO::{this.m_parent.Name}: {byId}", byId, worldMatrix, MyMeshDrawTechnique.MESH, RenderFlags.Visible | RenderFlags.CastShadows, CullingOptions.Default, Vector3.One, Vector3.Zero, 0f, 100000f, 0, 1f, true);
                            nullable2 = null;
                            MyRenderProxy.UpdateRenderObject(id, new MatrixD?(worldMatrix), new BoundingBox?(this.ModelBb), -1, nullable2);
                            this.InstanceOIDs[i] = id;
                        }
                    }
                    dataArray = null;
                }
            }
        }
    }
}

