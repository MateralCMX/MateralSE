namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public static class MyEntityList
    {
        [ThreadStatic]
        private static MyEntityListInfoItem m_gridItem;

        private static void AccountChildren(MyCubeGrid grid)
        {
            MyGridPhysicalHierarchy.Static.ApplyOnChildren(grid, delegate (MyCubeGrid childGrid) {
                MyEntityListInfoItem item;
                CreateListInfoForGrid(childGrid, out item);
                m_gridItem.Add(ref item);
                AccountChildren(childGrid);
            });
        }

        private static void CreateListInfoForGrid(MyCubeGrid grid, out MyEntityListInfoItem item)
        {
            long owner = 0L;
            string ownerName = string.Empty;
            if (grid.BigOwners.Count > 0)
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(grid.BigOwners[0]);
                if (identity != null)
                {
                    ownerName = identity.DisplayName;
                    owner = grid.BigOwners[0];
                }
            }
            item = new MyEntityListInfoItem(grid.DisplayName, grid.EntityId, grid.BlocksCount, grid.BlocksPCU, grid.Physics.Mass, grid.PositionComp.GetPosition(), grid.Physics.LinearVelocity.Length(), MySession.GetPlayerDistance(grid, MySession.Static.Players.GetOnlinePlayers()), ownerName, owner, MySession.GetOwnerLoginTimeSeconds(grid), MySession.GetOwnerLogoutTimeSeconds(grid));
        }

        private static void Depower(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                grid.ChangePowerProducerState(MyMultipleEnabledEnum.AllDisabled, -1L);
            }
        }

        public static string GetDescriptionText(MyEntityListInfoItem item, bool isGrid)
        {
            StringBuilder output = new StringBuilder();
            if (!isGrid)
            {
                output.Append(MyEntitySortOrder.Mass + ": ");
                if (item.Mass > 0f)
                {
                    MyValueFormatter.AppendWeightInBestUnit(item.Mass, output);
                }
                else
                {
                    output.Append("-");
                }
                output.AppendLine();
                output.Append(MyTexts.Get(MyStringId.GetOrCompute(4.ToString())) + ": ");
                MyValueFormatter.AppendDistanceInBestUnit((float) item.Position.Length(), output);
                output.AppendLine();
                object[] objArray1 = new object[] { MyTexts.Get(MyStringId.GetOrCompute(5.ToString())), ": ", item.Speed, " m/s" };
                output.Append(string.Concat(objArray1));
            }
            else
            {
                output.AppendLine(MyTexts.Get(MyStringId.GetOrCompute(1.ToString())) + ": " + item.BlockCount);
                output.AppendLine(MyTexts.Get(MyStringId.GetOrCompute(8.ToString())) + ": " + item.PCU);
                output.Append(MyTexts.Get(MyStringId.GetOrCompute(2.ToString())) + ": ");
                if (item.Mass > 0f)
                {
                    MyValueFormatter.AppendWeightInBestUnit(item.Mass, output);
                }
                else
                {
                    output.Append("-");
                }
                output.AppendLine();
                output.AppendLine(MyTexts.Get(MyStringId.GetOrCompute(3.ToString())) + ": " + item.OwnerName);
                object[] objArray2 = new object[] { MyTexts.Get(MyStringId.GetOrCompute(5.ToString())), ": ", item.Speed, " m/s" };
                output.AppendLine(string.Concat(objArray2));
                output.Append(MyTexts.Get(MyStringId.GetOrCompute(4.ToString())) + ": ");
                MyValueFormatter.AppendDistanceInBestUnit((float) item.Position.Length(), output);
                output.AppendLine();
                output.Append(MyTexts.Get(MyStringId.GetOrCompute(6.ToString())) + ": ");
                MyValueFormatter.AppendDistanceInBestUnit(item.DistanceFromPlayers, output);
                output.AppendLine();
                output.Append(MyTexts.Get(MyStringId.GetOrCompute(7.ToString())) + ": ");
                MyValueFormatter.AppendTimeInBestUnit(item.OwnerLogoutTime, output);
            }
            return output.ToString();
        }

        public static List<MyEntityListInfoItem> GetEntityList(MyEntityTypeEnum selectedType)
        {
            MyConcurrentHashSet<MyEntity> entities = MyEntities.GetEntities();
            List<MyEntityListInfoItem> list = new List<MyEntityListInfoItem>(entities.Count);
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            switch (selectedType)
            {
                case MyEntityTypeEnum.Grids:
                case MyEntityTypeEnum.SmallGrids:
                case MyEntityTypeEnum.LargeGrids:
                    foreach (MyCubeGrid grid in entities)
                    {
                        if (grid == null)
                        {
                            continue;
                        }
                        if (((selectedType != MyEntityTypeEnum.LargeGrids) || (grid.GridSizeEnum != MyCubeSize.Small)) && ((((selectedType != MyEntityTypeEnum.SmallGrids) || (grid.GridSizeEnum != MyCubeSize.Large)) && (!grid.Closed && (grid.Physics != null))) && ReferenceEquals(MyGridPhysicalHierarchy.Static.GetRoot(grid), grid)))
                        {
                            CreateListInfoForGrid(grid, out m_gridItem);
                            AccountChildren(grid);
                            list.Add(m_gridItem);
                        }
                    }
                    return list;

                case MyEntityTypeEnum.Characters:
                    break;

                case MyEntityTypeEnum.FloatingObjects:
                    foreach (MyEntity entity in entities)
                    {
                        MyFloatingObject obj2 = entity as MyFloatingObject;
                        if (obj2 != null)
                        {
                            if (obj2.Closed)
                            {
                                continue;
                            }
                            if (obj2.Physics == null)
                            {
                                continue;
                            }
                            list.Add(new MyEntityListInfoItem(obj2.DisplayName, obj2.EntityId, 0, 0, obj2.Physics.Mass, obj2.PositionComp.GetPosition(), obj2.Physics.LinearVelocity.Length(), MySession.GetPlayerDistance(obj2, onlinePlayers), "", 0L, 0f, 0f));
                        }
                        MyInventoryBagEntity entity2 = entity as MyInventoryBagEntity;
                        if (((entity2 != null) && !entity2.Closed) && (entity2.Physics != null))
                        {
                            MyIdentity identity2 = MySession.Static.Players.TryGetIdentity(entity2.OwnerIdentityId);
                            string ownerName = "";
                            float ownerLogin = 0f;
                            float ownerLogout = 0f;
                            if (identity2 != null)
                            {
                                ownerName = identity2.DisplayName;
                                ownerLogin = (int) (DateTime.Now - identity2.LastLoginTime).TotalSeconds;
                                ownerLogout = (int) (DateTime.Now - identity2.LastLogoutTime).TotalSeconds;
                            }
                            list.Add(new MyEntityListInfoItem(entity2.DisplayName, entity2.EntityId, 0, 0, entity2.Physics.Mass, entity2.PositionComp.GetPosition(), entity2.Physics.LinearVelocity.Length(), MySession.GetPlayerDistance(entity2, onlinePlayers), ownerName, entity2.OwnerIdentityId, ownerLogin, ownerLogout));
                        }
                    }
                    return list;

                case MyEntityTypeEnum.Planets:
                    foreach (MyPlanet planet in entities)
                    {
                        if (planet == null)
                        {
                            continue;
                        }
                        if (!planet.Closed)
                        {
                            list.Add(new MyEntityListInfoItem(planet.StorageName, planet.EntityId, 0, 0, 0f, planet.PositionComp.GetPosition(), 0f, MySession.GetPlayerDistance(planet, onlinePlayers), "", 0L, 0f, 0f));
                        }
                    }
                    return list;

                case MyEntityTypeEnum.Asteroids:
                    foreach (MyVoxelBase base2 in entities)
                    {
                        if (base2 == null)
                        {
                            continue;
                        }
                        if (!(base2 is MyPlanet) && !base2.Closed)
                        {
                            list.Add(new MyEntityListInfoItem(base2.StorageName, base2.EntityId, 0, 0, 0f, base2.PositionComp.GetPosition(), 0f, MySession.GetPlayerDistance(base2, onlinePlayers), "", 0L, 0f, 0f));
                        }
                    }
                    return list;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (MyIdentity identity in MySession.Static.Players.GetAllIdentities())
            {
                MyPlayer.PlayerId id;
                string displayName = identity.DisplayName;
                if (Sync.Players.TryGetPlayerId(identity.IdentityId, out id))
                {
                    MyPlayer player = null;
                    if (!Sync.Players.TryGetPlayerById(id, out player))
                    {
                        object[] objArray1 = new object[] { displayName, " (", MyTexts.Get(MyCommonTexts.OfflineStatus), ")" };
                        displayName = string.Concat(objArray1);
                    }
                }
                if (identity.Character != null)
                {
                    list.Add(new MyEntityListInfoItem(displayName, identity.Character.EntityId, 0, 0, identity.Character.CurrentMass, identity.Character.PositionComp.GetPosition(), identity.Character.Physics.LinearVelocity.Length(), 0f, identity.DisplayName, identity.IdentityId, (float) ((int) (DateTime.Now - identity.LastLoginTime).TotalSeconds), (float) ((int) (DateTime.Now - identity.LastLogoutTime).TotalSeconds)));
                }
                else
                {
                    foreach (long num in identity.SavedCharacters)
                    {
                        MyCharacter character;
                        if (MyEntities.TryGetEntityById<MyCharacter>(num, out character, false))
                        {
                            list.Add(new MyEntityListInfoItem(displayName, num, 0, 0, character.CurrentMass, character.PositionComp.GetPosition(), character.Physics.LinearVelocity.Length(), 0f, identity.DisplayName, identity.IdentityId, (float) ((int) (DateTime.Now - identity.LastLoginTime).TotalSeconds), (float) ((int) (DateTime.Now - identity.LastLogoutTime).TotalSeconds)));
                        }
                    }
                }
            }
            return list;
        }

        public static StringBuilder GetFormattedDisplayName(MyEntitySortOrder selectedOrder, MyEntityListInfoItem item, bool isGrid)
        {
            StringBuilder output = new StringBuilder(item.DisplayName);
            switch (selectedOrder)
            {
                case MyEntitySortOrder.DisplayName:
                    break;

                case MyEntitySortOrder.BlockCount:
                    if (isGrid)
                    {
                        output.Append(" | " + item.BlockCount);
                    }
                    break;

                case MyEntitySortOrder.Mass:
                    output.Append(" | ");
                    if (item.Mass == 0f)
                    {
                        output.Append("-");
                    }
                    else
                    {
                        MyValueFormatter.AppendWeightInBestUnit(item.Mass, output);
                    }
                    break;

                case MyEntitySortOrder.OwnerName:
                    if (isGrid)
                    {
                        output.Append(" | " + (string.IsNullOrEmpty(item.OwnerName) ? MyTexts.GetString(MySpaceTexts.BlockOwner_Nobody) : item.OwnerName));
                    }
                    break;

                case MyEntitySortOrder.DistanceFromCenter:
                    output.Append(" | ");
                    MyValueFormatter.AppendDistanceInBestUnit((float) item.Position.Length(), output);
                    break;

                case MyEntitySortOrder.Speed:
                    output.Append(" | " + item.Speed.ToString("0.### m/s"));
                    break;

                case MyEntitySortOrder.DistanceFromPlayers:
                    output.Append(" | ");
                    MyValueFormatter.AppendDistanceInBestUnit(item.DistanceFromPlayers, output);
                    break;

                case MyEntitySortOrder.OwnerLastLogout:
                    if (isGrid)
                    {
                        output.Append(" | " + (string.IsNullOrEmpty(item.OwnerName) ? MyTexts.GetString(MySpaceTexts.BlockOwner_Nobody) : item.OwnerName));
                        output.Append(": ");
                        MyValueFormatter.AppendTimeInBestUnit(item.OwnerLogoutTime, output);
                    }
                    break;

                case MyEntitySortOrder.PCU:
                    if (isGrid)
                    {
                        output.Append(" | " + item.PCU);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return output;
        }

        private static void Power(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                grid.ChangePowerProducerState(MyMultipleEnabledEnum.AllEnabled, -1L);
            }
        }

        public static void ProceedEntityAction(MyEntity entity, EntityListAction action)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid == null)
            {
                ProceedEntityActionInternal(entity, action);
            }
            else
            {
                if (action == EntityListAction.Remove)
                {
                    grid.DismountAllCockpits();
                }
                ProceedEntityActionHierarchy(MyGridPhysicalHierarchy.Static.GetRoot(grid), action);
            }
        }

        private static void ProceedEntityActionHierarchy(MyCubeGrid grid, EntityListAction action)
        {
            MyGridPhysicalHierarchy.Static.ApplyOnChildren(grid, x => ProceedEntityActionHierarchy(x, action));
            ProceedEntityActionInternal(grid, action);
        }

        private static void ProceedEntityActionInternal(MyEntity entity, EntityListAction action)
        {
            switch (action)
            {
                case EntityListAction.Remove:
                    entity.Close();
                    return;

                case EntityListAction.Stop:
                    Stop(entity);
                    return;

                case EntityListAction.Depower:
                    Depower(entity);
                    return;

                case EntityListAction.Power:
                    Power(entity);
                    return;
            }
        }

        public static void SortEntityList(MyEntitySortOrder selectedOrder, ref List<MyEntityListInfoItem> items, bool invertOrder)
        {
            switch (selectedOrder)
            {
                case MyEntitySortOrder.DisplayName:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCultureIgnoreCase);
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.BlockCount:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = b.BlockCount.CompareTo(a.BlockCount);
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.Mass:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        if (a.Mass == b.Mass)
                        {
                            return 0;
                        }
                        int num = (a.Mass != 0f) ? ((b.Mass != 0f) ? b.Mass.CompareTo(a.Mass) : 1) : -1;
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.OwnerName:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = string.Compare(a.OwnerName, b.OwnerName, StringComparison.CurrentCultureIgnoreCase);
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.DistanceFromCenter:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = a.Position.LengthSquared().CompareTo(b.Position.LengthSquared());
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.Speed:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = b.Speed.CompareTo(a.Speed);
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.DistanceFromPlayers:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = b.DistanceFromPlayers.CompareTo(a.DistanceFromPlayers);
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.OwnerLastLogout:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = b.OwnerLogoutTime.CompareTo(a.OwnerLogoutTime);
                        return !invertOrder ? num : -num;
                    });
                    return;

                case MyEntitySortOrder.PCU:
                    items.Sort(delegate (MyEntityListInfoItem a, MyEntityListInfoItem b) {
                        int num = b.PCU.CompareTo(a.PCU);
                        return !invertOrder ? num : -num;
                    });
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        private static void Stop(MyEntity entity)
        {
            if (entity.Physics != null)
            {
                entity.Physics.LinearVelocity = Vector3.Zero;
                entity.Physics.AngularVelocity = Vector3.Zero;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEntityList.<>c <>9 = new MyEntityList.<>c();
            public static Action<MyCubeGrid> <>9__15_0;

            internal void <AccountChildren>b__15_0(MyCubeGrid childGrid)
            {
                MyEntityList.MyEntityListInfoItem item;
                MyEntityList.CreateListInfoForGrid(childGrid, out item);
                MyEntityList.m_gridItem.Add(ref item);
                MyEntityList.AccountChildren(childGrid);
            }
        }

        public enum EntityListAction
        {
            Remove,
            Stop,
            Depower,
            Power
        }

        public class MyEntityListInfoItem
        {
            public string DisplayName;
            public long EntityId;
            public int BlockCount;
            public int PCU;
            public float Mass;
            public Vector3D Position;
            public string OwnerName;
            public long Owner;
            public float Speed;
            public float DistanceFromPlayers;
            public float OwnerLoginTime;
            public float OwnerLogoutTime;

            public MyEntityListInfoItem()
            {
            }

            public MyEntityListInfoItem(string displayName, long entityId, int blockCount, int pcu, float mass, Vector3D position, float speed, float distanceFromPlayers, string ownerName, long owner, float ownerLogin, float ownerLogout)
            {
                this.DisplayName = !string.IsNullOrEmpty(displayName) ? ((displayName.Length < 50) ? displayName : displayName.Substring(0, 0x31)) : "----";
                this.EntityId = entityId;
                this.BlockCount = blockCount;
                this.PCU = pcu;
                this.Mass = mass;
                this.Position = position;
                this.OwnerName = ownerName;
                this.Owner = owner;
                this.Speed = speed;
                this.DistanceFromPlayers = distanceFromPlayers;
                this.OwnerLoginTime = ownerLogin;
                this.OwnerLogoutTime = ownerLogout;
            }

            public void Add(ref MyEntityList.MyEntityListInfoItem item)
            {
                this.BlockCount += item.BlockCount;
                this.PCU += item.PCU;
                this.Mass += item.Mass;
                this.OwnerLoginTime = Math.Min(item.OwnerLoginTime, this.OwnerLoginTime);
                this.OwnerLogoutTime = Math.Min(item.OwnerLogoutTime, this.OwnerLogoutTime);
            }
        }

        public enum MyEntitySortOrder
        {
            DisplayName,
            BlockCount,
            Mass,
            OwnerName,
            DistanceFromCenter,
            Speed,
            DistanceFromPlayers,
            OwnerLastLogout,
            PCU
        }

        public enum MyEntityTypeEnum
        {
            Grids,
            SmallGrids,
            LargeGrids,
            Characters,
            FloatingObjects,
            Planets,
            Asteroids
        }
    }
}

