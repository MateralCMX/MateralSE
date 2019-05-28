namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_DebrisDefinition), (System.Type) null)]
    public class MyDebrisDefinition : MyDefinitionBase
    {
        public string Model;
        public MyDebrisType Type;
        public float MinAmount;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_DebrisDefinition definition = builder as MyObjectBuilder_DebrisDefinition;
            this.Model = definition.Model;
            this.Type = definition.Type;
            this.MinAmount = definition.MinAmount;
        }
    }
}

