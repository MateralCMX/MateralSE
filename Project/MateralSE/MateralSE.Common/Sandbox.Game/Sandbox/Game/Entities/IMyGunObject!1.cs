namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI.Interfaces;
    using VRageMath;

    public interface IMyGunObject<out T> where T: MyDeviceBase
    {
        void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status);
        void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status);
        void BeginShoot(MyShootActionEnum action);
        bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status);
        Vector3 DirectionToTarget(Vector3D target);
        void DrawHud(IMyCameraController camera, long playerId);
        void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate);
        void EndShoot(MyShootActionEnum action);
        int GetAmmunitionAmount();
        void OnControlAcquired(MyCharacter owner);
        void OnControlReleased();
        void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction = null);
        void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status);
        bool SupressShootAnimation();
        void UpdateSoundEmitter();

        float BackkickForcePerSecond { get; }

        float ShakeAmount { get; }

        MyDefinitionId DefinitionId { get; }

        bool EnabledInWorldRules { get; }

        T GunBase { get; }

        bool IsSkinnable { get; }

        bool IsShooting { get; }

        int ShootDirectionUpdateTime { get; }
    }
}

