namespace VRage.Game.GUI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;
    using VRageRender;
    using VRageRender.Messages;

    public class MyGuiTextures
    {
        private readonly Dictionary<MyStringHash, MyObjectBuilder_GuiTexture> m_textures = new Dictionary<MyStringHash, MyObjectBuilder_GuiTexture>();
        private readonly Dictionary<MyStringHash, MyObjectBuilder_CompositeTexture> m_compositeTextures = new Dictionary<MyStringHash, MyObjectBuilder_CompositeTexture>();
        private static MyGuiTextures m_instance;

        public MyObjectBuilder_CompositeTexture GetCompositeTexture(MyStringHash hash)
        {
            MyObjectBuilder_CompositeTexture texture = null;
            this.m_compositeTextures.TryGetValue(hash, out texture);
            return texture;
        }

        public MyObjectBuilder_GuiTexture GetTexture(MyStringHash hash)
        {
            MyObjectBuilder_GuiTexture texture = null;
            this.m_textures.TryGetValue(hash, out texture);
            return texture;
        }

        public void Reload()
        {
            this.m_textures.Clear();
            this.m_compositeTextures.Clear();
            IEnumerable<MyGuiTextureAtlasDefinition> allDefinitions = MyDefinitionManagerBase.Static.GetAllDefinitions<MyGuiTextureAtlasDefinition>();
            if (allDefinitions != null)
            {
                foreach (MyGuiTextureAtlasDefinition definition in allDefinitions)
                {
                    List<string> texturesToLoad = new List<string>();
                    foreach (KeyValuePair<MyStringHash, MyObjectBuilder_GuiTexture> pair in definition.Textures)
                    {
                        this.m_textures[pair.Key] = pair.Value;
                        texturesToLoad.Add(pair.Value.Path);
                    }
                    texturesToLoad.AddRange(MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, @"textures\gui\icons"), "*", MySearchOption.TopDirectoryOnly));
                    texturesToLoad.AddRange(MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, @"textures\gui\icons\cubes"), "*", MySearchOption.TopDirectoryOnly));
                    texturesToLoad.AddRange(MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, @"textures\gui\icons\component"), "*", MySearchOption.TopDirectoryOnly));
                    texturesToLoad.AddRange(MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, @"textures\gui\icons\skins"), "*", MySearchOption.AllDirectories));
                    MyRenderProxy.PreloadTextures(texturesToLoad, TextureType.GUI);
                    texturesToLoad.Clear();
                    texturesToLoad.AddRange(MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, "customworlds"), "*.jpg", MySearchOption.AllDirectories));
                    texturesToLoad.AddRange(MyFileSystem.GetFiles(Path.Combine(MyFileSystem.ContentPath, "scenarios"), "*.png", MySearchOption.AllDirectories));
                    MyRenderProxy.PreloadTextures(texturesToLoad, TextureType.GUIWithoutPremultiplyAlpha);
                    foreach (KeyValuePair<MyStringHash, MyObjectBuilder_CompositeTexture> pair2 in definition.CompositeTextures)
                    {
                        this.m_compositeTextures[pair2.Key] = pair2.Value;
                    }
                }
            }
        }

        public bool TryGetCompositeTexture(MyStringHash hash, out MyObjectBuilder_CompositeTexture texture) => 
            this.m_compositeTextures.TryGetValue(hash, out texture);

        public bool TryGetTexture(MyStringHash hash, out MyObjectBuilder_GuiTexture texture) => 
            this.m_textures.TryGetValue(hash, out texture);

        public static MyGuiTextures Static =>
            (m_instance ?? (m_instance = new MyGuiTextures()));
    }
}

