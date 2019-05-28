namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRageMath;
    using VRageRender.Voxels;

    public class MyUserClipmap : IMyClipmap
    {
        private uint m_nextMeshId = 1;
        private readonly Dictionary<uint, IMyVoxelActorCell> m_cells = new Dictionary<uint, IMyVoxelActorCell>();
        private readonly MyConcurrentQueue<IMessage> m_messageQueue = new MyConcurrentQueue<IMessage>();

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

        public void DebugDraw()
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
        }

        public IEnumerable<IMyVoxelActorCell> Cells =>
            this.m_cells.Values;

        public IMyVoxelActor Actor { get; private set; }

        public Vector3I Size { get; private set; }

        private interface IMessage
        {
            void Do(MyUserClipmap clipmap);
        }

        private class MCreateCell : MyUserClipmap.IMessage
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

            public void Do(MyUserClipmap clipmap)
            {
                clipmap.m_cells.Add(this.m_id, clipmap.Actor.CreateCell(this.m_offset, this.m_lod, false));
            }
        }

        private class MDeleteCell : MyUserClipmap.IMessage
        {
            private readonly uint m_id;

            public MDeleteCell(uint id)
            {
                this.m_id = id;
            }

            public void Do(MyUserClipmap clipmap)
            {
                IMyVoxelActorCell cell;
                if (clipmap.m_cells.TryGetValue(this.m_id, out cell))
                {
                    clipmap.Actor.DeleteCell(cell, false);
                    clipmap.m_cells.Remove(this.m_id);
                }
            }
        }
    }
}

