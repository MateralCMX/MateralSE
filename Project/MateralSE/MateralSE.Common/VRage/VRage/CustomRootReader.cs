namespace VRage
{
    using System;
    using System.Xml;

    public class CustomRootReader : XmlReader
    {
        private XmlReader m_source;
        private string m_customRootName;
        private int m_rootDepth;

        public override void Close()
        {
            this.m_source.Close();
        }

        public override string GetAttribute(int i) => 
            this.m_source.GetAttribute(i);

        public override string GetAttribute(string name) => 
            this.m_source.GetAttribute(name);

        public override string GetAttribute(string name, string namespaceURI) => 
            ((this.m_source.Depth == this.m_rootDepth) ? null : this.m_source.GetAttribute(name, namespaceURI));

        internal void Init(string customRootName, XmlReader source)
        {
            this.m_source = source;
            this.m_customRootName = customRootName;
            this.m_rootDepth = source.Depth;
        }

        public override string LookupNamespace(string prefix) => 
            this.m_source.LookupNamespace(prefix);

        public override bool MoveToAttribute(string name) => 
            this.m_source.MoveToAttribute(name);

        public override bool MoveToAttribute(string name, string ns) => 
            this.m_source.MoveToAttribute(name, ns);

        public override bool MoveToElement() => 
            this.m_source.MoveToElement();

        public override bool MoveToFirstAttribute() => 
            this.m_source.MoveToFirstAttribute();

        public override bool MoveToNextAttribute() => 
            this.m_source.MoveToNextAttribute();

        public override bool Read() => 
            this.m_source.Read();

        public override bool ReadAttributeValue() => 
            this.m_source.ReadAttributeValue();

        internal void Release()
        {
            this.m_source = null;
            this.m_customRootName = null;
            this.m_rootDepth = -1;
        }

        public override void ResolveEntity()
        {
            this.m_source.ResolveEntity();
        }

        public override int AttributeCount =>
            this.m_source.AttributeCount;

        public override string BaseURI =>
            this.m_source.BaseURI;

        public override int Depth =>
            this.m_source.Depth;

        public override bool EOF =>
            this.m_source.EOF;

        public override bool IsEmptyElement =>
            this.m_source.IsEmptyElement;

        public override XmlNameTable NameTable =>
            this.m_source.NameTable;

        public override XmlNodeType NodeType =>
            this.m_source.NodeType;

        public override string Prefix =>
            this.m_source.Prefix;

        public override System.Xml.ReadState ReadState =>
            this.m_source.ReadState;

        public override string Value =>
            this.m_source.Value;

        public override string LocalName =>
            ((this.m_source.Depth == this.m_rootDepth) ? this.m_source.NameTable.Get(this.m_customRootName) : this.m_source.LocalName);

        public override string NamespaceURI =>
            ((this.m_source.Depth == this.m_rootDepth) ? this.m_source.NameTable.Get("") : this.m_source.NamespaceURI);
    }
}

