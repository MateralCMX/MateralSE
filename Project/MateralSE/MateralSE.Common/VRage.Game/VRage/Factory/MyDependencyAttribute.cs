namespace VRage.Factory
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class MyDependencyAttribute : Attribute
    {
        public readonly Type Dependency;
        public bool Recursive;
        public bool Critical;

        public MyDependencyAttribute(Type dependency)
        {
            this.Dependency = dependency;
        }
    }
}

