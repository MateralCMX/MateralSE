namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyOreDetectorComponent
    {
        public const int QUERY_LOD = 2;
        public const int CELL_SIZE_IN_VOXELS_BITS = 3;
        public const int CELL_SIZE_IN_LOD_VOXELS = 8;
        public const float CELL_SIZE_IN_METERS = 32f;
        public const float CELL_SIZE_IN_METERS_HALF = 16f;
        private static readonly List<MyVoxelBase> m_inRangeCache = new List<MyVoxelBase>();
        private static readonly List<MyVoxelBase> m_notInRangeCache = new List<MyVoxelBase>();
        public CheckControlDelegate OnCheckControl;
        private readonly Dictionary<MyVoxelBase, MyOreDepositGroup> m_depositGroupsByEntity = new Dictionary<MyVoxelBase, MyOreDepositGroup>();
        private bool m_discardQueryResult;

        public MyOreDetectorComponent()
        {
            this.DetectionRadius = 50f;
            this.SetRelayedRequest = false;
            this.BroadcastUsingAntennas = false;
        }

        private void AddVoxelMapsInRange()
        {
            foreach (MyVoxelBase base2 in m_inRangeCache)
            {
                if (!this.m_depositGroupsByEntity.ContainsKey(base2.GetTopMostParent(null) as MyVoxelBase))
                {
                    this.m_depositGroupsByEntity.Add(base2, new MyOreDepositGroup(base2));
                }
            }
            m_inRangeCache.Clear();
        }

        public void Clear()
        {
            foreach (MyOreDepositGroup local1 in this.m_depositGroupsByEntity.Values)
            {
                local1.ClearMinMax();
                IEnumerator<MyEntityOreDeposit> enumerator = local1.Deposits.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        MyEntityOreDeposit current = enumerator.Current;
                        MyHud.OreMarkers.UnregisterMarker(current);
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
        }

        public void DiscardNextQuery()
        {
            this.m_discardQueryResult = true;
        }

        public void EnableNextQuery()
        {
            this.m_discardQueryResult = false;
        }

        private void RemoveVoxelMapsOutOfRange()
        {
            foreach (MyVoxelBase base2 in this.m_depositGroupsByEntity.Keys)
            {
                if (!m_inRangeCache.Contains(base2.GetTopMostParent(null) as MyVoxelBase))
                {
                    m_notInRangeCache.Add(base2);
                }
            }
            foreach (MyVoxelBase base3 in m_notInRangeCache)
            {
                MyOreDepositGroup group;
                if (this.m_depositGroupsByEntity.TryGetValue(base3, out group))
                {
                    group.RemoveMarks();
                }
                this.m_depositGroupsByEntity.Remove(base3);
            }
            m_notInRangeCache.Clear();
        }

        public void Update(Vector3D position, long detectorId, bool checkControl = true)
        {
            if ((!this.SetRelayedRequest & checkControl) && !this.OnCheckControl())
            {
                this.Clear();
            }
            else
            {
                this.SetRelayedRequest = false;
                BoundingSphereD sphere = new BoundingSphereD(position, (double) this.DetectionRadius);
                MyGamePruningStructure.GetAllVoxelMapsInSphere(ref sphere, m_inRangeCache);
                this.RemoveVoxelMapsOutOfRange();
                this.AddVoxelMapsInRange();
                this.UpdateDeposits(ref sphere, detectorId);
                m_inRangeCache.Clear();
            }
        }

        private void UpdateDeposits(ref BoundingSphereD sphere, long detectorId)
        {
            using (Dictionary<MyVoxelBase, MyOreDepositGroup>.ValueCollection.Enumerator enumerator = this.m_depositGroupsByEntity.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateDeposits(ref sphere, detectorId, this);
                }
            }
        }

        public float DetectionRadius { get; set; }

        public bool BroadcastUsingAntennas { get; set; }

        public bool SetRelayedRequest { get; set; }

        public bool WillDiscardNextQuery =>
            this.m_discardQueryResult;

        public delegate bool CheckControlDelegate();
    }
}

