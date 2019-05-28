namespace Sandbox.Game.AI.Navigation
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using VRageMath;

    public class MyCharacterAvoidance : MySteeringBase
    {
        private Vector3D m_debugDirection;

        public MyCharacterAvoidance(MyBotNavigation botNavigation, float weight) : base(botNavigation, weight)
        {
            this.m_debugDirection = Vector3D.Forward;
        }

        public override void AccumulateCorrection(ref Vector3 correction, ref float weight)
        {
            if (base.Parent.Speed >= 0.01f)
            {
                MyCharacter botEntity = base.Parent.BotEntity as MyCharacter;
                if (botEntity != null)
                {
                    Vector3D translation = base.Parent.PositionAndOrientation.Translation;
                    BoundingBoxD boundingBox = new BoundingBoxD(translation - (Vector3D.One * 3.0), translation + (Vector3D.One * 3.0));
                    Vector3D forwardVector = base.Parent.ForwardVector;
                    List<MyEntity> entitiesInAABB = MyEntities.GetEntitiesInAABB(ref boundingBox, false);
                    foreach (MyCharacter character2 in entitiesInAABB)
                    {
                        if (character2 == null)
                        {
                            continue;
                        }
                        if (!ReferenceEquals(character2, botEntity) && (character2.ModelName != botEntity.ModelName))
                        {
                            Vector3D vectord3 = character2.PositionComp.GetPosition() - translation;
                            double num = MathHelper.Clamp(vectord3.Normalize(), 0.0, 6.0);
                            Vector3D vectord4 = -vectord3;
                            if (Vector3D.Dot(vectord3, forwardVector) > -0.807)
                            {
                                correction += ((6.0 - num) * base.Weight) * vectord4;
                            }
                            if (!correction.IsValid())
                            {
                                Debugger.Break();
                            }
                        }
                    }
                    entitiesInAABB.Clear();
                    weight += base.Weight;
                }
            }
        }

        public override void DebugDraw()
        {
        }

        public override string GetName() => 
            "Character avoidance steering";
    }
}

