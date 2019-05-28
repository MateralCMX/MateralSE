namespace Sandbox.Game.SessionComponents
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
    public class IngameObjectiveAttribute : Attribute
    {
        private string m_id;
        private int m_priority;

        public IngameObjectiveAttribute(string id, int priority)
        {
            this.m_id = id;
            this.m_priority = priority;
        }

        public string Id =>
            this.m_id;

        public int Priority =>
            this.m_priority;
    }
}

