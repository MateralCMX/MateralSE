namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;
    using VRageMath;
    using VRageRender;
    using VRageRender.Voxels;

    public class MyUserController : IMyLodController
    {
        private uint m_nextMeshId = 1;
        private readonly Dictionary<uint, IMyVoxelActorCell> m_cells = new Dictionary<uint, IMyVoxelActorCell>();
        private readonly MyConcurrentQueue<IMessage> m_messageQueue = new MyConcurrentQueue<IMessage>();
        [CompilerGenerated]
        private Action<IMyLodController> Loaded;

        public event Action<IMyLodController> Loaded
        {
            [CompilerGenerated] add
            {
                Action<IMyLodController> loaded = this.Loaded;
                while (true)
                {
                    Action<IMyLodController> a = loaded;
                    Action<IMyLodController> action3 = (Action<IMyLodController>) Delegate.Combine(a, value);
                    loaded = Interlocked.CompareExchange<Action<IMyLodController>>(ref this.Loaded, action3, a);
                    if (ReferenceEquals(loaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyLodController> loaded = this.Loaded;
                while (true)
                {
                    Action<IMyLodController> source = loaded;
                    Action<IMyLodController> action3 = (Action<IMyLodController>) Delegate.Remove(source, value);
                    loaded = Interlocked.CompareExchange<Action<IMyLodController>>(ref this.Loaded, action3, source);
                    if (ReferenceEquals(loaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public void BindToActor(IMyVoxelActor actor)
        {
            if (this.Actor != null)
            {
                throw new InvalidOperationException();
            }
            this.Actor = actor;
        }

        public uint CreateCell(Vector3D offset, int lod)
        {
            uint nextMeshId = this.m_nextMeshId;
            this.m_nextMeshId = nextMeshId + 1;
            uint id = nextMeshId;
            this.m_messageQueue.Enqueue(new MCreateCell(id, offset, lod));
            return id;
        }

        public void DebugDraw(ref MatrixD cameraMatrix)
        {
        }

        public void DeleteCell(uint id)
        {
            this.m_messageQueue.Enqueue(new MDeleteCell(id));
        }

        public void InvalidateAll()
        {
        }

        public void InvalidateRange(Vector3I min, Vector3I max)
        {
        }

        public void Unload()
        {
            foreach (IMyVoxelActorCell cell in this.m_cells.Values)
            {
                this.Actor.DeleteCell(cell, false);
            }
            this.m_cells.Clear();
        }

        public void Update(ref MatrixD view, BoundingFrustumD viewFrustum, float farClipping)
        {
            IMessage message;
            while (this.m_messageQueue.TryDequeue(out message))
            {
                message.Do(this);
            }
            if (this.Loaded != null)
            {
                MyRenderProxy.EnqueueMainThreadCallback(delegate {
                    if (this.Loaded != null)
                    {
                        this.Loaded(this);
                    }
                });
            }
        }

        public IMyVoxelRenderDataProcessorProvider VoxelRenderDataProcessorProvider { get; set; }

        public IEnumerable<IMyVoxelActorCell> Cells =>
            this.m_cells.Values;

        public IMyVoxelActor Actor { get; private set; }

        public Vector3I Size { get; private set; }

        public float? SpherizeRadius =>
            null;

        public Vector3D SpherizePosition =>
            Vector3D.Zero;

        private interface IMessage
        {
            void Do(MyUserController controller);
        }

        private class MCreateCell : MyUserController.IMessage
        {
            private readonly Vector3D m_offset;
            private readonly int m_lod;
            private readonly uint m_id;

            public MCreateCell(uint id, Vector3D offset, int lod)
            {
                this.m_offset = offset;
                this.m_lod = lod;
                this.m_id = id;
            }

            public void Do(MyUserController controller)
            {
                controller.m_cells.Add(this.m_id, controller.Actor.CreateCell(this.m_offset, this.m_lod, false));
            }
        }

        private class MDeleteCell : MyUserController.IMessage
        {
            private readonly uint m_id;

            public MDeleteCell(uint id)
            {
                this.m_id = id;
            }

            public void Do(MyUserController controller)
            {
                IMyVoxelActorCell cell;
                if (controller.m_cells.TryGetValue(this.m_id, out cell))
                {
                    controller.Actor.DeleteCell(cell, false);
                    controller.m_cells.Remove(this.m_id);
                }
            }
        }
    }
}

