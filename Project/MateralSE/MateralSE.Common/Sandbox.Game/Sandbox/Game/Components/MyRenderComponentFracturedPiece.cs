namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    public class MyRenderComponentFracturedPiece : MyRenderComponent
    {
        private const string EMPTY_MODEL = @"Models\Debug\Error.mwm";
        private readonly List<ModelInfo> Models = new List<ModelInfo>();

        public void AddPiece(string modelName, MatrixD localTransform)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                modelName = @"Models\Debug\Error.mwm";
            }
            ModelInfo item = new ModelInfo {
                Name = modelName,
                LocalTransform = localTransform
            };
            this.Models.Add(item);
        }

        public override void AddRenderObjects()
        {
            if (this.Models.Count != 0)
            {
                MyCubeBlock entity = base.Container.Entity as MyCubeBlock;
                if (entity != null)
                {
                    this.CalculateBlockDepthBias(entity);
                }
                base.m_renderObjectIDs = new uint[this.Models.Count + 1];
                base.m_parentIDs = new uint[this.Models.Count + 1];
                base.m_parentIDs[0] = base.m_renderObjectIDs[0] = uint.MaxValue;
                string name = base.Container.Entity.Name;
                this.SetRenderObjectID(0, MyRenderProxy.CreateManualCullObject(name ?? "Fracture", base.Container.Entity.PositionComp.WorldMatrix));
                for (int i = 0; i < this.Models.Count; i++)
                {
                    base.m_parentIDs[i + 1] = base.m_renderObjectIDs[i + 1] = uint.MaxValue;
                    long entityId = base.Container.Entity.EntityId;
                    this.SetRenderObjectID(i + 1, MyRenderProxy.CreateRenderEntity("Fractured piece " + i.ToString() + " " + entityId.ToString(), this.Models[i].Name, this.Models[i].LocalTransform, MyMeshDrawTechnique.MESH, this.GetRenderFlags(), this.GetRenderCullingOptions(), base.m_diffuseColor, base.m_colorMaskHsv, 0f, float.MaxValue, base.DepthBias, 1f, base.FadeIn));
                    if (base.m_textureChanges != null)
                    {
                        MyRenderProxy.ChangeMaterialTexture(base.m_renderObjectIDs[i + 1], base.m_textureChanges);
                    }
                    base.SetParent(i + 1, base.m_renderObjectIDs[0], new Matrix?((Matrix) this.Models[i].LocalTransform));
                }
            }
        }

        public void ClearModels()
        {
            this.Models.Clear();
        }

        public override void InvalidateRenderObjects()
        {
            MatrixD worldMatrix = base.Container.Entity.PositionComp.WorldMatrix;
            if (((base.Container.Entity.Visible || base.Container.Entity.CastShadows) && (base.Container.Entity.InScene && base.Container.Entity.InvalidateOnMove)) && (base.m_renderObjectIDs.Length != 0))
            {
                BoundingBox? aabb = null;
                Matrix? localMatrix = null;
                MyRenderProxy.UpdateRenderObject(base.m_renderObjectIDs[0], new MatrixD?(worldMatrix), aabb, -1, localMatrix);
            }
        }

        public void RemovePiece(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                modelName = @"Models\Debug\Error.mwm";
            }
            this.Models.RemoveAll(m => m.Name == modelName);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ModelInfo
        {
            public string Name;
            public MatrixD LocalTransform;
        }
    }
}

