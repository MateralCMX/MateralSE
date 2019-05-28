namespace Sandbox.Game.SessionComponents
{
    using System;
    using VRage.Utils;

    public class MyIngameHelpDetail
    {
        public MyStringId TextEnum;
        public object[] Args;
        public Func<bool> FinishCondition;
        public bool Finished;
    }
}

