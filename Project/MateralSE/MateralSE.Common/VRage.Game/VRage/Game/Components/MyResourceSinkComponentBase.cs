namespace VRage.Game.Components
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.ModAPI;

    public abstract class MyResourceSinkComponentBase : MyEntityComponentBase
    {
        protected MyResourceSinkComponentBase()
        {
        }

        public abstract float CurrentInputByType(MyDefinitionId resourceTypeId);
        public abstract bool IsPowerAvailable(MyDefinitionId resourceTypeId, float power);
        public abstract bool IsPoweredByType(MyDefinitionId resourceTypeId);
        public abstract float MaxRequiredInputByType(MyDefinitionId resourceTypeId);
        public abstract float RequiredInputByType(MyDefinitionId resourceTypeId);
        public abstract void SetInputFromDistributor(MyDefinitionId resourceTypeId, float newResourceInput, bool isAdaptible, bool fireEvents = true);
        public abstract void SetMaxRequiredInputByType(MyDefinitionId resourceTypeId, float newMaxRequiredInput);
        public abstract void SetRequiredInputByType(MyDefinitionId resourceTypeId, float newRequiredInput);
        public abstract void SetRequiredInputFuncByType(MyDefinitionId resourceTypeId, Func<float> newRequiredInputFunc);
        public abstract float SuppliedRatioByType(MyDefinitionId resourceTypeId);

        public abstract ListReader<MyDefinitionId> AcceptedResources { get; }

        public abstract IMyEntity TemporaryConnectedEntity { get; set; }
    }
}

