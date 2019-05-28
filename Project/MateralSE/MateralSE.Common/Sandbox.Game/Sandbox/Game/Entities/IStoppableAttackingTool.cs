namespace Sandbox.Game.Entities
{
    using System;
    using VRage.Game.Entity;

    public interface IStoppableAttackingTool
    {
        void StopShooting(MyEntity attacker);
    }
}

