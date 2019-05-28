namespace VRageRender.Messages
{
    using System;

    public class SealedComponentData : UpdateData
    {
        public SealedComponentData() : base(null)
        {
        }

        public void SetComponent<T>()
        {
            base.ComponentType = typeof(T);
        }
    }
}

