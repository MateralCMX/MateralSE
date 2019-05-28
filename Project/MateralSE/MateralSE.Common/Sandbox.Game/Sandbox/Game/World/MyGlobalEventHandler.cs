namespace Sandbox.Game.World
{
    using System;
    using System.Reflection;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true), Obfuscation(Feature="cw symbol renaming", Exclude=true)]
    public class MyGlobalEventHandler : Attribute
    {
        public MyDefinitionId EventDefinitionId;

        public MyGlobalEventHandler(Type objectBuilderType, string subtypeName)
        {
            MyObjectBuilderType type = objectBuilderType;
            this.EventDefinitionId = new MyDefinitionId(type, subtypeName);
        }
    }
}

