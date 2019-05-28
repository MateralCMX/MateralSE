namespace KeenSoftwareHouse.Library.Extensions
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.All, AllowMultiple=false)]
    public class EnabledAttribute : Attribute
    {
        public EnabledAttribute(bool enabled = true)
        {
            this.Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }
}

