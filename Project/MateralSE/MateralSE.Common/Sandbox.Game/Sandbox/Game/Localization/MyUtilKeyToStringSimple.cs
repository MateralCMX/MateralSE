namespace Sandbox.Game.Localization
{
    using System;
    using VRage.Input;

    internal class MyUtilKeyToStringSimple : MyUtilKeyToString
    {
        private string m_name;

        public MyUtilKeyToStringSimple(MyKeys key, string name) : base(key)
        {
            this.m_name = name;
        }

        public override string Name =>
            this.m_name;
    }
}

