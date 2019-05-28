namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    public static class MyDetectedEntityInfoHelper
    {
        public static MyDetectedEntityInfo Create(MyEntity entity, long sensorOwner, Vector3D? hitPosition = new Vector3D?())
        {
            MyDetectedEntityType type;
            MyRelationsBetweenPlayerAndBlock neutral;
            string displayName;
            if (entity == null)
            {
                return new MyDetectedEntityInfo();
            }
            MatrixD zero = MatrixD.Zero;
            Vector3 velocity = (Vector3) Vector3D.Zero;
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
            if (entity.Physics != null)
            {
                zero = entity.Physics.GetWorldMatrix().GetOrientation();
                velocity = entity.Physics.LinearVelocity;
            }
            MyCubeGrid topMostParent = entity.GetTopMostParent(null) as MyCubeGrid;
            if (topMostParent != null)
            {
                type = (topMostParent.GridSizeEnum != MyCubeSize.Small) ? MyDetectedEntityType.LargeGrid : MyDetectedEntityType.SmallGrid;
                neutral = (topMostParent.BigOwners.Count != 0) ? MyIDModule.GetRelation(sensorOwner, topMostParent.BigOwners[0], MyOwnershipShareModeEnum.Faction, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare) : MyRelationsBetweenPlayerAndBlock.NoOwnership;
                if ((neutral == MyRelationsBetweenPlayerAndBlock.Owner) || (neutral == MyRelationsBetweenPlayerAndBlock.FactionShare))
                {
                    displayName = topMostParent.DisplayName;
                }
                else
                {
                    displayName = (topMostParent.GridSizeEnum != MyCubeSize.Small) ? MyTexts.GetString(MySpaceTexts.DetectedEntity_LargeGrid) : MyTexts.GetString(MySpaceTexts.DetectedEntity_SmallGrid);
                }
                zero = topMostParent.WorldMatrix.GetOrientation();
                return new MyDetectedEntityInfo(topMostParent.EntityId, displayName, type, hitPosition, zero, topMostParent.Physics.LinearVelocity, neutral, worldAABB, (long) totalGamePlayTimeInMilliseconds);
            }
            MyCharacter character = entity as MyCharacter;
            if (character != null)
            {
                type = !character.IsPlayer ? MyDetectedEntityType.CharacterOther : MyDetectedEntityType.CharacterHuman;
                neutral = MyIDModule.GetRelation(sensorOwner, character.GetPlayerIdentityId(), MyOwnershipShareModeEnum.Faction, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare);
                if ((neutral == MyRelationsBetweenPlayerAndBlock.Owner) || (neutral == MyRelationsBetweenPlayerAndBlock.FactionShare))
                {
                    displayName = character.DisplayNameText;
                }
                else
                {
                    displayName = !character.IsPlayer ? MyTexts.GetString(MySpaceTexts.DetectedEntity_CharacterOther) : MyTexts.GetString(MySpaceTexts.DetectedEntity_CharacterHuman);
                }
                return new MyDetectedEntityInfo(entity.EntityId, displayName, type, hitPosition, zero, velocity, neutral, character.Model.BoundingBox.Transform(character.WorldMatrix), (long) totalGamePlayTimeInMilliseconds);
            }
            neutral = MyRelationsBetweenPlayerAndBlock.Neutral;
            MyFloatingObject obj2 = entity as MyFloatingObject;
            if (obj2 != null)
            {
                displayName = obj2.Item.Content.SubtypeName;
                return new MyDetectedEntityInfo(entity.EntityId, displayName, MyDetectedEntityType.FloatingObject, hitPosition, zero, velocity, neutral, worldAABB, (long) totalGamePlayTimeInMilliseconds);
            }
            MyInventoryBagEntity entity2 = entity as MyInventoryBagEntity;
            if (entity2 != null)
            {
                displayName = entity2.DisplayName;
                return new MyDetectedEntityInfo(entity.EntityId, displayName, MyDetectedEntityType.FloatingObject, hitPosition, zero, velocity, neutral, worldAABB, (long) totalGamePlayTimeInMilliseconds);
            }
            MyPlanet planet = entity as MyPlanet;
            if (planet != null)
            {
                displayName = MyTexts.GetString(MySpaceTexts.DetectedEntity_Planet);
                return new MyDetectedEntityInfo(entity.EntityId, displayName, MyDetectedEntityType.Planet, hitPosition, zero, velocity, neutral, BoundingBoxD.CreateFromSphere(new BoundingSphereD(planet.PositionComp.GetPosition(), (double) planet.MaximumRadius)), (long) totalGamePlayTimeInMilliseconds);
            }
            MyVoxelPhysics physics = entity as MyVoxelPhysics;
            if (physics != null)
            {
                displayName = MyTexts.GetString(MySpaceTexts.DetectedEntity_Planet);
                return new MyDetectedEntityInfo(entity.EntityId, displayName, MyDetectedEntityType.Planet, hitPosition, zero, velocity, neutral, BoundingBoxD.CreateFromSphere(new BoundingSphereD(physics.Parent.PositionComp.GetPosition(), (double) physics.Parent.MaximumRadius)), (long) totalGamePlayTimeInMilliseconds);
            }
            if (entity is MyVoxelMap)
            {
                displayName = MyTexts.GetString(MySpaceTexts.DetectedEntity_Asteroid);
                return new MyDetectedEntityInfo(entity.EntityId, displayName, MyDetectedEntityType.Asteroid, hitPosition, zero, velocity, neutral, worldAABB, (long) totalGamePlayTimeInMilliseconds);
            }
            if (entity is MyMeteor)
            {
                displayName = MyTexts.GetString(MySpaceTexts.DetectedEntity_Meteor);
                return new MyDetectedEntityInfo(entity.EntityId, displayName, MyDetectedEntityType.Meteor, hitPosition, zero, velocity, neutral, worldAABB, (long) totalGamePlayTimeInMilliseconds);
            }
            if (entity is MyMissile)
            {
                return new MyDetectedEntityInfo(entity.EntityId, entity.DisplayName, MyDetectedEntityType.Missile, hitPosition, zero, velocity, neutral, worldAABB, (long) totalGamePlayTimeInMilliseconds);
            }
            Vector3D? nullable = null;
            MatrixD orientation = new MatrixD();
            Vector3 vector2 = new Vector3();
            return new MyDetectedEntityInfo(0L, string.Empty, MyDetectedEntityType.Unknown, nullable, orientation, vector2, MyRelationsBetweenPlayerAndBlock.NoOwnership, new BoundingBoxD(), (long) MySandboxGame.TotalGamePlayTimeInMilliseconds);
        }
    }
}

