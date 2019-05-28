namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class My5BitEncoding
    {
        private static My5BitEncoding m_default;
        private char[] m_encodeTable;
        private Dictionary<char, byte> m_decodeTable;

        public My5BitEncoding() : this(new char[] { 
            '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
            'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        })
        {
        }

        public My5BitEncoding(char[] characters)
        {
            if (characters.Length != 0x20)
            {
                throw new ArgumentException("Characters array must have 32 characters!");
            }
            this.m_encodeTable = new char[0x20];
            characters.CopyTo(this.m_encodeTable, 0);
            this.m_decodeTable = this.CreateDecodeDict();
        }

        private Dictionary<char, byte> CreateDecodeDict()
        {
            Dictionary<char, byte> dictionary = new Dictionary<char, byte>(this.m_encodeTable.Length);
            for (byte i = 0; i < ((byte) this.m_encodeTable.Length); i = (byte) (i + 1))
            {
                dictionary.Add(this.m_encodeTable[i], i);
            }
            return dictionary;
        }

        public byte[] Decode(char[] encoded5BitText)
        {
            List<byte> list = new List<byte>();
            int num = 0;
            int num2 = 0;
            char[] chArray = encoded5BitText;
            int index = 0;
            while (index < chArray.Length)
            {
                byte num4;
                char key = chArray[index];
                if (!this.m_decodeTable.TryGetValue(key, out num4))
                {
                    throw new ArgumentException("Encoded text is not valid for this encoding!");
                }
                num += num4 << (num2 & 0x1f);
                num2 += 5;
                while (true)
                {
                    if (num2 < 8)
                    {
                        index++;
                        break;
                    }
                    int num5 = num & 0xff;
                    num = num >> 8;
                    num2 -= 8;
                    list.Add((byte) num5);
                }
            }
            return list.ToArray();
        }

        public char[] Encode(byte[] data)
        {
            StringBuilder builder = new StringBuilder((data.Length * 8) / 5);
            int index = 0;
            int num2 = 0;
            byte[] buffer = data;
            int num3 = 0;
            while (num3 < buffer.Length)
            {
                byte num4 = buffer[num3];
                index += num4 << (num2 & 0x1f);
                num2 += 8;
                while (true)
                {
                    if (num2 < 5)
                    {
                        num3++;
                        break;
                    }
                    int num5 = index & 0x1f;
                    index = index >> 5;
                    num2 -= 5;
                    builder.Append(this.m_encodeTable[num5]);
                }
            }
            if (num2 > 0)
            {
                builder.Append(this.m_encodeTable[index]);
            }
            return builder.ToString().ToCharArray();
        }

        public static My5BitEncoding Default
        {
            get
            {
                if (m_default == null)
                {
                    m_default = new My5BitEncoding();
                }
                return m_default;
            }
        }
    }
}

