namespace ObjectBuilders.Definitions.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Data;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions.GUI;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiSkinDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlElement("GuiTexture")]
        public TextureStyleDefinition[] GuiTextures;
        [XmlElement("GuiIcon")]
        public IconStyleDefinition[] GuiIcons;
        [XmlElement("Button")]
        public ButtonStyleDefinition[] Buttons;
        [XmlElement("Label")]
        public LabelStyleDefinition[] Labels;
        [XmlElement("Checkbox")]
        public CheckboxStyleDefinition[] Checkboxes;
        [XmlElement("Combobox")]
        public ComboboxStyleDefinition[] Comboboxes;
        [XmlElement("Slider")]
        public SliderStyleDefinition[] Sliders;
        [XmlElement("Listbox")]
        public ListboxStyleDefinition[] Listboxes;
        [XmlElement("Textbox")]
        public TextboxStyleDefinition[] Textboxes;
        [XmlElement("Image")]
        public ImageStyleDefinition[] Images;
        [XmlElement("ContextMenu")]
        public ContextMenuStyleDefinition[] ContextMenus;
        [XmlElement("ButtonList")]
        public MyObjectBuilder_ButtonListStyleDefinition[] ButtonListStyles;

        public class ButtonStateDefinition
        {
            public SerializableCompositeTexture Texture;
            public string Font;
            public string CornerTextFont = "White";
            public float CornerTextSize = 0.8f;
        }

        public class ButtonStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public MyObjectBuilder_GuiSkinDefinition.ButtonStateDefinition Normal;
            public MyObjectBuilder_GuiSkinDefinition.ButtonStateDefinition Active;
            public MyObjectBuilder_GuiSkinDefinition.ButtonStateDefinition Highlight;
            public MyObjectBuilder_GuiSkinDefinition.ButtonStateDefinition ActiveHighlight;
            public MyObjectBuilder_GuiSkinDefinition.ButtonStateDefinition Disabled;
            public MyObjectBuilder_GuiSkinDefinition.PaddingDefinition Padding;
            public MyObjectBuilder_GuiSkinDefinition.ColorDefinition BackgroundColor;
        }

        public class CheckboxStateDefinition
        {
            public SerializableCompositeTexture Texture;
            [ModdableContentFile("dds")]
            public string Icon;
        }

        public class CheckboxStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public MyObjectBuilder_GuiSkinDefinition.CheckboxStateDefinition NormalChecked;
            public MyObjectBuilder_GuiSkinDefinition.CheckboxStateDefinition NormalUnchecked;
            public MyObjectBuilder_GuiSkinDefinition.CheckboxStateDefinition HighlightChecked;
            public MyObjectBuilder_GuiSkinDefinition.CheckboxStateDefinition HighlightUnchecked;
            public SerializableVector2 IconSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ColorDefinition
        {
            [XmlAttribute]
            public byte R;
            [XmlAttribute]
            public byte G;
            [XmlAttribute]
            public byte B;
            [XmlAttribute]
            public byte A;
            public static implicit operator Color(MyObjectBuilder_GuiSkinDefinition.ColorDefinition definition) => 
                new Color(definition.R, definition.G, definition.B, definition.A);

            public static implicit operator MyObjectBuilder_GuiSkinDefinition.ColorDefinition(Color color) => 
                new MyObjectBuilder_GuiSkinDefinition.ColorDefinition { 
                    A = color.A,
                    B = color.B,
                    G = color.G,
                    R = color.R
                };

            public static implicit operator Vector4(MyObjectBuilder_GuiSkinDefinition.ColorDefinition definition) => 
                new Vector4(((float) definition.R) / 255f, ((float) definition.G) / 255f, ((float) definition.B) / 255f, ((float) definition.A) / 255f);

            public static implicit operator MyObjectBuilder_GuiSkinDefinition.ColorDefinition(Vector4 vector) => 
                new MyObjectBuilder_GuiSkinDefinition.ColorDefinition { 
                    A = (byte) (vector.W * 255f),
                    B = (byte) (vector.Z * 255f),
                    G = (byte) (vector.Y * 255f),
                    R = (byte) (vector.X * 255f)
                };
        }

        public class ComboboxStateDefinition
        {
            public string ItemFont;
            public SerializableCompositeTexture Texture;
        }

        public class ComboboxStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public MyObjectBuilder_GuiSkinDefinition.ComboboxStateDefinition Normal;
            public MyObjectBuilder_GuiSkinDefinition.ComboboxStateDefinition Highlight;
            public float TextScale;
            [ModdableContentFile("dds")]
            public string ItemTextureHighlight;
            public SerializableCompositeTexture DropDownTexture;
            public MyObjectBuilder_GuiSkinDefinition.PaddingDefinition ScrollbarMargin;
        }

        public class ContextMenuStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public string ImageStyle;
            public string SeparatorStyle;
            public SerializableCompositeTexture TitleTexture;
            public float SeparatorHeight;
            public SerializableVector2 Margin;
        }

        public class IconStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            [ModdableContentFile("dds")]
            public string Normal;
            [ModdableContentFile("dds")]
            public string Highlight;
            [ModdableContentFile("dds")]
            public string Active;
            [ModdableContentFile("dds")]
            public string ActiveHighlight;
            [ModdableContentFile("dds")]
            public string Disabled;
        }

        public class ImageStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public SerializableCompositeTexture Texture;
            public MyObjectBuilder_GuiSkinDefinition.PaddingDefinition Padding;
        }

        public class LabelStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public string Font;
            public MyObjectBuilder_GuiSkinDefinition.ColorDefinition Color;
            public float TextScale;
        }

        public class ListboxStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public float TextScale;
            public string ItemFontHighlight;
            public string ItemFontNormal;
            public SerializableVector2 ItemSize;
            public SerializableVector2 ItemOffset;
            [ModdableContentFile("dds")]
            public string ItemTextureHighlight;
            public SerializableCompositeTexture Texture;
            public bool XSizeVariable;
            public bool DrawScrollbar;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PaddingDefinition
        {
            [XmlAttribute("Left")]
            public float Left;
            [XmlAttribute("Right")]
            public float Right;
            [XmlAttribute("Top")]
            public float Top;
            [XmlAttribute("Bottom")]
            public float Bottom;
        }

        public class SliderStateDefinition
        {
            public SerializableCompositeTexture TrackTexture;
            [ModdableContentFile("dds")]
            public string Thumb;
        }

        public class SliderStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public MyObjectBuilder_GuiSkinDefinition.SliderStateDefinition Normal;
            public MyObjectBuilder_GuiSkinDefinition.SliderStateDefinition Highlight;
            public SerializableVector2 ThumbSize;
        }

        public class StyleDefinitionBase
        {
            [XmlAttribute]
            public string StyleName;
        }

        public class TextboxStateDefinition
        {
            public SerializableCompositeTexture Texture;
            public string Font;
        }

        public class TextboxStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public MyObjectBuilder_GuiSkinDefinition.TextboxStateDefinition Normal;
            public MyObjectBuilder_GuiSkinDefinition.TextboxStateDefinition Highlight;
        }

        public class TextureStyleDefinition : MyObjectBuilder_GuiSkinDefinition.StyleDefinitionBase
        {
            public SerializableCompositeTexture Texture;
        }
    }
}

