namespace VRage.Game.Components.Session
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Serialization;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x3e8, typeof(MyObjectBuilder_SharedStorageComponent), (Type) null)]
    public class MySessionComponentScriptSharedStorage : MySessionComponentBase
    {
        private MyObjectBuilder_SharedStorageComponent m_objectBuilder;
        private static MySessionComponentScriptSharedStorage m_instance;

        public SerializableDictionary<string, bool> GetBools() => 
            this.m_objectBuilder.BoolStorage;

        public Dictionary<string, bool> GetBoolsByRegex(Regex nameRegex)
        {
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, bool> pair in this.m_objectBuilder.BoolStorage.Dictionary)
            {
                if (nameRegex.IsMatch(pair.Key))
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
            }
            return dictionary;
        }

        public SerializableDictionary<string, bool> GetExistingFieldsandStaticAttributes() => 
            this.m_objectBuilder.ExistingFieldsAndStaticAttribute;

        public SerializableDictionary<string, float> GetFloats() => 
            this.m_objectBuilder.FloatStorage;

        public SerializableDictionary<string, int> GetInts() => 
            this.m_objectBuilder.IntStorage;

        public SerializableDictionary<string, long> GetLongs() => 
            this.m_objectBuilder.LongStorage;

        public override MyObjectBuilder_SessionComponent GetObjectBuilder() => 
            this.m_objectBuilder;

        public SerializableDictionary<string, string> GetStrings() => 
            this.m_objectBuilder.StringStorage;

        public SerializableDictionary<string, SerializableVector3D> GetVector3D() => 
            this.m_objectBuilder.Vector3DStorage;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_SharedStorageComponent component = sessionComponent as MyObjectBuilder_SharedStorageComponent;
            MyObjectBuilder_SharedStorageComponent component1 = new MyObjectBuilder_SharedStorageComponent();
            component1.BoolStorage = component.BoolStorage;
            component1.FloatStorage = component.FloatStorage;
            component1.StringStorage = component.StringStorage;
            component1.IntStorage = component.IntStorage;
            component1.Vector3DStorage = component.Vector3DStorage;
            component1.LongStorage = component.LongStorage;
            component1.ExistingFieldsAndStaticAttribute = component.ExistingFieldsAndStaticAttribute;
            this.m_objectBuilder = component1;
            m_instance = this;
        }

        public bool ReadBool(string variableName)
        {
            bool flag;
            return ((this.m_objectBuilder != null) ? (this.m_objectBuilder.BoolStorage.Dictionary.TryGetValue(variableName, out flag) && flag) : false);
        }

        public float ReadFloat(string variableName)
        {
            float num;
            return ((this.m_objectBuilder != null) ? (!this.m_objectBuilder.FloatStorage.Dictionary.TryGetValue(variableName, out num) ? 0f : num) : 0f);
        }

        public int ReadInt(string variableName)
        {
            int num;
            return ((this.m_objectBuilder != null) ? (!this.m_objectBuilder.IntStorage.Dictionary.TryGetValue(variableName, out num) ? -1 : num) : -1);
        }

        public long ReadLong(string variableName)
        {
            long num;
            return ((this.m_objectBuilder != null) ? (!this.m_objectBuilder.LongStorage.Dictionary.TryGetValue(variableName, out num) ? -1L : num) : -1L);
        }

        public string ReadString(string variableName)
        {
            string str;
            return ((this.m_objectBuilder != null) ? (!this.m_objectBuilder.StringStorage.Dictionary.TryGetValue(variableName, out str) ? null : str) : null);
        }

        public Vector3D ReadVector3D(string variableName)
        {
            SerializableVector3D vectord;
            return ((this.m_objectBuilder != null) ? (!this.m_objectBuilder.Vector3DStorage.Dictionary.TryGetValue(variableName, out vectord) ? Vector3D.Zero : ((Vector3D) vectord)) : Vector3D.Zero);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            m_instance = null;
        }

        public bool Write(string variableName, bool value, bool @static = false)
        {
            if (this.m_objectBuilder == null)
            {
                return false;
            }
            if (this.m_objectBuilder.BoolStorage.Dictionary.ContainsKey(variableName))
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary[variableName])
                {
                    return false;
                }
                this.m_objectBuilder.BoolStorage.Dictionary[variableName] = value;
            }
            else
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.ContainsKey(variableName))
                {
                    return false;
                }
                this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.Add(variableName, @static);
                this.m_objectBuilder.BoolStorage.Dictionary.Add(variableName, value);
            }
            return true;
        }

        public bool Write(string variableName, int value, bool @static = false)
        {
            if (this.m_objectBuilder == null)
            {
                return false;
            }
            if (this.m_objectBuilder.IntStorage.Dictionary.ContainsKey(variableName))
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary[variableName])
                {
                    return false;
                }
                this.m_objectBuilder.IntStorage.Dictionary[variableName] = value;
            }
            else
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.ContainsKey(variableName))
                {
                    return false;
                }
                this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.Add(variableName, @static);
                this.m_objectBuilder.IntStorage.Dictionary.Add(variableName, value);
            }
            return true;
        }

        public bool Write(string variableName, long value, bool @static = false)
        {
            if (this.m_objectBuilder == null)
            {
                return false;
            }
            if (this.m_objectBuilder.LongStorage.Dictionary.ContainsKey(variableName))
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary[variableName])
                {
                    return false;
                }
                this.m_objectBuilder.LongStorage.Dictionary[variableName] = value;
            }
            else
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.ContainsKey(variableName))
                {
                    return false;
                }
                this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.Add(variableName, @static);
                this.m_objectBuilder.LongStorage.Dictionary.Add(variableName, value);
            }
            return true;
        }

        public bool Write(string variableName, float value, bool @static = false)
        {
            if (this.m_objectBuilder == null)
            {
                return false;
            }
            if (this.m_objectBuilder.FloatStorage.Dictionary.ContainsKey(variableName))
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary[variableName])
                {
                    return false;
                }
                this.m_objectBuilder.FloatStorage.Dictionary[variableName] = value;
            }
            else
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.ContainsKey(variableName))
                {
                    return false;
                }
                this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.Add(variableName, @static);
                this.m_objectBuilder.FloatStorage.Dictionary.Add(variableName, value);
            }
            return true;
        }

        public bool Write(string variableName, string value, bool @static = false)
        {
            if (this.m_objectBuilder == null)
            {
                return false;
            }
            if (this.m_objectBuilder.StringStorage.Dictionary.ContainsKey(variableName))
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary[variableName])
                {
                    return false;
                }
                this.m_objectBuilder.StringStorage.Dictionary[variableName] = value;
            }
            else
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.ContainsKey(variableName))
                {
                    return false;
                }
                this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.Add(variableName, @static);
                this.m_objectBuilder.StringStorage.Dictionary.Add(variableName, value);
            }
            return true;
        }

        public bool Write(string variableName, Vector3D value, bool @static = false)
        {
            if (this.m_objectBuilder == null)
            {
                return false;
            }
            if (this.m_objectBuilder.Vector3DStorage.Dictionary.ContainsKey(variableName))
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary[variableName])
                {
                    return false;
                }
                this.m_objectBuilder.Vector3DStorage.Dictionary[variableName] = value;
            }
            else
            {
                if (this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.ContainsKey(variableName))
                {
                    return false;
                }
                this.m_objectBuilder.ExistingFieldsAndStaticAttribute.Dictionary.Add(variableName, @static);
                this.m_objectBuilder.Vector3DStorage.Dictionary.Add(variableName, value);
            }
            return true;
        }

        public static MySessionComponentScriptSharedStorage Instance =>
            m_instance;
    }
}

