namespace VRage.Game.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public static class MyObjectBuilderExtensions
    {
        public static bool HasPlanets(this MyObjectBuilder_ScenarioDefinition scenario)
        {
            if (scenario.WorldGeneratorOperations != null)
            {
                foreach (MyObjectBuilder_WorldGeneratorOperation operation in scenario.WorldGeneratorOperations)
                {
                    if (operation is MyObjectBuilder_WorldGeneratorOperation_CreatePlanet)
                    {
                        return true;
                    }
                    if (operation is MyObjectBuilder_WorldGeneratorOperation_AddPlanetPrefab)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasPlanets(this MyObjectBuilder_Sector sector)
        {
            if (sector.SectorObjects != null)
            {
                using (List<MyObjectBuilder_EntityBase>.Enumerator enumerator = sector.SectorObjects.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current is MyObjectBuilder_Planet)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

