namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game.GUI.TextPanel;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRageMath;

    [ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MySerializedTextPanelData
    {
        [ProtoMember(0x10)]
        public float ChangeInterval;
        [ProtoMember(0x13), Serialize(MyObjectFlags.DefaultZero)]
        public List<string> SelectedImages;
        [ProtoMember(0x17)]
        public SerializableDefinitionId Font = ((SerializableDefinitionId) new MyDefinitionId(typeof(MyObjectBuilder_FontDefinition), "Debug"));
        [ProtoMember(0x1a)]
        public float FontSize = 1f;
        [ProtoMember(0x1d), DefaultValue("")]
        public string Text = "";
        [ProtoMember(0x20)]
        public ShowTextOnScreenFlag ShowText;
        [ProtoMember(0x23)]
        public Color FontColor = Color.White;
        [ProtoMember(0x26)]
        public Color BackgroundColor = Color.Black;
        [ProtoMember(0x29)]
        public int CurrentShownTexture;
        [ProtoMember(0x2c, IsRequired=false), DefaultValue(0)]
        public int Alignment;
        [ProtoMember(0x2f), DefaultValue(0)]
        public VRage.Game.GUI.TextPanel.ContentType ContentType;
        [ProtoMember(50), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public string SelectedScript;
        [ProtoMember(0x36)]
        public float TextPadding = 2f;
        [ProtoMember(0x39), DefaultValue(false)]
        public bool PreserveAspectRatio;
        [ProtoMember(60), DefaultValue(false)]
        public bool CustomizeScripts;
        [ProtoMember(0x3f)]
        public Color ScriptBackgroundColor = new Color(0, 0x58, 0x97);
        [ProtoMember(0x42)]
        public Color ScriptForegroundColor = new Color(0xb3, 0xed, 0xff);
    }
}

