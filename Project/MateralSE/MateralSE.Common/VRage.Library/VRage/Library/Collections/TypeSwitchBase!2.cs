namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public abstract class TypeSwitchBase<TKeyBase, TValBase> where TValBase: class
    {
        protected TypeSwitchBase()
        {
            this.Matches = new Dictionary<Type, TValBase>();
        }

        public TypeSwitchBase<TKeyBase, TValBase> Case<TKey>(TValBase action) where TKey: class, TKeyBase
        {
            this.Matches.Add(typeof(TKey), action);
            return (TypeSwitchBase<TKeyBase, TValBase>) this;
        }

        protected TValBase SwitchInternal<TKey>() where TKey: class, TKeyBase
        {
            TValBase local;
            if (this.Matches.TryGetValue(typeof(TKey), out local))
            {
                return local;
            }
            return default(TValBase);
        }

        public Dictionary<Type, TValBase> Matches { get; private set; }
    }
}

