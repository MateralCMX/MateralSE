namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    public interface IMyConstProperty
    {
        void Deserialize(XmlReader reader);
        void DeserializeFromObjectBuilder(GenerationProperty property);
        void DeserializeValue(XmlReader reader, out object value);
        IMyConstProperty Duplicate();
        object GetValue();
        Type GetValueType();
        void Serialize(XmlWriter writer);
        void SerializeValue(XmlWriter writer, object value);
        void SetValue(object val);

        string Name { get; set; }

        string ValueType { get; }

        string BaseValueType { get; }

        bool Animated { get; }

        bool Is2D { get; }
    }
}

