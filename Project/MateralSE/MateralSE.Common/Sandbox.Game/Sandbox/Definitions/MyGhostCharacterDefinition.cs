namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_GhostCharacterDefinition), (Type) null)]
    public class MyGhostCharacterDefinition : MyDefinitionBase
    {
        public List<MyDefinitionId> LeftHandWeapons = new List<MyDefinitionId>();
        public List<MyDefinitionId> RightHandWeapons = new List<MyDefinitionId>();

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GhostCharacterDefinition definition = builder as MyObjectBuilder_GhostCharacterDefinition;
            if (definition.LeftHandWeapons != null)
            {
                foreach (SerializableDefinitionId id in definition.LeftHandWeapons)
                {
                    this.LeftHandWeapons.Add(id);
                }
            }
            if (definition.RightHandWeapons != null)
            {
                foreach (SerializableDefinitionId id2 in definition.RightHandWeapons)
                {
                    this.RightHandWeapons.Add(id2);
                }
            }
        }
    }
}

