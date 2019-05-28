namespace VRage.Library.Collections
{
    using System;

    public sealed class TypeSwitch<TKeyBase> : TypeSwitchBase<TKeyBase, Func<TKeyBase>>
    {
        public TRet Switch<TRet>() where TRet: class, TKeyBase
        {
            Func<TKeyBase> func = base.SwitchInternal<TRet>();
            if (func != null)
            {
                return func();
            }
            return default(TRet);
        }
    }
}

