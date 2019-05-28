namespace VRage.Scripting
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MyWhitelistException : Exception
    {
        public MyWhitelistException()
        {
        }

        public MyWhitelistException(string message) : base(message)
        {
        }

        protected MyWhitelistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MyWhitelistException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

