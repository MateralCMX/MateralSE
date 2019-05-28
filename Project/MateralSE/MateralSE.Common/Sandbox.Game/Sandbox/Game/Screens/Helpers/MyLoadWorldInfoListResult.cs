namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Networking;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyLoadWorldInfoListResult : MyLoadListResult
    {
        public MyLoadWorldInfoListResult(string customPath = null) : base(customPath)
        {
        }

        protected override List<Tuple<string, MyWorldInfo>> GetAvailableSaves() => 
            MyLocalCache.GetAvailableWorldInfos(base.CustomPath);
    }
}

