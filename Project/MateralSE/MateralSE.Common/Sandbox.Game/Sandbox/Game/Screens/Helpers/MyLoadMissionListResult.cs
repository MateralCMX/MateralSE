namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Networking;
    using System;
    using System.Collections.Generic;

    public class MyLoadMissionListResult : MyLoadListResult
    {
        public MyLoadMissionListResult() : base(null)
        {
        }

        protected override List<Tuple<string, MyWorldInfo>> GetAvailableSaves() => 
            MyLocalCache.GetAvailableMissionInfos();
    }
}

