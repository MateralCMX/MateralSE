namespace Sandbox.Game.EntityComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ModelComponentDefinition), (Type) null)]
    public class MyModelComponentDefinition : MyComponentDefinitionBase
    {
        public Vector3 Size;
        public float Mass;
        public float Volume;
        public string Model;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_ModelComponentDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_ModelComponentDefinition;
            objectBuilder.Size = this.Size;
            objectBuilder.Mass = this.Mass;
            objectBuilder.Model = this.Model;
            objectBuilder.Volume = new float?(this.Volume * 1000f);
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ModelComponentDefinition definition = builder as MyObjectBuilder_ModelComponentDefinition;
            this.Size = definition.Size;
            this.Mass = definition.Mass;
            this.Model = definition.Model;
            this.Volume = (definition.Volume != null) ? (definition.Volume.Value / 1000f) : definition.Size.Volume;
        }
    }
}

