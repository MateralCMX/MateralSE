namespace VRage
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;

    public class CustomRootWriter : XmlWriter
    {
        private XmlWriter m_target;
        private string m_customRootType;
        private int m_currentDepth;

        public override void Close()
        {
            this.m_target.Close();
        }

        public override void Flush()
        {
            this.m_target.Flush();
        }

        internal void Init(string customRootType, XmlWriter target)
        {
            this.m_target = target;
            this.m_customRootType = customRootType;
            this.m_target.WriteAttributeString("xsi:type", this.m_customRootType);
            this.m_currentDepth = 0;
        }

        private static bool IsValidXmlString(string text) => 
            text.All<char>(new Func<char, bool>(XmlConvert.IsXmlChar));

        public override string LookupPrefix(string ns) => 
            this.m_target.LookupPrefix(ns);

        internal void Release()
        {
            this.m_target = null;
            this.m_customRootType = null;
        }

        private static string RemoveInvalidXmlChars(string text) => 
            new string((from ch in text
                where XmlConvert.IsXmlChar(ch)
                select ch).ToArray<char>());

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.m_target.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.m_target.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.m_target.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.m_target.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.m_target.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        public override void WriteEndAttribute()
        {
            this.m_target.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            while (this.m_currentDepth > 0)
            {
                this.WriteEndElement();
            }
        }

        public override void WriteEndElement()
        {
            this.m_currentDepth--;
            if (this.m_currentDepth > 0)
            {
                this.m_target.WriteEndElement();
            }
        }

        public override void WriteEntityRef(string name)
        {
            this.m_target.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.m_target.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.m_target.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            this.m_target.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.m_target.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.m_target.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument()
        {
        }

        public override void WriteStartDocument(bool standalone)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (this.m_currentDepth > 0)
            {
                this.m_target.WriteStartElement(prefix, localName, ns);
            }
            this.m_currentDepth++;
        }

        public override void WriteString(string text)
        {
            if (!IsValidXmlString(text))
            {
                string text1 = RemoveInvalidXmlChars(text);
                text = text1;
            }
            this.m_target.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.m_target.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.m_target.WriteWhitespace(ws);
        }

        public override System.Xml.WriteState WriteState =>
            this.m_target.WriteState;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CustomRootWriter.<>c <>9 = new CustomRootWriter.<>c();
            public static Func<char, bool> <>9__31_0;

            internal bool <RemoveInvalidXmlChars>b__31_0(char ch) => 
                XmlConvert.IsXmlChar(ch);
        }
    }
}

