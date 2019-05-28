namespace VRage.Game
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("StartingState")]
    public abstract class MyObjectBuilder_WorldGeneratorPlayerStartingState : MyObjectBuilder_Base
    {
        public string FactionTag;

        protected MyObjectBuilder_WorldGeneratorPlayerStartingState()
        {
        }
    }
}

