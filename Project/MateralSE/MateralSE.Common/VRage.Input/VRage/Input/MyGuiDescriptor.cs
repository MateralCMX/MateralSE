namespace VRage.Input
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Utils;

    public class MyGuiDescriptor
    {
        private const string LINE_SPLIT_SEPARATOR = " | ";
        private static readonly string[] LINE_SEPARATOR = new string[] { MyEnvironment.NewLine };
        private bool m_isDirty = true;
        protected StringBuilder m_name;
        protected StringBuilder m_description;
        private MyStringId? m_descriptionEnum;
        private MyStringId m_nameEnum;

        public MyGuiDescriptor(MyStringId name, MyStringId? description = new MyStringId?())
        {
            this.m_nameEnum = name;
            this.DescriptionEnum = description;
        }

        private void UpdateDirty()
        {
            if (this.m_isDirty)
            {
                this.m_name = MyTexts.Get(this.m_nameEnum);
                this.m_description.Clear();
                if (this.m_descriptionEnum != null)
                {
                    MyUtils.SplitStringBuilder(this.m_description, MyTexts.Get(this.m_descriptionEnum.Value), " | ");
                }
                this.m_isDirty = false;
            }
        }

        public MyStringId? DescriptionEnum
        {
            get => 
                this.m_descriptionEnum;
            set
            {
                MyStringId? nullable = value;
                MyStringId? descriptionEnum = this.m_descriptionEnum;
                if (((nullable != null) == (descriptionEnum != null)) ? ((nullable != null) ? (nullable.GetValueOrDefault() != descriptionEnum.GetValueOrDefault()) : false) : true)
                {
                    this.m_descriptionEnum = value;
                    this.m_isDirty = true;
                }
            }
        }

        public MyStringId NameEnum
        {
            get => 
                this.m_nameEnum;
            set
            {
                if (value != this.m_nameEnum)
                {
                    this.m_nameEnum = value;
                    this.m_isDirty = true;
                }
            }
        }

        public StringBuilder Name
        {
            get
            {
                this.UpdateDirty();
                return this.m_name;
            }
        }

        public StringBuilder Description
        {
            get
            {
                this.UpdateDirty();
                return this.m_description;
            }
        }
    }
}

