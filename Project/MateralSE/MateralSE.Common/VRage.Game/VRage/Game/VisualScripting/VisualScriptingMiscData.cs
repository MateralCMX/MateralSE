namespace VRage.Game.VisualScripting
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
    public class VisualScriptingMiscData : Attribute
    {
        private const int m_cadetBlue = -10510688;
        public readonly string Group;
        public readonly string Comment;
        public readonly int BackgroundColor;

        public VisualScriptingMiscData(string group, string comment = null, int backgroundColor = -10510688)
        {
            this.Group = group;
            this.Comment = comment;
            this.BackgroundColor = backgroundColor;
        }
    }
}

