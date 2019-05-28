namespace Sandbox.Game.Weapons
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRageMath;

    public static class MyWeaponPrediction
    {
        public static bool GetPredictedTargetPosition(MyGunBase gun, MyEntity shooter, MyEntity target, out Vector3 predictedPosition, out float timeToHit, float shootDelay = 0f)
        {
            float num8;
            if (((target == null) || ((target.PositionComp == null) || (shooter == null))) || (shooter.PositionComp == null))
            {
                predictedPosition = Vector3.Zero;
                timeToHit = 0f;
                return false;
            }
            Vector3 center = (Vector3) target.PositionComp.WorldAABB.Center;
            Vector3 muzzleWorldPosition = (Vector3) gun.GetMuzzleWorldPosition();
            Vector3 vector3 = center - muzzleWorldPosition;
            Vector3 zero = Vector3.Zero;
            if (target.Physics != null)
            {
                zero = target.Physics.LinearVelocity;
            }
            Vector3 linearVelocity = Vector3.Zero;
            if (shooter.Physics != null)
            {
                linearVelocity = shooter.Physics.LinearVelocity;
            }
            Vector3 vector6 = zero - linearVelocity;
            float projectileSpeed = GetProjectileSpeed(gun);
            float num2 = vector6.LengthSquared() - (projectileSpeed * projectileSpeed);
            float num3 = vector3.LengthSquared();
            float single1 = 2f * Vector3.Dot(vector6, vector3);
            float num4 = -single1 / (2f * num2);
            float num5 = ((float) Math.Sqrt((double) ((single1 * single1) - ((4f * num2) * num3)))) / (2f * num2);
            float num6 = num4 - num5;
            float num7 = num4 + num5;
            if ((num6 <= num7) || (num7 <= 0f))
            {
                num8 = num6;
            }
            else
            {
                num8 = num7;
            }
            predictedPosition = center + (vector6 * (num8 + shootDelay));
            Vector3 vector7 = predictedPosition - muzzleWorldPosition;
            timeToHit = vector7.Length() / projectileSpeed;
            return true;
        }

        public static float GetProjectileSpeed(MyGunBase gun)
        {
            if (gun == null)
            {
                return 0f;
            }
            float desiredSpeed = 0f;
            if (gun.CurrentAmmoMagazineDefinition != null)
            {
                desiredSpeed = MyDefinitionManager.Static.GetAmmoDefinition(gun.CurrentAmmoMagazineDefinition.AmmoDefinitionId).DesiredSpeed;
            }
            return desiredSpeed;
        }
    }
}

