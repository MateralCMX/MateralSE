namespace Sandbox.Graphics.GUI
{
    using System;
    using VRageMath;

    public class MyGuiStyleDefinition
    {
        public MyGuiCompositeTexture BackgroundTexture;
        public Vector2 BackgroundPaddingSize;
        public string ItemFontHighlight;
        public string ItemFontNormal;
        public MyGuiBorderThickness ItemMargin;
        public MyGuiBorderThickness ItemPadding;
        public MyGuiBorderThickness ContentPadding;
        public MyGuiHighlightTexture ItemTexture;
        public float ItemTextScale = 0.8f;
        public Vector2? SizeOverride;
        public bool FitSizeToItems;
        public bool BorderEnabled;
        public Vector4 BorderColor = Vector4.One;
    }
}

