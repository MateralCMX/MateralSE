namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using VRageMath;
    using VRageRender;

    public class MyGridObstacle : IMyObstacle
    {
        private List<BoundingBox> m_segments;
        private static MyVoxelSegmentation m_segmentation = new MyVoxelSegmentation();
        private MatrixD m_worldInv;
        private MyCubeGrid m_grid;

        public MyGridObstacle(MyCubeGrid grid)
        {
            this.m_grid = grid;
            this.Segment();
            this.Update();
        }

        public bool Contains(ref Vector3D point)
        {
            Vector3D vectord;
            List<BoundingBox>.Enumerator enumerator;
            Vector3D.Transform(ref point, ref this.m_worldInv, out vectord);
            Vector3 vector = Vector3.TransformNormal(MyGravityProviderSystem.CalculateNaturalGravityInPoint(this.m_grid.PositionComp.WorldAABB.Center), this.m_worldInv);
            if (!Vector3.IsZero(vector))
            {
                vector = Vector3.Normalize(vector);
                Ray ray = new Ray((Vector3) vectord, -vector * 2f);
                using (enumerator = this.m_segments.GetEnumerator())
                {
                    bool flag;
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            float? nullable = enumerator.Current.Intersects(ray);
                            if (nullable == null)
                            {
                                continue;
                            }
                            flag = true;
                        }
                        else
                        {
                            goto TR_0000;
                        }
                        break;
                    }
                    return flag;
                }
            }
            using (enumerator = this.m_segments.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    BoundingBox current = enumerator.Current;
                    if (current.Contains(vectord) == ContainmentType.Contains)
                    {
                        return true;
                    }
                }
            }
        TR_0000:
            return false;
        }

        public void DebugDraw()
        {
            MatrixD matrix = MatrixD.Invert(this.m_worldInv);
            Quaternion orientation = Quaternion.CreateFromRotationMatrix(matrix.GetOrientation());
            foreach (BoundingBox box in this.m_segments)
            {
                Vector3D halfExtents = new Vector3D(box.Size) * 0.51;
                Vector3D center = Vector3D.Transform(new Vector3D(box.Min + box.Max) * 0.5, matrix);
                MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(center, halfExtents, orientation), Color.Red, 0.5f, false, false, false);
            }
        }

        private void Segment()
        {
            m_segmentation.ClearInput();
            foreach (MySlimBlock local1 in this.m_grid.CubeBlocks)
            {
                Vector3I min = local1.Min;
                Vector3I max = local1.Max;
                Vector3I input = min;
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref min, ref max);
                while (iterator.IsValid())
                {
                    m_segmentation.AddInput(input);
                    iterator.GetNext(out input);
                }
            }
            List<Sandbox.Engine.Utils.MyVoxelSegmentation.Segment> list = m_segmentation.FindSegments(MyVoxelSegmentationType.Simple2, 1);
            this.m_segments = new List<BoundingBox>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                BoundingBox item = new BoundingBox {
                    Min = ((new Vector3(list[i].Min) - Vector3.Half) * this.m_grid.GridSize) - Vector3.Half,
                    Max = ((new Vector3(list[i].Max) + Vector3.Half) * this.m_grid.GridSize) + Vector3.Half
                };
                this.m_segments.Add(item);
            }
            m_segmentation.ClearInput();
        }

        public void Update()
        {
            this.Segment();
            this.m_worldInv = this.m_grid.PositionComp.WorldMatrixNormalizedInv;
        }
    }
}

