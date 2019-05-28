namespace ProtoBuf
{
    using System;

    public interface IExtensible
    {
        IExtension GetExtensionObject(bool createIfMissing);
    }
}

