namespace VRage.ObjectBuilders
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyObjectBuilderDefinitionAttribute : MyFactoryTagAttribute
    {
        private Type ObsoleteBy;
        public readonly string LegacyName;

        public MyObjectBuilderDefinitionAttribute(Type obsoleteBy = null, string LegacyName = null) : base(null, true)
        {
            this.ObsoleteBy = obsoleteBy;
            this.LegacyName = LegacyName;
        }
    }
}

