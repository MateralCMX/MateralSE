namespace Sandbox.Game.AI
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI.Navigation;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Weapons;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyBotAiming
    {
        public const float MISSING_PROBABILITY = 0.3f;
        private MyBotNavigation m_parent;
        private AimingMode m_mode;
        private MyEntity m_aimTarget;
        private Vector3 m_rotationHint;
        private Vector3? m_relativeTarget;
        private Vector3 m_dbgDesiredForward;

        public MyBotAiming(MyBotNavigation parent)
        {
            this.m_parent = parent;
            this.m_mode = AimingMode.FOLLOW_MOVEMENT;
            this.m_rotationHint = Vector3.Zero;
        }

        private void AddErrorToAiming(MyCharacter character, float errorLenght)
        {
            if (MyUtils.GetRandomFloat() < 0.3f)
            {
                character.AimedPoint += Vector3D.Normalize(MyUtils.GetRandomVector3()) * errorLenght;
            }
        }

        private void CalculateRotationHint(ref MatrixD parentMatrix, ref Vector3 desiredForward)
        {
            Vector3D upVector = this.m_parent.UpVector;
            if (desiredForward.LengthSquared() == 0f)
            {
                this.m_rotationHint.X = this.m_rotationHint.Y = 0f;
            }
            else
            {
                Vector3D vectord2 = Vector3D.Reject(desiredForward, parentMatrix.Up);
                Vector3D vectord3 = Vector3D.Reject(desiredForward, parentMatrix.Right);
                vectord2.Normalize();
                vectord3.Normalize();
                this.m_dbgDesiredForward = desiredForward;
                double num = 0.0;
                double num2 = 0.0;
                num2 = Math.Acos(MathHelper.Clamp(Vector3D.Dot(parentMatrix.Forward, vectord3), -1.0, 1.0));
                if (Vector3D.Dot(desiredForward, upVector) > Vector3D.Dot(parentMatrix.Forward, upVector))
                {
                    num2 = -num2;
                }
                num = Math.Acos(MathHelper.Clamp(Vector3D.Dot(parentMatrix.Forward, vectord2), -1.0, 1.0));
                if (Vector3D.Dot(parentMatrix.Right, vectord2) < 0.0)
                {
                    num = -num;
                }
                this.m_rotationHint.X = MathHelper.Clamp((float) num, -3f, 3f);
                this.m_rotationHint.Y = MathHelper.Clamp((float) num2, -3f, 3f);
            }
        }

        public void DebugDraw(MatrixD posAndOri)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOT_AIMING)
            {
                Vector3 translation = (Vector3) posAndOri.Translation;
                MyRenderProxy.DebugDrawArrow3D(translation, translation + posAndOri.Right, Color.Red, new Color?(Color.Red), false, 0.1, "X", 0.5f, false);
                MyRenderProxy.DebugDrawArrow3D(translation, translation + posAndOri.Up, Color.Green, new Color?(Color.Green), false, 0.1, "Y", 0.5f, false);
                MyRenderProxy.DebugDrawArrow3D(translation, translation + posAndOri.Forward, Color.Blue, new Color?(Color.Blue), false, 0.1, "-Z", 0.5f, false);
                MyRenderProxy.DebugDrawArrow3D(translation, translation + this.m_dbgDesiredForward, Color.Yellow, new Color?(Color.Yellow), false, 0.1, "Des.-Z", 0.5f, false);
                Vector3 pointFrom = translation + posAndOri.Forward;
                MyRenderProxy.DebugDrawArrow3D(pointFrom, (translation + posAndOri.Forward) + ((this.m_rotationHint.X * 10f) * posAndOri.Right), Color.Salmon, new Color?(Color.Salmon), false, 0.1, "Rot.X", 0.5f, false);
                MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom - ((this.m_rotationHint.Y * 10f) * posAndOri.Up), Color.LimeGreen, new Color?(Color.LimeGreen), false, 0.1, "Rot.Y", 0.5f, false);
                MyCharacter botEntity = this.m_parent.BotEntity as MyCharacter;
                if (botEntity != null)
                {
                    MyRenderProxy.DebugDrawSphere(botEntity.AimedPoint, 0.2f, Color.Orange, 1f, false, false, true, false);
                }
            }
        }

        public void FollowMovement()
        {
            this.m_aimTarget = null;
            this.m_mode = AimingMode.FOLLOW_MOVEMENT;
            this.m_relativeTarget = null;
        }

        private void PredictTargetPosition(ref Vector3 transformedRelativeTarget, MyCharacter bot)
        {
            if ((bot != null) && (bot.CurrentWeapon != null))
            {
                MyGunBase gunBase = bot.CurrentWeapon.GunBase as MyGunBase;
                if (gunBase != null)
                {
                    float num;
                    MyWeaponPrediction.GetPredictedTargetPosition(gunBase, bot, this.m_aimTarget, out transformedRelativeTarget, out num, 0.1666667f);
                }
            }
        }

        public void SetAbsoluteTarget(Vector3 absoluteTarget)
        {
            this.m_mode = AimingMode.TARGET;
            this.m_aimTarget = null;
            this.m_relativeTarget = new Vector3?(absoluteTarget);
            this.Update();
        }

        public void SetTarget(MyEntity entity, Vector3? relativeTarget = new Vector3?())
        {
            this.m_mode = AimingMode.TARGET;
            this.m_aimTarget = entity;
            this.m_relativeTarget = relativeTarget;
            this.Update();
        }

        public void StopAiming()
        {
            this.m_aimTarget = null;
            this.m_mode = AimingMode.FIXATED;
            this.m_relativeTarget = null;
        }

        public void Update()
        {
            if (this.m_mode == AimingMode.FIXATED)
            {
                this.m_rotationHint = Vector3.Zero;
            }
            else
            {
                MyCharacter botEntity = this.m_parent.BotEntity as MyCharacter;
                MatrixD aimingPositionAndOrientation = this.m_parent.AimingPositionAndOrientation;
                if (this.m_mode == AimingMode.FOLLOW_MOVEMENT)
                {
                    this.m_parent.PositionAndOrientationInverted;
                    this.m_parent.AimingPositionAndOrientationInverted;
                    Vector3 forwardVector = this.m_parent.ForwardVector;
                    this.CalculateRotationHint(ref aimingPositionAndOrientation, ref forwardVector);
                }
                else if (this.m_aimTarget == null)
                {
                    if (this.m_relativeTarget == null)
                    {
                        this.m_rotationHint = Vector3.Zero;
                    }
                    else
                    {
                        Vector3 desiredForward = this.m_relativeTarget.Value - this.m_parent.AimingPositionAndOrientation.Translation;
                        desiredForward.Normalize();
                        this.CalculateRotationHint(ref aimingPositionAndOrientation, ref desiredForward);
                        if (botEntity != null)
                        {
                            botEntity.AimedPoint = this.m_relativeTarget.Value;
                        }
                    }
                }
                else if (this.m_aimTarget.MarkedForClose)
                {
                    this.m_aimTarget = null;
                    this.m_rotationHint = Vector3.Zero;
                }
                else
                {
                    Vector3 translation;
                    if (this.m_relativeTarget != null)
                    {
                        translation = (Vector3) Vector3D.Transform(this.m_relativeTarget.Value, this.m_aimTarget.PositionComp.WorldMatrix);
                    }
                    else
                    {
                        translation = (Vector3) this.m_aimTarget.PositionComp.WorldMatrix.Translation;
                    }
                    this.PredictTargetPosition(ref translation, botEntity);
                    Vector3 desiredForward = translation - this.m_parent.AimingPositionAndOrientation.Translation;
                    desiredForward.Normalize();
                    this.CalculateRotationHint(ref aimingPositionAndOrientation, ref desiredForward);
                    if (botEntity != null)
                    {
                        botEntity.AimedPoint = translation;
                        this.AddErrorToAiming(botEntity, (this.m_aimTarget.PositionComp != null) ? (this.m_aimTarget.PositionComp.LocalVolume.Radius * 1.5f) : 1f);
                    }
                }
            }
        }

        public Vector3 RotationHint =>
            this.m_rotationHint;

        private enum AimingMode : byte
        {
            FIXATED = 0,
            TARGET = 1,
            FOLLOW_MOVEMENT = 2
        }
    }
}

