namespace Sandbox.Game.EntityComponents
{
    using System;
    using VRage.Game.Components;

    public class MyEntityReferenceComponent : MyEntityComponentBase
    {
        private int m_references;

        public void Ref()
        {
            this.m_references++;
        }

        public bool Unref()
        {
            this.m_references--;
            if (this.m_references > 0)
            {
                return false;
            }
            base.Entity.Close();
            return true;
        }

        public override string ComponentTypeDebugString =>
            "ReferenceCount";
    }
}

