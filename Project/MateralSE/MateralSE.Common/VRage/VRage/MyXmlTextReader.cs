namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Xml;

    public class MyXmlTextReader : XmlReader
    {
        private XmlReader m_reader;

        public MyXmlTextReader(Stream input, XmlReaderSettings settings)
        {
            this.m_reader = Create(input, settings);
        }

        public override string GetAttribute(int i) => 
            this.m_reader.GetAttribute(i);

        public override string GetAttribute(string name) => 
            this.m_reader.GetAttribute(name);

        public override string GetAttribute(string name, string namespaceURI) => 
            this.m_reader.GetAttribute(name, namespaceURI);

        public override string LookupNamespace(string prefix) => 
            this.m_reader.LookupNamespace(prefix);

        public override bool MoveToAttribute(string name) => 
            this.m_reader.MoveToAttribute(name);

        public override bool MoveToAttribute(string name, string ns) => 
            this.m_reader.MoveToAttribute(name, ns);

        public override bool MoveToElement() => 
            this.m_reader.MoveToElement();

        public override bool MoveToFirstAttribute() => 
            this.m_reader.MoveToFirstAttribute();

        public override bool MoveToNextAttribute() => 
            this.m_reader.MoveToNextAttribute();

        public override bool Read() => 
            this.m_reader.Read();

        public override bool ReadAttributeValue() => 
            this.m_reader.ReadAttributeValue();

        public override void ResolveEntity()
        {
            this.m_reader.ResolveEntity();
        }

        public Dictionary<string, string> DefinitionTypeOverrideMap { get; set; }

        public override int AttributeCount =>
            this.m_reader.AttributeCount;

        public override string BaseURI =>
            this.m_reader.BaseURI;

        public override int Depth =>
            this.m_reader.Depth;

        public override bool EOF =>
            this.m_reader.EOF;

        public override bool IsEmptyElement =>
            this.m_reader.IsEmptyElement;

        public override string LocalName =>
            this.m_reader.LocalName;

        public override XmlNameTable NameTable =>
            this.m_reader.NameTable;

        public override string NamespaceURI =>
            this.m_reader.NamespaceURI;

        public override XmlNodeType NodeType =>
            this.m_reader.NodeType;

        public override string Prefix =>
            this.m_reader.Prefix;

        public override System.Xml.ReadState ReadState =>
            this.m_reader.ReadState;

        public override string Value =>
            this.m_reader.Value;
    }
}

