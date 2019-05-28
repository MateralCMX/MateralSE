namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_DestructionDefinition), (Type) null)]
    public class MyDestructionDefinition : MyDefinitionBase
    {
        public float DestructionDamage;
        public string[] Icons;
        public float ConvertedFractureIntegrityRatio;
        public MyFracturedPieceDefinition[] FracturedPieceDefinitions;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_DestructionDefinition definition = builder as MyObjectBuilder_DestructionDefinition;
            this.DestructionDamage = definition.DestructionDamage;
            this.Icons = definition.Icons;
            this.ConvertedFractureIntegrityRatio = definition.ConvertedFractureIntegrityRatio;
            if ((definition.FracturedPieceDefinitions != null) && (definition.FracturedPieceDefinitions.Length != 0))
            {
                this.FracturedPieceDefinitions = new MyFracturedPieceDefinition[definition.FracturedPieceDefinitions.Length];
                for (int i = 0; i < definition.FracturedPieceDefinitions.Length; i++)
                {
                    this.FracturedPieceDefinitions[i] = new MyFracturedPieceDefinition { 
                        Id = definition.FracturedPieceDefinitions[i].Id,
                        Age = definition.FracturedPieceDefinitions[i].Age
                    };
                }
            }
        }

        public void Merge(MyDestructionDefinition src)
        {
            this.DestructionDamage = src.DestructionDamage;
            this.Icons = src.Icons;
            this.ConvertedFractureIntegrityRatio = src.ConvertedFractureIntegrityRatio;
            this.FracturedPieceDefinitions = src.FracturedPieceDefinitions;
        }

        public class MyFracturedPieceDefinition
        {
            public MyDefinitionId Id;
            public int Age;
        }
    }
}

