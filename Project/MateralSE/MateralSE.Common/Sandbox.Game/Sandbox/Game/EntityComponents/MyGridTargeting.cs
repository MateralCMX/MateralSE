namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRageMath;

    public class MyGridTargeting : MyEntityComponentBase
    {
        private MyCubeGrid m_grid;
        private BoundingSphere m_queryLocal = new BoundingSphere(Vector3.Zero, float.MinValue);
        private List<VRage.Game.Entity.MyEntity> m_targetRoots = new List<VRage.Game.Entity.MyEntity>();
        private MyListDictionary<MyCubeGrid, VRage.Game.Entity.MyEntity> m_targetBlocks = new MyListDictionary<MyCubeGrid, VRage.Game.Entity.MyEntity>();
        private List<long> m_ownersB = new List<long>();
        private List<long> m_ownersA = new List<long>();
        private static FastResourceLock m_scanLock = new FastResourceLock();
        private int m_lastScan;
        public bool AllowScanning = true;

        private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) => 
            (potentialDescendant.IsSubclassOf(potentialBase) || (potentialDescendant == potentialBase));

        private void m_grid_OnBlockAdded(MySlimBlock obj)
        {
            MyLargeTurretBase fatBlock = obj.FatBlock as MyLargeTurretBase;
            if (fatBlock != null)
            {
                this.m_queryLocal.Include(new BoundingSphere(obj.FatBlock.PositionComp.LocalMatrix.Translation, fatBlock.SearchingRange));
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_grid = base.Entity as MyCubeGrid;
            this.m_grid.OnBlockAdded += new Action<MySlimBlock>(this.m_grid_OnBlockAdded);
        }

        public void RescanIfNeeded()
        {
            if (this.AllowScanning && ((MySession.Static.GameplayFrameCounter - this.m_lastScan) > 100))
            {
                this.Scan();
            }
        }

        private void Scan()
        {
            using (m_scanLock.AcquireExclusiveUsing())
            {
                BoundingSphereD ed;
                int count;
                int num2;
                MyCubeGrid grid;
                bool flag;
                List<long>.Enumerator enumerator;
                if (!this.AllowScanning)
                {
                    return;
                }
                else if ((MySession.Static.GameplayFrameCounter - this.m_lastScan) > 100)
                {
                    this.m_lastScan = MySession.Static.GameplayFrameCounter;
                    ed = new BoundingSphereD(Vector3D.Transform(this.m_queryLocal.Center, this.m_grid.WorldMatrix), (double) this.m_queryLocal.Radius);
                    this.m_targetRoots.Clear();
                    this.m_targetBlocks.Clear();
                    MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref ed, this.m_targetRoots, MyEntityQueryType.Both);
                    MyMissiles.GetAllMissilesInSphere(ref ed, this.m_targetRoots);
                    count = this.m_targetRoots.Count;
                    this.m_ownersA.AddList<long>(this.m_grid.SmallOwners);
                    this.m_ownersA.AddList<long>(this.m_grid.BigOwners);
                    num2 = 0;
                }
                else
                {
                    return;
                }
                goto TR_0047;
            TR_000E:
                num2++;
                goto TR_0047;
            TR_0028:
                if (flag)
                {
                    List<VRage.Game.Entity.MyEntity> orAdd = this.m_targetBlocks.GetOrAdd(grid);
                    using (grid.Pin())
                    {
                        if (!grid.MarkedForClose)
                        {
                            grid.Hierarchy.QuerySphere(ref ed, orAdd);
                        }
                        goto TR_000E;
                    }
                }
                foreach (MyCubeBlock block in grid.GetFatBlocks())
                {
                    MyIDModule module;
                    IMyComponentOwner<MyIDModule> owner = block as IMyComponentOwner<MyIDModule>;
                    if ((owner != null) && owner.GetComponent(out module))
                    {
                        long ownerId = block.OwnerId;
                        using (enumerator = this.m_ownersA.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (MyIDModule.GetRelation(enumerator.Current, ownerId, MyOwnershipShareModeEnum.None, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare) == MyRelationsBetweenPlayerAndBlock.Enemies)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                if (flag)
                {
                    List<VRage.Game.Entity.MyEntity> orAdd = this.m_targetBlocks.GetOrAdd(grid);
                    if (!grid.Closed)
                    {
                        grid.Hierarchy.QuerySphere(ref ed, orAdd);
                    }
                }
                goto TR_000E;
            TR_0047:
                while (true)
                {
                    if (num2 < count)
                    {
                        grid = this.m_targetRoots[num2] as MyCubeGrid;
                        if (grid == null)
                        {
                            goto TR_000E;
                        }
                        else if ((grid.Physics == null) || grid.Physics.Enabled)
                        {
                            flag = false;
                            if ((grid.BigOwners.Count == 0) && (grid.SmallOwners.Count == 0))
                            {
                                using (enumerator = this.m_ownersA.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        if (MyIDModule.GetRelation(enumerator.Current, 0L, MyOwnershipShareModeEnum.None, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare) == MyRelationsBetweenPlayerAndBlock.Enemies)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    goto TR_0028;
                                }
                            }
                            this.m_ownersB.AddList<long>(grid.BigOwners);
                            this.m_ownersB.AddList<long>(grid.SmallOwners);
                            foreach (long num3 in this.m_ownersA)
                            {
                                foreach (long num4 in this.m_ownersB)
                                {
                                    if (MyIDModule.GetRelation(num3, num4, MyOwnershipShareModeEnum.None, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare) == MyRelationsBetweenPlayerAndBlock.Enemies)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    break;
                                }
                            }
                            this.m_ownersB.Clear();
                        }
                        else
                        {
                            goto TR_000E;
                        }
                        goto TR_0028;
                    }
                    else
                    {
                        this.m_ownersA.Clear();
                        for (int i = this.m_targetRoots.Count - 1; i >= 0; i--)
                        {
                            VRage.Game.Entity.MyEntity entity = this.m_targetRoots[i];
                            if (((entity is MyDebrisBase) || ((entity is MyFloatingObject) || (((entity.Physics != null) && !entity.Physics.Enabled) || (entity.GetTopMostParent(null).Physics == null)))) || !entity.GetTopMostParent(null).Physics.Enabled)
                            {
                                this.m_targetRoots.RemoveAtFast<VRage.Game.Entity.MyEntity>(i);
                            }
                        }
                    }
                    break;
                }
            }
        }

        public List<VRage.Game.Entity.MyEntity> TargetRoots
        {
            get
            {
                if (this.AllowScanning && ((MySession.Static.GameplayFrameCounter - this.m_lastScan) > 100))
                {
                    this.Scan();
                }
                return this.m_targetRoots;
            }
        }

        public MyListDictionary<MyCubeGrid, VRage.Game.Entity.MyEntity> TargetBlocks
        {
            get
            {
                if (this.AllowScanning && ((MySession.Static.GameplayFrameCounter - this.m_lastScan) > 100))
                {
                    this.Scan();
                }
                return this.m_targetBlocks;
            }
        }

        public override string ComponentTypeDebugString =>
            "MyGridTargeting";
    }
}

