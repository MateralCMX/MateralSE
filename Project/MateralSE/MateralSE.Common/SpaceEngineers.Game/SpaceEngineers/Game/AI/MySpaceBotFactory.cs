namespace SpaceEngineers.Game.AI
{
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MySpaceBotFactory : MyBotFactoryBase
    {
        public override bool CanCreateBotOfType(string behaviorType, bool load) => 
            true;

        public override bool GetBotGroupSpawnPositions(string behaviorType, int count, List<Vector3D> spawnPositions)
        {
            throw new NotImplementedException();
        }

        public override bool GetBotSpawnPosition(string behaviorType, out Vector3D spawnPosition)
        {
            if (behaviorType == "Spider")
            {
                MatrixD xd;
                Vector3D? oldPosition = null;
                bool spiderSpawnPosition = GetSpiderSpawnPosition(out xd, oldPosition);
                spawnPosition = xd.Translation;
                return spiderSpawnPosition;
            }
            if (MySession.Static.LocalCharacter == null)
            {
                spawnPosition = Vector3D.Zero;
                return false;
            }
            Vector3D position = MySession.Static.LocalCharacter.PositionComp.GetPosition();
            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            vector = (vector.LengthSquared() >= 0.0001f) ? ((Vector3) Vector3D.Normalize(vector)) : Vector3.Up;
            Vector3D tangent = Vector3.CalculatePerpendicularVector(vector);
            Vector3D bitangent = Vector3.Cross((Vector3) tangent, vector);
            spawnPosition = MyUtils.GetRandomDiscPosition(ref position, 5.0, ref tangent, ref bitangent);
            return true;
        }

        public static MyPlanetAnimalSpawnInfo GetDayOrNightAnimalSpawnInfo(MyPlanet planet, Vector3D position)
        {
            if (planet == null)
            {
                return null;
            }
            if (((planet.Generator.NightAnimalSpawnInfo != null) && ((planet.Generator.NightAnimalSpawnInfo.Animals != null) && (planet.Generator.NightAnimalSpawnInfo.Animals.Length != 0))) && MySectorWeatherComponent.IsThereNight(planet, ref position))
            {
                return planet.Generator.NightAnimalSpawnInfo;
            }
            if (((planet.Generator.AnimalSpawnInfo == null) || (planet.Generator.AnimalSpawnInfo.Animals == null)) || (planet.Generator.AnimalSpawnInfo.Animals.Length == 0))
            {
                return null;
            }
            return planet.Generator.AnimalSpawnInfo;
        }

        public static bool GetSpiderSpawnPosition(out MatrixD spawnPosition, Vector3D? oldPosition)
        {
            Vector3D vectord2;
            spawnPosition = MatrixD.Identity;
            Vector3D? nullable = null;
            MyPlanet closestPlanet = null;
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                if (player.Id.SerialId != 0)
                {
                    continue;
                }
                if (player.Character != null)
                {
                    nullable = new Vector3D?(player.GetPosition());
                    closestPlanet = MyGamePruningStructure.GetClosestPlanet(nullable.Value);
                    MyPlanetAnimalSpawnInfo dayOrNightAnimalSpawnInfo = GetDayOrNightAnimalSpawnInfo(closestPlanet, nullable.Value);
                    if (((dayOrNightAnimalSpawnInfo == null) || (dayOrNightAnimalSpawnInfo.Animals == null)) || !dayOrNightAnimalSpawnInfo.Animals.Any<MyPlanetAnimal>(x => x.AnimalType.Contains("Spider")))
                    {
                        nullable = null;
                        closestPlanet = null;
                        continue;
                    }
                    if ((oldPosition == null) || ReferenceEquals(closestPlanet, MyGamePruningStructure.GetClosestPlanet(oldPosition.Value)))
                    {
                        break;
                    }
                    nullable = null;
                    closestPlanet = null;
                }
            }
            if ((nullable == null) || (closestPlanet == null))
            {
                return false;
            }
            Vector3D worldGravity = closestPlanet.Components.Get<MyGravityProviderComponent>().GetWorldGravity(nullable.Value);
            if (Vector3D.IsZero(worldGravity))
            {
                worldGravity = Vector3D.Down;
            }
            else
            {
                worldGravity.Normalize();
            }
            worldGravity.CalculatePerpendicularVector(out vectord2);
            Vector3D bitangent = Vector3D.Cross(worldGravity, vectord2);
            Vector3D globalPos = MyUtils.GetRandomDiscPosition(ref nullable.Value, 20.0, ref vectord2, ref bitangent) - (worldGravity * 500.0);
            Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref globalPos);
            Vector3D vectord6 = nullable.Value - closestSurfacePointGlobal;
            if (!Vector3D.IsZero(vectord6))
            {
                vectord6.Normalize();
            }
            else
            {
                vectord6 = Vector3D.CalculatePerpendicularVector(worldGravity);
            }
            spawnPosition = MatrixD.CreateWorld(closestSurfacePointGlobal, vectord6, -worldGravity);
            return true;
        }

        public override int MaximumUncontrolledBotCount =>
            10;

        public override int MaximumBotPerPlayer =>
            0x20;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySpaceBotFactory.<>c <>9 = new MySpaceBotFactory.<>c();
            public static Func<MyPlanetAnimal, bool> <>9__6_0;

            internal bool <GetSpiderSpawnPosition>b__6_0(MyPlanetAnimal x) => 
                x.AnimalType.Contains("Spider");
        }
    }
}

