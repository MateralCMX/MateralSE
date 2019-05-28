namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;

    public class MyComponentContainerTemplate<T> where T: class
    {
        internal MyIndexedComponentContainer<Func<Type, T>> Components;

        public MyComponentContainerTemplate(List<Type> types, List<Func<Type, T>> compoentFactories)
        {
            this.Components = new MyIndexedComponentContainer<Func<Type, T>>();
            for (int i = 0; i < types.Count; i++)
            {
                this.Components.Add(types[i], compoentFactories[i]);
            }
        }
    }
}

