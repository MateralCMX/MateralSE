namespace VRage
{
    using System;
    using System.Collections.Generic;

    public sealed class MyServiceManager
    {
        private static MyServiceManager singleton = new MyServiceManager();
        private Dictionary<Type, object> services = new Dictionary<Type, object>();
        private object lockObject = new object();

        private MyServiceManager()
        {
        }

        public void AddService<T>(T serviceInstance) where T: class
        {
            object lockObject = this.lockObject;
            lock (lockObject)
            {
                this.services[typeof(T)] = serviceInstance;
            }
        }

        public T GetService<T>() where T: class
        {
            object obj2;
            object lockObject = this.lockObject;
            lock (lockObject)
            {
                this.services.TryGetValue(typeof(T), out obj2);
            }
            return (obj2 as T);
        }

        public void RemoveService<T>()
        {
            object lockObject = this.lockObject;
            lock (lockObject)
            {
                this.services.Remove(typeof(T));
            }
        }

        public static MyServiceManager Instance =>
            singleton;
    }
}

