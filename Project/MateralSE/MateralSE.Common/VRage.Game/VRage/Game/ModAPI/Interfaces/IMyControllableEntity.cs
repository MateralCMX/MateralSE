namespace VRage.Game.ModAPI.Interfaces
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRageMath;

    public interface IMyControllableEntity
    {
        void Crouch();
        void Die();
        void Down();
        void DrawHud(IMyCameraController camera, long playerId);
        MatrixD GetHeadMatrix(bool includeY, bool includeX = true, bool forceHeadAnim = false, bool forceHeadBone = false);
        void Jump(Vector3 moveindicator = new Vector3());
        void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator);
        void MoveAndRotateStopped();
        void PickUp();
        void PickUpContinues();
        void ShowInventory();
        void ShowTerminal();
        void SwitchDamping();
        void SwitchHelmet();
        void SwitchLandingGears();
        void SwitchLights();
        void SwitchReactors();
        void SwitchThrusts();
        void SwitchWalk();
        void Up();
        void Use();
        void UseContinues();

        IMyControllerInfo ControllerInfo { get; }

        IMyEntity Entity { get; }

        bool ForceFirstPersonCamera { get; set; }

        bool EnabledThrusts { get; }

        bool EnabledDamping { get; }

        bool EnabledLights { get; }

        bool EnabledLeadingGears { get; }

        bool EnabledReactors { get; }

        bool EnabledHelmet { get; }

        bool PrimaryLookaround { get; }
    }
}

