namespace VRage.Library.Collections
{
    using System;

    public sealed class TypeSwitchParam<TKeyBase, TParam1, TParam2> : TypeSwitchBase<TKeyBase, Func<TParam1, TParam2, TKeyBase>>
    {
        public TRet Switch<TRet>(TParam1 par1, TParam2 par2) where TRet: class, TKeyBase
        {
            Func<TParam1, TParam2, TKeyBase> func = base.SwitchInternal<TRet>();
            if (func != null)
            {
                return func(par1, par2);
            }
            return default(TRet);
        }
    }
}

