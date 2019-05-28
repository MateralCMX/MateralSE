namespace VRage.Game.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Library.Utils;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_GuiTextureAtlasDefinition), (Type) null)]
    public class MyGuiTextureAtlasDefinition : MyDefinitionBase
    {
        public readonly Dictionary<MyStringHash, MyObjectBuilder_GuiTexture> Textures = new Dictionary<MyStringHash, MyObjectBuilder_GuiTexture>(MyStringHash.Comparer);
        public readonly Dictionary<MyStringHash, MyObjectBuilder_CompositeTexture> CompositeTextures = new Dictionary<MyStringHash, MyObjectBuilder_CompositeTexture>(MyStringHash.Comparer);

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiTextureAtlasDefinition definition = builder as MyObjectBuilder_GuiTextureAtlasDefinition;
            this.Textures.Clear();
            this.CompositeTextures.Clear();
            foreach (MyObjectBuilder_GuiTexture texture in definition.Textures)
            {
                if (base.Context.IsBaseGame)
                {
                    texture.Path = Path.Combine(MyFileSystem.ContentPath, texture.Path);
                }
                string str = texture.Path.ToLower();
                bool flag = true;
                if (str.EndsWith(".dds"))
                {
                    MyImageHeaderUtils.DDS_HEADER dds_header;
                    if (MyImageHeaderUtils.Read_DDS_HeaderData(texture.Path, out dds_header))
                    {
                        texture.SizePx.X = (int) dds_header.dwWidth;
                        texture.SizePx.Y = (int) dds_header.dwHeight;
                        this.Textures.Add(texture.SubtypeId, texture);
                        flag = false;
                    }
                }
                else if (!str.EndsWith(".png"))
                {
                    MyLog.Default.WriteLine("GuiTextures.sbc");
                    MyLog.Default.WriteLine("Unsupported texture format! Texture: " + texture.Path);
                }
                else if (MyImageHeaderUtils.Read_PNG_Dimensions(texture.Path, out texture.SizePx.X, out texture.SizePx.Y))
                {
                    this.Textures.Add(texture.SubtypeId, texture);
                    flag = false;
                }
                if (flag)
                {
                    MyLog.Default.WriteLine("GuiTextures.sbc");
                    MyLog.Default.WriteLine("Failed to parse texture header! Texture: " + texture.Path);
                }
            }
            foreach (MyObjectBuilder_CompositeTexture texture2 in definition.CompositeTextures)
            {
                this.CompositeTextures.Add(texture2.SubtypeId, texture2);
            }
        }
    }
}

