namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public class MyNullRenderComponent : MyRenderComponentBase
    {
        public override void AddRenderObjects()
        {
        }

        protected override bool CanBeAddedToRender() => 
            false;

        public override void Draw()
        {
        }

        public override void InvalidateRenderObjects()
        {
        }

        public override bool IsVisible() => 
            false;

        public override void ReleaseRenderObjectID(int index)
        {
        }

        public override void RemoveRenderObjects()
        {
        }

        public override void SetRenderObjectID(int index, uint ID)
        {
        }

        public override void UpdateRenderEntity(Vector3 colorMaskHSV)
        {
        }

        protected override void UpdateRenderObjectVisibility(bool visible)
        {
        }

        public override object ModelStorage { get; set; }
    }
}

