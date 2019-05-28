namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;

    internal class MyEntityTerrainHeightProviderComponent : MyEntityComponentBase, IMyTerrainHeightProvider
    {
        private List<MyPhysics.HitInfo> m_raycastHits = new List<MyPhysics.HitInfo>(0x20);

        float IMyTerrainHeightProvider.GetReferenceTerrainHeight() => 
            base.Entity.PositionComp.LocalAABB.Min.Y;

        bool IMyTerrainHeightProvider.GetTerrainHeight(Vector3 bonePosition, Vector3 boneRigPosition, out float terrainHeight, out Vector3 terrainNormal)
        {
            MatrixD worldMatrix = base.Entity.PositionComp.WorldMatrix;
            Vector3D down = worldMatrix.Down;
            Vector3D vectord2 = Vector3D.Transform(new Vector3(bonePosition.X, base.Entity.PositionComp.LocalAABB.Min.Y, bonePosition.Z), ref worldMatrix);
            using (MyUtils.ReuseCollection<MyPhysics.HitInfo>(ref this.m_raycastHits))
            {
                MyPhysics.CastRay(vectord2 - down, vectord2 + down, this.m_raycastHits, 0x12);
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_INVERSE_KINEMATICS)
                {
                    MyRenderProxy.DebugDrawLine3D(vectord2 - down, vectord2 + down, Color.Red, Color.Yellow, false, false);
                }
                using (List<MyPhysics.HitInfo>.Enumerator enumerator = this.m_raycastHits.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyPhysics.HitInfo current = enumerator.Current;
                        IMyEntity hitEntity = current.HkHitInfo.GetHitEntity();
                        if (!ReferenceEquals(hitEntity, base.Entity) && !(hitEntity is MyCharacter))
                        {
                            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_INVERSE_KINEMATICS)
                            {
                                MyRenderProxy.DebugDrawSphere(current.Position, 0.05f, Color.Red, 1f, false, false, true, false);
                            }
                            Vector3D vectord3 = Vector3D.Transform(current.Position, base.Entity.PositionComp.WorldMatrixInvScaled);
                            terrainHeight = ((float) vectord3.Y) - base.Entity.PositionComp.LocalAABB.Min.Y;
                            float convexRadius = current.HkHitInfo.GetConvexRadius();
                            terrainHeight -= (convexRadius < 0.06f) ? convexRadius : 0.06f;
                            terrainNormal = (Vector3) Vector3D.Transform(current.HkHitInfo.Normal, base.Entity.WorldMatrixNormalizedInv.GetOrientation());
                            return true;
                        }
                    }
                }
            }
            terrainHeight = base.Entity.PositionComp.LocalAABB.Min.Y;
            terrainNormal = Vector3.Zero;
            return false;
        }

        public override string ComponentTypeDebugString =>
            "SkinnedEntityTerrainHeightProvider";
    }
}

