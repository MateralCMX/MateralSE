namespace Sandbox.Game.Gui
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo info, object instance)
        {
            object obj2 = null;
            FieldInfo info2 = info as FieldInfo;
            if (info2 != null)
            {
                obj2 = info2.GetValue(instance);
            }
            PropertyInfo info3 = info as PropertyInfo;
            if ((info3 != null) && (info3.GetIndexParameters().Length == 0))
            {
                obj2 = info3.GetValue(instance, null);
            }
            return obj2;
        }
    }
}

