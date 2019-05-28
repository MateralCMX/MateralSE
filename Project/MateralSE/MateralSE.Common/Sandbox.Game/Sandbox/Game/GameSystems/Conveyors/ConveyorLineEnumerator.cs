namespace Sandbox.Game.GameSystems.Conveyors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ConveyorLineEnumerator : IEnumerator<MyConveyorLine>, IDisposable, IEnumerator
    {
        private int index;
        private IMyConveyorEndpoint m_enumerated;
        private MyConveyorLine m_line;
        public ConveyorLineEnumerator(IMyConveyorEndpoint enumerated)
        {
            this.index = -1;
            this.m_enumerated = enumerated;
            this.m_line = null;
        }

        public MyConveyorLine Current =>
            this.m_line;
        public void Dispose()
        {
            this.m_enumerated = null;
            this.m_line = null;
        }

        object IEnumerator.Current =>
            this.m_line;
        public bool MoveNext()
        {
            while (this.MoveNextInternal())
            {
            }
            return (this.index < this.m_enumerated.GetLineCount());
        }

        private bool MoveNextInternal()
        {
            this.index++;
            if (this.index >= this.m_enumerated.GetLineCount())
            {
                return false;
            }
            this.m_line = this.m_enumerated.GetConveyorLine(this.index);
            return !this.m_line.IsWorking;
        }

        public void Reset()
        {
            this.index = 0;
        }
    }
}

