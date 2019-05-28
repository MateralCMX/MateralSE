namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Game.Voxels;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.Mesh;
    using VRage.Voxels.Sewing;
    using VRageMath;
    using VRageRender;
    using VRageRender.Voxels;

    public sealed class MyClipmapSewJob : MyPrecalcJob
    {
        public static bool DebugDrawDependencies = false;
        public static bool DebugDrawGeneration = false;
        public static VRage.Voxels.Sewing.VrTailor.GeneratedVertexProtocol GeneratedVertexProtocol = VRage.Voxels.Sewing.VrTailor.GeneratedVertexProtocol.Dynamic;
        private static readonly MyConcurrentPool<MyClipmapSewJob> m_instancePool = new MyConcurrentPool<MyClipmapSewJob>(0x10, null, 0x2710, null);
        private MyVoxelRenderCellData m_cellData;
        private bool m_forceStitchCommit;
        private volatile bool m_isCancelled;

        public MyClipmapSewJob() : base(true)
        {
        }

        public override void Cancel()
        {
            this.m_isCancelled = true;
        }

        public override void DebugDraw(Color c)
        {
        }

        [Conditional("DEBUG")]
        private unsafe void DebugDrawGenerated(VrSewGuide[] meshes)
        {
            if (DebugDrawGeneration)
            {
                int num;
                int num2;
                int num3;
                ushort* numPtr;
                VrTailor.VertexRef* refPtr;
                VrTailor.RemappedVertex* vertexPtr;
                MyIsoMeshTaylor.NativeInstance.DebugReadStudied(out refPtr, out num);
                MyIsoMeshTaylor.NativeInstance.DebugReadGenerated(out numPtr, out num2);
                MyIsoMeshTaylor.NativeInstance.DebugReadRemapped(out vertexPtr, out num3);
                for (int i = 0; i < num; i++)
                {
                    VrSewGuide guide2 = meshes[refPtr[i].Mesh];
                    VrVoxelVertex vertex = guide2.Mesh.Vertices[refPtr[i].Index];
                    MyRenderProxy.DebugDrawText3D(this.Position(guide2, refPtr[i].Index), vertex.Cell.ToString(), Color.Gray, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, true);
                }
                VrSewGuide mesh = meshes[0];
                for (int j = 0; j < num2; j++)
                {
                    Vector3I cell = mesh.Mesh.Vertices[numPtr[j]].Cell;
                    Vector3D worldCoord = this.Position(mesh, numPtr[j]);
                    MyRenderProxy.DebugDrawText3D(worldCoord, cell.ToString(), Color.Red, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, -1, true);
                    MyRenderProxy.DebugDrawText3D(worldCoord, j.ToString(), Color.Red, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, -1, true);
                    MyRenderProxy.DebugDrawSphere(worldCoord, 0.25f, Color.Red, 1f, true, false, true, true);
                }
                int index = 0;
                while (index < num3)
                {
                    Vector3I cell = vertexPtr[index].Cell;
                    Vector3D position = this.Position(mesh, cell) + (0.5 * mesh.Scale);
                    Vector3D vectord2 = this.Position(mesh, vertexPtr[index].Index);
                    byte generationCorner = vertexPtr[index].GenerationCorner;
                    int num8 = 0;
                    while (true)
                    {
                        if (num8 >= 3)
                        {
                            MyRenderProxy.DebugDrawSphere(position, 0.25f, Color.Blue, 1f, true, false, true, true);
                            MyRenderProxy.DebugDrawSphere(vectord2, 0.25f, Color.Green, 1f, true, false, true, true);
                            MyRenderProxy.DebugDrawArrow3D(position, vectord2, Color.Blue, new Color?(Color.Green), true, 0.1, null, 0.5f, true);
                            MyRenderProxy.DebugDrawText3D(position, cell.ToString(), Color.Blue, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, -1, true);
                            MyRenderProxy.DebugDrawText3D(position, index.ToString(), Color.Blue, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, -1, true);
                            MyRenderProxy.DebugDrawText3D((position + vectord2) / 2.0, $"G{index} : T{vertexPtr[index].ProducedTriangleCount}", Color.Lerp(Color.Blue, Color.Green, 0.5f), 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, true);
                            index++;
                            break;
                        }
                        int num9 = (generationCorner >> (num8 & 0x1f)) & 1;
                        Vector3I pos = cell;
                        ref Vector3I vectoriRef = ref pos;
                        int num10 = num8;
                        vectoriRef[num10] += (num9 == 1) ? -1 : 1;
                        MyRenderProxy.DebugDrawText3D(this.Position(mesh, pos) + (0.5 * mesh.Scale), pos.ToString(), Color.Orange, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, true);
                        num8++;
                    }
                }
                MyIsoMeshTaylor.NativeInstance.ClearBuffers();
            }
        }

        private void DebugDrawStitch(VrSewGuide[] meshes)
        {
            Vector3D center = this.GetCenter(meshes[0]);
            for (int i = 0; i < 8; i++)
            {
                if (meshes[i] != null)
                {
                    MyRenderProxy.DebugDrawArrow3D(center, this.GetCenter(meshes[i]), Color.Red, new Color?(Color.Green), true, 0.1, null, 0.5f, true);
                }
            }
        }

        public override void DoWork()
        {
            try
            {
                if (!this.m_isCancelled && base.IsValid)
                {
                    MyIsoMeshTaylor.NativeInstance.SetDebug(DebugDrawGeneration || (GeneratedVertexProtocol != VRage.Voxels.Sewing.VrTailor.GeneratedVertexProtocol.Dynamic));
                    MyIsoMeshTaylor.NativeInstance.SetGenerate(GeneratedVertexProtocol);
                    if (this.Clipmap.InstanceStitchMode == MyVoxelClipmap.StitchMode.Stitch)
                    {
                        if (this.Operation.Guides[0] != null)
                        {
                            if (this.Operation.Guides[0].Sewn)
                            {
                                this.Operation.Guides[0].Reset();
                            }
                            MyIsoMeshTaylor.NativeInstance.Sew(this.Operation.Guides, this.Operation.Operation);
                            if (DebugDrawDependencies)
                            {
                                this.DebugDrawStitch(this.Operation.Guides);
                            }
                            MyVoxelClipmap.CompoundStitchOperation compound = this.Operation.GetCompound();
                            if ((compound != null) && (compound.Children.Count > 0))
                            {
                                this.SewChildren(compound);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (this.Operation.Guides[0].Mesh != null)
                    {
                        MyRenderDataBuilder.Instance.Build(this.Operation.Guides[0].Mesh, out this.m_cellData, this.Clipmap.VoxelRenderDataProcessorProvider);
                    }
                    this.Clipmap.UpdateCellRender(this.Cell, this.Operation, ref this.m_cellData);
                    this.WorkTracker.Complete(this.Cell);
                    this.m_forceStitchCommit = false;
                }
                else
                {
                    this.Clipmap.CommitStitchOperation(this.Operation, true);
                    this.WorkTracker.Complete(this.Cell);
                }
            }
            finally
            {
            }
        }

        private bool Enqueue()
        {
            this.m_forceStitchCommit = true;
            this.WorkTracker.Add(this.Cell, this);
            if (MyPrecalcComponent.EnqueueBack(this))
            {
                return true;
            }
            this.WorkTracker.Complete(this.Cell);
            return false;
        }

        private Vector3D GetCenter(VrSewGuide mesh) => 
            Vector3D.Transform((Vector3D) (((mesh.End + mesh.Start) / 2.0) * mesh.Scale), this.Clipmap.LocalToWorld);

        protected override void OnComplete()
        {
            base.OnComplete();
            if (this.m_forceStitchCommit)
            {
                this.Clipmap.CommitStitchOperation(this.Operation, true);
            }
            this.m_cellData = new MyVoxelRenderCellData();
            m_instancePool.Return(this);
        }

        private unsafe Vector3D Position(VrSewGuide mesh, int index) => 
            Vector3D.Transform((Vector3D) ((mesh.Mesh.Vertices[index].Position + mesh.Start) * mesh.Scale), this.Clipmap.LocalToWorld);

        private Vector3D Position(VrSewGuide mesh, Vector3I pos)
        {
            pos = (Vector3I) (pos + mesh.Start);
            pos = pos << mesh.Lod;
            return Vector3D.Transform((Vector3D) pos, this.Clipmap.LocalToWorld);
        }

        private void SewChildren(MyVoxelClipmap.CompoundStitchOperation compound)
        {
            foreach (MyVoxelClipmap.StitchOperation operation in compound.Children)
            {
                int index = 0;
                while (true)
                {
                    if (index >= 8)
                    {
                        MyIsoMeshTaylor.NativeInstance.Sew(operation.Guides, operation.Operation, operation.Range.Value.Min, operation.Range.Value.Max);
                        if (DebugDrawDependencies)
                        {
                            this.DebugDrawStitch(operation.Guides);
                        }
                        break;
                    }
                    MyCellCoord coord = MyVoxelClipmap.MakeFulfilled(operation.Dependencies[index]);
                    if (operation.Guides[index] == null)
                    {
                        int lod = compound.Cell.Lod;
                        int num2 = coord.Lod;
                    }
                    index++;
                }
            }
        }

        internal static bool Start(MyWorkTracker<MyCellCoord, MyClipmapSewJob> tracker, MyVoxelClipmap clipmap, MyVoxelClipmap.StitchOperation operation)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException("tracker");
            }
            if (clipmap == null)
            {
                throw new ArgumentNullException("clipmap");
            }
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }
            if (tracker.Exists(operation.Cell))
            {
                object[] args = new object[] { operation.Cell };
                MyLog.Default.Error("A Stitch job for cell {0} is already scheduled.", args);
                return false;
            }
            MyClipmapSewJob local1 = m_instancePool.Get();
            local1.m_isCancelled = false;
            local1.Clipmap = clipmap;
            local1.WorkTracker = tracker;
            local1.Operation = operation;
            local1.Cell = operation.Cell;
            return local1.Enqueue();
        }

        public MyVoxelClipmap Clipmap { get; private set; }

        public MyCellCoord Cell { get; private set; }

        internal MyVoxelClipmap.StitchOperation Operation { get; private set; }

        public MyWorkTracker<MyCellCoord, MyClipmapSewJob> WorkTracker { get; private set; }

        public override bool IsCanceled =>
            this.m_isCancelled;

        public override int Priority =>
            (this.m_isCancelled ? 0x7fffffff : this.Cell.Lod);
    }
}

