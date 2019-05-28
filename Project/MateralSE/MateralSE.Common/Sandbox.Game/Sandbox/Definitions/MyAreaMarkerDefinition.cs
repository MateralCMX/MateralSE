namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_AreaMarkerDefinition), (Type) null)]
    public class MyAreaMarkerDefinition : MyDefinitionBase
    {
        public string Model;
        public string ColorMetalTexture;
        public string AddMapsTexture;
        public Vector3 ColorHSV;
        public Vector3 MarkerPosition;
        public int MaxNumber;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_AreaMarkerDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_AreaMarkerDefinition;
            objectBuilder.Model = this.Model;
            objectBuilder.ColorMetalTexture = this.ColorMetalTexture;
            objectBuilder.AddMapsTexture = this.AddMapsTexture;
            objectBuilder.ColorHSV = this.ColorHSV;
            objectBuilder.MarkerPosition = this.MarkerPosition;
            objectBuilder.MaxNumber = this.MaxNumber;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AreaMarkerDefinition definition = builder as MyObjectBuilder_AreaMarkerDefinition;
            this.Model = definition.Model;
            this.ColorMetalTexture = definition.ColorMetalTexture;
            this.AddMapsTexture = definition.AddMapsTexture;
            this.ColorHSV = (Vector3) definition.ColorHSV;
            this.MarkerPosition = (Vector3) definition.MarkerPosition;
            this.MaxNumber = definition.MaxNumber;
        }
    }
}

