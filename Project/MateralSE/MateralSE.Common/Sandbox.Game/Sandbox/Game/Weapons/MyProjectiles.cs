namespace Sandbox.Game.Weapons
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Generics;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    internal class MyProjectiles : MySessionComponentBase
    {
        private static MyObjectsPool<MyProjectile> m_projectiles;

        public static void Add(MyWeaponPropertiesWrapper props, Vector3D origin, Vector3 initialVelocity, Vector3 directionNormalized, IMyGunBaseUser user, MyEntity owner)
        {
            MyProjectile projectile;
            object obj1;
            m_projectiles.AllocateOrCreate(out projectile);
            projectile.Start(props.GetCurrentAmmoDefinitionAs<MyProjectileAmmoDefinition>(), props.WeaponDefinition, user.IgnoreEntities, origin, initialVelocity, directionNormalized, user.Weapon);
            MyEntity entity1 = user.Owner;
            MyEntity entity2 = entity1;
            if (entity1 == null)
            {
                MyEntity local1 = entity1;
                if ((user.IgnoreEntities == null) || (user.IgnoreEntities.Length == 0))
                {
                    entity2 = null;
                }
                else
                {
                    entity2 = user.IgnoreEntities[0];
                }
            }
            projectile.OwnerEntity = (MyEntity) obj1;
            projectile.OwnerEntityAbsolute = owner;
        }

        public static void AddShotgun(MyProjectileAmmoDefinition ammoDefinition, MyEntity ignorePhysObject, Vector3 origin, Vector3 initialVelocity, Vector3 directionNormalized, bool groupStart, float thicknessMultiplier, MyEntity weapon, float frontBillboardSize, MyEntity ownerEntity = null, float projectileCountMultiplier = 1f)
        {
            m_projectiles.Allocate(false);
        }

        public static void AddShrapnel(MyProjectileAmmoDefinition ammoDefinition, MyEntity[] ignoreEntities, Vector3 origin, Vector3 initialVelocity, Vector3 directionNormalized, bool groupStart, float thicknessMultiplier, float trailProbability, MyEntity weapon, MyEntity ownerEntity = null, float projectileCountMultiplier = 1f)
        {
            MyProjectile projectile;
            object obj1;
            m_projectiles.AllocateOrCreate(out projectile);
            projectile.Start(ammoDefinition, null, ignoreEntities, origin, initialVelocity, directionNormalized, weapon);
            MyEntity entity1 = ownerEntity;
            if (ownerEntity == null)
            {
                MyEntity local1 = ownerEntity;
                if ((ignoreEntities == null) || (ignoreEntities.Length == 0))
                {
                    entity1 = null;
                }
                else
                {
                    entity1 = ignoreEntities[0];
                }
            }
            projectile.OwnerEntity = (MyEntity) obj1;
        }

        public override void Draw()
        {
            using (HashSet<MyProjectile>.Enumerator enumerator = m_projectiles.Active.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Draw();
                }
            }
        }

        public override void LoadData()
        {
            if (m_projectiles == null)
            {
                m_projectiles = new MyObjectsPool<MyProjectile>(0x2000, null);
            }
        }

        protected override void UnloadData()
        {
            if (m_projectiles != null)
            {
                m_projectiles.DeallocateAll();
            }
            m_projectiles = null;
        }

        public override void UpdateBeforeSimulation()
        {
            foreach (MyProjectile projectile in m_projectiles.Active)
            {
                if (!projectile.Update())
                {
                    projectile.Close();
                    m_projectiles.MarkForDeallocate(projectile);
                }
            }
            m_projectiles.DeallocateAllMarked();
        }
    }
}

