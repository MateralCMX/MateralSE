namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_MissileAmmoDefinition), (Type) null)]
    public class MyMissileAmmoDefinition : MyAmmoDefinition
    {
        public const float MINIMAL_EXPLOSION_RADIUS = 0.6f;
        public float MissileMass;
        public float MissileExplosionRadius;
        public string MissileModelName;
        public float MissileAcceleration;
        public float MissileInitialSpeed;
        public bool MissileSkipAcceleration;
        public float MissileExplosionDamage;

        public override float GetDamageForMechanicalObjects() => 
            this.MissileExplosionDamage;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            base.AmmoType = MyAmmoType.Missile;
            MyObjectBuilder_MissileAmmoDefinition.AmmoMissileProperties missileProperties = (builder as MyObjectBuilder_MissileAmmoDefinition).MissileProperties;
            this.MissileAcceleration = missileProperties.MissileAcceleration;
            this.MissileExplosionDamage = missileProperties.MissileExplosionDamage;
            this.MissileExplosionRadius = missileProperties.MissileExplosionRadius;
            this.MissileInitialSpeed = missileProperties.MissileInitialSpeed;
            this.MissileMass = missileProperties.MissileMass;
            this.MissileModelName = missileProperties.MissileModelName;
            this.MissileSkipAcceleration = missileProperties.MissileSkipAcceleration;
        }
    }
}

