namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class Obfuscator
    {
        public const string NoRename = "cw symbol renaming";
        public static readonly bool EnableAttributeCheck = true;

        public static bool CheckAttribute(this MemberInfo member)
        {
            if (!EnableAttributeCheck)
            {
                return true;
            }
            using (IEnumerator<ObfuscationAttribute> enumerator = member.GetCustomAttributes(typeof(ObfuscationAttribute), false).OfType<ObfuscationAttribute>().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ObfuscationAttribute current = enumerator.Current;
                    if ((current.Feature == "cw symbol renaming") && current.Exclude)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

