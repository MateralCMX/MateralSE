namespace Sandbox.Definitions
{
    using System;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_PrefabThrowerDefinition), (Type) null)]
    public class MyPrefabThrowerDefinition : MyDefinitionBase
    {
        public float? Mass;
        public float MaxSpeed;
        public float MinSpeed;
        public float PushTime;
        public string PrefabToThrow;
        public MyCueId ThrowSound;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PrefabThrowerDefinition definition = builder as MyObjectBuilder_PrefabThrowerDefinition;
            if (definition.Mass != null)
            {
                this.Mass = definition.Mass;
            }
            this.MaxSpeed = definition.MaxSpeed;
            this.MinSpeed = definition.MinSpeed;
            this.PushTime = definition.PushTime;
            this.PrefabToThrow = definition.PrefabToThrow;
            this.ThrowSound = new MyCueId(MyStringHash.GetOrCompute(definition.ThrowSound));
        }
    }
}

