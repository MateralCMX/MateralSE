namespace Sandbox.Definitions.GUI
{
    using ObjectBuilders.Definitions.GUI;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.GUI;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_GuiSkinDefinition), (Type) null)]
    public class MyGuiSkinDefinition : MyDefinitionBase
    {
        public Dictionary<MyStringId, MyGuiCompositeTexture> Textures;
        public Dictionary<MyStringId, IconStyleDefinition> IconStyles;
        public Dictionary<MyStringId, MyGuiControlButton.StyleDefinition> ButtonStyles;
        public Dictionary<MyStringId, MyGuiControlImageButton.StyleDefinition> ImageButtonStyles;
        public Dictionary<MyStringId, MyGuiControlCombobox.StyleDefinition> ComboboxStyles;
        public Dictionary<MyStringId, MyGuiControlLabel.StyleDefinition> LabelStyles;
        public Dictionary<MyStringId, MyGuiControlCheckbox.StyleDefinition> CheckboxStyles;
        public Dictionary<MyStringId, MyGuiControlSliderBase.StyleDefinition> SliderStyles;
        public Dictionary<MyStringId, MyGuiControlListbox.StyleDefinition> ListboxStyles;
        public Dictionary<MyStringId, MyGuiControlTextbox.StyleDefinition> TextboxStyles;
        public Dictionary<MyStringId, MyGuiControlImage.StyleDefinition> ImageStyles;
        public Dictionary<MyStringId, MyContextMenuStyleDefinition> ContextMenuStyles;
        public Dictionary<MyStringId, MyButtonListStyleDefinition> ButtonListStyles;

        protected override unsafe void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            this.Textures = new Dictionary<MyStringId, MyGuiCompositeTexture>(MyStringId.Comparer);
            this.IconStyles = new Dictionary<MyStringId, IconStyleDefinition>(MyStringId.Comparer);
            this.ButtonStyles = new Dictionary<MyStringId, MyGuiControlButton.StyleDefinition>(MyStringId.Comparer);
            this.ComboboxStyles = new Dictionary<MyStringId, MyGuiControlCombobox.StyleDefinition>(MyStringId.Comparer);
            this.LabelStyles = new Dictionary<MyStringId, MyGuiControlLabel.StyleDefinition>(MyStringId.Comparer);
            this.CheckboxStyles = new Dictionary<MyStringId, MyGuiControlCheckbox.StyleDefinition>(MyStringId.Comparer);
            this.SliderStyles = new Dictionary<MyStringId, MyGuiControlSliderBase.StyleDefinition>(MyStringId.Comparer);
            this.ImageButtonStyles = new Dictionary<MyStringId, MyGuiControlImageButton.StyleDefinition>(MyStringId.Comparer);
            this.ListboxStyles = new Dictionary<MyStringId, MyGuiControlListbox.StyleDefinition>(MyStringId.Comparer);
            this.TextboxStyles = new Dictionary<MyStringId, MyGuiControlTextbox.StyleDefinition>(MyStringId.Comparer);
            this.ImageStyles = new Dictionary<MyStringId, MyGuiControlImage.StyleDefinition>(MyStringId.Comparer);
            this.ContextMenuStyles = new Dictionary<MyStringId, MyContextMenuStyleDefinition>(MyStringId.Comparer);
            this.ButtonListStyles = new Dictionary<MyStringId, MyButtonListStyleDefinition>(MyStringId.Comparer);
            MyObjectBuilder_GuiSkinDefinition definition = builder as MyObjectBuilder_GuiSkinDefinition;
            if (definition != null)
            {
                MyGuiHighlightTexture texture;
                if (definition.GuiTextures != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.TextureStyleDefinition definition2 in definition.GuiTextures)
                    {
                        this.Textures[MyStringId.GetOrCompute(definition2.StyleName)] = definition2.Texture;
                    }
                }
                if (definition.GuiIcons != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.IconStyleDefinition definition3 in definition.GuiIcons)
                    {
                        IconStyleDefinition definition1 = new IconStyleDefinition();
                        definition1.Normal = definition3.Normal;
                        definition1.Highlight = definition3.Highlight;
                        definition1.Active = definition3.Active;
                        definition1.ActiveHighlight = definition3.ActiveHighlight;
                        definition1.Disabled = definition3.Disabled;
                        IconStyleDefinition definition4 = definition1;
                        this.IconStyles[MyStringId.GetOrCompute(definition3.StyleName)] = definition4;
                    }
                }
                if (definition.Buttons != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.ButtonStyleDefinition definition5 in definition.Buttons)
                    {
                        MyGuiControlButton.StyleDefinition definition26 = new MyGuiControlButton.StyleDefinition();
                        definition26.BackgroundColor = (Vector4) definition5.BackgroundColor;
                        definition26.NormalTexture = definition5.Normal.Texture;
                        definition26.HighlightTexture = definition5.Highlight.Texture;
                        definition26.NormalFont = definition5.Normal.Font;
                        definition26.HighlightFont = definition5.Highlight.Font;
                        definition26.Padding = new MyGuiBorderThickness(definition5.Padding.Left / MyGuiConstants.GUI_OPTIMAL_SIZE.X, definition5.Padding.Right / MyGuiConstants.GUI_OPTIMAL_SIZE.X, definition5.Padding.Top / MyGuiConstants.GUI_OPTIMAL_SIZE.Y, definition5.Padding.Bottom / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
                        MyGuiControlButton.StyleDefinition definition6 = definition26;
                        MyGuiControlImageButton.StyleDefinition definition27 = new MyGuiControlImageButton.StyleDefinition();
                        definition27.BackgroundColor = (Vector4) definition5.BackgroundColor;
                        definition27.Padding = new MyGuiBorderThickness(definition5.Padding.Left / MyGuiConstants.GUI_OPTIMAL_SIZE.X, definition5.Padding.Right / MyGuiConstants.GUI_OPTIMAL_SIZE.X, definition5.Padding.Top / MyGuiConstants.GUI_OPTIMAL_SIZE.Y, definition5.Padding.Bottom / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
                        MyGuiControlImageButton.StyleDefinition definition7 = definition27;
                        MyGuiControlImageButton.StateDefinition definition28 = new MyGuiControlImageButton.StateDefinition();
                        definition28.Font = definition5.Normal.Font;
                        definition28.Texture = definition5.Normal.Texture;
                        definition28.CornerTextFont = definition5.Normal.CornerTextFont;
                        definition28.CornerTextSize = definition5.Normal.CornerTextSize;
                        definition7.Normal = definition28;
                        if (definition5.Disabled == null)
                        {
                            definition7.Disabled = definition7.Normal;
                        }
                        else
                        {
                            MyGuiControlImageButton.StateDefinition definition29 = new MyGuiControlImageButton.StateDefinition();
                            definition29.Font = definition5.Disabled.Font;
                            definition29.Texture = definition5.Disabled.Texture;
                            definition29.CornerTextFont = definition5.Disabled.CornerTextFont;
                            definition29.CornerTextSize = definition5.Disabled.CornerTextSize;
                            definition7.Disabled = definition29;
                        }
                        if (definition5.Active == null)
                        {
                            definition7.Active = definition7.Normal;
                        }
                        else
                        {
                            MyGuiControlImageButton.StateDefinition definition30 = new MyGuiControlImageButton.StateDefinition();
                            definition30.Font = definition5.Active.Font;
                            definition30.Texture = definition5.Active.Texture;
                            definition30.CornerTextFont = definition5.Active.CornerTextFont;
                            definition30.CornerTextSize = definition5.Active.CornerTextSize;
                            definition7.Active = definition30;
                        }
                        if (definition5.Highlight == null)
                        {
                            definition7.Highlight = definition7.Normal;
                        }
                        else
                        {
                            MyGuiControlImageButton.StateDefinition definition31 = new MyGuiControlImageButton.StateDefinition();
                            definition31.Font = definition5.Highlight.Font;
                            definition31.Texture = definition5.Highlight.Texture;
                            definition31.CornerTextFont = definition5.Highlight.CornerTextFont;
                            definition31.CornerTextSize = definition5.Highlight.CornerTextSize;
                            definition7.Highlight = definition31;
                        }
                        if (definition5.ActiveHighlight == null)
                        {
                            definition7.ActiveHighlight = definition7.Highlight;
                        }
                        else
                        {
                            MyGuiControlImageButton.StateDefinition definition32 = new MyGuiControlImageButton.StateDefinition();
                            definition32.Font = definition5.ActiveHighlight.Font;
                            definition32.Texture = definition5.ActiveHighlight.Texture;
                            definition32.CornerTextFont = definition5.ActiveHighlight.CornerTextFont;
                            definition32.CornerTextSize = definition5.ActiveHighlight.CornerTextSize;
                            definition7.ActiveHighlight = definition32;
                        }
                        this.ButtonStyles[MyStringId.GetOrCompute(definition5.StyleName)] = definition6;
                        this.ImageButtonStyles[MyStringId.GetOrCompute(definition5.StyleName)] = definition7;
                    }
                }
                if (definition.Labels != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.LabelStyleDefinition definition8 in definition.Labels)
                    {
                        MyGuiControlLabel.StyleDefinition definition33 = new MyGuiControlLabel.StyleDefinition();
                        definition33.Font = definition8.Font;
                        definition33.ColorMask = (Vector4) definition8.Color;
                        definition33.TextScale = definition8.TextScale;
                        MyGuiControlLabel.StyleDefinition definition9 = definition33;
                        this.LabelStyles[MyStringId.GetOrCompute(definition8.StyleName)] = definition9;
                    }
                }
                if (definition.Checkboxes != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.CheckboxStyleDefinition definition10 in definition.Checkboxes)
                    {
                        MyGuiControlCheckbox.StyleDefinition definition34 = new MyGuiControlCheckbox.StyleDefinition();
                        definition34.NormalCheckedTexture = definition10.NormalChecked.Texture;
                        definition34.NormalUncheckedTexture = definition10.NormalUnchecked.Texture;
                        definition34.HighlightCheckedTexture = definition10.HighlightChecked.Texture;
                        definition34.HighlightUncheckedTexture = definition10.HighlightUnchecked.Texture;
                        texture = new MyGuiHighlightTexture {
                            Highlight = definition10.HighlightChecked.Icon,
                            Normal = definition10.NormalChecked.Icon,
                            SizePx = (Vector2) definition10.IconSize
                        };
                        definition34.CheckedIcon = texture;
                        texture = new MyGuiHighlightTexture {
                            Highlight = definition10.HighlightUnchecked.Icon,
                            Normal = definition10.NormalUnchecked.Icon,
                            SizePx = (Vector2) definition10.IconSize
                        };
                        definition34.UncheckedIcon = texture;
                        MyGuiControlCheckbox.StyleDefinition definition11 = definition34;
                        this.CheckboxStyles[MyStringId.GetOrCompute(definition10.StyleName)] = definition11;
                    }
                }
                if (definition.Sliders != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.SliderStyleDefinition definition12 in definition.Sliders)
                    {
                        MyGuiHighlightTexture* texturePtr1;
                        MyGuiControlSliderBase.StyleDefinition definition35 = new MyGuiControlSliderBase.StyleDefinition();
                        definition35.RailTexture = definition12.Normal.TrackTexture;
                        definition35.RailHighlightTexture = (definition12.Highlight != null) ? definition12.Highlight.TrackTexture : definition12.Normal.TrackTexture;
                        texture = new MyGuiHighlightTexture();
                        MyGuiControlSliderBase.StyleDefinition local3 = definition35;
                        MyGuiControlSliderBase.StyleDefinition local4 = definition35;
                        texturePtr1->Highlight = (definition12.Highlight != null) ? definition12.Highlight.Thumb : definition12.Normal.Thumb;
                        texturePtr1 = (MyGuiHighlightTexture*) ref texture;
                        texture.Normal = definition12.Normal.Thumb;
                        texture.SizePx = (Vector2) definition12.ThumbSize;
                        local4.ThumbTexture = texture;
                        MyGuiControlSliderBase.StyleDefinition definition13 = local4;
                        this.SliderStyles[MyStringId.GetOrCompute(definition12.StyleName)] = definition13;
                    }
                }
                if (definition.Comboboxes != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.ComboboxStyleDefinition definition14 in definition.Comboboxes)
                    {
                        MyGuiControlCombobox.StyleDefinition definition36 = new MyGuiControlCombobox.StyleDefinition();
                        definition36.TextScale = definition14.TextScale;
                        definition36.ComboboxTextureNormal = definition14.Normal.Texture;
                        definition36.ComboboxTextureHighlight = (definition14.Highlight != null) ? definition14.Highlight.Texture : definition14.Normal.Texture;
                        MyGuiControlCombobox.StyleDefinition local1 = definition36;
                        local1.ItemFontNormal = definition14.Normal.ItemFont;
                        local1.ItemFontHighlight = (definition14.Highlight != null) ? definition14.Highlight.ItemFont : definition14.Normal.ItemFont;
                        MyGuiControlCombobox.StyleDefinition local2 = local1;
                        local2.ItemTextureHighlight = definition14.ItemTextureHighlight;
                        local2.DropDownHighlightExtraWidth = 0.007f;
                        local2.SelectedItemOffset = new Vector2(0.01f, 0.005f);
                        MyGuiBorderThickness thickness = new MyGuiBorderThickness {
                            Left = definition14.ScrollbarMargin.Left / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                            Right = definition14.ScrollbarMargin.Right / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                            Bottom = definition14.ScrollbarMargin.Bottom / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                            Top = definition14.ScrollbarMargin.Top / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
                        };
                        local2.ScrollbarMargin = thickness;
                        local2.DropDownTexture = definition14.DropDownTexture;
                        MyGuiControlCombobox.StyleDefinition definition15 = local2;
                        this.ComboboxStyles[MyStringId.GetOrCompute(definition14.StyleName)] = definition15;
                    }
                }
                if (definition.Listboxes != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.ListboxStyleDefinition definition16 in definition.Listboxes)
                    {
                        MyGuiControlListbox.StyleDefinition definition17 = new MyGuiControlListbox.StyleDefinition {
                            TextScale = definition16.TextScale,
                            ItemFontHighlight = definition16.ItemFontHighlight,
                            ItemFontNormal = definition16.ItemFontNormal,
                            ItemSize = (Vector2) definition16.ItemSize,
                            ItemsOffset = (Vector2) definition16.ItemOffset,
                            ItemTextureHighlight = definition16.ItemTextureHighlight,
                            Texture = definition16.Texture,
                            XSizeVariable = definition16.XSizeVariable,
                            DrawScroll = definition16.DrawScrollbar
                        };
                        this.ListboxStyles[MyStringId.GetOrCompute(definition16.StyleName)] = definition17;
                    }
                }
                if (definition.Textboxes != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.TextboxStyleDefinition definition18 in definition.Textboxes)
                    {
                        MyGuiControlTextbox.StyleDefinition definition19 = new MyGuiControlTextbox.StyleDefinition {
                            NormalFont = definition18.Normal.Font,
                            NormalTexture = definition18.Normal.Texture
                        };
                        if (definition18.Highlight != null)
                        {
                            definition19.HighlightFont = definition18.Highlight.Font;
                            definition19.HighlightTexture = definition18.Highlight.Texture;
                        }
                        else
                        {
                            definition19.HighlightFont = definition18.Normal.Font;
                            definition19.HighlightTexture = definition18.Normal.Texture;
                        }
                        this.TextboxStyles[MyStringId.GetOrCompute(definition18.StyleName)] = definition19;
                    }
                }
                if (definition.Images != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.ImageStyleDefinition definition20 in definition.Images)
                    {
                        MyGuiControlImage.StyleDefinition definition37 = new MyGuiControlImage.StyleDefinition();
                        definition37.BackgroundTexture = definition20.Texture;
                        definition37.Padding = new MyGuiBorderThickness(definition20.Padding.Left / MyGuiConstants.GUI_OPTIMAL_SIZE.X, definition20.Padding.Right / MyGuiConstants.GUI_OPTIMAL_SIZE.X, definition20.Padding.Top / MyGuiConstants.GUI_OPTIMAL_SIZE.Y, definition20.Padding.Bottom / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
                        MyGuiControlImage.StyleDefinition definition21 = definition37;
                        this.ImageStyles[MyStringId.GetOrCompute(definition20.StyleName)] = definition21;
                    }
                }
                if (definition.ContextMenus != null)
                {
                    foreach (MyObjectBuilder_GuiSkinDefinition.ContextMenuStyleDefinition definition22 in definition.ContextMenus)
                    {
                        MyContextMenuStyleDefinition definition38 = new MyContextMenuStyleDefinition();
                        definition38.TitleTexture = definition22.TitleTexture;
                        definition38.ImageStyle = MyStringId.GetOrCompute(definition22.ImageStyle);
                        definition38.SeparatorStyle = MyStringId.GetOrCompute(definition22.SeparatorStyle);
                        definition38.SeparatorHeight = definition22.SeparatorHeight;
                        definition38.Margin = (Vector2) definition22.Margin;
                        MyContextMenuStyleDefinition definition23 = definition38;
                        this.ContextMenuStyles[MyStringId.GetOrCompute(definition22.StyleName)] = definition23;
                    }
                }
                if (definition.ButtonListStyles != null)
                {
                    foreach (MyObjectBuilder_ButtonListStyleDefinition definition24 in definition.ButtonListStyles)
                    {
                        MyButtonListStyleDefinition definition25 = new MyButtonListStyleDefinition {
                            ButtonMargin = (Vector2) definition24.ButtonMargin,
                            ButtonSize = (Vector2) definition24.ButtonSize
                        };
                        this.ButtonListStyles[MyStringId.GetOrCompute(definition24.StyleName)] = definition25;
                    }
                }
            }
        }

        public class IconStyleDefinition
        {
            public string Normal;
            public string Highlight;
            public string Active;
            public string ActiveHighlight;
            public string Disabled;
        }

        public class MyContextMenuStyleDefinition
        {
            public MyGuiCompositeTexture TitleTexture;
            public MyStringId ImageStyle;
            public MyStringId SeparatorStyle;
            public float SeparatorHeight;
            public Vector2 Margin;
        }
    }
}

