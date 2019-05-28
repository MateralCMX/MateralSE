namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public abstract class MyCommand
    {
        protected Dictionary<string, MyCommandAction> m_methods = new Dictionary<string, MyCommandAction>();

        public StringBuilder Execute(string method, List<string> args)
        {
            MyCommandAction action;
            StringBuilder builder;
            if (!this.m_methods.TryGetValue(method, out action))
            {
                throw new MyConsoleMethodNotFoundException();
            }
            try
            {
                MyCommandArgs commandArgs = action.Parser(args);
                builder = action.CallAction(commandArgs);
            }
            catch
            {
                throw new MyConsoleInvalidArgumentsException();
            }
            return builder;
        }

        public StringBuilder GetHint(string method)
        {
            MyCommandAction action;
            return (!this.m_methods.TryGetValue(method, out action) ? null : action.AutocompleteHint);
        }

        public abstract string Prefix();

        public List<string> Methods =>
            this.m_methods.Keys.ToList<string>();

        protected class MyCommandAction
        {
            public StringBuilder AutocompleteHint = new StringBuilder("");
            public ParserDelegate Parser;
            public ActionDelegate CallAction;
        }
    }
}

