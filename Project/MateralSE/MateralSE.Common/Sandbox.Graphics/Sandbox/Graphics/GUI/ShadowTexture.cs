namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("MinSize = { MinSize }")]
    public class ShadowTexture
    {
        public ShadowTexture(string texture, float minwidth, float growFactorWidth, float growFactorHeight, float defaultAlpha)
        {
            this.Texture = texture;
            this.MinWidth = minwidth;
            this.GrowFactorWidth = growFactorWidth;
            this.GrowFactorHeight = growFactorHeight;
            this.DefaultAlpha = defaultAlpha;
        }

        public string Texture { get; private set; }

        public float MinWidth { get; private set; }

        public float GrowFactorWidth { get; private set; }

        public float GrowFactorHeight { get; private set; }

        public float DefaultAlpha { get; private set; }
    }
}

