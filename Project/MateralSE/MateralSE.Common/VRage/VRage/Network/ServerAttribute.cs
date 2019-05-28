namespace VRage.Network
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class ServerAttribute : Attribute
    {
        public readonly string Validation;
        public readonly ValidationType ValidationFlags;

        public ServerAttribute()
        {
        }

        public ServerAttribute(string validationMethod)
        {
            this.Validation = validationMethod;
        }

        public ServerAttribute(ValidationType validationFlags)
        {
            this.ValidationFlags = validationFlags;
        }
    }
}

