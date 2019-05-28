namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;

    public class UpdateData
    {
        public readonly Type DataType;

        public UpdateData()
        {
            throw new Exception("Invalid constructor");
        }

        protected UpdateData(Type componentType)
        {
            this.DataType = base.GetType();
            this.ComponentType = componentType;
        }

        public T As<T>() where T: UpdateData => 
            ((T) this);

        public Type ComponentType { get; internal set; }
    }
}

