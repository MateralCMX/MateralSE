namespace Sandbox.Game.World.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Noise;
    using VRage.Noise.Combiners;
    using VRageMath;

    public abstract class MyProceduralWorldModule
    {
        protected MyProceduralWorldModule m_parent;
        protected List<MyProceduralWorldModule> m_children = new List<MyProceduralWorldModule>();
        protected int m_seed;
        protected double m_objectDensity;
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);
        protected Dictionary<Vector3I, MyProceduralCell> m_cells = new Dictionary<Vector3I, MyProceduralCell>();
        protected CachingHashSet<MyProceduralCell> m_dirtyCells = new CachingHashSet<MyProceduralCell>();
        protected static List<MyObjectSeed> m_tempObjectSeedList = new List<MyObjectSeed>();
        protected static List<MyProceduralCell> m_tempProceduralCellsList = new List<MyProceduralCell>();
        protected List<IMyAsteroidFieldDensityFunction> m_densityFunctionsFilled = new List<IMyAsteroidFieldDensityFunction>();
        protected List<IMyAsteroidFieldDensityFunction> m_densityFunctionsRemoved = new List<IMyAsteroidFieldDensityFunction>();
        public readonly double CELL_SIZE;
        public readonly int SCALE;
        protected const int BIG_PRIME1 = 0x1001fff;
        protected const int BIG_PRIME2 = 0x2611501;
        protected const int BIG_PRIME3 = 0x1c8cfbff;
        protected const int TWIN_PRIME_MIDDLE1 = 240;
        protected const int TWIN_PRIME_MIDDLE2 = 0x138;
        protected const int TWIN_PRIME_MIDDLE3 = 0x1ce;
        private List<IMyModule> tmpDensityFunctions = new List<IMyModule>();

        protected MyProceduralWorldModule(double cellSize, int radiusMultiplier, int seed, double density, MyProceduralWorldModule parent = null)
        {
            this.CELL_SIZE = cellSize;
            this.SCALE = radiusMultiplier;
            this.m_seed = seed;
            this.m_objectDensity = density;
            this.m_parent = parent;
            if (parent != null)
            {
                parent.m_children.Add(this);
            }
        }

        protected void AddDensityFunctionFilled(IMyAsteroidFieldDensityFunction func)
        {
            this.m_densityFunctionsFilled.Add(func);
        }

        public void AddDensityFunctionRemoved(IMyAsteroidFieldDensityFunction func)
        {
            List<IMyAsteroidFieldDensityFunction> densityFunctionsRemoved = this.m_densityFunctionsRemoved;
            lock (densityFunctionsRemoved)
            {
                this.m_densityFunctionsRemoved.Add(func);
            }
        }

        protected void ChildrenAddDensityFunctionFilled(IMyAsteroidFieldDensityFunction func)
        {
            foreach (MyProceduralWorldModule local1 in this.m_children)
            {
                local1.AddDensityFunctionFilled(func);
                local1.ChildrenAddDensityFunctionFilled(func);
            }
        }

        protected void ChildrenAddDensityFunctionRemoved(IMyAsteroidFieldDensityFunction func)
        {
            foreach (MyProceduralWorldModule local1 in this.m_children)
            {
                local1.AddDensityFunctionRemoved(func);
                local1.ChildrenAddDensityFunctionRemoved(func);
            }
        }

        protected void ChildrenRemoveDensityFunctionFilled(IMyAsteroidFieldDensityFunction func)
        {
            foreach (MyProceduralWorldModule local1 in this.m_children)
            {
                local1.ChildrenRemoveDensityFunctionFilled(func);
                local1.RemoveDensityFunctionFilled(func);
            }
        }

        protected void ChildrenRemoveDensityFunctionRemoved(IMyAsteroidFieldDensityFunction func)
        {
            foreach (MyProceduralWorldModule local1 in this.m_children)
            {
                local1.ChildrenRemoveDensityFunctionRemoved(func);
                local1.RemoveDensityFunctionRemoved(func);
            }
        }

        protected abstract void CloseObjectSeed(MyObjectSeed objectSeed);
        public abstract void GenerateObjects(List<MyObjectSeed> list, HashSet<MyObjectSeedParams> existingObjectsSeeds);
        protected void GenerateObjectSeeds(ref BoundingSphereD sphere)
        {
            Vector3I_RangeIterator cellsIterator = this.GetCellsIterator(sphere);
            while (cellsIterator.IsValid())
            {
                Vector3I current = cellsIterator.Current;
                if (!this.m_cells.ContainsKey(current))
                {
                    BoundingBoxD box = new BoundingBoxD((Vector3D) (current * this.CELL_SIZE), (current + 1) * this.CELL_SIZE);
                    if (sphere.Contains(box) != ContainmentType.Disjoint)
                    {
                        MyProceduralCell cell = this.GenerateProceduralCell(ref current);
                        if (cell != null)
                        {
                            this.m_cells.Add(current, cell);
                            BoundingBoxD boundingVolume = cell.BoundingVolume;
                            cell.proxyId = this.m_cellsTree.AddProxy(ref boundingVolume, cell, 0, true);
                        }
                    }
                }
                cellsIterator.MoveNext();
            }
        }

        protected abstract MyProceduralCell GenerateProceduralCell(ref Vector3I cellId);
        internal void GetAllCells(List<MyProceduralCell> list)
        {
            this.m_cellsTree.GetAll<MyProceduralCell>(list, false, null);
        }

        protected IMyModule GetCellDensityFunctionFilled(BoundingBoxD bbox)
        {
            foreach (IMyAsteroidFieldDensityFunction function in this.m_densityFunctionsFilled)
            {
                if (function.ExistsInCell(ref bbox))
                {
                    this.tmpDensityFunctions.Add(function);
                }
            }
            if (this.tmpDensityFunctions.Count == 0)
            {
                return null;
            }
            int count = this.tmpDensityFunctions.Count;
            while (count > 1)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= (count / 2))
                    {
                        if ((count % 2) == 1)
                        {
                            this.tmpDensityFunctions[count - 1] = this.tmpDensityFunctions[count / 2];
                        }
                        count = (count / 2) + (count % 2);
                        break;
                    }
                    this.tmpDensityFunctions[num2] = new MyMax(this.tmpDensityFunctions[num2 * 2], this.tmpDensityFunctions[(num2 * 2) + 1]);
                    num2++;
                }
            }
            this.tmpDensityFunctions.Clear();
            return this.tmpDensityFunctions[0];
        }

        protected IMyModule GetCellDensityFunctionRemoved(BoundingBoxD bbox)
        {
            foreach (IMyAsteroidFieldDensityFunction function in this.m_densityFunctionsRemoved)
            {
                if (function == null)
                {
                    continue;
                }
                if (function.ExistsInCell(ref bbox))
                {
                    this.tmpDensityFunctions.Add(function);
                }
            }
            if (this.tmpDensityFunctions.Count == 0)
            {
                return null;
            }
            int count = this.tmpDensityFunctions.Count;
            while (count > 1)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= (count / 2))
                    {
                        if ((count % 2) == 1)
                        {
                            this.tmpDensityFunctions[count - 1] = this.tmpDensityFunctions[count / 2];
                        }
                        count = (count / 2) + (count % 2);
                        break;
                    }
                    this.tmpDensityFunctions[num2] = new MyMin(this.tmpDensityFunctions[num2 * 2], this.tmpDensityFunctions[(num2 * 2) + 1]);
                    num2++;
                }
            }
            this.tmpDensityFunctions.Clear();
            return this.tmpDensityFunctions[0];
        }

        protected int GetCellSeed(ref Vector3I cell) => 
            (((this.m_seed + (cell.X * 0x1001fff)) + (cell.Y * 0x2611501)) + (cell.Z * 0x1c8cfbff));

        protected Vector3I_RangeIterator GetCellsIterator(BoundingBoxD bbox)
        {
            Vector3I start = Vector3I.Floor(bbox.Min / this.CELL_SIZE);
            Vector3I end = Vector3I.Floor(bbox.Max / this.CELL_SIZE);
            return new Vector3I_RangeIterator(ref start, ref end);
        }

        protected Vector3I_RangeIterator GetCellsIterator(BoundingSphereD sphere) => 
            this.GetCellsIterator(BoundingBoxD.CreateFromSphere(sphere));

        protected int GetObjectIdSeed(MyObjectSeed objectSeed) => 
            ((((((objectSeed.CellId.GetHashCode() * 0x18d) ^ this.m_seed) * 0x18d) ^ objectSeed.Params.Index) * 0x18d) ^ objectSeed.Params.Seed);

        public unsafe void GetObjectSeeds(BoundingSphereD sphere, List<MyObjectSeed> list, bool scale = true)
        {
            BoundingSphereD ed = sphere;
            if (scale)
            {
                double* numPtr1 = (double*) ref ed.Radius;
                numPtr1[0] *= this.SCALE;
            }
            this.GenerateObjectSeeds(ref ed);
            this.OverlapAllBoundingSphere(ref ed, list);
        }

        public unsafe void MarkCellsDirty(BoundingSphereD toMark, BoundingSphereD? toExclude = new BoundingSphereD?(), bool scale = true)
        {
            BoundingSphereD ed;
            BoundingSphereD* edPtr1 = (BoundingSphereD*) new BoundingSphereD(toMark.Center, toMark.Radius * (scale ? ((double) this.SCALE) : ((double) 1)));
            BoundingSphereD ed2 = new BoundingSphereD();
            if (toExclude != null)
            {
                ed2 = toExclude.Value;
                if (scale)
                {
                    double* numPtr1 = (double*) ref ed2.Radius;
                    numPtr1[0] *= this.SCALE;
                }
            }
            edPtr1 = (BoundingSphereD*) ref ed;
            Vector3I_RangeIterator cellsIterator = this.GetCellsIterator(ed);
            while (cellsIterator.IsValid())
            {
                MyProceduralCell cell;
                Vector3I current = cellsIterator.Current;
                if (this.m_cells.TryGetValue(current, out cell) && ((toExclude == null) || (ed2.Contains(cell.BoundingVolume) == ContainmentType.Disjoint)))
                {
                    this.m_dirtyCells.Add(cell);
                }
                cellsIterator.MoveNext();
            }
        }

        protected void OverlapAllBoundingBox(ref BoundingBoxD box, List<MyObjectSeed> list)
        {
            this.m_cellsTree.OverlapAllBoundingBox<MyProceduralCell>(ref box, m_tempProceduralCellsList, 0, true);
            using (List<MyProceduralCell>.Enumerator enumerator = m_tempProceduralCellsList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OverlapAllBoundingBox(ref box, list, false);
                }
            }
            m_tempProceduralCellsList.Clear();
        }

        protected void OverlapAllBoundingSphere(ref BoundingSphereD sphere, List<MyObjectSeed> list)
        {
            this.m_cellsTree.OverlapAllBoundingSphere<MyProceduralCell>(ref sphere, m_tempProceduralCellsList, true);
            using (List<MyProceduralCell>.Enumerator enumerator = m_tempProceduralCellsList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OverlapAllBoundingSphere(ref sphere, list, false);
                }
            }
            m_tempProceduralCellsList.Clear();
        }

        public unsafe void ProcessDirtyCells(Dictionary<MyEntity, MyEntityTracker> trackedEntities)
        {
            this.m_dirtyCells.ApplyAdditions();
            if (this.m_dirtyCells.Count != 0)
            {
                foreach (MyProceduralCell cell in this.m_dirtyCells)
                {
                    using (Dictionary<MyEntity, MyEntityTracker>.ValueCollection.Enumerator enumerator2 = trackedEntities.Values.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            BoundingSphereD boundingVolume = enumerator2.Current.BoundingVolume;
                            double* numPtr1 = (double*) ref boundingVolume.Radius;
                            numPtr1[0] *= this.SCALE;
                            if (boundingVolume.Contains(cell.BoundingVolume) != ContainmentType.Disjoint)
                            {
                                this.m_dirtyCells.Remove(cell, false);
                                break;
                            }
                        }
                    }
                }
                this.m_dirtyCells.ApplyRemovals();
                using (HashSet<MyProceduralCell>.Enumerator enumerator = this.m_dirtyCells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.GetAll(m_tempObjectSeedList, true);
                        foreach (MyObjectSeed seed in m_tempObjectSeedList)
                        {
                            if (seed.Params.Generated)
                            {
                                this.CloseObjectSeed(seed);
                            }
                        }
                        m_tempObjectSeedList.Clear();
                    }
                }
                foreach (MyProceduralCell cell2 in this.m_dirtyCells)
                {
                    this.m_cells.Remove(cell2.CellId);
                    this.m_cellsTree.RemoveProxy(cell2.proxyId);
                }
                this.m_dirtyCells.Clear();
            }
        }

        protected void RemoveDensityFunctionFilled(IMyAsteroidFieldDensityFunction func)
        {
            this.m_densityFunctionsFilled.Remove(func);
        }

        protected void RemoveDensityFunctionRemoved(IMyAsteroidFieldDensityFunction func)
        {
            List<IMyAsteroidFieldDensityFunction> densityFunctionsRemoved = this.m_densityFunctionsRemoved;
            lock (densityFunctionsRemoved)
            {
                this.m_densityFunctionsRemoved.Remove(func);
            }
        }
    }
}

