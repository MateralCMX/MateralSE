namespace VRage.Library.Collections
{
    using System;

    public sealed class TypeSwitchParam<TKeyBase, TParam> : TypeSwitchBase<TKeyBase, Func<TParam, TKeyBase>>
    {
        public TRet Switch<TRet>(TParam par) where TRet: class, TKeyBase
        {
            Func<TParam, TKeyBase> func = base.SwitchInternal<TRet>();
            if (func != null)
            {
                return func(par);
            }
            return default(TRet);
        }
    }
}

