namespace Sandbox.Game.GameSystems.ContextHandling
{
    using System;

    public class MyGameFocusManager
    {
        private IMyFocusHolder m_currentFocusHolder;

        public void Clear()
        {
            if (this.m_currentFocusHolder != null)
            {
                this.m_currentFocusHolder.OnLostFocus();
            }
            this.m_currentFocusHolder = null;
        }

        public void Register(IMyFocusHolder newFocusHolder)
        {
            if ((this.m_currentFocusHolder != null) && !ReferenceEquals(newFocusHolder, this.m_currentFocusHolder))
            {
                this.m_currentFocusHolder.OnLostFocus();
            }
            this.m_currentFocusHolder = newFocusHolder;
        }

        public void Unregister(IMyFocusHolder focusHolder)
        {
            if (ReferenceEquals(this.m_currentFocusHolder, focusHolder))
            {
                this.m_currentFocusHolder = null;
            }
        }
    }
}

