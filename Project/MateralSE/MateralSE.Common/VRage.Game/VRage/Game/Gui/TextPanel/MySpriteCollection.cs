namespace VRage.Game.GUI.TextPanel
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MySpriteCollection
    {
        [ProtoMember(10), Nullable]
        public MySprite[] Sprites;
        public MySpriteCollection(MySprite[] sprites)
        {
            this.Sprites = sprites;
        }
    }
}

