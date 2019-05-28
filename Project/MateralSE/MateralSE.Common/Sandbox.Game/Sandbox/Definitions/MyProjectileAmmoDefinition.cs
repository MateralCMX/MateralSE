namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ProjectileAmmoDefinition), (Type) null)]
    public class MyProjectileAmmoDefinition : MyAmmoDefinition
    {
        public float ProjectileHitImpulse;
        public float ProjectileTrailScale;
        public Vector3 ProjectileTrailColor;
        public string ProjectileTrailMaterial;
        public float ProjectileTrailProbability;
        public string ProjectileOnHitEffectName;
        public float ProjectileMassDamage;
        public float ProjectileHealthDamage;
        public bool HeadShot;
        public float ProjectileHeadShotDamage;
        public int ProjectileCount;

        public override float GetDamageForMechanicalObjects() => 
            this.ProjectileMassDamage;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            base.AmmoType = MyAmmoType.HighSpeed;
            MyObjectBuilder_ProjectileAmmoDefinition.AmmoProjectileProperties projectileProperties = (builder as MyObjectBuilder_ProjectileAmmoDefinition).ProjectileProperties;
            this.ProjectileHealthDamage = projectileProperties.ProjectileHealthDamage;
            this.ProjectileHitImpulse = projectileProperties.ProjectileHitImpulse;
            this.ProjectileMassDamage = projectileProperties.ProjectileMassDamage;
            this.ProjectileOnHitEffectName = projectileProperties.ProjectileOnHitEffectName;
            this.ProjectileTrailColor = (Vector3) projectileProperties.ProjectileTrailColor;
            this.ProjectileTrailMaterial = projectileProperties.ProjectileTrailMaterial;
            this.ProjectileTrailProbability = projectileProperties.ProjectileTrailProbability;
            this.ProjectileTrailScale = projectileProperties.ProjectileTrailScale;
            this.HeadShot = projectileProperties.HeadShot;
            this.ProjectileHeadShotDamage = projectileProperties.ProjectileHeadShotDamage;
            this.ProjectileCount = projectileProperties.ProjectileCount;
        }
    }
}

