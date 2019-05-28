namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox.Game.Components;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRageMath;

    public class MyParentedSubpartRenderComponent : MyRenderComponent
    {
        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            this.UpdateParent();
        }

        public void GetCullObjectRelativeMatrix(out Matrix relativeMatrix)
        {
            relativeMatrix = base.Entity.PositionComp.LocalMatrix * base.Entity.Parent.PositionComp.LocalMatrix;
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            MyEntity entity = (MyEntity) base.Entity;
            entity.InvalidateOnMove = false;
            entity.NeedsWorldMatrix = false;
        }

        public virtual void OnParented()
        {
        }

        public void UpdateParent()
        {
            if (base.GetRenderObjectID() != uint.MaxValue)
            {
                uint cellParentCullObject = base.Entity.Parent.Render.ParentIDs[0];
                if (cellParentCullObject != uint.MaxValue)
                {
                    Matrix matrix;
                    this.GetCullObjectRelativeMatrix(out matrix);
                    base.SetParent(0, cellParentCullObject, new Matrix?(matrix));
                    this.OnParented();
                }
            }
        }
    }
}

