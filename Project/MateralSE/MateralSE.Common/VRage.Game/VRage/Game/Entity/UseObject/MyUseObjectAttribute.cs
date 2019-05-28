namespace VRage.Game.Entity.UseObject
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public class MyUseObjectAttribute : Attribute
    {
        public readonly string DummyName;

        public MyUseObjectAttribute(string dummyName)
        {
            this.DummyName = dummyName;
        }
    }
}

