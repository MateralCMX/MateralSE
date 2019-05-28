namespace VRageRender.Models
{
    using System;
    using System.Collections.Generic;
    using VRageRender.Import;

    public class MyMeshMaterial
    {
        public MyMeshDrawTechnique DrawTechnique;
        public string Name;
        public string GlassCW;
        public string GlassCCW;
        public bool GlassSmooth;
        public Dictionary<string, string> Textures;

        public override int GetHashCode()
        {
            int num = 1;
            int num2 = 0;
            int num3 = 3;
            foreach (KeyValuePair<string, string> pair in this.Textures)
            {
                num = (((num * 0x18d) ^ pair.Key.GetHashCode()) * 0x18d) ^ pair.Value.GetHashCode();
                num2 = (num2 + (1 << (++num3 & 0x1f))) + (1 << (++num3 & 0x1f));
            }
            return ((num * 0x18d) ^ num2);
        }
    }
}

