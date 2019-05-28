namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MyEventArgs : EventArgs
    {
        private Dictionary<MyStringId, object> m_args;

        public MyEventArgs()
        {
            this.m_args = new Dictionary<MyStringId, object>(MyStringId.Comparer);
        }

        public MyEventArgs(KeyValuePair<MyStringId, object> arg)
        {
            this.m_args = new Dictionary<MyStringId, object>(MyStringId.Comparer);
            this.SetArg(arg.Key, arg.Value);
        }

        public MyEventArgs(KeyValuePair<MyStringId, object>[] args)
        {
            this.m_args = new Dictionary<MyStringId, object>(MyStringId.Comparer);
            foreach (KeyValuePair<MyStringId, object> pair in args)
            {
                this.SetArg(pair.Key, pair.Value);
            }
        }

        public object GetArg(MyStringId argName) => 
            (this.ArgNames.Contains<MyStringId>(argName) ? this.m_args[argName] : null);

        public void SetArg(MyStringId argName, object value)
        {
            this.m_args.Remove(argName);
            this.m_args.Add(argName, value);
        }

        public Dictionary<MyStringId, object>.KeyCollection ArgNames =>
            this.m_args.Keys;
    }
}

