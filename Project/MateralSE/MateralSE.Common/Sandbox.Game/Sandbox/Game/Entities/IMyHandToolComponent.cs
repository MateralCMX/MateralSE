namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Character;
    using System;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;

    public interface IMyHandToolComponent
    {
        void DrawHud();
        string GetStateForTarget(MyEntity targetEntity, uint shapeKey);
        bool Hit(MyEntity entity, MyHitInfo hitInfo, uint shapeKey, float efficiency);
        void OnControlAcquired(MyCharacter owner);
        void OnControlReleased();
        void Shoot();
        void Update();
    }
}

