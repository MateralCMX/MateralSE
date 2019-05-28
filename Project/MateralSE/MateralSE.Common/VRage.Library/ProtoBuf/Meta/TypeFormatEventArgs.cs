namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using System;

    public class TypeFormatEventArgs : EventArgs
    {
        private System.Type type;
        private string formattedName;
        private readonly bool typeFixed;

        internal TypeFormatEventArgs(string formattedName)
        {
            if (Helpers.IsNullOrEmpty(formattedName))
            {
                throw new ArgumentNullException("formattedName");
            }
            this.formattedName = formattedName;
            this.typeFixed = false;
        }

        internal TypeFormatEventArgs(System.Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.type = type;
            this.typeFixed = true;
        }

        public System.Type Type
        {
            get => 
                this.type;
            set
            {
                if (this.type != value)
                {
                    if (this.typeFixed)
                    {
                        throw new InvalidOperationException("The type is fixed and cannot be changed");
                    }
                    this.type = value;
                }
            }
        }

        public string FormattedName
        {
            get => 
                this.formattedName;
            set
            {
                if (this.formattedName != value)
                {
                    if (!this.typeFixed)
                    {
                        throw new InvalidOperationException("The formatted-name is fixed and cannot be changed");
                    }
                    this.formattedName = value;
                }
            }
        }
    }
}

