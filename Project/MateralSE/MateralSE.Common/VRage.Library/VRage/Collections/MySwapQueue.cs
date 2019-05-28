namespace VRage.Collections
{
    using System;

    public class MySwapQueue
    {
        public static MySwapQueue<T> Create<T>() where T: class, new() => 
            new MySwapQueue<T>(Activator.CreateInstance<T>(), Activator.CreateInstance<T>(), Activator.CreateInstance<T>());
    }
}

