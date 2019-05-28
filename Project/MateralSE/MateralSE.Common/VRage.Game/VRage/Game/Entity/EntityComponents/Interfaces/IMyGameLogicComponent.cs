namespace VRage.Game.Entity.EntityComponents.Interfaces
{
    using System;

    public interface IMyGameLogicComponent
    {
        void Close();
        void RegisterForUpdate();
        void UnregisterForUpdate();
        void UpdateAfterSimulation(bool entityUpdate);
        void UpdateAfterSimulation10(bool entityUpdate);
        void UpdateAfterSimulation100(bool entityUpdate);
        void UpdateBeforeSimulation(bool entityUpdate);
        void UpdateBeforeSimulation10(bool entityUpdate);
        void UpdateBeforeSimulation100(bool entityUpdate);
        void UpdateOnceBeforeFrame(bool entityUpdate);

        bool EntityUpdate { get; set; }
    }
}

