namespace Sandbox.Game.Gui
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public class MyDebugScreenAttribute : Attribute
    {
        public readonly string Group;
        public readonly string Name;
        public readonly MyDirectXSupport DirectXSupport;

        public MyDebugScreenAttribute(string group, string name)
        {
            this.Group = group;
            this.Name = name;
            this.DirectXSupport = MyDirectXSupport.ALL;
        }

        public MyDebugScreenAttribute(string group, string name, MyDirectXSupport directXSupport)
        {
            this.Group = group;
            this.Name = name;
            this.DirectXSupport = directXSupport;
        }
    }
}

