namespace Sandbox.Game
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRageMath;

    public class MyGridExplosion
    {
        public bool GridWasHit;
        public readonly HashSet<MyCubeGrid> AffectedCubeGrids = new HashSet<MyCubeGrid>();
        public readonly HashSet<MySlimBlock> AffectedCubeBlocks = new HashSet<MySlimBlock>();
        private Dictionary<MySlimBlock, float> m_damagedBlocks = new Dictionary<MySlimBlock, float>();
        private Dictionary<MySlimBlock, MyRaycastDamageInfo> m_damageRemaining = new Dictionary<MySlimBlock, MyRaycastDamageInfo>();
        private Stack<MySlimBlock> m_castBlocks = new Stack<MySlimBlock>();
        private BoundingSphereD m_explosion;
        private float m_explosionDamage;
        private int stackOverflowGuard;
        private const int MAX_PHYSICS_RECURSION_COUNT = 10;
        private List<Vector3I> m_cells = new List<Vector3I>();

        private MyRaycastDamageInfo CastDDA(MySlimBlock cubeBlock)
        {
            Vector3D vectord;
            if (this.m_damageRemaining.ContainsKey(cubeBlock))
            {
                return this.m_damageRemaining[cubeBlock];
            }
            this.stackOverflowGuard = 0;
            this.m_castBlocks.Push(cubeBlock);
            cubeBlock.ComputeWorldCenter(out vectord);
            this.m_cells.Clear();
            Vector3I? gridSizeInflate = null;
            cubeBlock.CubeGrid.RayCastCells(vectord, this.m_explosion.Center, this.m_cells, gridSizeInflate, false, true);
            (this.m_explosion.Center - vectord).Normalize();
            using (List<Vector3I>.Enumerator enumerator = this.m_cells.GetEnumerator())
            {
                while (true)
                {
                    MyRaycastDamageInfo info;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Vector3I current = enumerator.Current;
                    Vector3D fromWorldPos = Vector3D.Transform((Vector3) (current * cubeBlock.CubeGrid.GridSize), cubeBlock.CubeGrid.WorldMatrix);
                    bool flag1 = MyDebugDrawSettings.DEBUG_DRAW_EXPLOSION_DDA_RAYCASTS;
                    MySlimBlock objA = cubeBlock.CubeGrid.GetCubeBlock(current);
                    if (objA == null)
                    {
                        if (this.IsExplosionInsideCell(current, cubeBlock.CubeGrid))
                        {
                            info = new MyRaycastDamageInfo(this.m_explosionDamage, (float) (fromWorldPos - this.m_explosion.Center).Length());
                        }
                        else
                        {
                            info = this.CastPhysicsRay(fromWorldPos);
                        }
                    }
                    else if (ReferenceEquals(objA, cubeBlock))
                    {
                        if (!this.IsExplosionInsideCell(current, cubeBlock.CubeGrid))
                        {
                            continue;
                        }
                        info = new MyRaycastDamageInfo(this.m_explosionDamage, (float) (fromWorldPos - this.m_explosion.Center).Length());
                    }
                    else
                    {
                        if (!this.m_damageRemaining.ContainsKey(objA))
                        {
                            if (this.m_castBlocks.Contains(objA))
                            {
                                continue;
                            }
                            this.m_castBlocks.Push(objA);
                            continue;
                        }
                        info = this.m_damageRemaining[objA];
                    }
                    return info;
                }
            }
            return new MyRaycastDamageInfo(this.m_explosionDamage, (float) (vectord - this.m_explosion.Center).Length());
        }

        private MyRaycastDamageInfo CastPhysicsRay(Vector3D fromWorldPos)
        {
            Vector3D zero = Vector3D.Zero;
            IMyEntity entity = null;
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(fromWorldPos, this.m_explosion.Center, 0x1d);
            if (nullable != null)
            {
                entity = (nullable.Value.HkHitInfo.Body.UserObject != null) ? ((MyPhysicsBody) nullable.Value.HkHitInfo.Body.UserObject).Entity : null;
                zero = nullable.Value.Position;
            }
            Vector3D normal = this.m_explosion.Center - fromWorldPos;
            float distanceToExplosion = (float) normal.Normalize();
            MyCubeGrid cubeGrid = entity as MyCubeGrid;
            if (cubeGrid == null)
            {
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    cubeGrid = block.CubeGrid;
                }
            }
            if (cubeGrid == null)
            {
                if (nullable == null)
                {
                    return new MyRaycastDamageInfo(this.m_explosionDamage, distanceToExplosion);
                }
                bool flag3 = MyDebugDrawSettings.DEBUG_DRAW_EXPLOSION_HAVOK_RAYCASTS;
                return new MyRaycastDamageInfo(0f, distanceToExplosion);
            }
            Vector3D vectord3 = Vector3D.Transform(zero, cubeGrid.PositionComp.WorldMatrixNormalizedInv) * cubeGrid.GridSizeR;
            Vector3D vectord4 = (Vector3D.TransformNormal(normal, cubeGrid.PositionComp.WorldMatrixNormalizedInv) * 1.0) / 8.0;
            for (int i = 0; i < 5; i++)
            {
                Vector3I pos = Vector3I.Round(vectord3);
                MySlimBlock cubeBlock = cubeGrid.GetCubeBlock(pos);
                if (cubeBlock != null)
                {
                    return (!this.m_castBlocks.Contains(cubeBlock) ? this.CastDDA(cubeBlock) : new MyRaycastDamageInfo(0f, distanceToExplosion));
                }
                vectord3 += vectord4;
            }
            zero = Vector3D.Transform(vectord3 * cubeGrid.GridSize, cubeGrid.WorldMatrix);
            BoundingBoxD xd = new BoundingBoxD(Vector3D.Min(fromWorldPos, zero), Vector3D.Max(fromWorldPos, zero));
            if (xd.Contains(this.m_explosion.Center) == ContainmentType.Contains)
            {
                return new MyRaycastDamageInfo(this.m_explosionDamage, distanceToExplosion);
            }
            this.stackOverflowGuard++;
            if (this.stackOverflowGuard > 10)
            {
                bool flag1 = MyDebugDrawSettings.DEBUG_DRAW_EXPLOSION_HAVOK_RAYCASTS;
                return new MyRaycastDamageInfo(0f, distanceToExplosion);
            }
            bool flag2 = MyDebugDrawSettings.DEBUG_DRAW_EXPLOSION_HAVOK_RAYCASTS;
            return this.CastPhysicsRay(zero);
        }

        public unsafe void ComputeDamagedBlocks()
        {
            foreach (MySlimBlock block in this.AffectedCubeBlocks)
            {
                this.m_castBlocks.Clear();
                MyRaycastDamageInfo info = this.CastDDA(block);
                while (this.m_castBlocks.Count > 0)
                {
                    MySlimBlock key = this.m_castBlocks.Pop();
                    if (key.FatBlock is MyWarhead)
                    {
                        this.m_damagedBlocks[key] = 1E+07f;
                        continue;
                    }
                    float num = (float) (key.WorldAABB.Center - this.m_explosion.Center).Length();
                    if (info.DamageRemaining <= 0f)
                    {
                        info.DamageRemaining = 0f;
                    }
                    else
                    {
                        float num2 = MathHelper.Clamp((float) (1f - ((num - info.DistanceToExplosion) / (((float) this.m_explosion.Radius) - info.DistanceToExplosion))), (float) 0f, (float) 1f);
                        if (num2 <= 0f)
                        {
                            this.m_damagedBlocks.Add(key, info.DamageRemaining);
                        }
                        else
                        {
                            this.m_damagedBlocks.Add(key, (info.DamageRemaining * num2) * key.DeformationRatio);
                            MyRaycastDamageInfo* infoPtr1 = (MyRaycastDamageInfo*) ref info;
                            infoPtr1->DamageRemaining = Math.Max((float) 0f, (float) ((info.DamageRemaining * num2) - (key.Integrity / key.DeformationRatio)));
                        }
                    }
                    info.DistanceToExplosion = Math.Abs(num);
                    this.m_damageRemaining.Add(key, info);
                }
            }
        }

        public MyRaycastDamageInfo ComputeDamageForEntity(Vector3D worldPosition) => 
            new MyRaycastDamageInfo(this.m_explosionDamage, (float) (worldPosition - this.m_explosion.Center).Length());

        [Conditional("DEBUG")]
        private void DrawRay(Vector3D from, Vector3D to, float damage, bool depthRead = true)
        {
            if (damage <= 0f)
            {
                Color blue = Color.Blue;
            }
            else
            {
                Color.Lerp(Color.Green, Color.Red, damage / this.m_explosionDamage);
            }
        }

        [Conditional("DEBUG")]
        private void DrawRay(Vector3D from, Vector3D to, Color color, bool depthRead = true)
        {
            if (MyAlexDebugInputComponent.Static != null)
            {
                MyAlexDebugInputComponent.Static.AddDebugLine(new MyAlexDebugInputComponent.LineInfo((Vector3) from, (Vector3) to, color, false));
            }
        }

        public void Init(BoundingSphereD explosion, float explosionDamage)
        {
            this.m_explosion = explosion;
            this.m_explosionDamage = explosionDamage;
            this.AffectedCubeBlocks.Clear();
            this.AffectedCubeGrids.Clear();
            this.m_damageRemaining.Clear();
            this.m_damagedBlocks.Clear();
            this.m_castBlocks.Clear();
        }

        private bool IsExplosionInsideCell(Vector3I cell, MyCubeGrid cellGrid) => 
            (cellGrid.WorldToGridInteger(this.m_explosion.Center) == cell);

        public Dictionary<MySlimBlock, float> DamagedBlocks =>
            this.m_damagedBlocks;

        public Dictionary<MySlimBlock, MyRaycastDamageInfo> DamageRemaining =>
            this.m_damageRemaining;

        public float Damage =>
            this.m_explosionDamage;

        public BoundingSphereD Sphere =>
            this.m_explosion;

        [StructLayout(LayoutKind.Sequential)]
        public struct MyRaycastDamageInfo
        {
            public float DamageRemaining;
            public float DistanceToExplosion;
            public MyRaycastDamageInfo(float damageRemaining, float distanceToExplosion)
            {
                this.DamageRemaining = damageRemaining;
                this.DistanceToExplosion = distanceToExplosion;
            }
        }
    }
}

