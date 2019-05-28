namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_FactionDefinition), (Type) null)]
    public class MyFactionDefinition : MyDefinitionBase
    {
        public string Tag;
        public string Name;
        public string Founder;
        public bool AcceptHumans;
        public bool AutoAcceptMember;
        public bool EnableFriendlyFire;
        public bool IsDefault;
        public MyRelationsBetweenFactions DefaultRelation;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_FactionDefinition definition = builder as MyObjectBuilder_FactionDefinition;
            this.Tag = definition.Tag;
            this.Name = definition.Name;
            this.Founder = definition.Founder;
            this.AcceptHumans = definition.AcceptHumans;
            this.AutoAcceptMember = definition.AutoAcceptMember;
            this.EnableFriendlyFire = definition.EnableFriendlyFire;
            this.IsDefault = definition.IsDefault;
            this.DefaultRelation = definition.DefaultRelation;
        }

        public override void Postprocess()
        {
            base.Postprocess();
            MyDefinitionManager.Static.RegisterFactionDefinition(this);
        }
    }
}

