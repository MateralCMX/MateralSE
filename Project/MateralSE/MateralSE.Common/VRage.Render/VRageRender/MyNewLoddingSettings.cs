namespace VRageRender
{
    using System;
    using System.Xml.Serialization;

    public class MyNewLoddingSettings
    {
        public MyPassLoddingSetting GBuffer = MyPassLoddingSetting.Default;
        private MyPassLoddingSetting[] m_cascadeDepth = new MyPassLoddingSetting[0];
        public MyPassLoddingSetting SingleDepth = MyPassLoddingSetting.Default;
        public MyPassLoddingSetting Forward = MyPassLoddingSetting.Default;
        public MyGlobalLoddingSettings Global = MyGlobalLoddingSettings.Default;

        public void CopyFrom(MyNewLoddingSettings settings)
        {
            this.GBuffer = settings.GBuffer;
            this.CascadeDepths = settings.CascadeDepths;
            this.SingleDepth = settings.SingleDepth;
            this.Forward = settings.Forward;
            this.Global = settings.Global;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MyNewLoddingSettings))
            {
                return false;
            }
            MyNewLoddingSettings settings = (MyNewLoddingSettings) obj;
            return (!this.GBuffer.Equals(settings.GBuffer) ? (this.CascadeDepths.Equals(settings.CascadeDepths) ? (!this.SingleDepth.Equals(settings.SingleDepth) ? (!this.Forward.Equals(settings.Forward) ? !this.Global.Equals(settings.Global) : false) : false) : false) : false);
        }

        [XmlArrayItem("CascadeDepth")]
        public MyPassLoddingSetting[] CascadeDepths
        {
            get => 
                this.m_cascadeDepth;
            set
            {
                if (this.m_cascadeDepth.Length != value.Length)
                {
                    this.m_cascadeDepth = new MyPassLoddingSetting[value.Length];
                }
                value.CopyTo(this.m_cascadeDepth, 0);
            }
        }
    }
}

