namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Utils;

    public interface IMyControllableEntity : VRage.Game.ModAPI.Interfaces.IMyControllableEntity
    {
        void BeginShoot(MyShootActionEnum action);
        bool CanSwitchAmmoMagazine();
        bool CanSwitchToWeapon(MyDefinitionId? weaponDefinition);
        void EndShoot(MyShootActionEnum action);
        MyEntityCameraSettings GetCameraEntitySettings();
        void OnBeginShoot(MyShootActionEnum action);
        void OnEndShoot(MyShootActionEnum action);
        void PickUpFinished();
        bool ShouldEndShootingOnPause(MyShootActionEnum action);
        void Sprint(bool enabled);
        void SwitchAmmoMagazine();
        void SwitchBroadcasting();
        void SwitchToWeapon(MyToolbarItemWeapon weapon);
        void SwitchToWeapon(MyDefinitionId weaponDefinition);
        void UseFinished();

        MyControllerInfo ControllerInfo { get; }

        MyEntity Entity { get; }

        float HeadLocalXAngle { get; set; }

        float HeadLocalYAngle { get; set; }

        bool EnabledBroadcasting { get; }

        MyToolbarType ToolbarType { get; }

        MyStringId ControlContext { get; }

        MyToolbar Toolbar { get; }

        MyEntity RelativeDampeningEntity { get; set; }
    }
}

