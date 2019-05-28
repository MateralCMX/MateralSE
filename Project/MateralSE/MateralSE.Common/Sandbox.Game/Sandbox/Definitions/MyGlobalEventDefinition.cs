namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_GlobalEventDefinition), (Type) null)]
    public class MyGlobalEventDefinition : MyDefinitionBase
    {
        public TimeSpan? MinActivationTime;
        public TimeSpan? MaxActivationTime;
        public TimeSpan? FirstActivationTime;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            if (builder.Id.TypeId == typeof(MyObjectBuilder_GlobalEventDefinition))
            {
                builder.Id = new SerializableDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), builder.Id.SubtypeName);
            }
            base.Init(builder);
            MyObjectBuilder_GlobalEventDefinition definition = builder as MyObjectBuilder_GlobalEventDefinition;
            if ((definition.MinActivationTimeMs != null) && (definition.MaxActivationTimeMs == null))
            {
                definition.MaxActivationTimeMs = definition.MinActivationTimeMs;
            }
            if ((definition.MaxActivationTimeMs != null) && (definition.MinActivationTimeMs == null))
            {
                definition.MinActivationTimeMs = definition.MaxActivationTimeMs;
            }
            if (definition.MinActivationTimeMs != null)
            {
                this.MinActivationTime = new TimeSpan?(TimeSpan.FromTicks(definition.MinActivationTimeMs.Value * 0x2710L));
            }
            if (definition.MaxActivationTimeMs != null)
            {
                this.MaxActivationTime = new TimeSpan?(TimeSpan.FromTicks(definition.MaxActivationTimeMs.Value * 0x2710L));
            }
            if (definition.FirstActivationTimeMs != null)
            {
                this.FirstActivationTime = new TimeSpan?(TimeSpan.FromTicks(definition.FirstActivationTimeMs.Value * 0x2710L));
            }
        }
    }
}

