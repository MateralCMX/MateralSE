namespace Sandbox.AppCode.Game.Sessions
{
    using System;

    internal class MyDataCorruptedException : ApplicationException
    {
        public MyDataCorruptedException()
        {
        }

        public MyDataCorruptedException(string message) : base(message)
        {
        }

        public MyDataCorruptedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

