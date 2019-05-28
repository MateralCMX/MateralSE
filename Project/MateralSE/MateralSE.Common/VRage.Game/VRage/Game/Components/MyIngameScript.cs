namespace VRage.Game.Components
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public abstract class MyIngameScript
    {
        protected MyIngameScript()
        {
        }

        public abstract void Init(IMyCubeBlock block);
    }
}

