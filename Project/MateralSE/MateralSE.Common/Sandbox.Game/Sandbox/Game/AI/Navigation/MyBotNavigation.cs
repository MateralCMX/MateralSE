namespace Sandbox.Game.AI.Navigation
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;

    public class MyBotNavigation
    {
        private List<MySteeringBase> m_steerings = new List<MySteeringBase>();
        private MyPathSteering m_path;
        private MyBotAiming m_aiming;
        private MyEntity m_entity;
        private MyDestinationSphere m_destinationSphere;
        private Vector3 m_forwardVector;
        private Vector3 m_correction;
        private Vector3 m_upVector;
        private float m_speed;
        private bool m_wasStopped;
        private float m_rotationSpeedModifier;
        private Vector3 m_gravityDirection;
        private float? m_maximumRotationAngle;
        private MyStuckDetection m_stuckDetection;
        private MatrixD m_worldMatrix;
        private MatrixD m_invWorldMatrix;
        private MatrixD m_aimingPositionAndOrientation;
        private MatrixD m_invAimingPositionAndOrientation;

        public MyBotNavigation()
        {
            this.m_path = new MyPathSteering(this);
            this.m_steerings.Add(this.m_path);
            this.m_aiming = new MyBotAiming(this);
            this.m_stuckDetection = new MyStuckDetection(0.05f, MathHelper.ToRadians((float) 2f));
            this.m_destinationSphere = new MyDestinationSphere(ref Vector3D.Zero, 0f);
            this.m_wasStopped = false;
        }

        private void AccumulateCorrection()
        {
            this.m_rotationSpeedModifier = 1f;
            float weight = 0f;
            for (int i = 0; i < this.m_steerings.Count; i++)
            {
                this.m_steerings[i].AccumulateCorrection(ref this.m_correction, ref weight);
            }
            if (this.m_maximumRotationAngle != null)
            {
                double num4 = Vector3D.Dot(Vector3D.Normalize(this.m_forwardVector - this.m_correction), this.m_forwardVector);
                if (num4 < Math.Cos((double) this.m_maximumRotationAngle.Value))
                {
                    float num5 = (float) Math.Acos(MathHelper.Clamp(num4, -1.0, 1.0));
                    this.m_rotationSpeedModifier = num5 / this.m_maximumRotationAngle.Value;
                    this.m_correction /= this.m_rotationSpeedModifier;
                }
            }
            if (weight > 1f)
            {
                this.m_correction /= weight;
            }
        }

        public void AddSteering(MySteeringBase steering)
        {
            this.m_steerings.Add(steering);
        }

        public void AimAt(MyEntity entity, Vector3D? worldPosition = new Vector3D?())
        {
            if (worldPosition == null)
            {
                Vector3? relativeTarget = null;
                this.m_aiming.SetTarget(entity, relativeTarget);
            }
            else if (entity == null)
            {
                this.m_aiming.SetAbsoluteTarget(worldPosition.Value);
            }
            else
            {
                MatrixD worldMatrixNormalizedInv = entity.PositionComp.WorldMatrixNormalizedInv;
                Vector3 vector = (Vector3) Vector3D.Transform(worldPosition.Value, worldMatrixNormalizedInv);
                this.m_aiming.SetTarget(entity, new Vector3?(vector));
            }
        }

        public void AimWithMovement()
        {
            this.m_aiming.FollowMovement();
        }

        [Conditional("DEBUG")]
        private void AssertIsValid()
        {
        }

        public void ChangeEntity(IMyControllableEntity newEntity)
        {
            this.m_entity = newEntity?.Entity;
            if (this.m_entity != null)
            {
                this.m_forwardVector = (Vector3) this.PositionAndOrientation.Forward;
                this.m_upVector = (Vector3) this.PositionAndOrientation.Up;
                this.m_speed = 0f;
                this.m_rotationSpeedModifier = 1f;
            }
        }

        public bool CheckReachability(Vector3D worldPosition, float threshold, MyEntity relativeEntity = null)
        {
            if (MyAIComponent.Static.Pathfinding == null)
            {
                return false;
            }
            this.m_destinationSphere.Init(ref worldPosition, 0f);
            return MyAIComponent.Static.Pathfinding.ReachableUnderThreshold(this.PositionAndOrientation.Translation, this.m_destinationSphere, threshold);
        }

        public void Cleanup()
        {
            using (List<MySteeringBase>.Enumerator enumerator = this.m_steerings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Cleanup();
                }
            }
        }

        private void CorrectMovement(Vector3 rotationHint)
        {
            this.m_correction = Vector3.Zero;
            if (!this.Navigating)
            {
                this.m_speed = 0f;
            }
            else
            {
                this.AccumulateCorrection();
                if (!this.HasRotation(10f))
                {
                    this.m_stuckDetection.SetRotating(false);
                }
                else
                {
                    this.m_correction = Vector3.Zero;
                    this.m_speed = 0f;
                    this.m_stuckDetection.SetRotating(true);
                }
                Vector3 vector = (this.m_forwardVector * this.m_speed) + this.m_correction;
                this.m_speed = vector.Length();
                if (this.m_speed <= 0.001f)
                {
                    this.m_speed = 0f;
                }
                else
                {
                    this.m_forwardVector = vector / this.m_speed;
                    if (this.m_speed > 1f)
                    {
                        this.m_speed = 1f;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        public void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                this.m_aiming.DebugDraw(this.m_aimingPositionAndOrientation);
                if (MyDebugDrawSettings.DEBUG_DRAW_BOT_STEERING)
                {
                    foreach (MySteeringBase local1 in this.m_steerings)
                    {
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_BOT_NAVIGATION)
                {
                    Vector3 translation = (Vector3) this.PositionAndOrientation.Translation;
                    Vector3.Cross(this.m_forwardVector, this.UpVector);
                    if (this.Stuck)
                    {
                        MyRenderProxy.DebugDrawSphere(translation, 1f, Color.Red.ToVector3(), 1f, false, false, true, false);
                    }
                    MyRenderProxy.DebugDrawArrow3D(translation, translation + this.ForwardVector, Color.Blue, new Color?(Color.Blue), false, 0.1, "Nav. FW", 0.5f, false);
                    MyRenderProxy.DebugDrawArrow3D(translation + this.ForwardVector, (translation + this.ForwardVector) + this.m_correction, Color.LightBlue, new Color?(Color.LightBlue), false, 0.1, "Correction", 0.5f, false);
                    if (this.m_destinationSphere != null)
                    {
                        this.m_destinationSphere.DebugDraw();
                    }
                    MyCharacter botEntity = this.BotEntity as MyCharacter;
                    if (botEntity != null)
                    {
                        MatrixD matrix = MatrixD.Invert(botEntity.GetViewMatrix());
                        MatrixD xd3 = botEntity.GetHeadMatrix(true, true, false, false, false);
                        MyRenderProxy.DebugDrawLine3D(matrix.Translation, Vector3D.Transform(Vector3D.Forward * 50.0, matrix), Color.Yellow, Color.White, false, false);
                        MyRenderProxy.DebugDrawLine3D(xd3.Translation, Vector3D.Transform(Vector3D.Forward * 50.0, xd3), Color.Red, Color.Red, false, false);
                        if (botEntity.CurrentWeapon != null)
                        {
                            Vector3 vector2 = botEntity.CurrentWeapon.DirectionToTarget(botEntity.AimedPoint);
                            MyRenderProxy.DebugDrawSphere(botEntity.AimedPoint, 1f, Color.Yellow, 1f, false, false, true, false);
                            Vector3D pointFrom = (botEntity.CurrentWeapon as MyEntity).WorldMatrix.Translation;
                            MyRenderProxy.DebugDrawLine3D(pointFrom, (botEntity.CurrentWeapon as MyEntity).WorldMatrix.Translation + (vector2 * 20f), Color.Purple, Color.Purple, false, false);
                        }
                    }
                }
            }
        }

        public void Flyto(Vector3D worldPosition, MyEntity relativeEntity = null)
        {
            this.m_path.SetTarget(worldPosition, 1f, relativeEntity, 1f, true);
            this.m_stuckDetection.Reset(false);
        }

        public void FollowPath(IMyPath path)
        {
            this.m_path.SetPath(path, 1f);
            this.m_stuckDetection.Reset(false);
        }

        public void Goto(IMyDestinationShape destination, MyEntity relativeEntity = null)
        {
            if (MyAIComponent.Static.Pathfinding != null)
            {
                IMyPath path = MyAIComponent.Static.Pathfinding.FindPathGlobal(this.PositionAndOrientation.Translation, destination, relativeEntity);
                if (path != null)
                {
                    this.m_path.SetPath(path, 1f);
                    this.m_stuckDetection.Reset(false);
                }
            }
        }

        public void Goto(Vector3D position, float radius = 0f, MyEntity relativeEntity = null)
        {
            this.m_destinationSphere.Init(ref position, radius);
            this.Goto(this.m_destinationSphere, relativeEntity);
        }

        public void GotoNoPath(Vector3D worldPosition, float radius = 0f, MyEntity relativeEntity = null, bool resetStuckDetection = true)
        {
            this.m_path.SetTarget(worldPosition, radius, relativeEntity, 1f, false);
            if (resetStuckDetection)
            {
                this.m_stuckDetection.Reset(false);
            }
        }

        public bool HasRotation(float epsilon = 0.0316f) => 
            (this.m_aiming.RotationHint.LengthSquared() > (epsilon * epsilon));

        public bool HasSteeringOfType(Type steeringType)
        {
            using (List<MySteeringBase>.Enumerator enumerator = this.m_steerings.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.GetType() == steeringType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasXRotation(float epsilon) => 
            (Math.Abs(this.m_aiming.RotationHint.Y) > epsilon);

        public bool HasYRotation(float epsilon) => 
            (Math.Abs(this.m_aiming.RotationHint.X) > epsilon);

        private void MoveCharacter()
        {
            MyCharacter entity = this.m_entity as MyCharacter;
            if (entity != null)
            {
                if (this.m_speed != 0f)
                {
                    MyCharacterJetpackComponent jetpackComp = entity.JetpackComp;
                    if (((jetpackComp != null) && !jetpackComp.TurnedOn) && this.m_path.Flying)
                    {
                        jetpackComp.TurnOnJetpack(true, false, false);
                    }
                    else if (((jetpackComp != null) && jetpackComp.TurnedOn) && !this.m_path.Flying)
                    {
                        jetpackComp.TurnOnJetpack(false, false, false);
                    }
                    Vector3 vector = Vector3.TransformNormal(this.m_forwardVector, entity.PositionComp.WorldMatrixNormalizedInv);
                    Vector3 vector2 = this.m_aiming.RotationHint * this.m_rotationSpeedModifier;
                    if (this.m_path.Flying)
                    {
                        if (vector.Y > 0f)
                        {
                            entity.Up();
                        }
                        else
                        {
                            entity.Down();
                        }
                    }
                    entity.MoveAndRotate(vector * this.m_speed, new Vector2(vector2.Y * 30f, vector2.X * 30f), 0f);
                }
                else if (this.m_speed == 0f)
                {
                    if (!this.HasRotation(0.0316f))
                    {
                        if (this.m_wasStopped)
                        {
                            entity.MoveAndRotate(Vector3.Zero, Vector2.Zero, 0f);
                            this.m_wasStopped = true;
                        }
                    }
                    else
                    {
                        int num1;
                        if (entity.WantsWalk || entity.IsCrouching)
                        {
                            num1 = 1;
                        }
                        else
                        {
                            num1 = 2;
                        }
                        float num = num1;
                        Vector3 vector3 = this.m_aiming.RotationHint * this.m_rotationSpeedModifier;
                        entity.MoveAndRotate(Vector3.Zero, new Vector2((vector3.Y * 20f) * num, (vector3.X * 25f) * num), 0f);
                        this.m_wasStopped = false;
                    }
                }
            }
            Vector3D targetLocation = new Vector3D();
            this.m_stuckDetection.Update(this.m_worldMatrix.Translation, this.m_aiming.RotationHint, targetLocation);
        }

        public void RemoveSteering(MySteeringBase steering)
        {
            this.m_steerings.Remove(steering);
        }

        public void Stop()
        {
            this.m_path.UnsetPath();
            this.m_stuckDetection.Stop();
        }

        public void StopAiming()
        {
            this.m_aiming.StopAiming();
        }

        public void StopImmediate(bool forceUpdate = false)
        {
            this.Stop();
            this.m_speed = 0f;
            if (forceUpdate)
            {
                this.MoveCharacter();
            }
        }

        public void Update(int behaviorTicks)
        {
            this.m_stuckDetection.SetCurrentTicks(behaviorTicks);
            if (this.m_entity != null)
            {
                this.UpdateMatrices();
                this.m_gravityDirection = MyGravityProviderSystem.CalculateTotalGravityInPoint(this.m_entity.PositionComp.WorldMatrix.Translation);
                if (!Vector3.IsZero(this.m_gravityDirection, 0.01f))
                {
                    this.m_gravityDirection = (Vector3) Vector3D.Normalize(this.m_gravityDirection);
                }
                this.m_upVector = !MyPerGameSettings.NavmeshPresumesDownwardGravity ? -this.m_gravityDirection : Vector3.Up;
                if (!this.m_speed.IsValid())
                {
                    this.m_forwardVector = (Vector3) this.PositionAndOrientation.Forward;
                    this.m_speed = 0f;
                    this.m_rotationSpeedModifier = 1f;
                }
                using (List<MySteeringBase>.Enumerator enumerator = this.m_steerings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Update();
                    }
                }
                this.m_aiming.Update();
                this.CorrectMovement(this.m_aiming.RotationHint);
                if (this.m_speed < 0.1f)
                {
                    this.m_speed = 0f;
                }
                this.MoveCharacter();
            }
        }

        private void UpdateMatrices()
        {
            if (!(this.m_entity is MyCharacter))
            {
                this.m_worldMatrix = this.m_entity.PositionComp.WorldMatrix;
                this.m_invWorldMatrix = this.m_entity.PositionComp.WorldMatrixInvScaled;
                this.m_aimingPositionAndOrientation = this.m_worldMatrix;
                this.m_invAimingPositionAndOrientation = this.m_invWorldMatrix;
            }
            else
            {
                MyCharacter entity = this.m_entity as MyCharacter;
                this.m_worldMatrix = entity.WorldMatrix;
                this.m_invWorldMatrix = Matrix.Invert((Matrix) this.m_worldMatrix);
                this.m_aimingPositionAndOrientation = entity.GetHeadMatrix(true, true, false, true, false);
                this.m_invAimingPositionAndOrientation = MatrixD.Invert(this.m_aimingPositionAndOrientation);
            }
        }

        public Vector3 ForwardVector =>
            this.m_forwardVector;

        public Vector3 UpVector =>
            this.m_upVector;

        public float Speed =>
            this.m_speed;

        public bool Navigating =>
            this.m_path.TargetSet;

        public bool Stuck =>
            this.m_stuckDetection.IsStuck;

        public Vector3D TargetPoint =>
            this.m_destinationSphere.GetDestination();

        public MyEntity BotEntity =>
            this.m_entity;

        public float? MaximumRotationAngle
        {
            get => 
                this.m_maximumRotationAngle;
            set => 
                (this.m_maximumRotationAngle = value);
        }

        public Vector3 GravityDirection =>
            this.m_gravityDirection;

        public MatrixD PositionAndOrientation =>
            ((this.m_entity != null) ? this.m_worldMatrix : MatrixD.Identity);

        public MatrixD PositionAndOrientationInverted =>
            ((this.m_entity != null) ? this.m_invWorldMatrix : MatrixD.Identity);

        public MatrixD AimingPositionAndOrientation =>
            ((this.m_entity != null) ? this.m_aimingPositionAndOrientation : MatrixD.Identity);

        public MatrixD AimingPositionAndOrientationInverted =>
            ((this.m_entity != null) ? this.m_invAimingPositionAndOrientation : MatrixD.Identity);
    }
}

