namespace Sandbox.Definitions
{
    using Sandbox.Game.World;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ScenarioDefinition), (Type) null)]
    public class MyScenarioDefinition : MyDefinitionBase
    {
        public MyDefinitionId GameDefinition;
        public MyDefinitionId Environment;
        public BoundingBoxD? WorldBoundaries;
        public MyWorldGeneratorStartingStateBase[] PossiblePlayerStarts;
        public MyWorldGeneratorOperationBase[] WorldGeneratorOperations;
        public bool AsteroidClustersEnabled;
        public float AsteroidClustersOffset;
        public bool CentralClusterEnabled;
        public MyEnvironmentHostilityEnum DefaultEnvironment;
        public MyStringId[] CreativeModeWeapons;
        public MyStringId[] SurvivalModeWeapons;
        public StartingItem[] CreativeModeComponents;
        public StartingItem[] SurvivalModeComponents;
        public StartingPhysicalItem[] CreativeModePhysicalItems;
        public StartingPhysicalItem[] SurvivalModePhysicalItems;
        public StartingItem[] CreativeModeAmmoItems;
        public StartingItem[] SurvivalModeAmmoItems;
        public MyObjectBuilder_InventoryItem[] CreativeInventoryItems;
        public MyObjectBuilder_InventoryItem[] SurvivalInventoryItems;
        public MyObjectBuilder_Toolbar CreativeDefaultToolbar;
        public MyObjectBuilder_Toolbar SurvivalDefaultToolbar;
        public MyStringId MainCharacterModel;
        public DateTime GameDate;
        public Vector3 SunDirection;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_ScenarioDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_ScenarioDefinition;
            objectBuilder.AsteroidClusters.Enabled = this.AsteroidClustersEnabled;
            objectBuilder.AsteroidClusters.Offset = this.AsteroidClustersOffset;
            objectBuilder.AsteroidClusters.CentralCluster = this.CentralClusterEnabled;
            objectBuilder.DefaultEnvironment = this.DefaultEnvironment;
            objectBuilder.CreativeDefaultToolbar = this.CreativeDefaultToolbar;
            objectBuilder.SurvivalDefaultToolbar = this.SurvivalDefaultToolbar;
            objectBuilder.MainCharacterModel = this.MainCharacterModel.ToString();
            objectBuilder.GameDate = this.GameDate.Ticks;
            if ((this.PossiblePlayerStarts != null) && (this.PossiblePlayerStarts.Length != 0))
            {
                objectBuilder.PossibleStartingStates = new MyObjectBuilder_WorldGeneratorPlayerStartingState[this.PossiblePlayerStarts.Length];
                for (int i = 0; i < this.PossiblePlayerStarts.Length; i++)
                {
                    objectBuilder.PossibleStartingStates[i] = this.PossiblePlayerStarts[i].GetObjectBuilder();
                }
            }
            if ((this.WorldGeneratorOperations != null) && (this.WorldGeneratorOperations.Length != 0))
            {
                objectBuilder.WorldGeneratorOperations = new MyObjectBuilder_WorldGeneratorOperation[this.WorldGeneratorOperations.Length];
                for (int i = 0; i < this.WorldGeneratorOperations.Length; i++)
                {
                    objectBuilder.WorldGeneratorOperations[i] = this.WorldGeneratorOperations[i].GetObjectBuilder();
                }
            }
            if ((this.CreativeModeWeapons != null) && (this.CreativeModeWeapons.Length != 0))
            {
                objectBuilder.CreativeModeWeapons = new string[this.CreativeModeWeapons.Length];
                for (int i = 0; i < this.CreativeModeWeapons.Length; i++)
                {
                    objectBuilder.CreativeModeWeapons[i] = this.CreativeModeWeapons[i].ToString();
                }
            }
            if ((this.SurvivalModeWeapons != null) && (this.SurvivalModeWeapons.Length != 0))
            {
                objectBuilder.SurvivalModeWeapons = new string[this.SurvivalModeWeapons.Length];
                for (int i = 0; i < this.SurvivalModeWeapons.Length; i++)
                {
                    objectBuilder.SurvivalModeWeapons[i] = this.SurvivalModeWeapons[i].ToString();
                }
            }
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            BoundingBoxD? nullable1;
            base.Init(builder);
            MyObjectBuilder_ScenarioDefinition definition = (MyObjectBuilder_ScenarioDefinition) builder;
            this.GameDefinition = definition.GameDefinition;
            this.Environment = definition.EnvironmentDefinition;
            this.AsteroidClustersEnabled = definition.AsteroidClusters.Enabled;
            this.AsteroidClustersOffset = definition.AsteroidClusters.Offset;
            this.CentralClusterEnabled = definition.AsteroidClusters.CentralCluster;
            this.DefaultEnvironment = definition.DefaultEnvironment;
            this.CreativeDefaultToolbar = definition.CreativeDefaultToolbar;
            this.SurvivalDefaultToolbar = definition.SurvivalDefaultToolbar;
            this.MainCharacterModel = MyStringId.GetOrCompute(definition.MainCharacterModel);
            this.GameDate = new DateTime(definition.GameDate);
            this.SunDirection = (Vector3) definition.SunDirection;
            if ((definition.PossibleStartingStates != null) && (definition.PossibleStartingStates.Length != 0))
            {
                this.PossiblePlayerStarts = new MyWorldGeneratorStartingStateBase[definition.PossibleStartingStates.Length];
                for (int i = 0; i < definition.PossibleStartingStates.Length; i++)
                {
                    this.PossiblePlayerStarts[i] = MyWorldGenerator.StartingStateFactory.CreateInstance(definition.PossibleStartingStates[i]);
                }
            }
            if ((definition.WorldGeneratorOperations != null) && (definition.WorldGeneratorOperations.Length != 0))
            {
                this.WorldGeneratorOperations = new MyWorldGeneratorOperationBase[definition.WorldGeneratorOperations.Length];
                for (int i = 0; i < definition.WorldGeneratorOperations.Length; i++)
                {
                    this.WorldGeneratorOperations[i] = MyWorldGenerator.OperationFactory.CreateInstance(definition.WorldGeneratorOperations[i]);
                }
            }
            if ((definition.CreativeModeWeapons != null) && (definition.CreativeModeWeapons.Length != 0))
            {
                this.CreativeModeWeapons = new MyStringId[definition.CreativeModeWeapons.Length];
                for (int i = 0; i < definition.CreativeModeWeapons.Length; i++)
                {
                    this.CreativeModeWeapons[i] = MyStringId.GetOrCompute(definition.CreativeModeWeapons[i]);
                }
            }
            if ((definition.SurvivalModeWeapons != null) && (definition.SurvivalModeWeapons.Length != 0))
            {
                this.SurvivalModeWeapons = new MyStringId[definition.SurvivalModeWeapons.Length];
                for (int i = 0; i < definition.SurvivalModeWeapons.Length; i++)
                {
                    this.SurvivalModeWeapons[i] = MyStringId.GetOrCompute(definition.SurvivalModeWeapons[i]);
                }
            }
            if ((definition.CreativeModeComponents != null) && (definition.CreativeModeComponents.Length != 0))
            {
                this.CreativeModeComponents = new StartingItem[definition.CreativeModeComponents.Length];
                for (int i = 0; i < definition.CreativeModeComponents.Length; i++)
                {
                    this.CreativeModeComponents[i].amount = (MyFixedPoint) definition.CreativeModeComponents[i].amount;
                    this.CreativeModeComponents[i].itemName = MyStringId.GetOrCompute(definition.CreativeModeComponents[i].itemName);
                }
            }
            if ((definition.SurvivalModeComponents != null) && (definition.SurvivalModeComponents.Length != 0))
            {
                this.SurvivalModeComponents = new StartingItem[definition.SurvivalModeComponents.Length];
                for (int i = 0; i < definition.SurvivalModeComponents.Length; i++)
                {
                    this.SurvivalModeComponents[i].amount = (MyFixedPoint) definition.SurvivalModeComponents[i].amount;
                    this.SurvivalModeComponents[i].itemName = MyStringId.GetOrCompute(definition.SurvivalModeComponents[i].itemName);
                }
            }
            if ((definition.CreativeModePhysicalItems != null) && (definition.CreativeModePhysicalItems.Length != 0))
            {
                this.CreativeModePhysicalItems = new StartingPhysicalItem[definition.CreativeModePhysicalItems.Length];
                for (int i = 0; i < definition.CreativeModePhysicalItems.Length; i++)
                {
                    this.CreativeModePhysicalItems[i].amount = (MyFixedPoint) definition.CreativeModePhysicalItems[i].amount;
                    this.CreativeModePhysicalItems[i].itemName = MyStringId.GetOrCompute(definition.CreativeModePhysicalItems[i].itemName);
                    this.CreativeModePhysicalItems[i].itemType = MyStringId.GetOrCompute(definition.CreativeModePhysicalItems[i].itemType);
                }
            }
            if ((definition.SurvivalModePhysicalItems != null) && (definition.SurvivalModePhysicalItems.Length != 0))
            {
                this.SurvivalModePhysicalItems = new StartingPhysicalItem[definition.SurvivalModePhysicalItems.Length];
                for (int i = 0; i < definition.SurvivalModePhysicalItems.Length; i++)
                {
                    this.SurvivalModePhysicalItems[i].amount = (MyFixedPoint) definition.SurvivalModePhysicalItems[i].amount;
                    this.SurvivalModePhysicalItems[i].itemName = MyStringId.GetOrCompute(definition.SurvivalModePhysicalItems[i].itemName);
                    this.SurvivalModePhysicalItems[i].itemType = MyStringId.GetOrCompute(definition.SurvivalModePhysicalItems[i].itemType);
                }
            }
            if ((definition.CreativeModeAmmoItems != null) && (definition.CreativeModeAmmoItems.Length != 0))
            {
                this.CreativeModeAmmoItems = new StartingItem[definition.CreativeModeAmmoItems.Length];
                for (int i = 0; i < definition.CreativeModeAmmoItems.Length; i++)
                {
                    this.CreativeModeAmmoItems[i].amount = (MyFixedPoint) definition.CreativeModeAmmoItems[i].amount;
                    this.CreativeModeAmmoItems[i].itemName = MyStringId.GetOrCompute(definition.CreativeModeAmmoItems[i].itemName);
                }
            }
            if ((definition.SurvivalModeAmmoItems != null) && (definition.SurvivalModeAmmoItems.Length != 0))
            {
                this.SurvivalModeAmmoItems = new StartingItem[definition.SurvivalModeAmmoItems.Length];
                for (int i = 0; i < definition.SurvivalModeAmmoItems.Length; i++)
                {
                    this.SurvivalModeAmmoItems[i].amount = (MyFixedPoint) definition.SurvivalModeAmmoItems[i].amount;
                    this.SurvivalModeAmmoItems[i].itemName = MyStringId.GetOrCompute(definition.SurvivalModeAmmoItems[i].itemName);
                }
            }
            this.CreativeInventoryItems = definition.CreativeInventoryItems;
            this.SurvivalInventoryItems = definition.SurvivalInventoryItems;
            SerializableBoundingBoxD? worldBoundaries = definition.WorldBoundaries;
            if (worldBoundaries != null)
            {
                nullable1 = new BoundingBoxD?(worldBoundaries.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            this.WorldBoundaries = nullable1;
        }

        public bool HasPlanets =>
            ((this.WorldGeneratorOperations != null) && this.WorldGeneratorOperations.Any<MyWorldGeneratorOperationBase>(s => ((s is MyWorldGenerator.OperationAddPlanetPrefab) || (s is MyWorldGenerator.OperationCreatePlanet))));

        public MyObjectBuilder_Toolbar DefaultToolbar =>
            (MySession.Static.CreativeMode ? this.CreativeDefaultToolbar : this.SurvivalDefaultToolbar);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyScenarioDefinition.<>c <>9 = new MyScenarioDefinition.<>c();
            public static Func<MyWorldGeneratorOperationBase, bool> <>9__27_0;

            internal bool <get_HasPlanets>b__27_0(MyWorldGeneratorOperationBase s) => 
                ((s is MyWorldGenerator.OperationAddPlanetPrefab) || (s is MyWorldGenerator.OperationCreatePlanet));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartingItem
        {
            public MyFixedPoint amount;
            public MyStringId itemName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartingPhysicalItem
        {
            public MyFixedPoint amount;
            public MyStringId itemName;
            public MyStringId itemType;
        }
    }
}

