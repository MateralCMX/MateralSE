namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyFatBlockReader<TBlock> : IEnumerator<TBlock>, IDisposable, IEnumerator where TBlock: MyCubeBlock
    {
        private HashSet<MySlimBlock>.Enumerator m_enumerator;
        public MyFatBlockReader(MyCubeGrid grid) : this(grid.GetBlocks().GetEnumerator())
        {
        }

        public MyFatBlockReader(HashSet<MySlimBlock> set) : this(set.GetEnumerator())
        {
        }

        public MyFatBlockReader(HashSet<MySlimBlock>.Enumerator enumerator)
        {
            this.m_enumerator = enumerator;
        }

        public unsafe MyFatBlockReader<TBlock> GetEnumerator() => 
            *(((MyFatBlockReader<TBlock>*) this));

        public TBlock Current =>
            ((TBlock) this.m_enumerator.Current.FatBlock);
        public void Dispose()
        {
            this.m_enumerator.Dispose();
        }

        object IEnumerator.Current =>
            this.Current;
        public bool MoveNext()
        {
            while (this.m_enumerator.MoveNext())
            {
                if (this.m_enumerator.Current.FatBlock is TBlock)
                {
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            IEnumerator<MySlimBlock> enumerator = this.m_enumerator;
            enumerator.Reset();
            this.m_enumerator = (HashSet<MySlimBlock>.Enumerator) enumerator;
        }
    }
}

