namespace VRage
{
    using System;
    using System.Xml.Serialization;

    public interface IMyXmlSerializable : IXmlSerializable
    {
        object Data { get; }
    }
}

