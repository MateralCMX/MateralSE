namespace Sandbox.Game
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyExplosionInfo
    {
        public float PlayerDamage;
        public float Damage;
        public BoundingSphereD ExplosionSphere;
        public float StrengthImpulse;
        public float StrengthAngularImpulse;
        public MyEntity ExcludedEntity;
        public MyEntity OwnerEntity;
        public MyEntity HitEntity;
        public MyExplosionFlags ExplosionFlags;
        public MyExplosionTypeEnum ExplosionType;
        public int LifespanMiliseconds;
        public int ObjectsRemoveDelayInMiliseconds;
        public float ParticleScale;
        public float VoxelCutoutScale;
        public Vector3? Direction;
        public Vector3D VoxelExplosionCenter;
        public bool PlaySound;
        public bool CheckIntersections;
        public Vector3 Velocity;
        public long OriginEntity;
        public string CustomEffect;
        public MySoundPair CustomSound;
        public bool KeepAffectedBlocks;
        public MyExplosionInfo(float playerDamage, float damage, BoundingSphereD explosionSphere, MyExplosionTypeEnum type, bool playSound, bool checkIntersection = true)
        {
            MyEntity entity;
            this.PlayerDamage = playerDamage;
            this.Damage = damage;
            this.ExplosionSphere = explosionSphere;
            this.StrengthImpulse = this.StrengthAngularImpulse = 0f;
            this.HitEntity = (MyEntity) (entity = null);
            this.ExcludedEntity = this.OwnerEntity = entity;
            this.ExplosionFlags = MyExplosionFlags.APPLY_DEFORMATION | MyExplosionFlags.CREATE_PARTICLE_EFFECT | MyExplosionFlags.CREATE_DECALS | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE | MyExplosionFlags.AFFECT_VOXELS | MyExplosionFlags.CREATE_DEBRIS;
            this.ExplosionType = type;
            this.LifespanMiliseconds = 700;
            this.ObjectsRemoveDelayInMiliseconds = 0;
            this.ParticleScale = 1f;
            this.VoxelCutoutScale = 1f;
            this.Direction = null;
            this.VoxelExplosionCenter = explosionSphere.Center;
            this.PlaySound = playSound;
            this.CheckIntersections = checkIntersection;
            this.Velocity = Vector3.Zero;
            this.OriginEntity = 0L;
            this.CustomEffect = "";
            this.CustomSound = null;
            this.KeepAffectedBlocks = false;
        }

        private void SetFlag(MyExplosionFlags flag, bool value)
        {
            if (value)
            {
                this.ExplosionFlags |= flag;
            }
            else
            {
                this.ExplosionFlags &= ~flag;
            }
        }

        private bool HasFlag(MyExplosionFlags flag) => 
            ((this.ExplosionFlags & flag) == flag);

        public bool AffectVoxels
        {
            get => 
                this.HasFlag(MyExplosionFlags.AFFECT_VOXELS);
            set => 
                this.SetFlag(MyExplosionFlags.AFFECT_VOXELS, value);
        }
        public bool CreateDebris
        {
            get => 
                this.HasFlag(MyExplosionFlags.CREATE_DEBRIS);
            set => 
                this.SetFlag(MyExplosionFlags.CREATE_DEBRIS, value);
        }
        public bool CreateParticleDebris
        {
            get => 
                this.HasFlag(MyExplosionFlags.CREATE_PARTICLE_DEBRIS);
            set => 
                this.SetFlag(MyExplosionFlags.CREATE_PARTICLE_DEBRIS, value);
        }
        public bool ApplyForceAndDamage
        {
            get => 
                this.HasFlag(MyExplosionFlags.APPLY_FORCE_AND_DAMAGE);
            set => 
                this.SetFlag(MyExplosionFlags.APPLY_FORCE_AND_DAMAGE, value);
        }
        public bool CreateDecals
        {
            get => 
                this.HasFlag(MyExplosionFlags.CREATE_DECALS);
            set => 
                this.SetFlag(MyExplosionFlags.CREATE_DECALS, value);
        }
        public bool ForceDebris
        {
            get => 
                this.HasFlag(MyExplosionFlags.FORCE_DEBRIS);
            set => 
                this.SetFlag(MyExplosionFlags.FORCE_DEBRIS, value);
        }
        public bool CreateParticleEffect
        {
            get => 
                this.HasFlag(MyExplosionFlags.CREATE_PARTICLE_EFFECT);
            set => 
                this.SetFlag(MyExplosionFlags.CREATE_PARTICLE_EFFECT, value);
        }
        public bool CreateShrapnels
        {
            get => 
                this.HasFlag(MyExplosionFlags.CREATE_SHRAPNELS);
            set => 
                this.SetFlag(MyExplosionFlags.CREATE_SHRAPNELS, value);
        }
    }
}

