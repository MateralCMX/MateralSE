namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRageRender;
    using VRageRender.Import;

    public class MyRenderComponent : MyRenderComponentBase
    {
        protected MyModel m_model;

        public MyRenderComponent()
        {
            base.m_parentIDs = (uint[]) base.m_parentIDs.Clone();
            base.m_renderObjectIDs = (uint[]) base.m_renderObjectIDs.Clone();
        }

        public override void AddRenderObjects()
        {
            if ((this.m_model != null) && (base.m_renderObjectIDs[0] == uint.MaxValue))
            {
                this.SetRenderObjectID(0, MyRenderProxy.CreateRenderEntity(base.Container.Entity.GetFriendlyName() + " " + base.Container.Entity.EntityId.ToString(), this.m_model.AssetName, base.Container.Entity.PositionComp.WorldMatrix, MyMeshDrawTechnique.MESH, this.GetRenderFlags(), this.GetRenderCullingOptions(), base.m_diffuseColor, base.m_colorMaskHsv, base.Transparency, float.MaxValue, base.DepthBias, this.m_model.ScaleFactor, (base.Transparency == 0f) && base.FadeIn));
                if (base.m_textureChanges != null)
                {
                    MyRenderProxy.ChangeMaterialTexture(base.m_renderObjectIDs[0], base.m_textureChanges);
                }
            }
        }

        public override void Draw()
        {
        }

        public override bool IsVisible() => 
            (MyEntities.IsVisible(base.Container.Entity) ? (base.Visible ? base.Container.Entity.InScene : false) : false);

        public override void ReleaseRenderObjectID(int index)
        {
            if (base.m_renderObjectIDs[index] != uint.MaxValue)
            {
                MyEntities.RemoveRenderObjectFromMap(base.m_renderObjectIDs[index]);
                MyRenderProxy.RemoveRenderObject(base.m_renderObjectIDs[index], MyRenderProxy.ObjectType.Invalid, base.FadeOut);
                base.m_renderObjectIDs[index] = uint.MaxValue;
                base.m_parentIDs[index] = uint.MaxValue;
            }
        }

        public override void SetRenderObjectID(int index, uint ID)
        {
            base.m_renderObjectIDs[index] = ID;
            MyEntities.AddRenderObjectToMap(ID, base.Container.Entity);
            base.PropagateVisibilityUpdates(false);
        }

        public MyModel Model
        {
            get => 
                this.m_model;
            set => 
                (this.m_model = value);
        }

        public override object ModelStorage
        {
            get => 
                this.Model;
            set => 
                (this.Model = (MyModel) value);
        }

        public override bool NeedsDraw
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.NeedsDraw) != 0);
            set
            {
                if (value != this.NeedsDraw)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.NeedsDraw;
                    if (value)
                    {
                        IMyEntity entity2 = base.Container.Entity;
                        entity2.Flags |= EntityFlags.NeedsDraw;
                    }
                    if (base.Container.Entity.InScene)
                    {
                        if (value)
                        {
                            MyEntities.RegisterForDraw(base.Container.Entity);
                        }
                        else
                        {
                            MyEntities.UnregisterForDraw(base.Container.Entity);
                        }
                    }
                }
            }
        }
    }
}

