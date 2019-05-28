namespace Sandbox.Game.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;

    [PreloadRequired]
    public class MyCommandConsole : MyCommand
    {
        static MyCommandConsole()
        {
            MyConsole.AddCommand(new MyCommandConsole());
        }

        private MyCommandConsole()
        {
            MyCommand.MyCommandAction action3 = new MyCommand.MyCommandAction();
            MyCommand.MyCommandAction action4 = new MyCommand.MyCommandAction();
            action4.Parser = (ParserDelegate) (x => null);
            MyCommand.MyCommandAction local2 = action4;
            local2.CallAction = x => this.ClearConsole(x);
            base.m_methods.Add("Clear", local2);
            MyCommand.MyCommandAction action1 = new MyCommand.MyCommandAction();
            MyCommand.MyCommandAction action2 = new MyCommand.MyCommandAction();
            action2.Parser = (ParserDelegate) (x => null);
            MyCommand.MyCommandAction local4 = action2;
            local4.CallAction = new ActionDelegate(this.ScriptConsole);
            base.m_methods.Add("Script", local4);
        }

        private StringBuilder ClearConsole(MyCommandArgs args)
        {
            MyConsole.Clear();
            return new StringBuilder("Console cleared...");
        }

        public override string Prefix() => 
            "Console";

        private StringBuilder ScriptConsole(MyCommandArgs x) => 
            new StringBuilder("Scripting mode. Send blank line to compile and run.");

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCommandConsole.<>c <>9 = new MyCommandConsole.<>c();
            public static ParserDelegate <>9__2_0;
            public static ParserDelegate <>9__2_2;

            internal MyCommandArgs <.ctor>b__2_0(List<string> x) => 
                null;

            internal MyCommandArgs <.ctor>b__2_2(List<string> x) => 
                null;
        }
    }
}

