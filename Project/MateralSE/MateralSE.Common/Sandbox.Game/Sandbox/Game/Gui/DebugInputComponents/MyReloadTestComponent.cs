namespace Sandbox.Game.Gui.DebugInputComponents
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.World;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class MyReloadTestComponent : MySessionComponentBase
    {
        public static bool Enabled;

        public static void DoReload()
        {
            GC.Collect(2, GCCollectionMode.Forced);
            MySandboxGame.Log.WriteLine($"RELOAD TEST, Menu GC: {GC.GetTotalMemory(false).ToString("##,#")} B");
            MySandboxGame.Log.WriteLine($"RELOAD TEST, Menu WS: {Process.GetCurrentProcess().PrivateMemorySize64.ToString("##,#")} B");
            Tuple<string, MyWorldInfo> tuple = (from s in MyLocalCache.GetAvailableWorldInfos(null)
                orderby s.Item2.LastSaveTime descending
                select s).FirstOrDefault<Tuple<string, MyWorldInfo>>();
            if (tuple != null)
            {
                MyOnlineModeEnum? onlineMode = null;
                MySessionLoader.LoadSingleplayerSession(tuple.Item1, null, null, onlineMode, 0);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if ((Enabled && (MySandboxGame.IsGameReady && (MySession.Static != null))) && (MySession.Static.ElapsedPlayTime.TotalSeconds > 5.0))
            {
                GC.Collect(2, GCCollectionMode.Forced);
                MySandboxGame.Log.WriteLine($"RELOAD TEST, Game GC: {GC.GetTotalMemory(false).ToString("##,#")} B");
                MySandboxGame.Log.WriteLine($"RELOAD TEST, Game WS: {Process.GetCurrentProcess().PrivateMemorySize64.ToString("##,#")} B");
                MySessionLoader.UnloadAndExitToMenu();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyReloadTestComponent.<>c <>9 = new MyReloadTestComponent.<>c();
            public static Func<Tuple<string, MyWorldInfo>, DateTime> <>9__2_0;

            internal DateTime <DoReload>b__2_0(Tuple<string, MyWorldInfo> s) => 
                s.Item2.LastSaveTime;
        }
    }
}

