namespace Sandbox.Game.Localization
{
    using System;
    using VRage;
    using VRage.Input;
    using VRage.Utils;

    internal class MyUtilKeyToStringLocalized : MyUtilKeyToString
    {
        private MyStringId m_name;

        public MyUtilKeyToStringLocalized(MyKeys key, MyStringId name) : base(key)
        {
            this.m_name = name;
        }

        public override string Name =>
            MyTexts.GetString(this.m_name);
    }
}

