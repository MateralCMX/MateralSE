namespace VRage.Serialization
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ProtoContract, XmlRoot("Dictionary"), Obfuscation(Feature="cw symbol renaming", Exclude=true, ApplyToMembers=true)]
    public class SerializableDictionary<T, U>
    {
        [ProtoMember(0x12)]
        private Dictionary<T, U> m_dictionary;

        public SerializableDictionary()
        {
            this.m_dictionary = new Dictionary<T, U>();
        }

        public SerializableDictionary(Dictionary<T, U> dict)
        {
            this.m_dictionary = new Dictionary<T, U>();
            this.Dictionary = dict;
        }

        [XmlIgnore]
        public Dictionary<T, U> Dictionary
        {
            get => 
                this.m_dictionary;
            set => 
                (this.m_dictionary = value);
        }

        [XmlArray("dictionary"), XmlArrayItem("item"), NoSerialize]
        public Entry<T, U>[] DictionaryEntryProp
        {
            get
            {
                Entry<T, U>[] entryArray = new Entry<T, U>[this.Dictionary.Count];
                int index = 0;
                foreach (KeyValuePair<T, U> pair in this.Dictionary)
                {
                    entryArray[index] = new Entry<T, U> { 
                        Key = pair.Key,
                        Value = pair.Value
                    };
                    index++;
                }
                return entryArray;
            }
            set
            {
                this.Dictionary.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    try
                    {
                        this.Dictionary.Add(value[i].Key, value[i].Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public U this[T key]
        {
            get => 
                this.Dictionary[key];
            set => 
                (this.Dictionary[key] = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Entry
        {
            public T Key;
            public U Value;
        }
    }
}

