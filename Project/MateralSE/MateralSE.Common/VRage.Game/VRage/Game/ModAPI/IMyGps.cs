namespace VRage.Game.ModAPI
{
    using System;
    using VRageMath;

    public interface IMyGps
    {
        string ToString();
        void UpdateHash();

        int Hash { get; }

        string Name { get; set; }

        string Description { get; set; }

        Vector3D Coords { get; set; }

        bool ShowOnHud { get; set; }

        TimeSpan? DiscardAt { get; set; }

        string ContainerRemainingTime { get; set; }
    }
}

