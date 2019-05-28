namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner]
    internal class MyAlesDebugInputComponent : MyDebugComponent
    {
        private bool m_questlogOpened;
        private MyGuiScreenBase guiScreen;
        private static MyRandom random = new MyRandom();
        private MyRandom m_random = new MyRandom();

        public MyAlesDebugInputComponent()
        {
            this.AddShortcut(MyKeys.U, true, false, false, false, () => "Reload particles", delegate {
                this.ReloadParticleDefinition();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Teleport to gps", delegate {
                this.TravelToWaypointClient();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Init questlog", delegate {
                this.ToggleQuestlog();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Show/Hide QL", delegate {
                this.m_questlogOpened = !this.m_questlogOpened;
                MyHud.Questlog.Visible = this.m_questlogOpened;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "QL: Prew page", () => true);
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "QL: Next page", () => true);
            int shortLine = 30;
            this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "QL: Add short line", delegate {
                MyHud.Questlog.AddDetail(RandomString(shortLine), true, false);
                return true;
            });
            int longLine = 60;
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "QL: Add long line", delegate {
                MyHud.Questlog.AddDetail(RandomString(longLine), true, false);
                return true;
            });
        }

        public override string GetName() => 
            "Ales";

        public static string RandomString(int length) => 
            new string((from s in Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789           ", length) select s[random.Next(s.Length)]).ToArray<char>()).Trim();

        private void ReloadParticleDefinition()
        {
            MyDefinitionManager.Static.ReloadParticles();
        }

        private void ToggleQuestlog()
        {
            MyHud.Questlog.QuestTitle = "Test Questlog title message";
            MyHud.Questlog.CleanDetails();
        }

        [Event(null, 0xcf), Reliable, Server]
        public static void TravelToWaypoint(Vector3D pos)
        {
            MyMultiplayer.TeleportControlledEntity(pos);
        }

        private void TravelToWaypointClient()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenDialogTeleportCheat());
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAlesDebugInputComponent.<>c <>9 = new MyAlesDebugInputComponent.<>c();
            public static Func<string> <>9__5_0;
            public static Func<string> <>9__5_2;
            public static Func<string> <>9__5_4;
            public static Func<string> <>9__5_6;
            public static Func<string> <>9__5_8;
            public static Func<bool> <>9__5_9;
            public static Func<string> <>9__5_10;
            public static Func<bool> <>9__5_11;
            public static Func<string> <>9__5_12;
            public static Func<string> <>9__5_14;
            public static Func<string, char> <>9__10_0;

            internal string <.ctor>b__5_0() => 
                "Reload particles";

            internal string <.ctor>b__5_10() => 
                "QL: Next page";

            internal bool <.ctor>b__5_11() => 
                true;

            internal string <.ctor>b__5_12() => 
                "QL: Add short line";

            internal string <.ctor>b__5_14() => 
                "QL: Add long line";

            internal string <.ctor>b__5_2() => 
                "Teleport to gps";

            internal string <.ctor>b__5_4() => 
                "Init questlog";

            internal string <.ctor>b__5_6() => 
                "Show/Hide QL";

            internal string <.ctor>b__5_8() => 
                "QL: Prew page";

            internal bool <.ctor>b__5_9() => 
                true;

            internal char <RandomString>b__10_0(string s) => 
                s[MyAlesDebugInputComponent.random.Next(s.Length)];
        }
    }
}

