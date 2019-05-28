namespace VRage.Algorithms
{
    using System;
    using System.Runtime.InteropServices;

    public class MyUnionFind
    {
        private UF[] m_indices;
        private int m_size;

        public MyUnionFind()
        {
        }

        public MyUnionFind(int initialSize)
        {
            this.Resize(initialSize);
        }

        public unsafe void Clear()
        {
            UF* ufPtr;
            UF[] pinned ufArray;
            if (((ufArray = this.m_indices) == null) || (ufArray.Length == 0))
            {
                ufPtr = null;
            }
            else
            {
                ufPtr = ufArray;
            }
            for (int i = 0; i < this.m_size; i++)
            {
                ufPtr[i].Parent = i;
                ufPtr[i].Rank = 0;
            }
            ufArray = null;
        }

        public unsafe int Find(int a)
        {
            UF* ufPtr;
            UF[] pinned ufArray;
            if (((ufArray = this.m_indices) == null) || (ufArray.Length == 0))
            {
                ufPtr = null;
            }
            else
            {
                ufPtr = ufArray;
            }
            return this.Find(ufPtr, a);
        }

        private unsafe int Find(UF* uf, int a)
        {
            step* prev = null;
            while (uf[a].Parent != a)
            {
                step* stepPtr2 = (step*) stackalloc byte[(((IntPtr) 1) * sizeof(step))];
                stepPtr2->Element = a;
                stepPtr2->Prev = prev;
                prev = stepPtr2;
                a = uf[a].Parent;
            }
            while (prev != null)
            {
                uf[prev->Element].Parent = a;
                prev = prev->Prev;
            }
            return a;
        }

        private bool IsInRange(int index) => 
            ((index >= 0) && (index < this.m_size));

        public void Resize(int count = 0)
        {
            if ((this.m_indices == null) || (this.m_indices.Length < count))
            {
                this.m_indices = new UF[count];
            }
            this.m_size = count;
            this.Clear();
        }

        public unsafe void Union(int a, int b)
        {
            UF* ufPtr;
            UF[] pinned ufArray;
            if (((ufArray = this.m_indices) == null) || (ufArray.Length == 0))
            {
                ufPtr = null;
            }
            else
            {
                ufPtr = ufArray;
            }
            int index = this.Find(ufPtr, a);
            int num2 = this.Find(ufPtr, b);
            if (index != num2)
            {
                if (ufPtr[index].Rank < ufPtr[num2].Rank)
                {
                    ufPtr[index].Parent = num2;
                }
                else if (ufPtr[index].Rank > ufPtr[num2].Rank)
                {
                    ufPtr[num2].Parent = index;
                }
                else
                {
                    ufPtr[num2].Parent = index;
                    int* numPtr1 = (int*) ref ufPtr[index].Rank;
                    numPtr1[0]++;
                }
                ufArray = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct step
        {
            public unsafe MyUnionFind.step* Prev;
            public int Element;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UF
        {
            public int Parent;
            public int Rank;
        }
    }
}

