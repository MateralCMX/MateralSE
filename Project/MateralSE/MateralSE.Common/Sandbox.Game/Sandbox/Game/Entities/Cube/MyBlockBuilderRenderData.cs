namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Models;
    using VRage.Generics;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    public class MyBlockBuilderRenderData
    {
        private static MyObjectsPool<MyEntities> m_entitiesPool = new MyObjectsPool<MyEntities>(1, null);
        private Dictionary<int, MyEntities> m_allEntities = new Dictionary<int, MyEntities>();
        private List<int> m_tmpRemovedModels = new List<int>();
        private float Transparency = (MyFakes.ENABLE_TRANSPARENT_CUBE_BUILDER ? 0.5f : 0f);

        public void AddInstance(int model, MatrixD matrix, ref MatrixD invGridWorldMatrix, Vector3 colorMaskHsv = new Vector3(), Vector3UByte[] bones = null, float gridSize = 1f)
        {
            MyEntities entities;
            if (!this.m_allEntities.TryGetValue(model, out entities))
            {
                m_entitiesPool.AllocateOrCreate(out entities);
                this.m_allEntities.Add(model, entities);
            }
            entities.AddModel(model, (Matrix) (matrix * invGridWorldMatrix), colorMaskHsv, this.Transparency);
        }

        public void BeginCollectingInstanceData()
        {
            foreach (KeyValuePair<int, MyEntities> pair in this.m_allEntities)
            {
                pair.Value.PrepareCollecting();
            }
        }

        public void EndCollectingInstanceData(MatrixD gridWorldMatrix, bool useTransparency)
        {
            foreach (KeyValuePair<int, MyEntities> pair in this.m_allEntities)
            {
                pair.Value.ShrinkRenderEnties();
                if (pair.Value.IsEmpty())
                {
                    this.m_tmpRemovedModels.Add(pair.Key);
                }
            }
            foreach (int num2 in this.m_tmpRemovedModels)
            {
                this.m_allEntities.Remove(num2);
            }
            this.m_tmpRemovedModels.Clear();
            float transparency = useTransparency ? this.Transparency : 0f;
            foreach (KeyValuePair<int, MyEntities> pair2 in this.m_allEntities)
            {
                pair2.Value.Update(gridWorldMatrix, transparency);
            }
        }

        public void UnloadRenderObjects()
        {
            foreach (KeyValuePair<int, MyEntities> pair in this.m_allEntities)
            {
                pair.Value.Clear();
            }
            this.m_allEntities.Clear();
        }

        private class MyEntities
        {
            private List<MyBlockBuilderRenderData.MyEntity> RenderEntities = new List<MyBlockBuilderRenderData.MyEntity>();
            private int NumUsedModels;

            public void AddModel(int model, Matrix localMatrix, Vector3 colorMaskHsv, float transparency)
            {
                RenderFlags flags = ((RenderFlags) 0) | RenderFlags.Visible;
                int num = this.NumUsedModels + 1;
                this.NumUsedModels = num;
                if (this.RenderEntities.Count >= num)
                {
                    MyBlockBuilderRenderData.MyEntity entity3 = this.RenderEntities[this.NumUsedModels - 1];
                    entity3.LocalMatrix = localMatrix;
                    entity3.ColorMashHsv = colorMaskHsv;
                    this.RenderEntities[this.NumUsedModels - 1] = entity3;
                }
                else
                {
                    string byId = MyModel.GetById(model);
                    MyBlockBuilderRenderData.MyEntity item = new MyBlockBuilderRenderData.MyEntity {
                        LocalMatrix = localMatrix,
                        RenderEntityId = MyRenderProxy.CreateRenderEntity("Cube builder, part: " + model, byId, MatrixD.Identity, MyMeshDrawTechnique.MESH, flags, CullingOptions.Default, Vector3.One, colorMaskHsv, transparency, float.MaxValue, 0, 1f, false),
                        ColorMashHsv = colorMaskHsv
                    };
                    this.RenderEntities.Add(item);
                }
            }

            public void Clear()
            {
                this.NumUsedModels = 0;
                this.ShrinkRenderEnties();
            }

            public bool IsEmpty() => 
                (this.NumUsedModels == 0);

            public void PrepareCollecting()
            {
                this.NumUsedModels = 0;
            }

            public void ShrinkRenderEnties()
            {
                for (int i = this.NumUsedModels; i < this.RenderEntities.Count; i++)
                {
                    MyRenderProxy.RemoveRenderObject(this.RenderEntities[i].RenderEntityId, MyRenderProxy.ObjectType.Entity, false);
                }
                this.RenderEntities.RemoveRange(this.NumUsedModels, this.RenderEntities.Count - this.NumUsedModels);
            }

            public void Update(MatrixD gridWorldMatrix, float transparency)
            {
                foreach (MyBlockBuilderRenderData.MyEntity entity in this.RenderEntities)
                {
                    entity.Update(gridWorldMatrix, transparency);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyEntity
        {
            public uint RenderEntityId;
            public Matrix LocalMatrix;
            public Vector3 ColorMashHsv;
            public void Update(MatrixD gridWorldMatrix, float transparency)
            {
                MatrixD xd = this.LocalMatrix * gridWorldMatrix;
                BoundingBox? aabb = null;
                Matrix? localMatrix = null;
                MyRenderProxy.UpdateRenderObject(this.RenderEntityId, new MatrixD?(xd), aabb, -1, localMatrix);
                MyRenderProxy.UpdateRenderEntity(this.RenderEntityId, new Color?(Vector3.One), new Vector3?(this.ColorMashHsv), new float?(transparency), false);
            }
        }
    }
}

