namespace VRage.Game.GUI.TextPanel
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using VRage.Serialization;
    using VRageMath;

    [Serializable, StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MySprite
    {
        [ProtoMember(0x11), DefaultValue(0)]
        public SpriteType Type;
        [ProtoMember(0x17), Nullable, DefaultValue((string) null)]
        public Vector2? Position;
        [ProtoMember(0x1d), Nullable, DefaultValue((string) null)]
        public Vector2? Size;
        [ProtoMember(0x23), Nullable, DefaultValue((string) null)]
        public VRageMath.Color? Color;
        [ProtoMember(0x29), Nullable, DefaultValue((string) null)]
        public string Data;
        [ProtoMember(0x2f), Nullable, DefaultValue((string) null)]
        public string FontId;
        [ProtoMember(0x35), DefaultValue(2)]
        public TextAlignment Alignment;
        [ProtoMember(0x3b), DefaultValue((float) 0f)]
        public float RotationOrScale;
        public MySprite(SpriteType type = 0, string data = null, Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), VRageMath.Color? color = new VRageMath.Color?(), string fontId = null, TextAlignment alignment = 2, float rotation = 0f)
        {
            this.Type = type;
            this.Data = data;
            this.Position = position;
            this.Size = size;
            this.Color = color;
            this.FontId = fontId;
            this.Alignment = alignment;
            this.RotationOrScale = rotation;
        }

        public static MySprite CreateSprite(string sprite, Vector2 position, Vector2 size) => 
            new MySprite(SpriteType.TEXTURE, sprite, new Vector2?(position), new Vector2?(size), null, null, TextAlignment.CENTER, 0f);

        public static MySprite CreateText(string text, string fontId, VRageMath.Color color, float scale = 1f, TextAlignment alignment = 2)
        {
            Vector2? position = null;
            position = null;
            return new MySprite(SpriteType.TEXT, text, position, position, new VRageMath.Color?(color), fontId, alignment, scale);
        }
    }
}

