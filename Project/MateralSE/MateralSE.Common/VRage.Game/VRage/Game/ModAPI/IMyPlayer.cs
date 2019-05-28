namespace VRage.Game.ModAPI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.ModAPI;
    using VRageMath;

    public interface IMyPlayer
    {
        event Action<IMyPlayer, IMyIdentity> IdentityChanged;

        void AddGrid(long gridEntityId);
        void ChangeOrSwitchToColor(Vector3 color);
        Vector3D GetPosition();
        MyRelationsBetweenPlayerAndBlock GetRelationTo(long playerId);
        void RemoveGrid(long gridEntityId);
        void SetDefaultColors();
        void SpawnAt(MatrixD worldMatrix, Vector3 velocity, IMyEntity spawnedBy);
        void SpawnAt(MatrixD worldMatrix, Vector3 velocity, IMyEntity spawnedBy, bool findFreePlace = true, string modelName = null, Color? color = new Color?());
        void SpawnIntoCharacter(IMyCharacter character);

        IMyNetworkClient Client { get; }

        HashSet<long> Grids { get; }

        IMyEntityController Controller { get; }

        ulong SteamUserId { get; }

        string DisplayName { get; }

        [Obsolete("Use IdentityId instead.")]
        long PlayerID { get; }

        long IdentityId { get; }

        [Obsolete("Use Promote Level instead")]
        bool IsAdmin { get; }

        [Obsolete("Use Promote Level instead")]
        bool IsPromoted { get; }

        MyPromoteLevel PromoteLevel { get; }

        IMyCharacter Character { get; }

        bool IsBot { get; }

        IMyIdentity Identity { get; }

        ListReader<long> RespawnShip { get; }

        List<Vector3> BuildColorSlots { get; set; }

        ListReader<Vector3> DefaultBuildColorSlots { get; }

        Vector3 SelectedBuildColor { get; set; }

        int SelectedBuildColorSlot { get; set; }
    }
}

