namespace VRage.Game.Components
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class MySessionComponentDescriptor : Attribute
    {
        public MyUpdateOrder UpdateOrder;
        public int Priority;
        public MyObjectBuilderType ObjectBuilderType;
        public Type ComponentType;

        public MySessionComponentDescriptor(MyUpdateOrder updateOrder) : this(updateOrder, 0x3e8)
        {
        }

        public MySessionComponentDescriptor(MyUpdateOrder updateOrder, int priority) : this(updateOrder, priority, null, null)
        {
        }

        public MySessionComponentDescriptor(MyUpdateOrder updateOrder, int priority, Type obType, Type registrationType = null)
        {
            this.UpdateOrder = updateOrder;
            this.Priority = priority;
            this.ObjectBuilderType = obType;
            if ((obType != null) && !typeof(MyObjectBuilder_SessionComponent).IsAssignableFrom(obType))
            {
                this.ObjectBuilderType = MyObjectBuilderType.Invalid;
            }
            this.ComponentType = registrationType;
        }
    }
}

