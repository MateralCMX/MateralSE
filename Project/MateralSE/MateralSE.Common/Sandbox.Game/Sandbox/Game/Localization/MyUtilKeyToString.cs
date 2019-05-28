namespace Sandbox.Game.Localization
{
    using System;
    using VRage.Input;

    internal abstract class MyUtilKeyToString
    {
        public MyKeys Key;

        public MyUtilKeyToString(MyKeys key)
        {
            this.Key = key;
        }

        public abstract string Name { get; }
    }
}

