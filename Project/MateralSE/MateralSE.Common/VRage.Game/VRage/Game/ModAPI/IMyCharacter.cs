namespace VRage.Game.ModAPI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRageMath;

    public interface IMyCharacter : VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyControllableEntity, IMyCameraController, IMyDestroyableObject, IMyDecalProxy
    {
        event Action<IMyCharacter> CharacterDied;

        event CharacterMovementStateChangedDelegate MovementStateChanged;

        [Obsolete("OnMovementStateChanged is deprecated, use MovementStateChanged")]
        event CharacterMovementStateDelegate OnMovementStateChanged;

        float GetOutsideTemperature();
        float GetSuitGasFillLevel(MyDefinitionId gasDefinitionId);
        void Kill(object killData = null);
        void TriggerCharacterAnimationEvent(string eventName, bool sync);

        Vector3D AimedPoint { get; set; }

        MyDefinitionBase Definition { get; }

        float EnvironmentOxygenLevel { get; }

        float OxygenLevel { get; }

        float BaseMass { get; }

        float CurrentMass { get; }

        float SuitEnergyLevel { get; }

        bool IsDead { get; }

        bool IsPlayer { get; }

        bool IsBot { get; }

        MyCharacterMovementEnum CurrentMovementState { get; set; }

        MyCharacterMovementEnum PreviousMovementState { get; }

        VRage.ModAPI.IMyEntity EquippedTool { get; }
    }
}

