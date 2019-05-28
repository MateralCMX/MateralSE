namespace VRage.Game.Components
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class MyEntityComponentDescriptor : Attribute
    {
        public Type EntityBuilderType;
        public string[] EntityBuilderSubTypeNames;
        public bool? EntityUpdate;

        [Obsolete("Use the 3 parameter overload instead!")]
        public MyEntityComponentDescriptor(Type entityBuilderType, params string[] entityBuilderSubTypeNames)
        {
            this.EntityBuilderType = entityBuilderType;
            this.EntityBuilderSubTypeNames = entityBuilderSubTypeNames;
            this.EntityUpdate = null;
        }

        public MyEntityComponentDescriptor(Type entityBuilderType, bool useEntityUpdate, params string[] entityBuilderSubTypeNames)
        {
            this.EntityBuilderType = entityBuilderType;
            this.EntityUpdate = new bool?(useEntityUpdate);
            this.EntityBuilderSubTypeNames = entityBuilderSubTypeNames;
        }
    }
}

