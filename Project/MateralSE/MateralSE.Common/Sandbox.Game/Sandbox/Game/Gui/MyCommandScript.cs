namespace Sandbox.Game.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using VRage;

    public class MyCommandScript : MyCommand
    {
        private Type m_type;
        private static StringBuilder m_cache = new StringBuilder();

        public MyCommandScript(Type type)
        {
            this.m_type = type;
            int num = 0;
            foreach (MethodInfo method in type.GetMethods())
            {
                if (method.IsPublic && method.IsStatic)
                {
                    MyCommand.MyCommandAction action1 = new MyCommand.MyCommandAction();
                    action1.AutocompleteHint = this.GetArgsString(method);
                    action1.Parser = x => this.ParseArgs(x, method);
                    action1.CallAction = x => this.Invoke(x, method);
                    MyCommand.MyCommandAction action = action1;
                    num++;
                    base.m_methods.Add($"{num}{method.Name}", action);
                }
            }
        }

        private StringBuilder GetArgsString(MethodInfo method)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ParameterInfo info in method.GetParameters())
            {
                builder.Append($"{info.ParameterType.Name} {info.Name}, ");
            }
            return builder;
        }

        private StringBuilder Invoke(MyCommandArgs x, MethodInfo method)
        {
            m_cache.Clear();
            MyCommandMethodArgs args = x as MyCommandMethodArgs;
            if (args.Args == null)
            {
                m_cache.Append($"Invoking {method.Name} failed");
            }
            else
            {
                m_cache.Append("Success. ");
                object obj2 = method.Invoke(null, args.Args);
                if (obj2 != null)
                {
                    m_cache.Append(obj2.ToString());
                }
            }
            return m_cache;
        }

        private MyCommandArgs ParseArgs(List<string> x, MethodInfo method)
        {
            MyCommandMethodArgs args = new MyCommandMethodArgs();
            ParameterInfo[] parameters = method.GetParameters();
            List<object> list = new List<object>();
            for (int i = 0; (i < parameters.Length) && (i < x.Count); i++)
            {
                Type parameterType = parameters[i].ParameterType;
                Type[] types = new Type[] { typeof(string), parameterType.MakeByRefType() };
                MethodInfo info = parameterType.GetMethod("TryParse", types);
                if (info == null)
                {
                    list.Add(x[i]);
                }
                else
                {
                    object obj2 = Activator.CreateInstance(parameterType);
                    object[] objArray = new object[] { x[i], obj2 };
                    info.Invoke(null, objArray);
                    list.Add(objArray[1]);
                }
            }
            if (parameters.Length == list.Count)
            {
                args.Args = list.ToArray();
            }
            return args;
        }

        public override string Prefix() => 
            this.m_type.Name;

        private class MyCommandMethodArgs : MyCommandArgs
        {
            public object[] Args;
        }
    }
}

