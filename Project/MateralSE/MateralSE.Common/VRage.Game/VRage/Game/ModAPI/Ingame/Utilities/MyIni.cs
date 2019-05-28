namespace VRage.Game.ModAPI.Ingame.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MyIni
    {
        private readonly Dictionary<MyIniKey, StringSegment> m_items = new Dictionary<MyIniKey, StringSegment>(MyIniKeyComparer.DEFAULT);
        private readonly Dictionary<StringSegment, int> m_sections = new Dictionary<StringSegment, int>(StringSegmentIgnoreCaseComparer.DEFAULT);
        private readonly Dictionary<MyIniKey, StringSegment> m_itemComments = new Dictionary<MyIniKey, StringSegment>(MyIniKeyComparer.DEFAULT);
        private readonly Dictionary<StringSegment, StringSegment> m_sectionComments = new Dictionary<StringSegment, StringSegment>(StringSegmentIgnoreCaseComparer.DEFAULT);
        private string m_content;
        private int m_sectionCounter;
        private StringBuilder m_tmpContentBuilder;
        private StringBuilder m_tmpValueBuilder;
        private List<MyIniKey> m_tmpKeyList;
        private List<string> m_tmpStringList;
        private StringSegment m_endComment;
        private StringSegment m_endContent;

        private void AddSection(ref StringSegment section)
        {
            if (!this.m_sections.ContainsKey(section))
            {
                this.m_sections[section] = this.m_sectionCounter;
                this.m_sectionCounter++;
            }
        }

        public void Clear()
        {
            this.m_items.Clear();
            this.m_sections.Clear();
            this.m_content = null;
            this.m_sectionCounter = 0;
            this.m_endContent = new StringSegment();
            if (this.m_tmpContentBuilder != null)
            {
                this.m_tmpContentBuilder.Clear();
            }
            if (this.m_tmpValueBuilder != null)
            {
                this.m_tmpValueBuilder.Clear();
            }
            if (this.m_tmpKeyList != null)
            {
                this.m_tmpKeyList.Clear();
            }
            if (this.m_tmpStringList != null)
            {
                this.m_tmpStringList.Clear();
            }
        }

        public bool ContainsKey(MyIniKey key) => 
            this.m_items.ContainsKey(key);

        public bool ContainsKey(string section, string name) => 
            this.ContainsKey(new MyIniKey(section, name));

        public bool ContainsSection(string section) => 
            this.m_sections.ContainsKey(new StringSegment(section));

        public void Delete(MyIniKey key)
        {
            if (key.IsEmpty)
            {
                throw new ArgumentException("Key cannot be empty", "key");
            }
            this.m_items.Remove(key);
            this.m_content = null;
        }

        public void Delete(string section, string name)
        {
            this.Delete(new MyIniKey(section, name));
            this.m_content = null;
        }

        private static int FindSection(string config, string section)
        {
            TextPtr ptr = new TextPtr(config);
            if (MatchesSection(ref ptr, section))
            {
                return ptr.Index;
            }
            while (true)
            {
                if (!ptr.IsOutOfBounds())
                {
                    ptr = ptr.Find("\n") + 1;
                    if (ptr.Char == '[')
                    {
                        if (MatchesSection(ref ptr, section))
                        {
                            return ptr.Index;
                        }
                        continue;
                    }
                    if (!ptr.StartsWith("---"))
                    {
                        continue;
                    }
                    ptr = (ptr + 3).SkipWhitespace(false);
                    if (!ptr.IsEndOfLine())
                    {
                        continue;
                    }
                }
                return -1;
            }
        }

        private string GenerateContent()
        {
            string str6;
            StringBuilder tmpContentBuilder = this.TmpContentBuilder;
            List<MyIniKey> tmpKeyList = this.TmpKeyList;
            List<string> tmpStringList = this.TmpStringList;
            try
            {
                StringSegment endCommentSegment;
                bool flag = false;
                using (IEnumerator<StringSegment> enumerator = (from s in this.m_sections.Keys
                    orderby this.m_sections[s]
                    select s).GetEnumerator())
                {
                    int num;
                    goto TR_0030;
                TR_000F:
                    num++;
                TR_0022:
                    while (true)
                    {
                        if (num >= tmpKeyList.Count)
                        {
                            break;
                        }
                        MyIniKey key = tmpKeyList[num];
                        StringSegment nameSegment = key.NameSegment;
                        endCommentSegment = this.GetCommentSegment(key);
                        if (!endCommentSegment.IsEmpty)
                        {
                            endCommentSegment.GetLines(tmpStringList);
                            foreach (string str3 in tmpStringList)
                            {
                                tmpContentBuilder.Append(";");
                                tmpContentBuilder.Append(str3);
                                tmpContentBuilder.Append('\n');
                            }
                        }
                        tmpContentBuilder.Append(nameSegment.Text, nameSegment.Start, nameSegment.Length);
                        tmpContentBuilder.Append('=');
                        StringSegment segment4 = this.m_items[key];
                        if (NeedsMultilineFormat(ref segment4))
                        {
                            this.Realize(ref key, ref segment4);
                            segment4.GetLines(tmpStringList);
                            tmpContentBuilder.Append('\n');
                            foreach (string str4 in tmpStringList)
                            {
                                tmpContentBuilder.Append("|");
                                tmpContentBuilder.Append(str4);
                                tmpContentBuilder.Append('\n');
                            }
                        }
                        else
                        {
                            tmpContentBuilder.Append(segment4.Text, segment4.Start, segment4.Length);
                            tmpContentBuilder.Append('\n');
                        }
                        goto TR_000F;
                    }
                TR_0030:
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        StringSegment current = enumerator.Current;
                        if (flag)
                        {
                            tmpContentBuilder.Append('\n');
                        }
                        flag = true;
                        endCommentSegment = this.GetSectionCommentSegment(current);
                        if (!endCommentSegment.IsEmpty)
                        {
                            endCommentSegment.GetLines(tmpStringList);
                            foreach (string str2 in tmpStringList)
                            {
                                tmpContentBuilder.Append(";");
                                tmpContentBuilder.Append(str2);
                                tmpContentBuilder.Append('\n');
                            }
                        }
                        tmpContentBuilder.Append("[");
                        tmpContentBuilder.Append(current);
                        tmpContentBuilder.Append("]\n");
                        this.GetKeys(current, tmpKeyList);
                        num = 0;
                        goto TR_0022;
                    }
                }
                endCommentSegment = this.GetEndCommentSegment();
                if (!endCommentSegment.IsEmpty)
                {
                    tmpContentBuilder.Append('\n');
                    endCommentSegment.GetLines(tmpStringList);
                    foreach (string str5 in tmpStringList)
                    {
                        tmpContentBuilder.Append(";");
                        tmpContentBuilder.Append(str5);
                        tmpContentBuilder.Append('\n');
                    }
                }
                if (this.m_endContent.Length > 0)
                {
                    tmpContentBuilder.Append('\n');
                    tmpContentBuilder.Append("---\n");
                    tmpContentBuilder.Append(this.m_endContent);
                }
                string str = tmpContentBuilder.ToString();
                tmpContentBuilder.Clear();
                tmpKeyList.Clear();
                str6 = str;
            }
            finally
            {
                tmpContentBuilder.Clear();
                tmpKeyList.Clear();
            }
            return str6;
        }

        public MyIniValue Get(MyIniKey key)
        {
            StringSegment segment;
            if (!this.m_items.TryGetValue(key, out segment))
            {
                return MyIniValue.EMPTY;
            }
            this.Realize(ref key, ref segment);
            return new MyIniValue(key, segment.ToString());
        }

        public MyIniValue Get(string section, string name) => 
            this.Get(new MyIniKey(section, name));

        public string GetComment(MyIniKey key)
        {
            StringSegment commentSegment = this.GetCommentSegment(key);
            return (!commentSegment.IsEmpty ? commentSegment.ToString() : null);
        }

        public string GetComment(string section, string name) => 
            this.GetComment(new MyIniKey(section, name));

        private StringSegment GetCommentSegment(MyIniKey key)
        {
            StringSegment segment;
            if (!this.m_itemComments.TryGetValue(key, out segment))
            {
                return new StringSegment();
            }
            if (!segment.IsCached)
            {
                this.RealizeComment(ref segment);
                this.m_itemComments[key] = segment;
            }
            return segment;
        }

        private StringSegment GetEndCommentSegment()
        {
            StringSegment endComment = this.m_endComment;
            if (!endComment.IsCached)
            {
                this.RealizeComment(ref endComment);
                this.m_endComment = endComment;
            }
            return endComment;
        }

        public void GetKeys(List<MyIniKey> keys)
        {
            if (keys != null)
            {
                keys.Clear();
                foreach (MyIniKey key in this.m_items.Keys)
                {
                    keys.Add(key);
                }
            }
        }

        public void GetKeys(string section, List<MyIniKey> keys)
        {
            if (keys != null)
            {
                this.GetKeys(new StringSegment(section), keys);
            }
        }

        private void GetKeys(StringSegment section, List<MyIniKey> keys)
        {
            if (keys != null)
            {
                keys.Clear();
                foreach (MyIniKey key in this.m_items.Keys)
                {
                    StringSegment sectionSegment = key.SectionSegment;
                    if (sectionSegment.EqualsIgnoreCase(section))
                    {
                        keys.Add(key);
                    }
                }
            }
        }

        public string GetSectionComment(string section)
        {
            StringSegment key = new StringSegment(section);
            StringSegment sectionCommentSegment = this.GetSectionCommentSegment(key);
            return (!sectionCommentSegment.IsEmpty ? sectionCommentSegment.ToString() : null);
        }

        private StringSegment GetSectionCommentSegment(StringSegment key)
        {
            StringSegment segment;
            if (!this.m_sectionComments.TryGetValue(key, out segment))
            {
                return new StringSegment();
            }
            if (!segment.IsCached)
            {
                this.RealizeComment(ref segment);
                this.m_sectionComments[key] = segment;
            }
            return segment;
        }

        public void GetSections(List<string> names)
        {
            if (names != null)
            {
                names.Clear();
                foreach (StringSegment segment in this.m_sections.Keys)
                {
                    names.Add(segment.ToString());
                }
            }
        }

        public static bool HasSection(string config, string section) => 
            (FindSection(config, section) >= 0);

        public void Invalidate()
        {
            this.m_content = null;
        }

        private static bool MatchesSection(ref TextPtr ptr, string section)
        {
            if (!ptr.StartsWith("["))
            {
                return false;
            }
            TextPtr ptr2 = ptr + 1;
            if (!ptr2.StartsWithCaseInsensitive(section))
            {
                return false;
            }
            return (ptr2 + section.Length).StartsWith("]");
        }

        private static bool NeedsMultilineFormat(ref StringSegment str) => 
            ((str.Length > 0) && (char.IsWhiteSpace(str[0]) || (char.IsWhiteSpace(str[str.Length - 1]) || (str.IndexOf('\n') >= 0))));

        private void ReadPrefix(ref TextPtr ptr, out StringSegment prefix)
        {
            bool flag = false;
            TextPtr ptr2 = ptr;
            while (!ptr.IsOutOfBounds())
            {
                if (ptr.IsStartOfLine() && (ptr.Char == ';'))
                {
                    if (!flag)
                    {
                        flag = true;
                        ptr2 = ptr;
                    }
                    ptr = ptr.FindEndOfLine(false);
                }
                TextPtr ptr3 = ptr.SkipWhitespace(false);
                if (!ptr3.IsNewLine())
                {
                    break;
                }
                ptr = (ptr3.Char != '\r') ? (ptr3 + 1) : (ptr3 + 2);
            }
            if (flag)
            {
                TextPtr ptr4 = ptr;
                while (true)
                {
                    if (!char.IsWhiteSpace(ptr4.Char) || (ptr4 <= ptr2))
                    {
                        int length = ptr4.Index - ptr2.Index;
                        if (length <= 0)
                        {
                            break;
                        }
                        prefix = new StringSegment(ptr.Content, ptr2.Index, length);
                        return;
                    }
                    ptr4 -= 1;
                }
            }
            prefix = new StringSegment();
        }

        private void Realize(ref MyIniKey key, ref StringSegment value)
        {
            if (!value.IsCached)
            {
                string text = value.Text;
                TextPtr ptr = new TextPtr(text, value.Start);
                if ((value.Length > 0) && ptr.IsNewLine())
                {
                    StringBuilder tmpValueBuilder = this.TmpValueBuilder;
                    try
                    {
                        ptr = ptr.FindEndOfLine(true) + 1;
                        int count = (value.Start + value.Length) - ptr.Index;
                        tmpValueBuilder.Append(text, ptr.Index, count);
                        tmpValueBuilder.Replace("\n|", "\n");
                        this.m_items[key] = value = new StringSegment(tmpValueBuilder.ToString());
                    }
                    finally
                    {
                        tmpValueBuilder.Clear();
                    }
                }
                else
                {
                    this.m_items[key] = value = new StringSegment(value.ToString());
                }
            }
        }

        private void RealizeComment(ref StringSegment comment)
        {
            if (!comment.IsCached)
            {
                TextPtr ptr = new TextPtr(comment.Text, comment.Start);
                if (comment.Length > 0)
                {
                    StringBuilder tmpValueBuilder = this.TmpValueBuilder;
                    try
                    {
                        TextPtr ptr2 = ptr + comment.Length;
                        for (bool flag = false; ptr < ptr2; flag = true)
                        {
                            if (flag)
                            {
                                tmpValueBuilder.Append('\n');
                            }
                            if (ptr.Char != ';')
                            {
                                ptr = ptr.SkipWhitespace(false);
                                if (!ptr.IsEndOfLine())
                                {
                                    break;
                                }
                                ptr = (ptr.Char != '\r') ? (ptr + 1) : (ptr + 2);
                            }
                            else
                            {
                                ptr += 1;
                                TextPtr ptr3 = ptr.FindEndOfLine(false);
                                int count = ptr3.Index - ptr.Index;
                                tmpValueBuilder.Append(ptr.Content, ptr.Index, count);
                                ptr = ptr3.SkipWhitespace(false);
                                if (ptr.IsEndOfLine())
                                {
                                    ptr = (ptr.Char != '\r') ? (ptr + 1) : (ptr + 2);
                                }
                            }
                        }
                        comment = new StringSegment(tmpValueBuilder.ToString());
                    }
                    finally
                    {
                        tmpValueBuilder.Clear();
                    }
                }
            }
        }

        public void Set(MyIniKey key, bool value)
        {
            this.Set(key, value ? "true" : "false");
        }

        public void Set(MyIniKey key, byte value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, decimal value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, double value)
        {
            this.Set(key, value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, short value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, int value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, long value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, sbyte value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, float value)
        {
            this.Set(key, value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, string value)
        {
            if (key.IsEmpty)
            {
                throw new ArgumentException("Key cannot be empty", "key");
            }
            if (value == null)
            {
                this.Delete(key);
            }
            else
            {
                StringSegment sectionSegment = key.SectionSegment;
                this.AddSection(ref sectionSegment);
                this.m_items[key] = new StringSegment(value);
                this.m_content = null;
            }
        }

        public void Set(MyIniKey key, ushort value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, uint value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(MyIniKey key, ulong value)
        {
            this.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, bool value)
        {
            this.Set(section, name, value ? "true" : "false");
        }

        public void Set(string section, string name, byte value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, decimal value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, double value)
        {
            this.Set(section, name, value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, short value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, int value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, long value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, sbyte value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, float value)
        {
            this.Set(section, name, value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, string value)
        {
            this.Set(new MyIniKey(section, name), value);
        }

        public void Set(string section, string name, ushort value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, uint value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string section, string name, ulong value)
        {
            this.Set(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetComment(MyIniKey key, string comment)
        {
            if (!this.m_items.ContainsKey(key))
            {
                throw new ArgumentException("No item named " + key);
            }
            if (comment == null)
            {
                this.m_itemComments.Remove(key);
            }
            else
            {
                StringSegment segment = new StringSegment(comment);
                this.m_itemComments[key] = segment;
                this.m_content = null;
            }
        }

        public void SetComment(string section, string name, string comment)
        {
            this.SetComment(new MyIniKey(section, name), comment);
        }

        public void SetEndComment(string comment)
        {
            if (comment == null)
            {
                this.m_endComment = new StringSegment();
            }
            else
            {
                this.m_endComment = new StringSegment(comment);
                this.m_content = null;
            }
        }

        public void SetSectionComment(string section, string comment)
        {
            StringSegment key = new StringSegment(section);
            if (!this.m_sections.ContainsKey(key))
            {
                throw new ArgumentException("No section named " + section);
            }
            if (comment == null)
            {
                this.m_sectionComments.Remove(key);
            }
            else
            {
                StringSegment segment2 = new StringSegment(comment);
                this.m_sectionComments[key] = segment2;
                this.m_content = null;
            }
        }

        public override string ToString()
        {
            if (this.m_content == null)
            {
                this.m_content = this.GenerateContent();
            }
            return this.m_content;
        }

        public bool TryParse(string content)
        {
            MyIniParseResult result = new MyIniParseResult();
            return this.TryParseCore(content, null, ref result);
        }

        public bool TryParse(string content, out MyIniParseResult result) => 
            this.TryParse(content, null, out result);

        public bool TryParse(string content, string section)
        {
            MyIniParseResult result = new MyIniParseResult();
            return this.TryParseCore(content, section, ref result);
        }

        public bool TryParse(string content, string section, out MyIniParseResult result)
        {
            result = new MyIniParseResult(new TextPtr(content), null);
            return this.TryParseCore(content, section, ref result);
        }

        private bool TryParseCore(string content, string section, ref MyIniParseResult result)
        {
            content = content ?? "";
            if (string.Equals(this.m_content, content, StringComparison.Ordinal))
            {
                return true;
            }
            this.Clear();
            TextPtr ptr = new TextPtr(content);
            if (section != null)
            {
                int num = FindSection(content, section);
                if (num == -1)
                {
                    if (result.IsDefined)
                    {
                        result = new MyIniParseResult(new TextPtr(content), $"Cannot find section "{section}"");
                    }
                    return false;
                }
                ptr += num;
            }
            while (true)
            {
                if (!ptr.IsOutOfBounds())
                {
                    if (this.TryParseSection(ref ptr, ref result, ReferenceEquals(section, null)))
                    {
                        if (section != null)
                        {
                            this.m_content = null;
                            return true;
                        }
                        continue;
                    }
                    if (result.IsDefined && !result.Success)
                    {
                        return false;
                    }
                }
                this.m_content = content;
                return true;
            }
        }

        private bool TryParseItem(ref StringSegment section, ref TextPtr ptr, ref MyIniParseResult result, bool parseEndContent)
        {
            StringSegment segment;
            TextPtr ptr3;
            TextPtr ptr2 = ptr;
            this.ReadPrefix(ref ptr2, out segment);
            this.m_endComment = segment;
            if (ptr2.StartsWith("---"))
            {
                ptr3 = (ptr2 + 3).SkipWhitespace(false);
                if (ptr3.IsEndOfLine())
                {
                    this.m_endComment = segment;
                    ptr2 = ptr3;
                    ptr3 = new TextPtr(ptr2.Content, ptr2.Content.Length);
                    ptr = ptr3;
                    if (parseEndContent)
                    {
                        ptr2 = ptr2.FindEndOfLine(true);
                        this.m_endContent = new StringSegment(ptr2.Content, ptr2.Index, ptr3.Index - ptr2.Index);
                    }
                    return false;
                }
            }
            ptr2 = ptr2.TrimStart();
            if (ptr2.IsOutOfBounds() || (ptr2.Char == '['))
            {
                return false;
            }
            ptr3 = ptr2.FindInLine('=');
            if (ptr3.IsOutOfBounds())
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(ptr2, "Expected key=value definition");
                }
                return false;
            }
            StringSegment segment2 = new StringSegment(ptr2.Content, ptr2.Index, ptr3.TrimEnd().Index - ptr2.Index);
            string str = MyIniKey.ValidateKey(ref segment2);
            if (str != null)
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(ptr2, $"Key {str}");
                }
                return false;
            }
            ptr2 = (ptr3 + 1).TrimStart();
            ptr3 = ptr2.FindEndOfLine(false);
            StringSegment segment3 = new StringSegment(ptr2.Content, ptr2.Index, ptr3.TrimEnd().Index - ptr2.Index);
            if (segment3.Length == 0)
            {
                TextPtr ptr5 = ptr3.FindEndOfLine(true);
                if (ptr5.Char == '|')
                {
                    TextPtr ptr6 = ptr5;
                    while (true)
                    {
                        ptr5 = ptr6.FindEndOfLine(false);
                        ptr6 = ptr5.FindEndOfLine(true);
                        if (ptr6.Char != '|')
                        {
                            ptr3 = ptr5;
                            break;
                        }
                    }
                }
                segment3 = new StringSegment(ptr2.Content, ptr2.Index, ptr3.Index - ptr2.Index);
            }
            MyIniKey key = new MyIniKey(section, segment2);
            if (this.m_items.ContainsKey(key))
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(new TextPtr(segment2.Text, segment2.Start), $"Duplicate key {key}");
                }
                return false;
            }
            this.m_items[key] = segment3;
            if (!segment.IsEmpty)
            {
                this.m_itemComments[key] = segment;
                this.m_endComment = new StringSegment();
            }
            ptr = ptr3.FindEndOfLine(true);
            return true;
        }

        private bool TryParseSection(ref TextPtr ptr, ref MyIniParseResult result, bool parseEndContent)
        {
            StringSegment segment;
            TextPtr ptr2 = ptr;
            this.ReadPrefix(ref ptr2, out segment);
            this.m_endComment = segment;
            if (ptr2.Char != '[')
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(ptr, "Expected [section] definition");
                }
                return false;
            }
            TextPtr ptr3 = ptr2.FindEndOfLine(false);
            while ((ptr3.Index > ptr2.Index) && (ptr3.Char != ']'))
            {
                ptr3 -= 1;
            }
            if (ptr3.Char != ']')
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(ptr, "Expected [section] definition");
                }
                return false;
            }
            ptr2 += 1;
            StringSegment segment2 = new StringSegment(ptr2.Content, ptr2.Index, ptr3.Index - ptr2.Index);
            string str = MyIniKey.ValidateSection(ref segment2);
            if (str != null)
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(ptr2, $"Section {str}");
                }
                return false;
            }
            ptr2 = (ptr3 + 1).SkipWhitespace(false);
            if (!ptr2.IsEndOfLine())
            {
                if (result.IsDefined)
                {
                    result = new MyIniParseResult(ptr2, "Expected newline");
                }
                return false;
            }
            ptr2 = ptr2.FindEndOfLine(true);
            this.AddSection(ref segment2);
            if (!segment.IsEmpty)
            {
                this.m_sectionComments[segment2] = segment;
                this.m_endComment = new StringSegment();
            }
            while (this.TryParseItem(ref segment2, ref ptr2, ref result, parseEndContent))
            {
            }
            if (result.IsDefined && !result.Success)
            {
                return false;
            }
            ptr = ptr2;
            return true;
        }

        private StringBuilder TmpContentBuilder
        {
            get
            {
                if (this.m_tmpContentBuilder == null)
                {
                    this.m_tmpContentBuilder = new StringBuilder();
                }
                return this.m_tmpContentBuilder;
            }
        }

        private StringBuilder TmpValueBuilder
        {
            get
            {
                if (this.m_tmpValueBuilder == null)
                {
                    this.m_tmpValueBuilder = new StringBuilder();
                }
                return this.m_tmpValueBuilder;
            }
        }

        private List<MyIniKey> TmpKeyList
        {
            get
            {
                if (this.m_tmpKeyList == null)
                {
                    this.m_tmpKeyList = new List<MyIniKey>();
                }
                return this.m_tmpKeyList;
            }
        }

        private List<string> TmpStringList
        {
            get
            {
                if (this.m_tmpStringList == null)
                {
                    this.m_tmpStringList = new List<string>();
                }
                return this.m_tmpStringList;
            }
        }

        public string EndContent
        {
            get => 
                this.m_endContent.ToString();
            set
            {
                StringSegment segment1;
                if (value != null)
                {
                    segment1 = new StringSegment(value);
                }
                else
                {
                    segment1 = new StringSegment();
                }
                this.m_endContent = segment1;
                this.m_content = null;
            }
        }

        public string EndComment
        {
            get
            {
                StringSegment endCommentSegment = this.GetEndCommentSegment();
                return (!endCommentSegment.IsEmpty ? endCommentSegment.ToString() : null);
            }
            set
            {
                if (value == null)
                {
                    this.m_endComment = new StringSegment();
                }
                else
                {
                    this.m_endComment = new StringSegment(value);
                    this.m_content = null;
                }
            }
        }

        private class MyIniKeyComparer : IEqualityComparer<MyIniKey>
        {
            public static readonly MyIni.MyIniKeyComparer DEFAULT = new MyIni.MyIniKeyComparer();

            public bool Equals(MyIniKey x, MyIniKey y) => 
                x.Equals(y);

            public int GetHashCode(MyIniKey obj) => 
                obj.GetHashCode();
        }
    }
}

