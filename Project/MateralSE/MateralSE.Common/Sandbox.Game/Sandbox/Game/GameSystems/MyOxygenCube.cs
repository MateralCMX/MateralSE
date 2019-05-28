namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyOxygenCube
    {
        private readonly Vector3I m_defaultCellSize;
        private readonly Vector3I m_defaultBaseOffset;
        private Vector3I m_cellSize;
        private Vector3I m_baseOffset;
        private ConcurrentDictionary<Vector3I, MyOxygenBlock[]> m_dictionary;

        public MyOxygenCube()
        {
            this.m_defaultCellSize = new Vector3I(10, 10, 10);
            this.m_defaultBaseOffset = new Vector3I(5, 5, 5);
            this.m_cellSize = this.m_defaultCellSize;
            this.m_baseOffset = this.m_defaultBaseOffset;
            this.m_dictionary = new ConcurrentDictionary<Vector3I, MyOxygenBlock[]>(new Vector3I.EqualityComparer());
        }

        public MyOxygenCube(Vector3I cellSize)
        {
            this.m_defaultCellSize = new Vector3I(10, 10, 10);
            this.m_defaultBaseOffset = new Vector3I(5, 5, 5);
            this.m_cellSize = cellSize;
            this.m_baseOffset = (Vector3I) (cellSize / 2);
            this.m_dictionary = new ConcurrentDictionary<Vector3I, MyOxygenBlock[]>(new Vector3I.EqualityComparer());
        }

        public void Add(Vector3I key, MyOxygenBlock value)
        {
            Vector3I vectori;
            MyOxygenBlock[] blockArray;
            this.GetCellPosition(key, out vectori);
            if (!this.m_dictionary.TryGetValue(vectori, out blockArray))
            {
                blockArray = new MyOxygenBlock[this.m_cellSize.Volume()];
                this.m_dictionary.TryAdd(vectori, blockArray);
            }
            Vector3I vectori2 = key - vectori;
            blockArray[(vectori2.X + (vectori2.Y * this.m_cellSize.X)) + ((vectori2.Z * this.m_cellSize.X) * this.m_cellSize.Y)] = value;
        }

        private unsafe void GetCellPosition(Vector3I key, out Vector3I cellPosition)
        {
            if (-this.m_baseOffset.X > key.X)
            {
                int* numPtr1 = (int*) ref key.X;
                numPtr1[0] -= this.m_cellSize.X - 1;
            }
            if (-this.m_baseOffset.Y > key.Y)
            {
                int* numPtr2 = (int*) ref key.Y;
                numPtr2[0] -= this.m_cellSize.Y - 1;
            }
            if (-this.m_baseOffset.Z > key.Z)
            {
                int* numPtr3 = (int*) ref key.Z;
                numPtr3[0] -= this.m_cellSize.Z - 1;
            }
            Vector3I vectori = (Vector3I) ((key + this.m_baseOffset) / this.m_cellSize);
            cellPosition = (Vector3I) (this.m_baseOffset + ((vectori - 1) * this.m_cellSize));
        }

        public bool TryGetValue(Vector3I key, out MyOxygenBlock value)
        {
            Vector3I vectori;
            MyOxygenBlock[] blockArray;
            this.GetCellPosition(key, out vectori);
            if (!this.m_dictionary.TryGetValue(vectori, out blockArray))
            {
                value = null;
                return false;
            }
            Vector3I vectori2 = key - vectori;
            value = blockArray[(vectori2.X + (vectori2.Y * this.m_cellSize.X)) + ((vectori2.Z * this.m_cellSize.X) * this.m_cellSize.Y)];
            return (value != null);
        }

        public MyOxygenBlock this[int x, int y, int z]
        {
            get
            {
                MyOxygenBlock block;
                this.TryGetValue(new Vector3I(x, y, z), out block);
                return block;
            }
            set => 
                this.Add(new Vector3I(x, y, z), value);
        }

        private class Cell
        {
            public MyOxygenBlock[] Data;
        }
    }
}

