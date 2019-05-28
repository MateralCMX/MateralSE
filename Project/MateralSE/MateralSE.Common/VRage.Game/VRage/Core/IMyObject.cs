namespace VRage.Core
{
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public interface IMyObject
    {
        void Deserialize(MyObjectBuilder_Base builder);
        MyObjectBuilder_Base Serialize();

        MyDefinitionId DefinitionId { get; }

        bool NeedsSerialize { get; }
    }
}

