namespace VRage.Serialization
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml.Serialization;

    [ProtoContract, XmlRoot("Dictionary"), Obfuscation(Feature="cw symbol renaming", Exclude=true, ApplyToMembers=true)]
    public class SerializableDictionaryHack<T, U>
    {
        [ProtoMember(20)]
        private Dictionary<T, U> m_dictionary;

        public SerializableDictionaryHack()
        {
            this.m_dictionary = new Dictionary<T, U>();
        }

        public SerializableDictionaryHack(Dictionary<T, U> dict)
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

        [XmlArray("dictionary"), XmlArrayItem("item", Type=typeof(DictionaryEntry))]
        public DictionaryEntry[] DictionaryEntryProp
        {
            get
            {
                DictionaryEntry[] entryArray = new DictionaryEntry[this.Dictionary.Count];
                int index = 0;
                foreach (KeyValuePair<T, U> pair in this.Dictionary)
                {
                    entryArray[index] = new DictionaryEntry { 
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
                        this.Dictionary.Add((T) value[i].Key, (U) value[i].Value);
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
    }
}

