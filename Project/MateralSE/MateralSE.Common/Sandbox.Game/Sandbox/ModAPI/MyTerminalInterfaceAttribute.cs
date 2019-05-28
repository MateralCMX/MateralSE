namespace Sandbox.ModAPI
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class MyTerminalInterfaceAttribute : Attribute
    {
        public readonly Type[] LinkedTypes;

        public MyTerminalInterfaceAttribute(params Type[] linkedTypes)
        {
            this.LinkedTypes = linkedTypes;
        }
    }
}

