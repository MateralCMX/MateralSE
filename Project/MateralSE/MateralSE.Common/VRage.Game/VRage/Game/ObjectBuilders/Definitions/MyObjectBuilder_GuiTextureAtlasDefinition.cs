namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiTextureAtlasDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("Texture")]
        public MyObjectBuilder_GuiTexture[] Textures;
        [XmlArrayItem("CompositeTexture")]
        public MyObjectBuilder_CompositeTexture[] CompositeTextures;
    }
}

