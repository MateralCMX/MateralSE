namespace VRage.Game.Components
{
    using System;
    using VRage.Game;

    public abstract class MyResourceSourceComponentBase : MyEntityComponentBase
    {
        protected MyResourceSourceComponentBase()
        {
        }

        public abstract float CurrentOutputByType(MyDefinitionId resourceTypeId);
        public abstract float DefinedOutputByType(MyDefinitionId resourceTypeId);
        public abstract float MaxOutputByType(MyDefinitionId resourceTypeId);
        public abstract bool ProductionEnabledByType(MyDefinitionId resourceTypeId);
    }
}

