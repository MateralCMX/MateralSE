namespace Sandbox.Game.GUI.DebugInputComponents
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Input;

    public class MyResearchDebugInputComponent : MyDebugComponent
    {
        public MyResearchDebugInputComponent()
        {
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Show Your Research", new Func<bool>(this.ShowResearch));
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Toggle Pretty Mode", new Func<bool>(this.ShowResearchPretty));
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Unlock Your Research", new Func<bool>(this.UnlockResearch));
            this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Unlock All Research", new Func<bool>(this.UnlockAllResearch));
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Reset Your Research", new Func<bool>(this.ResetResearch));
            this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Reset All Research", new Func<bool>(this.ResetAllResearch));
        }

        public override string GetName() => 
            "Research";

        public override bool HandleInput() => 
            ((MySession.Static != null) ? base.HandleInput() : false);

        private bool ResetAllResearch()
        {
            using (IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCharacter controlledEntity = enumerator.Current.Controller.ControlledEntity as MyCharacter;
                    if (controlledEntity != null)
                    {
                        MySessionComponentResearch.Static.ResetResearch(controlledEntity);
                    }
                }
            }
            return true;
        }

        private bool ResetResearch()
        {
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                MySessionComponentResearch.Static.ResetResearch(MySession.Static.LocalCharacter);
            }
            return true;
        }

        private bool ShowResearch()
        {
            MySessionComponentResearch.Static.DEBUG_SHOW_RESEARCH = !MySessionComponentResearch.Static.DEBUG_SHOW_RESEARCH;
            return true;
        }

        private bool ShowResearchPretty()
        {
            MySessionComponentResearch.Static.DEBUG_SHOW_RESEARCH_PRETTY = !MySessionComponentResearch.Static.DEBUG_SHOW_RESEARCH_PRETTY;
            return true;
        }

        private bool UnlockAllResearch()
        {
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                MySessionComponentResearch.Static.DebugUnlockAllResearch(player.Identity.IdentityId);
            }
            return true;
        }

        private bool UnlockResearch()
        {
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                MySessionComponentResearch.Static.DebugUnlockAllResearch(MySession.Static.LocalCharacter.GetPlayerIdentityId());
            }
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyResearchDebugInputComponent.<>c <>9 = new MyResearchDebugInputComponent.<>c();
            public static Func<string> <>9__0_0;
            public static Func<string> <>9__0_1;
            public static Func<string> <>9__0_2;
            public static Func<string> <>9__0_3;
            public static Func<string> <>9__0_4;
            public static Func<string> <>9__0_5;

            internal string <.ctor>b__0_0() => 
                "Show Your Research";

            internal string <.ctor>b__0_1() => 
                "Toggle Pretty Mode";

            internal string <.ctor>b__0_2() => 
                "Unlock Your Research";

            internal string <.ctor>b__0_3() => 
                "Unlock All Research";

            internal string <.ctor>b__0_4() => 
                "Reset Your Research";

            internal string <.ctor>b__0_5() => 
                "Reset All Research";
        }
    }
}

