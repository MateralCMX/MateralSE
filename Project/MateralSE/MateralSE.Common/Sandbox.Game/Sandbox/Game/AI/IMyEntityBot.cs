namespace Sandbox.Game.AI
{
    using System;
    using VRage.Game.Entity;

    public interface IMyEntityBot : IMyBot
    {
        void Spawn(Vector3D? spawnPosition, bool spawnedByPlayer);

        MyEntity BotEntity { get; }

        bool ShouldFollowPlayer { get; set; }
    }
}

