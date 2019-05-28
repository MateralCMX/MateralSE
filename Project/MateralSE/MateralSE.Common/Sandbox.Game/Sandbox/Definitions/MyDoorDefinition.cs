namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_DoorDefinition), (Type) null)]
    public class MyDoorDefinition : MyCubeBlockDefinition
    {
        public string ResourceSinkGroup;
        public float MaxOpen;
        public string OpenSound;
        public string CloseSound;
        public float OpeningSpeed;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_DoorDefinition definition = builder as MyObjectBuilder_DoorDefinition;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.MaxOpen = definition.MaxOpen;
            this.OpenSound = definition.OpenSound;
            this.CloseSound = definition.CloseSound;
            this.OpeningSpeed = definition.OpeningSpeed;
        }
    }
}

