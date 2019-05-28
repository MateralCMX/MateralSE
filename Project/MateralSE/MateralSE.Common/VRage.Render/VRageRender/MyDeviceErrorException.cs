namespace VRageRender
{
    using System;

    public class MyDeviceErrorException : Exception
    {
        public string Message;

        public MyDeviceErrorException(string message)
        {
            this.Message = message;
        }
    }
}

