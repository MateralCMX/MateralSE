namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Networking;
    using System;
    using System.Collections.Generic;

    public class MyLoadTutorialListResult : MyLoadListResult
    {
        public MyLoadTutorialListResult() : base(null)
        {
        }

        protected override List<Tuple<string, MyWorldInfo>> GetAvailableSaves() => 
            MyLocalCache.GetAvailableTutorialInfos();
    }
}

