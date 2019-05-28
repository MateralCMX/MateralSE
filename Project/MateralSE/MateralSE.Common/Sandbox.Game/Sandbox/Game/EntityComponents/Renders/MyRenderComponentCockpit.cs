namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.Entity;
    using VRageRender;
    using VRageRender.Import;

    public class MyRenderComponentCockpit : MyRenderComponentScreenAreas
    {
        protected MyCockpit m_cockpit;

        public MyRenderComponentCockpit(MyEntity entity) : base(entity)
        {
        }

        public override void AddRenderObjects()
        {
            if (((base.m_model != null) && (this.m_cockpit != null)) && (base.m_renderObjectIDs[0] == uint.MaxValue))
            {
                if (!string.IsNullOrEmpty(this.m_cockpit.BlockDefinition.InteriorModel))
                {
                    base.ResizeRenderObjectArray(2);
                }
                this.SetRenderObjectID(0, MyRenderProxy.CreateRenderEntity(base.Container.Entity.GetFriendlyName() + " " + base.Container.Entity.EntityId.ToString(), base.m_model.AssetName, base.Container.Entity.PositionComp.WorldMatrix, MyMeshDrawTechnique.MESH, this.GetRenderFlags(), this.GetRenderCullingOptions(), base.m_diffuseColor, base.m_colorMaskHsv, base.Transparency, float.MaxValue, base.DepthBias, base.m_model.ScaleFactor, (base.Transparency == 0f) && base.FadeIn));
                if (base.m_textureChanges != null)
                {
                    MyRenderProxy.ChangeMaterialTexture(base.m_renderObjectIDs[0], base.m_textureChanges);
                }
                if (!string.IsNullOrEmpty(this.m_cockpit.BlockDefinition.InteriorModel))
                {
                    this.SetRenderObjectID(1, MyRenderProxy.CreateRenderEntity(base.Container.Entity.GetFriendlyName() + " " + base.Container.Entity.EntityId.ToString() + "_interior", this.m_cockpit.BlockDefinition.InteriorModel, base.Container.Entity.PositionComp.WorldMatrix, MyMeshDrawTechnique.MESH, this.GetRenderFlags(), this.GetRenderCullingOptions(), base.m_diffuseColor, base.m_colorMaskHsv, base.Transparency, float.MaxValue, base.DepthBias, base.m_model.ScaleFactor, base.FadeIn));
                    MyRenderProxy.UpdateRenderObjectVisibility(base.m_renderObjectIDs[1], false, this.NearFlag);
                    if (base.m_textureChanges != null)
                    {
                        MyRenderProxy.ChangeMaterialTexture(base.m_renderObjectIDs[1], base.m_textureChanges);
                    }
                }
                this.m_cockpit.UpdateCockpitModel();
                base.UpdateGridParent();
                base.UpdateRenderAreas();
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_cockpit = base.Container.Entity as MyCockpit;
        }

        public uint ExteriorRenderId =>
            base.m_renderObjectIDs[0];

        public uint InteriorRenderId =>
            base.m_renderObjectIDs[1];
    }
}

