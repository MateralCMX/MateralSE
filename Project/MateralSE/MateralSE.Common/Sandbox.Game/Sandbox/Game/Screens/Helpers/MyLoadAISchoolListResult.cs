namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Networking;
    using System;
    using System.Collections.Generic;

    public class MyLoadAISchoolListResult : MyLoadListResult
    {
        public MyLoadAISchoolListResult() : base(null)
        {
        }

        protected override List<Tuple<string, MyWorldInfo>> GetAvailableSaves() => 
            MyLocalCache.GetAvailableAISchoolInfos();
    }
}

