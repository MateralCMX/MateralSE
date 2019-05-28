namespace Unsharper
{
    using System;

    [UnsharperDisableReflection]
    public class UnsharperStaticInitializersPriority : Attribute
    {
        public UnsharperStaticInitializersPriority(int i)
        {
        }
    }
}

