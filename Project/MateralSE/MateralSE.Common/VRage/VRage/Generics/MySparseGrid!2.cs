namespace VRage.Generics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRageMath;

    public class MySparseGrid<TItemData, TCellData> : IDictionary<Vector3I, TItemData>, ICollection<KeyValuePair<Vector3I, TItemData>>, IEnumerable<KeyValuePair<Vector3I, TItemData>>, IEnumerable
    {
        private int m_itemCount;
        private Dictionary<Vector3I, Cell<TItemData, TCellData>> m_cells;
        private HashSet<Vector3I> m_dirtyCells;
        public readonly int CellSize;

        public MySparseGrid(int cellSize)
        {
            this.m_cells = new Dictionary<Vector3I, Cell<TItemData, TCellData>>();
            this.m_dirtyCells = new HashSet<Vector3I>();
            this.CellSize = cellSize;
        }

        public Vector3I Add(Vector3I pos, TItemData data)
        {
            Vector3I cell = (Vector3I) (pos / this.CellSize);
            this.GetCell(cell, true).m_items.Add(pos, data);
            this.MarkDirty(cell);
            this.m_itemCount++;
            return cell;
        }

        public void Clear()
        {
            this.m_cells.Clear();
            this.m_itemCount = 0;
        }

        public void ClearCells()
        {
            foreach (KeyValuePair<Vector3I, Cell<TItemData, TCellData>> pair in this.m_cells)
            {
                pair.Value.m_items.Clear();
            }
            this.m_itemCount = 0;
        }

        public bool Contains(Vector3I pos)
        {
            Cell<TItemData, TCellData> cell = this.GetCell((Vector3I) (pos / this.CellSize), false);
            return ((cell != null) && cell.m_items.ContainsKey(pos));
        }

        public TItemData Get(Vector3I pos) => 
            this.GetCell((Vector3I) (pos / this.CellSize), false).m_items[pos];

        public Cell<TItemData, TCellData> GetCell(Vector3I cell) => 
            this.m_cells[cell];

        private Cell<TItemData, TCellData> GetCell(Vector3I cell, bool createIfNotExists)
        {
            Cell<TItemData, TCellData> cell2;
            if (!this.m_cells.TryGetValue(cell, out cell2) & createIfNotExists)
            {
                cell2 = new Cell<TItemData, TCellData>();
                this.m_cells[cell] = cell2;
            }
            return cell2;
        }

        public Dictionary<Vector3I, Cell<TItemData, TCellData>>.Enumerator GetEnumerator() => 
            this.m_cells.GetEnumerator();

        public bool IsDirty(Vector3I cell) => 
            this.m_dirtyCells.Contains(cell);

        public void MarkDirty(Vector3I cell)
        {
            this.m_dirtyCells.Add(cell);
        }

        public bool Remove(Vector3I pos, bool removeEmptyCell = true)
        {
            Vector3I vectori = (Vector3I) (pos / this.CellSize);
            Cell<TItemData, TCellData> cell = this.GetCell(vectori, false);
            if ((cell == null) || !cell.m_items.Remove(pos))
            {
                return false;
            }
            this.MarkDirty(vectori);
            this.m_itemCount--;
            if (removeEmptyCell && (cell.m_items.Count == 0))
            {
                this.m_cells.Remove(vectori);
            }
            return true;
        }

        void ICollection<KeyValuePair<Vector3I, TItemData>>.Add(KeyValuePair<Vector3I, TItemData> item)
        {
            this.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<Vector3I, TItemData>>.Clear()
        {
            this.Clear();
        }

        bool ICollection<KeyValuePair<Vector3I, TItemData>>.Contains(KeyValuePair<Vector3I, TItemData> item)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        void ICollection<KeyValuePair<Vector3I, TItemData>>.CopyTo(KeyValuePair<Vector3I, TItemData>[] array, int arrayIndex)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        bool ICollection<KeyValuePair<Vector3I, TItemData>>.Remove(KeyValuePair<Vector3I, TItemData> item)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        void IDictionary<Vector3I, TItemData>.Add(Vector3I key, TItemData value)
        {
            this.Add(key, value);
        }

        bool IDictionary<Vector3I, TItemData>.ContainsKey(Vector3I key) => 
            this.Contains(key);

        bool IDictionary<Vector3I, TItemData>.Remove(Vector3I key) => 
            this.Remove(key, true);

        bool IDictionary<Vector3I, TItemData>.TryGetValue(Vector3I key, out TItemData value) => 
            this.TryGet(key, out value);

        IEnumerator<KeyValuePair<Vector3I, TItemData>> IEnumerable<KeyValuePair<Vector3I, TItemData>>.GetEnumerator()
        {
            throw new InvalidOperationException("Operation not supported");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new InvalidOperationException("Operation not supported");
        }

        public bool TryGet(Vector3I pos, out TItemData data)
        {
            Cell<TItemData, TCellData> cell = this.GetCell((Vector3I) (pos / this.CellSize), false);
            if (cell != null)
            {
                return cell.m_items.TryGetValue(pos, out data);
            }
            data = default(TItemData);
            return false;
        }

        public bool TryGetCell(Vector3I cell, out Cell<TItemData, TCellData> result) => 
            this.m_cells.TryGetValue(cell, out result);

        public void UnmarkDirty(Vector3I cell)
        {
            this.m_dirtyCells.Remove(cell);
        }

        public void UnmarkDirtyAll()
        {
            this.m_dirtyCells.Clear();
        }

        public DictionaryReader<Vector3I, Cell<TItemData, TCellData>> Cells =>
            new DictionaryReader<Vector3I, Cell<TItemData, TCellData>>(this.m_cells);

        public HashSetReader<Vector3I> DirtyCells =>
            this.m_dirtyCells;

        public int ItemCount =>
            this.m_itemCount;

        public int CellCount =>
            this.m_cells.Count;

        ICollection<Vector3I> IDictionary<Vector3I, TItemData>.Keys
        {
            get
            {
                throw new InvalidOperationException("Operation not supported");
            }
        }

        ICollection<TItemData> IDictionary<Vector3I, TItemData>.Values
        {
            get
            {
                throw new InvalidOperationException("Operation not supported");
            }
        }

        TItemData IDictionary<Vector3I, TItemData>.this[Vector3I key]
        {
            get => 
                this.Get(key);
            set
            {
                this.Remove(key, true);
                this.Add(key, value);
            }
        }

        int ICollection<KeyValuePair<Vector3I, TItemData>>.Count =>
            this.m_itemCount;

        bool ICollection<KeyValuePair<Vector3I, TItemData>>.IsReadOnly =>
            false;

        public class Cell
        {
            internal Dictionary<Vector3I, TItemData> m_items;
            public TCellData CellData;

            public Cell()
            {
                this.m_items = new Dictionary<Vector3I, TItemData>();
            }

            public DictionaryReader<Vector3I, TItemData> Items =>
                new DictionaryReader<Vector3I, TItemData>(this.m_items);
        }
    }
}

