namespace Sandbox.Game.Components
{
    using System;
    using VRage.Game.Components;

    internal class MyFracturePiecePositionComponent : MyPositionComponent
    {
        protected override void OnWorldPositionChanged(object source, bool updateChildren, bool forceUpdateAllChildren)
        {
            base.m_worldVolumeDirty = true;
            base.m_worldAABBDirty = true;
            base.m_normalizedInvMatrixDirty = true;
            base.m_invScaledMatrixDirty = true;
            if (((base.Entity.Physics != null) && base.Entity.Physics.Enabled) && !ReferenceEquals(base.Entity.Physics, source))
            {
                base.Entity.Physics.OnWorldPositionChanged(source);
            }
            if (base.Container.Entity.Render != null)
            {
                base.Container.Entity.Render.InvalidateRenderObjects();
            }
        }

        protected override void UpdateChildren(object source, bool forceUpdateAllChildren)
        {
        }
    }
}

