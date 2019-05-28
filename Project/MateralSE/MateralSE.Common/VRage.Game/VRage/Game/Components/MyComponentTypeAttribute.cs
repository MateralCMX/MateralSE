namespace VRage.Game.Components
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class MyComponentTypeAttribute : Attribute
    {
        public readonly Type ComponentType;

        public MyComponentTypeAttribute(Type componentType)
        {
            this.ComponentType = componentType;
        }
    }
}

