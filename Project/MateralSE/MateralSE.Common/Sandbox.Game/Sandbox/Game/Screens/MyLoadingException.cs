namespace Sandbox.Game.Screens
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public class MyLoadingException : Exception
    {
        public MyLoadingException(string message, Exception innerException = null) : base(message.ToString(), innerException)
        {
        }

        public MyLoadingException(MyStringId message, Exception innerException = null) : base(MyTexts.GetString(message), innerException)
        {
        }
    }
}

