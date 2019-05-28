namespace Sandbox.Game
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class MyStatLogicDescriptor : Attribute
    {
        public string ComponentName;

        public MyStatLogicDescriptor(string componentName)
        {
            this.ComponentName = componentName;
        }
    }
}

