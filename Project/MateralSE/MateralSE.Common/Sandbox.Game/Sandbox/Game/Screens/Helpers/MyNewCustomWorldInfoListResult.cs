namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Networking;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyNewCustomWorldInfoListResult : MyLoadListResult
    {
        public MyNewCustomWorldInfoListResult(string customPath = null) : base(customPath)
        {
        }

        protected override List<Tuple<string, MyWorldInfo>> GetAvailableSaves() => 
            MyLocalCache.GetAvailableWorldInfos(base.CustomPath);
    }
}

