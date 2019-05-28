namespace VRage.Common.Utils
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRage.Serialization;

    public sealed class MyChecksums
    {
        private string m_publicKey;

        public MyChecksums()
        {
            this.Items = new SerializableDictionaryHack<string, string>();
        }

        public string PublicKey
        {
            get => 
                this.m_publicKey;
            set
            {
                this.m_publicKey = value;
                this.PublicKeyAsArray = Convert.FromBase64String(this.m_publicKey);
            }
        }

        public SerializableDictionaryHack<string, string> Items { get; set; }

        [XmlIgnore]
        public byte[] PublicKeyAsArray { get; private set; }
    }
}

