namespace Sandbox.Game.Entities.UseObject
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.Entity.UseObject;

    public interface IMyUsableEntity
    {
        UseActionResult CanUse(UseActionEnum actionEnum, IMyControllableEntity user);
        void RemoveUsers(bool local);
    }
}

