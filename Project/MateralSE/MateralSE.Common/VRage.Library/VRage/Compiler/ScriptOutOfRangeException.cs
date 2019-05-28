namespace VRage.Compiler
{
    using System;

    public class ScriptOutOfRangeException : Exception
    {
        public ScriptOutOfRangeException()
        {
        }

        public ScriptOutOfRangeException(string message) : base(message)
        {
        }
    }
}

