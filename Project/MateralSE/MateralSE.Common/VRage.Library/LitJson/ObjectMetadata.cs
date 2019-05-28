namespace LitJson
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjectMetadata
    {
        private Type element_type;
        private bool is_dictionary;
        private IDictionary<string, PropertyMetadata> properties;
        public Type ElementType
        {
            get => 
                ((this.element_type != null) ? this.element_type : typeof(JsonData));
            set => 
                (this.element_type = value);
        }
        public bool IsDictionary
        {
            get => 
                this.is_dictionary;
            set => 
                (this.is_dictionary = value);
        }
        public IDictionary<string, PropertyMetadata> Properties
        {
            get => 
                this.properties;
            set => 
                (this.properties = value);
        }
    }
}

