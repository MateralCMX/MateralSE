namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using VRage.FileSystem;
    using VRage.Utils;
    using VRageMath;

    public class MyFont
    {
        protected const char REPLACEMENT_CHARACTER = '□';
        protected const char ELLIPSIS = '…';
        public const char NEW_LINE = '\n';
        private static readonly KernPairComparer m_kernPairComparer = new KernPairComparer();
        protected readonly Dictionary<int, MyBitmapInfo> m_bitmapInfoByID = new Dictionary<int, MyBitmapInfo>();
        protected readonly Dictionary<char, MyGlyphInfo> m_glyphInfoByChar = new Dictionary<char, MyGlyphInfo>();
        protected readonly Dictionary<KernPair, sbyte> m_kernByPair = new Dictionary<KernPair, sbyte>(m_kernPairComparer);
        protected readonly string m_fontDirectory;
        public int Spacing;
        public bool KernEnabled = true;
        public float Depth;

        public MyFont(string fontFilePath, int spacing = 1)
        {
            MyRenderProxy.Log.WriteLine("MyFont.Ctor - START");
            using (MyRenderProxy.Log.IndentUsing(LoggingOptions.MISC_RENDER_ASSETS))
            {
                this.Spacing = spacing;
                MyRenderProxy.Log.WriteLine("Font filename: " + fontFilePath);
                string path = fontFilePath;
                if (!Path.IsPathRooted(fontFilePath))
                {
                    path = Path.Combine(MyFileSystem.ContentPath, fontFilePath);
                }
                if (!MyFileSystem.FileExists(path))
                {
                    throw new Exception($"Unable to find font path '{path}'.");
                }
                this.m_fontDirectory = Path.GetDirectoryName(path);
                this.LoadFontXML(path);
                MyRenderProxy.Log.WriteLine("FontFilePath: " + path);
                MyRenderProxy.Log.WriteLine("LineHeight: " + this.LineHeight);
                MyRenderProxy.Log.WriteLine("Baseline: " + this.Baseline);
                MyRenderProxy.Log.WriteLine("KernEnabled: " + this.KernEnabled.ToString());
            }
            MyRenderProxy.Log.WriteLine("MyFont.Ctor - END");
        }

        protected int CalcKern(char chLeft, char chRight)
        {
            sbyte num = 0;
            this.m_kernByPair.TryGetValue(new KernPair(chLeft, chRight), out num);
            return num;
        }

        protected bool CanUseReplacementCharacter(char c) => 
            (!char.IsWhiteSpace(c) && !char.IsControl(c));

        protected bool CanWriteOrReplace(ref char c)
        {
            if (!this.m_glyphInfoByChar.ContainsKey(c))
            {
                if (!this.CanUseReplacementCharacter(c))
                {
                    return false;
                }
                c = '□';
            }
            return true;
        }

        public int ComputeCharsThatFit(StringBuilder text, float scale, float maxTextWidth)
        {
            scale *= 0.7783784f;
            maxTextWidth /= scale;
            float num = 0f;
            char chLeft = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (this.CanWriteOrReplace(ref c))
                {
                    MyGlyphInfo info = this.m_glyphInfoByChar[c];
                    if (this.KernEnabled)
                    {
                        num += this.CalcKern(chLeft, c);
                        chLeft = c;
                    }
                    num += info.pxAdvanceWidth;
                    if (i < (text.Length - 1))
                    {
                        num += this.Spacing;
                    }
                    if (num > maxTextWidth)
                    {
                        return i;
                    }
                }
            }
            return text.Length;
        }

        protected float ComputeScaledAdvanceWithKern(char c, char cLast, float scale)
        {
            if (!this.CanWriteOrReplace(ref c))
            {
                return 0f;
            }
            MyGlyphInfo info = this.m_glyphInfoByChar[c];
            float num = 0f;
            if (this.KernEnabled)
            {
                num += this.CalcKern(cLast, c) * scale;
            }
            return (num + (info.pxAdvanceWidth * scale));
        }

        private static string GetXMLAttribute(XmlNode n, string strAttr)
        {
            XmlAttribute namedItem = n.Attributes.GetNamedItem(strAttr) as XmlAttribute;
            return ((namedItem == null) ? "" : namedItem.Value);
        }

        private void LoadFontXML(string path)
        {
            XmlDocument document = new XmlDocument();
            using (Stream stream = MyFileSystem.OpenRead(path))
            {
                document.Load(stream);
            }
            this.LoadFontXML(document.ChildNodes);
        }

        private void LoadFontXML(XmlNodeList xnl)
        {
            foreach (XmlNode node in xnl)
            {
                if (node.Name == "font")
                {
                    this.Baseline = int.Parse(GetXMLAttribute(node, "base"));
                    this.LineHeight = int.Parse(GetXMLAttribute(node, "height"));
                    this.LoadFontXML_font(node.ChildNodes);
                }
            }
        }

        private void LoadFontXML_bitmaps(XmlNodeList xnl)
        {
            foreach (XmlNode node in xnl)
            {
                if (node.Name == "bitmap")
                {
                    MyBitmapInfo info;
                    string xMLAttribute = GetXMLAttribute(node, "id");
                    char[] separator = new char[] { 'x' };
                    string[] strArray = GetXMLAttribute(node, "size").Split(separator);
                    info.strFilename = GetXMLAttribute(node, "name");
                    info.nX = int.Parse(strArray[0]);
                    info.nY = int.Parse(strArray[1]);
                    this.m_bitmapInfoByID[int.Parse(xMLAttribute)] = info;
                }
            }
        }

        private void LoadFontXML_font(XmlNodeList xnl)
        {
            foreach (XmlNode node in xnl)
            {
                if (node.Name == "bitmaps")
                {
                    this.LoadFontXML_bitmaps(node.ChildNodes);
                }
                if (node.Name == "glyphs")
                {
                    this.LoadFontXML_glyphs(node.ChildNodes);
                }
                if (node.Name == "kernpairs")
                {
                    this.LoadFontXML_kernpairs(node.ChildNodes);
                }
            }
        }

        private void LoadFontXML_glyphs(XmlNodeList xnl)
        {
            foreach (XmlNode node in xnl)
            {
                if (node.Name == "glyph")
                {
                    string xMLAttribute = GetXMLAttribute(node, "ch");
                    string s = GetXMLAttribute(node, "bm");
                    string str3 = GetXMLAttribute(node, "loc");
                    string str4 = GetXMLAttribute(node, "size");
                    string str5 = GetXMLAttribute(node, "aw");
                    string str6 = GetXMLAttribute(node, "lsb");
                    if (str3 == "")
                    {
                        str3 = GetXMLAttribute(node, "origin");
                    }
                    char[] separator = new char[] { ',' };
                    string[] strArray = str3.Split(separator);
                    char[] chArray2 = new char[] { 'x' };
                    string[] strArray2 = str4.Split(chArray2);
                    MyGlyphInfo info1 = new MyGlyphInfo();
                    info1.nBitmapID = ushort.Parse(s);
                    info1.pxLocX = ushort.Parse(strArray[0]);
                    info1.pxLocY = ushort.Parse(strArray[1]);
                    info1.pxWidth = byte.Parse(strArray2[0]);
                    info1.pxHeight = byte.Parse(strArray2[1]);
                    info1.pxAdvanceWidth = byte.Parse(str5);
                    info1.pxLeftSideBearing = sbyte.Parse(str6);
                    MyGlyphInfo info = info1;
                    this.m_glyphInfoByChar[xMLAttribute[0]] = info;
                }
            }
        }

        private void LoadFontXML_kernpairs(XmlNodeList xnl)
        {
            foreach (XmlNode node in xnl)
            {
                if (node.Name == "kernpair")
                {
                    KernPair pair = new KernPair(GetXMLAttribute(node, "left")[0], GetXMLAttribute(node, "right")[0]);
                    this.m_kernByPair[pair] = sbyte.Parse(GetXMLAttribute(node, "adjust"));
                }
            }
        }

        public Vector2 MeasureString(StringBuilder text, float scale)
        {
            scale *= 0.7783784f;
            float num = 0f;
            char chLeft = '\0';
            float num2 = 0f;
            int num3 = 1;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    num3++;
                    num = 0f;
                    chLeft = '\0';
                }
                else if (this.CanWriteOrReplace(ref c))
                {
                    MyGlyphInfo info = this.m_glyphInfoByChar[c];
                    if (this.KernEnabled)
                    {
                        num += this.CalcKern(chLeft, c);
                        chLeft = c;
                    }
                    num += info.pxAdvanceWidth;
                    if (i < (text.Length - 1))
                    {
                        num += this.Spacing;
                    }
                    if (num > num2)
                    {
                        num2 = num;
                    }
                }
            }
            return new Vector2(num2 * scale, (num3 * this.LineHeight) * scale);
        }

        public int Baseline { get; private set; }

        public int LineHeight { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        protected struct KernPair
        {
            public char Left;
            public char Right;
            public KernPair(char l, char r)
            {
                this.Left = l;
                this.Right = r;
            }
        }

        protected class KernPairComparer : IComparer<MyFont.KernPair>, IEqualityComparer<MyFont.KernPair>
        {
            public int Compare(MyFont.KernPair x, MyFont.KernPair y) => 
                ((x.Left == y.Left) ? x.Right.CompareTo(y.Right) : x.Left.CompareTo(y.Left));

            public bool Equals(MyFont.KernPair x, MyFont.KernPair y) => 
                ((x.Left == y.Left) && (x.Right == y.Right));

            public int GetHashCode(MyFont.KernPair x) => 
                (x.Left.GetHashCode() ^ x.Right.GetHashCode());
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct MyBitmapInfo
        {
            public string strFilename;
            public int nX;
            public int nY;
        }

        protected class MyGlyphInfo
        {
            public ushort nBitmapID;
            public ushort pxLocX;
            public ushort pxLocY;
            public byte pxWidth;
            public byte pxHeight;
            public byte pxAdvanceWidth;
            public sbyte pxLeftSideBearing;
        }
    }
}

