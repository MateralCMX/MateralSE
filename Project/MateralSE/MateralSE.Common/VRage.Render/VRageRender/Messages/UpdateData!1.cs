namespace VRageRender.Messages
{
    using System;

    public class UpdateData<T> : UpdateData
    {
        public UpdateData() : base(typeof(T))
        {
        }
    }
}

