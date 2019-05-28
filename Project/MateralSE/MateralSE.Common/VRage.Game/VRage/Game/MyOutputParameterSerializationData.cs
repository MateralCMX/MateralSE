namespace VRage.Game
{
    using System;
    using System.Collections.Generic;

    public class MyOutputParameterSerializationData
    {
        public string Type;
        public string Name;
        public IdentifierList Outputs;

        public MyOutputParameterSerializationData()
        {
            this.Outputs.Ids = new List<MyVariableIdentifier>();
        }
    }
}

