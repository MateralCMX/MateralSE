namespace Sandbox.Game.AI.Navigation
{
    using Havok;
    using Sandbox.Engine.Physics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using VRageMath;

    public class MyTreeAvoidance : MySteeringBase
    {
        private List<HkBodyCollision> m_trees;

        public MyTreeAvoidance(MyBotNavigation navigation, float weight) : base(navigation, weight)
        {
            this.m_trees = new List<HkBodyCollision>();
        }

        public override void AccumulateCorrection(ref Vector3 correction, ref float weight)
        {
            if (base.Parent.Speed >= 0.01)
            {
                MatrixD positionAndOrientation = base.Parent.PositionAndOrientation;
                Vector3D translation = positionAndOrientation.Translation;
                Quaternion identity = Quaternion.Identity;
                MyPhysics.GetPenetrationsShape((HkShape) new HkSphereShape(6f), ref translation, ref identity, this.m_trees, 9);
                foreach (HkBodyCollision collision in this.m_trees)
                {
                    if (collision.Body == null)
                    {
                        continue;
                    }
                    MyPhysicsBody userObject = collision.Body.UserObject as MyPhysicsBody;
                    if (userObject != null)
                    {
                        HkShape shape = collision.Body.GetShape();
                        if (shape.ShapeType == HkShapeType.StaticCompound)
                        {
                            int num;
                            uint num2;
                            HkStaticCompoundShape shape2 = (HkStaticCompoundShape) shape;
                            shape2.DecomposeShapeKey(collision.ShapeKey, out num, out num2);
                            Vector3D vectord2 = shape2.GetInstanceTransform(num).Translation + userObject.GetWorldMatrix().Translation;
                            Vector3D direction = vectord2 - translation;
                            double num3 = direction.Normalize();
                            direction = Vector3D.Reject(base.Parent.ForwardVector, direction);
                            direction.Y = 0.0;
                            if (((direction.Z * direction.Z) + (direction.X * direction.X)) < 0.1)
                            {
                                direction = translation - vectord2;
                                direction = Vector3D.Cross(Vector3D.Up, direction);
                                if (Vector3D.TransformNormal(direction, base.Parent.PositionAndOrientationInverted).X < 0.0)
                                {
                                    direction = -direction;
                                }
                            }
                            direction.Normalize();
                            correction += ((6.0 - num3) * base.Weight) * direction;
                            if (!correction.IsValid())
                            {
                                Debugger.Break();
                            }
                        }
                    }
                }
                this.m_trees.Clear();
                weight += base.Weight;
            }
        }

        public override string GetName() => 
            "Tree avoidance steering";
    }
}

