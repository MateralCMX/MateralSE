namespace VRage.Serialization
{
    using System;

    public class MySerializeException : Exception
    {
        public MySerializeErrorEnum Error;

        public MySerializeException(MySerializeErrorEnum error)
        {
            this.Error = error;
        }
    }
}

