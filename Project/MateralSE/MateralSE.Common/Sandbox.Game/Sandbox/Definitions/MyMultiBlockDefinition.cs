namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_MultiBlockDefinition), (Type) null)]
    public class MyMultiBlockDefinition : MyDefinitionBase
    {
        public MyMultiBlockPartDefinition[] BlockDefinitions;
        public Vector3I Min;
        public Vector3I Max;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MultiBlockDefinition definition = builder as MyObjectBuilder_MultiBlockDefinition;
            if ((definition.BlockDefinitions != null) && (definition.BlockDefinitions.Length != 0))
            {
                this.BlockDefinitions = new MyMultiBlockPartDefinition[definition.BlockDefinitions.Length];
                for (int i = 0; i < definition.BlockDefinitions.Length; i++)
                {
                    this.BlockDefinitions[i] = new MyMultiBlockPartDefinition();
                    MyObjectBuilder_MultiBlockDefinition.MyOBMultiBlockPartDefinition definition2 = definition.BlockDefinitions[i];
                    this.BlockDefinitions[i].Id = definition2.Id;
                    this.BlockDefinitions[i].Min = (Vector3I) definition2.Position;
                    this.BlockDefinitions[i].Forward = definition2.Orientation.Forward;
                    this.BlockDefinitions[i].Up = definition2.Orientation.Up;
                }
            }
        }

        public class MyMultiBlockPartDefinition
        {
            public MyDefinitionId Id;
            public Vector3I Min;
            public Vector3I Max;
            public Base6Directions.Direction Forward;
            public Base6Directions.Direction Up;
        }
    }
}

