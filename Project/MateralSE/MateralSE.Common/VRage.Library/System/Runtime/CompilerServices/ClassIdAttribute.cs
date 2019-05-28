namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public class ClassIdAttribute : Attribute, IComparable<ClassIdAttribute>
    {
        private string m_path;
        private int m_line;

        public ClassIdAttribute([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            this.m_path = path;
            this.m_line = line;
        }

        public int CompareTo(ClassIdAttribute other)
        {
            int num = this.m_path.CompareTo(other.m_path);
            return ((num != 0) ? num : this.m_line.CompareTo(other.m_line));
        }

        public static ClassIdAttribute Get<T>() => 
            ((ClassIdAttribute) GetCustomAttribute(typeof(T), typeof(ClassIdAttribute)));

        public static ClassIdAttribute Get(Type t) => 
            ((ClassIdAttribute) GetCustomAttribute(t, typeof(ClassIdAttribute)));
    }
}

