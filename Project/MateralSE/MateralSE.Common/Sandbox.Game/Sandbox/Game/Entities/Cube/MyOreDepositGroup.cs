namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Voxels;
    using VRageMath;

    internal class MyOreDepositGroup
    {
        private readonly MyVoxelBase m_voxelMap;
        private readonly Action<List<MyEntityOreDeposit>, List<Vector3I>, MyOreDetectorComponent> m_onDepositQueryComplete;
        private Dictionary<Vector3I, MyEntityOreDeposit> m_depositsByCellCoord_Main = new Dictionary<Vector3I, MyEntityOreDeposit>(Vector3I.Comparer);
        private Dictionary<Vector3I, MyEntityOreDeposit> m_depositsByCellCoord_Swap = new Dictionary<Vector3I, MyEntityOreDeposit>(Vector3I.Comparer);
        private Vector3I m_lastDetectionMin;
        private Vector3I m_lastDetectionMax;
        private int m_tasksRunning;

        public MyOreDepositGroup(MyVoxelBase voxelMap)
        {
            this.m_voxelMap = voxelMap;
            this.m_onDepositQueryComplete = new Action<List<MyEntityOreDeposit>, List<Vector3I>, MyOreDetectorComponent>(this.OnDepositQueryComplete);
            this.m_lastDetectionMax = new Vector3I(-2147483648);
            this.m_lastDetectionMin = new Vector3I(0x7fffffff);
        }

        public void ClearMinMax()
        {
            this.m_lastDetectionMin = this.m_lastDetectionMax = Vector3I.Zero;
        }

        private void OnDepositQueryComplete(List<MyEntityOreDeposit> deposits, List<Vector3I> emptyCells, MyOreDetectorComponent detectorComponent)
        {
            foreach (MyEntityOreDeposit deposit in deposits)
            {
                Vector3I cellCoord = deposit.CellCoord;
                this.m_depositsByCellCoord_Swap[cellCoord] = deposit;
            }
            this.m_tasksRunning--;
            if (this.m_tasksRunning == 0)
            {
                if (detectorComponent == null)
                {
                    goto TR_000C;
                }
                else if (!detectorComponent.WillDiscardNextQuery)
                {
                    Dictionary<Vector3I, MyEntityOreDeposit> dictionary = this.m_depositsByCellCoord_Main;
                    this.m_depositsByCellCoord_Main = this.m_depositsByCellCoord_Swap;
                    this.m_depositsByCellCoord_Swap = dictionary;
                    foreach (MyEntityOreDeposit deposit4 in this.m_depositsByCellCoord_Swap.Values)
                    {
                        MyHud.OreMarkers.UnregisterMarker(deposit4);
                    }
                    this.m_depositsByCellCoord_Swap.Clear();
                    foreach (MyEntityOreDeposit deposit5 in this.m_depositsByCellCoord_Main.Values)
                    {
                        MyHud.OreMarkers.RegisterMarker(deposit5);
                    }
                }
                else
                {
                    goto TR_000C;
                }
            }
            return;
        TR_000C:
            foreach (MyEntityOreDeposit deposit2 in this.m_depositsByCellCoord_Main.Values)
            {
                MyHud.OreMarkers.UnregisterMarker(deposit2);
            }
            foreach (MyEntityOreDeposit deposit3 in this.m_depositsByCellCoord_Swap.Values)
            {
                MyHud.OreMarkers.UnregisterMarker(deposit3);
            }
            this.m_depositsByCellCoord_Main.Clear();
            this.m_depositsByCellCoord_Swap.Clear();
        }

        internal void RemoveMarks()
        {
            foreach (MyEntityOreDeposit deposit in this.m_depositsByCellCoord_Main.Values)
            {
                MyHud.OreMarkers.UnregisterMarker(deposit);
            }
        }

        public void UpdateDeposits(ref BoundingSphereD worldDetectionSphere, long detectorId, MyOreDetectorComponent detectorComponent)
        {
            if (this.m_tasksRunning == 0)
            {
                MySession @static = MySession.Static;
                if ((@static != null) && @static.Ready)
                {
                    Vector3I vectori;
                    Vector3I vectori2;
                    Vector3D worldPosition = worldDetectionSphere.Center - worldDetectionSphere.Radius;
                    Vector3D vectord2 = worldDetectionSphere.Center + worldDetectionSphere.Radius;
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.m_voxelMap.PositionLeftBottomCorner, ref worldPosition, out vectori);
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.m_voxelMap.PositionLeftBottomCorner, ref vectord2, out vectori2);
                    vectori = (Vector3I) (vectori + this.m_voxelMap.StorageMin);
                    vectori2 = (Vector3I) (vectori2 + this.m_voxelMap.StorageMin);
                    this.m_voxelMap.Storage.ClampVoxelCoord(ref vectori, 1);
                    this.m_voxelMap.Storage.ClampVoxelCoord(ref vectori2, 1);
                    vectori = vectori >> 5;
                    vectori2 = vectori2 >> 5;
                    if ((vectori != this.m_lastDetectionMin) || (vectori2 != this.m_lastDetectionMax))
                    {
                        Vector3I vectori3;
                        Vector3I vectori4;
                        this.m_lastDetectionMin = vectori;
                        this.m_lastDetectionMax = vectori2;
                        int num = Math.Max((vectori2.X - vectori.X) / 2, 1);
                        int num2 = Math.Max((vectori2.Y - vectori.Y) / 2, 1);
                        vectori3.Z = vectori.Z;
                        vectori4.Z = vectori2.Z;
                        int num3 = 0;
                        while (num3 < 2)
                        {
                            int num4 = 0;
                            while (true)
                            {
                                if (num4 >= 2)
                                {
                                    num3++;
                                    break;
                                }
                                vectori3.X = vectori.X + (num3 * num);
                                vectori3.Y = vectori.Y + (num4 * num2);
                                vectori4.X = vectori3.X + num;
                                vectori4.Y = vectori3.Y + num2;
                                MyDepositQuery.Start(vectori3, vectori4, detectorId, this.m_voxelMap, this.m_onDepositQueryComplete, detectorComponent);
                                this.m_tasksRunning++;
                                num4++;
                            }
                        }
                    }
                }
            }
        }

        public ICollection<MyEntityOreDeposit> Deposits =>
            this.m_depositsByCellCoord_Main.Values;
    }
}

