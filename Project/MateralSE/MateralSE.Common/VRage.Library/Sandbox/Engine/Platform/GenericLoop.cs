namespace Sandbox.Engine.Platform
{
    using System;
    using System.Runtime.CompilerServices;

    public class GenericLoop
    {
        private VoidAction m_tickCallback;
        public bool IsDone;

        public virtual void Run(VoidAction tickCallback)
        {
            this.m_tickCallback = tickCallback;
            while (!this.IsDone)
            {
                this.m_tickCallback();
            }
        }

        public delegate void VoidAction();
    }
}

