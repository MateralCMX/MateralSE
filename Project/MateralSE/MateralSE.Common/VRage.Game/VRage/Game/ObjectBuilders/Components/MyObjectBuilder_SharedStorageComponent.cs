﻿namespace VRage.Game.ObjectBuilders.Components
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SharedStorageComponent : MyObjectBuilder_SessionComponent
    {
        public SerializableDictionary<string, bool> ExistingFieldsAndStaticAttribute = new SerializableDictionary<string, bool>();
        public SerializableDictionary<string, bool> BoolStorage = new SerializableDictionary<string, bool>();
        public SerializableDictionary<string, int> IntStorage = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, long> LongStorage = new SerializableDictionary<string, long>();
        public SerializableDictionary<string, string> StringStorage = new SerializableDictionary<string, string>();
        public SerializableDictionary<string, float> FloatStorage = new SerializableDictionary<string, float>();
        public SerializableDictionary<string, SerializableVector3D> Vector3DStorage = new SerializableDictionary<string, SerializableVector3D>();
    }
}
