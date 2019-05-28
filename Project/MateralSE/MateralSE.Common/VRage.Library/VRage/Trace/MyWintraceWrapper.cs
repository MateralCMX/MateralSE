namespace VRage.Trace
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;

    internal class MyWintraceWrapper : ITrace
    {
        private static readonly Type m_winTraceType;
        private static readonly object m_winWatches;
        private readonly object m_trace;
        private readonly Action m_clearAll;
        private readonly Action<string, object> m_send;
        private readonly Action<string, string> m_debugSend;

        static MyWintraceWrapper()
        {
            Assembly assembly1 = TryLoad("TraceTool.dll");
            Assembly assembly3 = assembly1;
            if (assembly1 == null)
            {
                Assembly local1 = assembly1;
                Assembly assembly2 = TryLoad(MyFileSystem.ExePath + "/../../../../../../3rd/TraceTool/TraceTool.dll");
                assembly3 = assembly2 ?? TryLoad(MyFileSystem.ExePath + "/../../../3rd/TraceTool/TraceTool.dll");
            }
            Assembly assembly = assembly3;
            if (assembly != null)
            {
                m_winTraceType = assembly.GetType("TraceTool.WinTrace");
                m_winWatches = assembly.GetType("TraceTool.TTrace").GetProperty("Watches").GetGetMethod().Invoke(null, new object[0]);
            }
        }

        private MyWintraceWrapper(object trace)
        {
            this.m_trace = trace;
            this.m_clearAll = Expression.Lambda<Action>(Expression.Call(Expression.Constant(this.m_trace), trace.GetType().GetMethod("ClearAll")), Array.Empty<ParameterExpression>()).Compile();
            this.m_clearAll();
            ParameterExpression expression = Expression.Parameter(typeof(string));
            ParameterExpression expression2 = Expression.Parameter(typeof(object));
            MethodCallExpression body = Expression.Call(Expression.Constant(m_winWatches), m_winWatches.GetType().GetMethod("Send"), expression, expression2);
            ParameterExpression[] parameters = new ParameterExpression[] { expression, expression2 };
            this.m_send = Expression.Lambda<Action<string, object>>(body, parameters).Compile();
            ParameterExpression expression4 = Expression.Parameter(typeof(string));
            ParameterExpression expression5 = Expression.Parameter(typeof(string));
            MemberExpression instance = Expression.PropertyOrField(Expression.Constant(this.m_trace), "Debug");
            Type[] types = new Type[] { typeof(string), typeof(string) };
            MethodCallExpression expression7 = Expression.Call(instance, instance.Expression.Type.GetMethod("Send", types), expression4, expression5);
            ParameterExpression[] expressionArray2 = new ParameterExpression[] { expression4, expression5 };
            this.m_debugSend = Expression.Lambda<Action<string, string>>(expression7, expressionArray2).Compile();
        }

        public static ITrace CreateTrace(string id, string name)
        {
            if (m_winTraceType == null)
            {
                return new MyNullTrace();
            }
            object[] args = new object[] { id, name };
            return new MyWintraceWrapper(Activator.CreateInstance(m_winTraceType, args));
        }

        public void Send(string msg, string comment = null)
        {
            try
            {
                this.m_debugSend(msg, comment);
            }
            catch
            {
            }
        }

        private static Assembly TryLoad(string assembly)
        {
            if (!File.Exists(assembly))
            {
                return null;
            }
            try
            {
                return Assembly.LoadFrom(assembly);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Watch(string name, object value)
        {
            try
            {
                this.m_send(name, value);
            }
            catch
            {
            }
        }
    }
}

