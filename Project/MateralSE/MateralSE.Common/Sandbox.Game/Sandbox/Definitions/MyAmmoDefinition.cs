namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_AmmoDefinition), (Type) null)]
    public abstract class MyAmmoDefinition : MyDefinitionBase
    {
        public MyAmmoType AmmoType;
        public float DesiredSpeed;
        public float SpeedVar;
        public float MaxTrajectory;
        public bool IsExplosive;
        public float BackkickForce;
        public MyStringHash PhysicalMaterial;

        protected MyAmmoDefinition()
        {
        }

        public abstract float GetDamageForMechanicalObjects();
        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AmmoDefinition definition = builder as MyObjectBuilder_AmmoDefinition;
            this.DesiredSpeed = definition.BasicProperties.DesiredSpeed;
            this.SpeedVar = MathHelper.Clamp(definition.BasicProperties.SpeedVariance, 0f, 1f);
            this.MaxTrajectory = definition.BasicProperties.MaxTrajectory;
            this.IsExplosive = definition.BasicProperties.IsExplosive;
            this.BackkickForce = definition.BasicProperties.BackkickForce;
            this.PhysicalMaterial = MyStringHash.GetOrCompute(definition.BasicProperties.PhysicalMaterial);
        }
    }
}

