namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;

    public class MyRenderException : Exception
    {
        private MyRenderExceptionEnum m_type;

        public MyRenderException(string message, MyRenderExceptionEnum type = 0) : base(message)
        {
            this.m_type = type;
        }

        public MyRenderExceptionEnum Type =>
            this.m_type;
    }
}

