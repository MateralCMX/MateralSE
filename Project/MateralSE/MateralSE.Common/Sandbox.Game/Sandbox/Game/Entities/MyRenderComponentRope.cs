namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Components;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentRope : MyRenderComponent
    {
        public Vector3D WorldPivotA = -Vector3D.One;
        public Vector3D WorldPivotB = Vector3D.One;

        public override void AddRenderObjects()
        {
            MyRopeData data;
            MyRopeComponent.GetRopeData(base.Container.Entity.EntityId, out data);
            this.SetRenderObjectID(0, MyRenderProxy.CreateLineBasedObject(data.Definition.ColorMetalTexture, data.Definition.NormalGlossTexture, data.Definition.AddMapsTexture, "Rope"));
        }

        public override void Draw()
        {
        }

        public override void InvalidateRenderObjects()
        {
            MyRenderProxy.UpdateLineBasedObject(base.m_renderObjectIDs[0], this.WorldPivotA, this.WorldPivotB);
        }

        public override bool IsVisible() => 
            false;

        public override void ReleaseRenderObjectID(int index)
        {
            MyRenderProxy.RemoveRenderObject(base.m_renderObjectIDs[0], MyRenderProxy.ObjectType.Entity, false);
        }

        public override void SetRenderObjectID(int index, uint ID)
        {
            base.m_renderObjectIDs[0] = ID;
            base.PropagateVisibilityUpdates(false);
        }

        public override object ModelStorage
        {
            get => 
                null;
            set
            {
            }
        }
    }
}

