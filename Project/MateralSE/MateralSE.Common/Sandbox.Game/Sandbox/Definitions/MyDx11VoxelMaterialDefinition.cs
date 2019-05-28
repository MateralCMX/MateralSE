namespace Sandbox.Definitions
{
    using Medieval.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_Dx11VoxelMaterialDefinition), (Type) null)]
    public class MyDx11VoxelMaterialDefinition : MyVoxelMaterialDefinition
    {
        public string VoxelHandPreview;

        protected override void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
            MyObjectBuilder_Dx11VoxelMaterialDefinition definition = (MyObjectBuilder_Dx11VoxelMaterialDefinition) ob;
            this.VoxelHandPreview = definition.VoxelHandPreview;
        }
    }
}

