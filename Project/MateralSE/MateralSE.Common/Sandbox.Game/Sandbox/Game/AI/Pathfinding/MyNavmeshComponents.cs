namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRageMath;

    public class MyNavmeshComponents
    {
        private Dictionary<ulong, CellInfo> m_cellInfos = new Dictionary<ulong, CellInfo>();
        private Dictionary<int, ulong> m_componentCells = new Dictionary<int, ulong>();
        private bool m_cellOpen = false;
        private bool m_componentOpen = false;
        private Dictionary<ushort, List<int>> m_componentIndicesAvailable = new Dictionary<ushort, List<int>>();
        private int m_nextComponentIndex = 0;
        private List<Vector3> m_lastCellComponentCenters = new List<Vector3>();
        private ulong m_cellCoord;
        private ushort m_componentNum;
        private List<MyIntervalList> m_components = null;

        public void AddComponentTriangle(MyNavigationTriangle triangle, Vector3 center)
        {
            MyIntervalList list = this.m_components[this.m_componentNum];
            list.Add(triangle.Index);
            float num2 = 1f / ((float) list.Count);
            this.m_lastCellComponentCenters[this.m_componentNum] = (center * num2) + (this.m_lastCellComponentCenters[this.m_componentNum] * (1f - num2));
        }

        private int AllocateComponentStartingIndex(ushort componentNum)
        {
            List<int> list = null;
            if (!this.m_componentIndicesAvailable.TryGetValue(componentNum, out list) || (list.Count <= 0))
            {
                this.m_nextComponentIndex += componentNum;
                return this.m_nextComponentIndex;
            }
            list.RemoveAt(list.Count - 1);
            return list[list.Count - 1];
        }

        public void ClearCell(ulong packedCoord, ref CellInfo cellInfo)
        {
            for (int i = 0; i < cellInfo.ComponentNum; i++)
            {
                this.m_componentCells.Remove(cellInfo.StartingIndex + i);
            }
            this.m_cellInfos.Remove(packedCoord);
            this.DeallocateComponentStartingIndex(cellInfo.StartingIndex, cellInfo.ComponentNum);
        }

        public void CloseAndCacheCell(ref ClosedCellInfo output)
        {
            CellInfo info = new CellInfo();
            bool flag = true;
            if (!this.m_cellInfos.TryGetValue(this.m_cellCoord, out info))
            {
                output.NewCell = true;
            }
            else
            {
                output.NewCell = false;
                output.OldComponentNum = info.ComponentNum;
                output.OldStartingIndex = info.StartingIndex;
                if (info.ComponentNum == this.m_componentNum)
                {
                    flag = false;
                    info.ComponentNum = output.OldComponentNum;
                    info.StartingIndex = output.OldStartingIndex;
                }
            }
            if (flag)
            {
                info.ComponentNum = this.m_componentNum;
                info.StartingIndex = this.AllocateComponentStartingIndex(this.m_componentNum);
                if (!output.NewCell)
                {
                    this.DeallocateComponentStartingIndex(output.OldStartingIndex, output.OldComponentNum);
                    for (int j = 0; j < output.OldComponentNum; j++)
                    {
                        this.m_componentCells.Remove(output.OldStartingIndex + j);
                    }
                }
                for (int i = 0; i < info.ComponentNum; i++)
                {
                    this.m_componentCells[info.StartingIndex + i] = this.m_cellCoord;
                }
            }
            this.m_cellInfos[this.m_cellCoord] = info;
            output.ComponentNum = info.ComponentNum;
            output.ExploredDirections = info.ExploredDirections;
            output.StartingIndex = info.StartingIndex;
            this.m_components = null;
            this.m_componentNum = 0;
            this.m_cellOpen = false;
        }

        public void CloseComponent()
        {
            this.m_componentNum = (ushort) (this.m_componentNum + 1);
            this.m_componentOpen = false;
        }

        private void DeallocateComponentStartingIndex(int index, ushort componentNum)
        {
            List<int> list = null;
            if (!this.m_componentIndicesAvailable.TryGetValue(componentNum, out list))
            {
                list = new List<int>();
                this.m_componentIndicesAvailable[componentNum] = list;
            }
            list.Add(index);
        }

        public bool GetComponentCell(int componentIndex, out ulong cellIndex) => 
            this.m_componentCells.TryGetValue(componentIndex, out cellIndex);

        public Vector3 GetComponentCenter(int index) => 
            this.m_lastCellComponentCenters[index];

        public bool GetComponentInfo(int componentIndex, ulong cellIndex, out Base6Directions.DirectionFlags exploredDirections)
        {
            CellInfo info;
            exploredDirections = 0;
            this.TryGetCell(cellIndex, out info);
            int num = componentIndex - info.StartingIndex;
            if ((num < 0) || (num >= info.ComponentNum))
            {
                return false;
            }
            exploredDirections = info.ExploredDirections;
            return true;
        }

        public ICollection<ulong> GetPresentCells() => 
            this.m_cellInfos.Keys;

        public unsafe void MarkExplored(ulong otherCell, Base6Directions.Direction direction)
        {
            CellInfo info = new CellInfo();
            if (this.m_cellInfos.TryGetValue(otherCell, out info))
            {
                Base6Directions.DirectionFlags* flagsPtr1 = (Base6Directions.DirectionFlags*) ref info.ExploredDirections;
                *((sbyte*) flagsPtr1) = *(((byte*) flagsPtr1)) | Base6Directions.GetDirectionFlag(direction);
                this.m_cellInfos[otherCell] = info;
            }
        }

        public void OpenCell(ulong cellCoord)
        {
            this.m_cellOpen = true;
            this.m_cellCoord = cellCoord;
            this.m_componentNum = 0;
            this.m_components = new List<MyIntervalList>();
            this.m_lastCellComponentCenters.Clear();
        }

        public void OpenComponent()
        {
            this.m_componentOpen = true;
            this.m_lastCellComponentCenters.Add(Vector3.Zero);
            this.m_components.Add(new MyIntervalList());
        }

        public void SetExplored(ulong packedCoord, Base6Directions.DirectionFlags directionFlags)
        {
            CellInfo info = new CellInfo();
            if (this.m_cellInfos.TryGetValue(packedCoord, out info))
            {
                info.ExploredDirections = directionFlags;
                this.m_cellInfos[packedCoord] = info;
            }
        }

        public bool TryGetCell(ulong packedCoord, out CellInfo cellInfo) => 
            this.m_cellInfos.TryGetValue(packedCoord, out cellInfo);

        public bool TryGetComponentCell(int componentIndex, out ulong cellIndex) => 
            this.m_componentCells.TryGetValue(componentIndex, out cellIndex);

        [StructLayout(LayoutKind.Sequential)]
        public struct CellInfo
        {
            public int StartingIndex;
            public ushort ComponentNum;
            public Base6Directions.DirectionFlags ExploredDirections;
            public override string ToString()
            {
                object[] objArray1 = new object[] { this.ComponentNum.ToString(), " components: ", this.StartingIndex, " - ", (this.StartingIndex + this.ComponentNum) - 1, ", Expl.: ", this.ExploredDirections.ToString() };
                return string.Concat(objArray1);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ClosedCellInfo
        {
            public bool NewCell;
            public int OldStartingIndex;
            public ushort OldComponentNum;
            public int StartingIndex;
            public ushort ComponentNum;
            public Base6Directions.DirectionFlags ExploredDirections;
        }
    }
}

