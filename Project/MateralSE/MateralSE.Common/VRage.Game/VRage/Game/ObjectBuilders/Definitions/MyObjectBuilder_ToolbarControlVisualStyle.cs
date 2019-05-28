namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ToolbarControlVisualStyle : MyObjectBuilder_Base
    {
        [XmlElement(typeof(MyAbstractXmlSerializer<ConditionBase>))]
        public ConditionBase VisibleCondition;
        public ColorStyle ColorPanelStyle;
        public Vector2 CenterPosition;
        public Vector2 SelectedItemPosition;
        public float? SelectedItemTextScale;
        public MyGuiDrawAlignEnum OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
        public ToolbarItemStyle ItemStyle;
        public ToolbarPageStyle PageStyle;
        [XmlArrayItem("StatControl")]
        public MyObjectBuilder_StatControls[] StatControls;

        public class ColorStyle
        {
            public Vector2 Offset;
            public Vector2 VoxelHandPosition;
            public Vector2 Size;
            public MyStringHash Texture;
        }

        public class ToolbarItemStyle
        {
            public MyStringHash Texture = MyStringHash.GetOrCompute(@"Textures\GUI\Controls\grid_item.dds");
            public MyStringHash TextureHighlight = MyStringHash.GetOrCompute(@"Textures\GUI\Controls\grid_item_highlight.dds");
            public MyStringHash VariantTexture = MyStringHash.GetOrCompute(@"Textures\GUI\Icons\VariantsAvailable.dds");
            public Vector2? VariantOffset;
            public string FontNormal = "Blue";
            public string FontHighlight = "White";
            public float TextScale = 0.75f;
            public Vector2 ItemTextureScale = Vector2.Zero;
            public MyGuiOffset? Margin;
        }

        public class ToolbarPageStyle
        {
            public MyStringHash PageCompositeTexture;
            public MyStringHash PageHighlightCompositeTexture;
            public Vector2 PagesOffset;
            public float? NumberSize;
        }
    }
}

