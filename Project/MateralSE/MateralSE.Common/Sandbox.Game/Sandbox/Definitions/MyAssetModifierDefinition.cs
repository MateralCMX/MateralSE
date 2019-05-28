namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageRender.Messages;

    [MyDefinitionType(typeof(MyObjectBuilder_AssetModifierDefinition), (Type) null)]
    public class MyAssetModifierDefinition : MyDefinitionBase
    {
        public List<MyObjectBuilder_AssetModifierDefinition.MyAssetTexture> Textures;

        public string GetFilepath(string location, MyTextureType type)
        {
            using (List<MyObjectBuilder_AssetModifierDefinition.MyAssetTexture>.Enumerator enumerator = this.Textures.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyObjectBuilder_AssetModifierDefinition.MyAssetTexture current = enumerator.Current;
                    if ((current.Location == location) && (current.Type == type))
                    {
                        return current.Filepath;
                    }
                }
            }
            return null;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AssetModifierDefinition definition = builder as MyObjectBuilder_AssetModifierDefinition;
            this.Textures = definition.Textures;
        }
    }
}

