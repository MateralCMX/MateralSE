namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_MainMenuInventorySceneDefinition), (Type) null)]
    public class MyMainMenuInventorySceneDefinition : MyDefinitionBase
    {
        public string SceneDirectory;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MainMenuInventorySceneDefinition definition = builder as MyObjectBuilder_MainMenuInventorySceneDefinition;
            this.SceneDirectory = definition.SceneDirectory;
        }
    }
}

