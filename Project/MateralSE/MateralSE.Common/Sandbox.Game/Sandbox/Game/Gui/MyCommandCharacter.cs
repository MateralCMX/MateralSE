namespace Sandbox.Game.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;

    [PreloadRequired]
    public class MyCommandCharacter : MyCommand
    {
        static MyCommandCharacter()
        {
            MyConsole.AddCommand(new MyCommandCharacter());
        }

        private MyCommandCharacter()
        {
            MyCommand.MyCommandAction action1 = new MyCommand.MyCommandAction();
            action1.AutocompleteHint = new StringBuilder("int_val1 int_val2 ...");
            action1.Parser = x => this.ParseValues(x);
            action1.CallAction = x => this.PassValuesToCharacter(x);
            base.m_methods.Add("AddSomeValues", action1);
        }

        private MyCommandArgs ParseValues(List<string> args)
        {
            MyCommandArgsValuesList list = new MyCommandArgsValuesList {
                values = new List<int>()
            };
            foreach (string str in args)
            {
                list.values.Add(int.Parse(str));
            }
            return list;
        }

        private StringBuilder PassValuesToCharacter(MyCommandArgs args)
        {
            MyCommandArgsValuesList list = args as MyCommandArgsValuesList;
            if (list.values.Count == 0)
            {
                return new StringBuilder("No values passed onto character");
            }
            foreach (int local1 in list.values)
            {
            }
            StringBuilder builder = new StringBuilder().Append("Added values ");
            foreach (int num in list.values)
            {
                builder.Append(num).Append(" ");
            }
            builder.Append("to character");
            return builder;
        }

        public override string Prefix() => 
            "Character";

        private class MyCommandArgsValuesList : MyCommandArgs
        {
            public List<int> values;
        }
    }
}

