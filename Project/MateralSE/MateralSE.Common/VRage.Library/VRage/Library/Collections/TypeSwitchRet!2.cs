namespace VRage.Library.Collections
{
    using System;

    public sealed class TypeSwitchRet<TKeyBase, TRetBase> : TypeSwitchBase<TKeyBase, Func<TRetBase>>
    {
        public TRet Switch<TKey, TRet>() where TKey: class, TKeyBase where TRet: class, TRetBase
        {
            Func<TRetBase> func = base.SwitchInternal<TKey>();
            if (func != null)
            {
                return func();
            }
            return default(TRet);
        }
    }
}

