namespace VRage.Game.Components
{
    using System;

    public abstract class MyDebugRenderComponentBase
    {
        protected MyDebugRenderComponentBase()
        {
        }

        public abstract void DebugDraw();
        public abstract void DebugDrawInvalidTriangles();
        public virtual void PrepareForDraw()
        {
        }
    }
}

