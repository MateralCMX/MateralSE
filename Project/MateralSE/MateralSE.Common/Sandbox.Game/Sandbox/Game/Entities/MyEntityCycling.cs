namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;

    public static class MyEntityCycling
    {
        public static void FindNext(MyEntityCyclingOrder order, ref float metric, ref long entityId, bool findLarger, CyclingOptions options)
        {
            Metric metric2 = new Metric {
                Value = metric,
                EntityId = entityId
            };
            Metric metric3 = findLarger ? Metric.Max : Metric.Min;
            Metric metric4 = metric3;
            Metric metric5 = metric3;
            foreach (MyEntity entity in MyEntities.GetEntities())
            {
                if (options.Enabled)
                {
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (options.OnlyLargeGrids)
                    {
                        if (grid == null)
                        {
                            continue;
                        }
                        if (grid.GridSizeEnum != MyCubeSize.Large)
                        {
                            continue;
                        }
                    }
                    if (options.OnlySmallGrids && ((grid == null) || (grid.GridSizeEnum != MyCubeSize.Small)))
                    {
                        continue;
                    }
                }
                Metric metric7 = new Metric(GetMetric(order, entity), entity.EntityId);
                if (metric7.Value != 0f)
                {
                    if (findLarger)
                    {
                        if ((metric7 > metric2) && (metric7 < metric4))
                        {
                            metric4 = metric7;
                        }
                        if (metric7 < metric5)
                        {
                            metric5 = metric7;
                        }
                    }
                    else
                    {
                        if ((metric7 < metric2) && (metric7 > metric4))
                        {
                            metric4 = metric7;
                        }
                        if (metric7 > metric5)
                        {
                            metric5 = metric7;
                        }
                    }
                }
            }
            if (metric4 == metric3)
            {
                metric4 = metric5;
            }
            metric = metric4.Value;
            entityId = metric4.EntityId;
        }

        private static float GetActiveBlockCount<T>(MyCubeGrid grid, bool includePassive = false) where T: MyFunctionalBlock
        {
            if (grid == null)
            {
                return 0f;
            }
            int num = 0;
            using (HashSet<MySlimBlock>.Enumerator enumerator = grid.GetBlocks().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T fatBlock = enumerator.Current.FatBlock as T;
                    if ((fatBlock != null) && (includePassive || fatBlock.IsWorking))
                    {
                        num++;
                    }
                }
            }
            return (float) num;
        }

        public static float GetMetric(MyEntityCyclingOrder order, MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            MyPhysicsComponentBase physics = entity.Physics;
            switch (order)
            {
                case MyEntityCyclingOrder.Characters:
                    return ((entity is MyCharacter) ? ((float) 1) : ((float) 0));

                case MyEntityCyclingOrder.BiggestGrids:
                    return ((grid != null) ? ((float) grid.GetBlocks().Count) : ((float) 0));

                case MyEntityCyclingOrder.Fastest:
                    if (physics == null)
                    {
                        return 0f;
                    }
                    return (float) Math.Round((double) physics.LinearVelocity.Length(), 2);

                case MyEntityCyclingOrder.BiggestDistanceFromPlayers:
                    if ((entity is MyVoxelBase) || (entity is MySafeZone))
                    {
                        return 0f;
                    }
                    return GetPlayerDistance(entity);

                case MyEntityCyclingOrder.MostActiveDrills:
                    return GetActiveBlockCount<MyShipDrill>(grid, false);

                case MyEntityCyclingOrder.MostActiveReactors:
                    return GetActiveBlockCount<MyReactor>(grid, false);

                case MyEntityCyclingOrder.MostActiveProductionBuildings:
                    return GetActiveBlockCount<MyProductionBlock>(grid, false);

                case MyEntityCyclingOrder.MostActiveSensors:
                    return GetActiveBlockCount<MySensorBlock>(grid, false);

                case MyEntityCyclingOrder.MostActiveThrusters:
                    return GetActiveBlockCount<MyThrust>(grid, false);

                case MyEntityCyclingOrder.MostWheels:
                    return GetActiveBlockCount<MyMotorSuspension>(grid, true);

                case MyEntityCyclingOrder.StaticObjects:
                    int num1;
                    if (((entity.Physics == null) || (entity.Physics.IsPhantom || (entity.Physics.AngularVelocity.AbsMax() >= 0.05f))) || (entity.Physics.LinearVelocity.AbsMax() >= 0.05f))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = 1;
                    }
                    return (float) num1;

                case MyEntityCyclingOrder.FloatingObjects:
                    return ((entity is MyFloatingObject) ? ((float) 1) : ((float) 0));

                case MyEntityCyclingOrder.Planets:
                    return ((entity is MyPlanet) ? ((float) 1) : ((float) 0));

                case MyEntityCyclingOrder.OwnerLoginTime:
                    return GetOwnerLoginTime(grid);
            }
            return 0f;
        }

        private static float GetOwnerLoginTime(MyCubeGrid grid)
        {
            if (grid == null)
            {
                return 0f;
            }
            if (grid.BigOwners.Count == 0)
            {
                return 0f;
            }
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(grid.BigOwners[0]);
            if (identity == null)
            {
                return 0f;
            }
            return (float) Math.Round((DateTime.Now - identity.LastLoginTime).TotalDays, 2);
        }

        private static float GetPlayerDistance(MyEntity entity)
        {
            MatrixD worldMatrix = entity.WorldMatrix;
            Vector3D translation = worldMatrix.Translation;
            float maxValue = float.MaxValue;
            using (IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IMyControllableEntity controlledEntity = enumerator.Current.Controller.ControlledEntity;
                    if (controlledEntity != null)
                    {
                        float num2 = Vector3.DistanceSquared((Vector3) controlledEntity.Entity.WorldMatrix.Translation, (Vector3) translation);
                        if (num2 < maxValue)
                        {
                            maxValue = num2;
                        }
                    }
                }
            }
            return (float) Math.Sqrt((double) maxValue);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Metric
        {
            public static readonly MyEntityCycling.Metric Min;
            public static readonly MyEntityCycling.Metric Max;
            public float Value;
            public long EntityId;
            public Metric(float value, long entityId)
            {
                this.Value = value;
                this.EntityId = entityId;
            }

            public static bool operator >(MyEntityCycling.Metric a, MyEntityCycling.Metric b) => 
                ((a.Value > b.Value) || ((a.Value == b.Value) && (a.EntityId > b.EntityId)));

            public static bool operator <(MyEntityCycling.Metric a, MyEntityCycling.Metric b) => 
                (b > a);

            public static bool operator >=(MyEntityCycling.Metric a, MyEntityCycling.Metric b) => 
                ((a.Value > b.Value) || ((a.Value == b.Value) && (a.EntityId >= b.EntityId)));

            public static bool operator <=(MyEntityCycling.Metric a, MyEntityCycling.Metric b) => 
                (b >= a);

            public static bool operator ==(MyEntityCycling.Metric a, MyEntityCycling.Metric b) => 
                ((a.Value == b.Value) && (a.EntityId == b.EntityId));

            public static bool operator !=(MyEntityCycling.Metric a, MyEntityCycling.Metric b) => 
                !(a == b);

            static Metric()
            {
                MyEntityCycling.Metric metric = new MyEntityCycling.Metric {
                    Value = float.MinValue,
                    EntityId = 0L
                };
                Min = metric;
                metric = new MyEntityCycling.Metric {
                    Value = float.MaxValue,
                    EntityId = 0L
                };
                Max = metric;
            }
        }
    }
}

