namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipControllerDefinition), (Type) null)]
    public class MyShipControllerDefinition : MyCubeBlockDefinition
    {
        public bool EnableFirstPerson;
        public bool EnableShipControl;
        public bool EnableBuilderCockpit;
        public string GlassModel;
        public string InteriorModel;
        public string CharacterAnimation;
        public string GetInSound;
        public string GetOutSound;
        public Vector3D RaycastOffset = Vector3D.Zero;
        public List<ScreenArea> ScreenAreas;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipControllerDefinition definition = builder as MyObjectBuilder_ShipControllerDefinition;
            this.EnableFirstPerson = definition.EnableFirstPerson;
            this.EnableShipControl = definition.EnableShipControl;
            this.EnableBuilderCockpit = definition.EnableBuilderCockpit;
            this.GetInSound = definition.GetInSound;
            this.GetOutSound = definition.GetOutSound;
            this.RaycastOffset = definition.RaycastOffset;
        }
    }
}

