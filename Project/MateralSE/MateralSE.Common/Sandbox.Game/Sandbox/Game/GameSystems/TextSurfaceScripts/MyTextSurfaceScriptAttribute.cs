namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class MyTextSurfaceScriptAttribute : Attribute
    {
        public string Id;
        public string DisplayName;

        public MyTextSurfaceScriptAttribute(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }
    }
}

