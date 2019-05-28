namespace Sandbox.Engine.Utils
{
    using System;

    public class MyObfuscation
    {
        public static readonly bool Enabled = (new MyObfuscation().GetType().Name != "MyObfuscation");

        private MyObfuscation()
        {
        }
    }
}

