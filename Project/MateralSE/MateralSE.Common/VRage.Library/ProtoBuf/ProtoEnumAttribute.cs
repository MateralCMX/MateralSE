namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public sealed class ProtoEnumAttribute : Attribute
    {
        private bool hasValue;
        private int enumValue;
        private string name;

        public bool HasValue() => 
            this.hasValue;

        public int Value
        {
            get => 
                this.enumValue;
            set
            {
                this.enumValue = value;
                this.hasValue = true;
            }
        }

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }
    }
}

