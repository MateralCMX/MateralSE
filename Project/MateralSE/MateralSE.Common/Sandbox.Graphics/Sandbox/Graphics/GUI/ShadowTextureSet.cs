namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ShadowTextureSet
    {
        public ShadowTextureSet()
        {
            this.Textures = new List<ShadowTexture>();
        }

        public void AddTextures(IEnumerable<ShadowTexture> textures)
        {
            this.Textures.AddRange(textures);
            this.Textures.Sort((t1, t2) => (t1.MinWidth != t2.MinWidth) ? ((Comparison<ShadowTexture>) ((t1.MinWidth >= t2.MinWidth) ? ((Comparison<ShadowTexture>) 1) : ((Comparison<ShadowTexture>) (-1)))) : ((Comparison<ShadowTexture>) 0));
        }

        public ShadowTexture GetOptimalTexture(float size)
        {
            int num = 0;
            int num2 = this.Textures.Count - 1;
            for (int i = num2 - num; i >= 0; i = num2 - num)
            {
                int num4 = (i / 2) + num;
                ShadowTexture texture = this.Textures[num4];
                float minWidth = texture.MinWidth;
                if ((size == minWidth) || ((i == 0) && (size > minWidth)))
                {
                    return texture;
                }
                if (minWidth > size)
                {
                    num2 = num4 - 1;
                }
                else
                {
                    num = num4 + 1;
                }
            }
            return ((num2 > 0) ? this.Textures[num2] : this.Textures[num]);
        }

        public List<ShadowTexture> Textures { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ShadowTextureSet.<>c <>9 = new ShadowTextureSet.<>c();
            public static Comparison<ShadowTexture> <>9__5_0;

            internal int <AddTextures>b__5_0(ShadowTexture t1, ShadowTexture t2) => 
                ((t1.MinWidth != t2.MinWidth) ? ((t1.MinWidth >= t2.MinWidth) ? 1 : -1) : 0);
        }
    }
}

