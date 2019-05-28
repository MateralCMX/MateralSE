namespace VRage.Scripting
{
    using Sandbox.ModAPI;
    using System;
    using System.Runtime.CompilerServices;

    public class MyIngameScripting : IMyIngameScripting
    {
        public static IMyScriptBlacklist ScriptBlacklist = MyScriptCompiler.Static.Whitelist;

        static MyIngameScripting()
        {
            Static = new MyIngameScripting();
        }

        public void Clean()
        {
            ScriptBlacklist = null;
            Static = null;
        }

        public static MyIngameScripting Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            internal set => 
                (<Static>k__BackingField = value);
        }

        IMyScriptBlacklist IMyIngameScripting.ScriptBlacklist =>
            ScriptBlacklist;
    }
}

