namespace VRage.Game.ModAPI
{
    using System;
    using System.Runtime.CompilerServices;

    public interface IMyIdentity
    {
        event Action<IMyCharacter, IMyCharacter> CharacterChanged;

        [Obsolete("Use IdentityId instead.")]
        long PlayerId { get; }

        long IdentityId { get; }

        string DisplayName { get; }

        string Model { get; }

        Vector3? ColorMask { get; }

        bool IsDead { get; }
    }
}

