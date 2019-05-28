namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Utils;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    public class MyCharacterWeaponPositionComponent : MyCharacterComponent
    {
        private float m_animationToIKDelay = 0.3f;
        private float m_currentAnimationToIkTime = 0.3f;
        private float m_currentScatterToAnimRatio = 1f;
        private int m_animationToIkState;
        private Vector4 m_weaponPositionVariantWeightCounters = new Vector4(1f, 0f, 0f, 0f);
        private float m_sprintStatusWeight;
        private float m_sprintStatusGainSpeed = 0.06666667f;
        private float m_backkickSpeed;
        private float m_backkickPos;
        private bool m_lastStateWasFalling;
        private bool m_lastStateWasCrouching;
        private float m_suppressBouncingForTimeSec;
        private float m_lastLocalRotX;
        private readonly MyAverageFiltering m_spineRestPositionX = new MyAverageFiltering(0x10);
        private readonly MyAverageFiltering m_spineRestPositionY = new MyAverageFiltering(0x10);
        private readonly MyAverageFiltering m_spineRestPositionZ = new MyAverageFiltering(0x10);
        private float m_currentScatterBlend;
        private Vector3 m_currentScatterPos;
        private Vector3 m_lastScatterPos;
        private static readonly Vector3 m_weaponIronsightTranslation = new Vector3(0f, -0.11f, -0.22f);
        private static readonly Vector3 m_toolIronsightTranslation = new Vector3(0f, -0.21f, -0.25f);
        private static readonly float m_suppressBouncingDelay = 0.5f;

        public void AddBackkick(float backkickForce)
        {
            this.m_backkickSpeed = Math.Max(this.m_backkickSpeed, backkickForce * 1f);
        }

        private void ApplyBackkick(ref MatrixD weaponMatrixLocal)
        {
            weaponMatrixLocal.Translation += weaponMatrixLocal.Backward * this.m_backkickPos;
        }

        private unsafe void ApplyWeaponBouncing(MyHandItemDefinition handItemDefinition, ref MatrixD weaponMatrixLocal, float fpsBounceMultiplier, float ironsightWeight)
        {
            if (base.Character.AnimationController.CharacterBones.IsValidIndex<MyCharacterBone>(base.Character.SpineBoneIndex))
            {
                Vector3* vectorPtr1;
                float z;
                bool flag = base.Character.ControllerInfo.IsLocallyControlled();
                bool flag2 = (base.Character.IsInFirstPersonView || base.Character.ForceFirstPersonCamera) & flag;
                Vector3 translation = base.Character.AnimationController.CharacterBonesSorted[0].Translation;
                MyCharacterBone bone1 = base.Character.AnimationController.CharacterBones[base.Character.SpineBoneIndex];
                Vector3 vector2 = bone1.AbsoluteTransform.Translation - translation;
                this.m_spineRestPositionX.Add((double) vector2.X);
                this.m_spineRestPositionY.Add((double) vector2.Y);
                this.m_spineRestPositionZ.Add((double) vector2.Z);
                Vector3 position = bone1.GetAbsoluteRigTransform().Translation;
                Vector3 vector4 = new Vector3((double) position.X, this.m_spineRestPositionY.Get(), (double) position.Z);
                Vector3 vector5 = (vector2 - vector4) * fpsBounceMultiplier;
                if (!flag2)
                {
                    z = 0f;
                }
                else
                {
                    vectorPtr1 = (Vector3*) ref vector5;
                    z = vector5.Z;
                }
                vectorPtr1->Z = z;
                this.m_sprintStatusWeight += base.Character.IsSprinting ? this.m_sprintStatusGainSpeed : -this.m_sprintStatusGainSpeed;
                this.m_sprintStatusWeight = MathHelper.Clamp(this.m_sprintStatusWeight, 0f, 1f);
                if (!flag2)
                {
                    vector5 *= handItemDefinition.AmplitudeMultiplier3rd;
                }
                else
                {
                    vector5 *= 1f + (Math.Max((float) 0f, (float) (handItemDefinition.RunMultiplier - 1f)) * this.m_sprintStatusWeight);
                    float* singlePtr1 = (float*) ref vector5.X;
                    singlePtr1[0] *= handItemDefinition.XAmplitudeScale;
                    float* singlePtr2 = (float*) ref vector5.Y;
                    singlePtr2[0] *= handItemDefinition.YAmplitudeScale;
                    float* singlePtr3 = (float*) ref vector5.Z;
                    singlePtr3[0] *= handItemDefinition.ZAmplitudeScale;
                }
                weaponMatrixLocal.Translation += vector5;
                BoundingBox localAABB = base.Character.PositionComp.LocalAABB;
                if ((ironsightWeight < 1f) && (weaponMatrixLocal.M43 > (((position.Z + translation.Z) - (localAABB.Max.Z * 0.5)) - (base.Character.HandItemDefinition.RightHand.Translation.Z * 0.75))))
                {
                    double num = ((position.Z + translation.Z) - (localAABB.Max.Z * 0.5)) - (base.Character.HandItemDefinition.RightHand.Translation.Z * 0.75);
                    weaponMatrixLocal.M43 = MathHelper.Lerp(num, weaponMatrixLocal.M43, (double) ironsightWeight);
                }
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS)
                {
                    MatrixD? cameraViewMatrix = null;
                    MyDebugDrawHelper.DrawNamedPoint(Vector3D.Transform(position, base.Character.WorldMatrix), "spine", new Color?(Color.Gray), cameraViewMatrix);
                }
            }
        }

        private MatrixD GetWeaponRelativeMatrix()
        {
            if (((base.Character.CurrentWeapon == null) || (base.Character.HandItemDefinition == null)) || !base.Character.AnimationController.CharacterBones.IsValidIndex<MyCharacterBone>(base.Character.WeaponBone))
            {
                return MatrixD.Identity;
            }
            return MatrixD.Invert(base.Character.HandItemDefinition.RightHand);
        }

        public virtual void Init(MyObjectBuilder_Character characterBuilder)
        {
        }

        public void Update(bool timeAdvanced = true)
        {
            if (base.Character.Definition != null)
            {
                this.UpdateLogicalWeaponPosition();
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    if (timeAdvanced)
                    {
                        this.m_backkickSpeed *= 0.85f;
                        this.m_backkickPos = (this.m_backkickPos * 0.5f) + this.m_backkickSpeed;
                    }
                    this.UpdateIkTransitions();
                    this.UpdateGraphicalWeaponPosition();
                }
                this.m_lastStateWasFalling = base.Character.IsFalling;
                this.m_lastStateWasCrouching = base.Character.IsCrouching;
                if (timeAdvanced)
                {
                    this.m_suppressBouncingForTimeSec -= 0.01666667f;
                    if (this.m_suppressBouncingForTimeSec < 0f)
                    {
                        this.m_suppressBouncingForTimeSec = 0f;
                    }
                }
            }
        }

        private unsafe Vector4D UpdateAndGetWeaponVariantWeights(MyHandItemDefinition handItemDefinition)
        {
            float num;
            int num1;
            int num5;
            float single1;
            float single2;
            float single3;
            base.Character.AnimationController.Variables.GetValue(MyAnimationVariableStorageHints.StrIdSpeed, out num);
            if (base.Character.IsSprinting || MyCharacter.IsRunningState(base.Character.GetPreviousMovementState()))
            {
                num1 = (int) (num > base.Character.Definition.MaxWalkSpeed);
            }
            else
            {
                num1 = 0;
            }
            bool flag = (bool) num1;
            if (base.Character.IsShooting(MyShootActionEnum.PrimaryAction) || ((base.Character.ZoomMode == MyZoomModeEnum.Classic) && (base.Character.IsShooting(MyShootActionEnum.SecondaryAction) || base.Character.IsShooting(MyShootActionEnum.TertiaryAction))))
            {
                num5 = (int) !base.Character.IsSprinting;
            }
            else
            {
                num5 = 0;
            }
            this.IsShooting = (bool) num5;
            this.IsInIronSight = (base.Character.ZoomMode == MyZoomModeEnum.IronSight) && !base.Character.IsSprinting;
            this.ShouldSupressShootAnimation = base.Character.ShouldSupressShootAnimation;
            bool isShooting = this.IsShooting;
            bool isInIronSight = this.IsInIronSight;
            float num2 = 0.01666667f / handItemDefinition.BlendTime;
            float num3 = 0.01666667f / handItemDefinition.ShootBlend;
            float* singlePtr1 = &this.m_weaponPositionVariantWeightCounters.X;
            if ((flag || isShooting) || isInIronSight)
            {
                single1 = (isShooting | isInIronSight) ? -num3 : -num2;
            }
            else
            {
                single1 = num2;
            }
            *(singlePtr1[0]) = singlePtr1 + single1;
            float* singlePtr2 = (float*) ref this.m_weaponPositionVariantWeightCounters.Y;
            if ((!flag || isShooting) || isInIronSight)
            {
                single2 = (isShooting | isInIronSight) ? -num3 : -num2;
            }
            else
            {
                single2 = num2;
            }
            singlePtr2[0] += single2;
            float* singlePtr3 = (float*) ref this.m_weaponPositionVariantWeightCounters.Z;
            if (!isShooting || isInIronSight)
            {
                single3 = isInIronSight ? -num3 : -num2;
            }
            else
            {
                single3 = num3;
            }
            singlePtr3[0] += single3;
            float* singlePtr4 = (float*) ref this.m_weaponPositionVariantWeightCounters.W;
            singlePtr4[0] += isInIronSight ? num3 : (isShooting ? -num3 : -num2);
            this.m_weaponPositionVariantWeightCounters = Vector4.Clamp(this.m_weaponPositionVariantWeightCounters, Vector4.Zero, Vector4.One);
            Vector4D vectord = new Vector4D((double) MathHelper.SmoothStep(0f, 1f, this.m_weaponPositionVariantWeightCounters.X), (double) MathHelper.SmoothStep(0f, 1f, this.m_weaponPositionVariantWeightCounters.Y), (double) MathHelper.SmoothStep(0f, 1f, this.m_weaponPositionVariantWeightCounters.Z), (double) MathHelper.SmoothStep(0f, 1f, this.m_weaponPositionVariantWeightCounters.W));
            return (vectord / (((vectord.X + vectord.Y) + vectord.Z) + vectord.W));
        }

        private void UpdateGraphicalWeaponPosition()
        {
            MyAnimationControllerComponent animationController = base.Character.AnimationController;
            MyHandItemDefinition handItemDefinition = base.Character.HandItemDefinition;
            if (((handItemDefinition != null) && (base.Character.CurrentWeapon != null)) && (animationController.CharacterBones != null))
            {
                bool flag = base.Character.ControllerInfo.IsLocallyControlled() && ReferenceEquals(MySession.Static.CameraController, base.Character);
                bool flag2 = (base.Character.IsInFirstPersonView || base.Character.ForceFirstPersonCamera) & flag;
                if (MyFakes.FORCE_CHARTOOLS_1ST_PERSON)
                {
                    flag2 = true;
                }
                bool jetpackRunning = base.Character.JetpackRunning;
                if (this.m_lastStateWasFalling & jetpackRunning)
                {
                    this.m_currentAnimationToIkTime = this.m_animationToIKDelay * ((float) Math.Cos((double) (base.Character.HeadLocalXAngle - this.m_lastLocalRotX)));
                }
                if (this.m_lastStateWasCrouching != base.Character.IsCrouching)
                {
                    this.m_suppressBouncingForTimeSec = m_suppressBouncingDelay;
                }
                if (this.m_suppressBouncingForTimeSec > 0f)
                {
                    this.m_spineRestPositionX.Clear();
                    this.m_spineRestPositionY.Clear();
                    this.m_spineRestPositionZ.Clear();
                }
                this.m_lastLocalRotX = base.Character.HeadLocalXAngle;
                if (flag2)
                {
                    this.UpdateGraphicalWeaponPosition1st(handItemDefinition);
                }
                else
                {
                    this.UpdateGraphicalWeaponPosition3rd(handItemDefinition);
                }
            }
        }

        private void UpdateGraphicalWeaponPosition1st(MyHandItemDefinition handItemDefinition)
        {
            bool jetpackRunning = base.Character.JetpackRunning;
            MyAnimationControllerComponent animationController = base.Character.AnimationController;
            MatrixD xd = base.Character.GetHeadMatrix(false, !jetpackRunning, false, true, true) * base.Character.PositionComp.WorldMatrixInvScaled;
            MatrixD itemLocation = handItemDefinition.ItemLocation;
            MatrixD itemWalkingLocation = handItemDefinition.ItemWalkingLocation;
            MatrixD itemShootLocation = handItemDefinition.ItemShootLocation;
            MatrixD itemIronsightLocation = handItemDefinition.ItemIronsightLocation;
            MatrixD xd6 = animationController.CharacterBones.IsValidIndex<MyCharacterBone>(base.Character.WeaponBone) ? (this.GetWeaponRelativeMatrix() * animationController.CharacterBones[base.Character.WeaponBone].AbsoluteTransform) : this.GetWeaponRelativeMatrix();
            itemIronsightLocation.Translation = m_weaponIronsightTranslation;
            if (base.Character.CurrentWeapon is MyEngineerToolBase)
            {
                itemIronsightLocation.Translation = m_toolIronsightTranslation;
            }
            Vector4D vectord = this.UpdateAndGetWeaponVariantWeights(handItemDefinition);
            MatrixD xd7 = MatrixD.Normalize((MatrixD) ((((vectord.X * itemLocation) + (vectord.Y * itemWalkingLocation)) + (vectord.Z * itemShootLocation)) + (vectord.W * itemIronsightLocation)));
            double num = 0.0;
            if (handItemDefinition.ItemPositioning == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.X;
            }
            if (handItemDefinition.ItemPositioningWalk == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.Y;
            }
            if (handItemDefinition.ItemPositioningShoot == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.Z;
            }
            if (handItemDefinition.ItemPositioningIronsight == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.W;
            }
            num /= ((vectord.X + vectord.Y) + vectord.Z) + vectord.W;
            double num2 = 0.0;
            if (handItemDefinition.ItemPositioning != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.X;
            }
            if (handItemDefinition.ItemPositioningWalk != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.Y;
            }
            if (handItemDefinition.ItemPositioningShoot != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.Z;
            }
            if (handItemDefinition.ItemPositioningIronsight != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.W;
            }
            num2 /= ((vectord.X + vectord.Y) + vectord.Z) + vectord.W;
            MatrixD weaponMatrixLocal = xd7 * xd;
            this.ApplyWeaponBouncing(handItemDefinition, ref weaponMatrixLocal, (float) (1.0 - (0.95 * vectord.W)), (float) vectord.W);
            MyEngineerToolBase currentWeapon = base.Character.CurrentWeapon as MyEngineerToolBase;
            if (currentWeapon != null)
            {
                currentWeapon.SensorDisplacement = (Vector3) -xd7.Translation;
            }
            double amount = (num * this.m_currentAnimationToIkTime) / ((double) this.m_animationToIKDelay);
            MatrixD weaponAnimMatrix = MatrixD.Lerp(xd6, weaponMatrixLocal, amount);
            this.UpdateScattering(ref weaponAnimMatrix, handItemDefinition);
            this.ApplyBackkick(ref weaponAnimMatrix);
            MatrixD matrix = weaponAnimMatrix * base.Character.WorldMatrix;
            this.GraphicalPositionWorld = matrix.Translation;
            this.ArmsIkWeight = (float) num2;
            ((MyEntity) base.Character.CurrentWeapon).WorldMatrix = matrix;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS)
            {
                MyDebugDrawHelper.DrawNamedColoredAxis(xd6 * base.Character.WorldMatrix, 0.25f, "weapon anim " + (100.0 - (100.0 * amount)) + "%", new Color?(Color.Orange));
                MyDebugDrawHelper.DrawNamedColoredAxis(weaponMatrixLocal * base.Character.WorldMatrix, 0.25f, "weapon data " + (100.0 * amount) + "%", new Color?(Color.Magenta));
                MyDebugDrawHelper.DrawNamedColoredAxis(matrix, 0.25f, "weapon final", new Color?(Color.White));
            }
        }

        private unsafe void UpdateGraphicalWeaponPosition3rd(MyHandItemDefinition handItemDefinition)
        {
            bool jetpackRunning = base.Character.JetpackRunning;
            MyAnimationControllerComponent animationController = base.Character.AnimationController;
            MatrixD xd = base.Character.GetHeadMatrix(false, !jetpackRunning, false, true, true) * base.Character.PositionComp.WorldMatrixInvScaled;
            if (animationController.CharacterBones.IsValidIndex<MyCharacterBone>(base.Character.HeadBoneIndex))
            {
                double* numPtr1 = (double*) ref xd.M42;
                numPtr1[0] += animationController.CharacterBonesSorted[0].Translation.Y;
            }
            MatrixD xd2 = handItemDefinition.ItemLocation3rd;
            MatrixD xd3 = handItemDefinition.ItemWalkingLocation3rd;
            MatrixD xd4 = handItemDefinition.ItemShootLocation3rd;
            MatrixD itemIronsightLocation = handItemDefinition.ItemIronsightLocation;
            MatrixD xd6 = animationController.CharacterBones.IsValidIndex<MyCharacterBone>(base.Character.WeaponBone) ? (this.GetWeaponRelativeMatrix() * animationController.CharacterBones[base.Character.WeaponBone].AbsoluteTransform) : this.GetWeaponRelativeMatrix();
            itemIronsightLocation.Translation = m_weaponIronsightTranslation;
            if (base.Character.CurrentWeapon is MyEngineerToolBase)
            {
                itemIronsightLocation.Translation = m_toolIronsightTranslation;
            }
            Vector4D vectord = this.UpdateAndGetWeaponVariantWeights(handItemDefinition);
            MatrixD weaponMatrixLocal = MatrixD.Normalize((MatrixD) ((((vectord.X * xd2) + (vectord.Y * xd3)) + (vectord.Z * xd4)) + (vectord.W * itemIronsightLocation)));
            double num = 0.0;
            if (handItemDefinition.ItemPositioning3rd == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.X;
            }
            if (handItemDefinition.ItemPositioningWalk3rd == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.Y;
            }
            if (handItemDefinition.ItemPositioningShoot3rd == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.Z;
            }
            if (handItemDefinition.ItemPositioningIronsight3rd == MyItemPositioningEnum.TransformFromData)
            {
                num += vectord.W;
            }
            num /= ((vectord.X + vectord.Y) + vectord.Z) + vectord.W;
            double num2 = 0.0;
            if (handItemDefinition.ItemPositioning3rd != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.X;
            }
            if (handItemDefinition.ItemPositioningWalk3rd != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.Y;
            }
            if (handItemDefinition.ItemPositioningShoot3rd != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.Z;
            }
            if (handItemDefinition.ItemPositioningIronsight3rd != MyItemPositioningEnum.TransformFromAnim)
            {
                num2 += vectord.W;
            }
            num2 /= ((vectord.X + vectord.Y) + vectord.Z) + vectord.W;
            this.ApplyWeaponBouncing(handItemDefinition, ref weaponMatrixLocal, (float) (1.0 - (0.95 * vectord.W)), 0f);
            double* numPtr2 = (double*) ref xd.M43;
            numPtr2[0] += (0.5 * weaponMatrixLocal.M43) * Math.Max(0.0, xd.M32);
            double* numPtr3 = (double*) ref xd.M42;
            numPtr3[0] += (0.5 * weaponMatrixLocal.M42) * Math.Max(0.0, xd.M32);
            double* numPtr4 = (double*) ref xd.M42;
            numPtr4[0] -= 0.25 * Math.Max(0.0, xd.M32);
            double* numPtr5 = (double*) ref xd.M43;
            numPtr5[0] -= 0.05 * Math.Min(0.0, xd.M32);
            double* numPtr6 = (double*) ref xd.M41;
            numPtr6[0] -= 0.25 * Math.Max(0.0, xd.M32);
            MatrixD xd8 = weaponMatrixLocal * xd;
            MyEngineerToolBase currentWeapon = base.Character.CurrentWeapon as MyEngineerToolBase;
            if (currentWeapon != null)
            {
                currentWeapon.SensorDisplacement = (Vector3) -weaponMatrixLocal.Translation;
            }
            double amount = (num * this.m_currentAnimationToIkTime) / ((double) this.m_animationToIKDelay);
            MatrixD weaponAnimMatrix = MatrixD.Lerp(xd6, xd8, amount);
            this.UpdateScattering(ref weaponAnimMatrix, handItemDefinition);
            this.ApplyBackkick(ref weaponAnimMatrix);
            MatrixD matrix = weaponAnimMatrix * base.Character.WorldMatrix;
            this.GraphicalPositionWorld = matrix.Translation;
            this.ArmsIkWeight = (float) num2;
            ((MyEntity) base.Character.CurrentWeapon).WorldMatrix = matrix;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS)
            {
                MyDebugDrawHelper.DrawNamedColoredAxis(xd6 * base.Character.WorldMatrix, 0.25f, "weapon anim " + (100.0 - (100.0 * amount)) + "%", new Color?(Color.Orange));
                MyDebugDrawHelper.DrawNamedColoredAxis(xd8 * base.Character.WorldMatrix, 0.25f, "weapon data " + (100.0 * amount) + "%", new Color?(Color.Magenta));
                MyDebugDrawHelper.DrawNamedColoredAxis(matrix, 0.25f, "weapon final", new Color?(Color.White));
            }
        }

        internal void UpdateIkTransitions()
        {
            int num1;
            if ((base.Character.HandItemDefinition == null) || (base.Character.CurrentWeapon == null))
            {
                num1 = -1;
            }
            else
            {
                num1 = 1;
            }
            this.m_animationToIkState = num1;
            this.m_currentAnimationToIkTime += this.m_animationToIkState * 0.01666667f;
            if (this.m_currentAnimationToIkTime >= this.m_animationToIKDelay)
            {
                this.m_currentAnimationToIkTime = this.m_animationToIKDelay;
            }
            else if (this.m_currentAnimationToIkTime <= 0f)
            {
                this.m_currentAnimationToIkTime = 0f;
            }
        }

        private void UpdateLogicalWeaponPosition()
        {
            Vector3 vector;
            if (base.Character.IsCrouching)
            {
                vector = new Vector3(0f, base.Character.Definition.CharacterCollisionCrouchHeight - (base.Character.Definition.CharacterHeadHeight * 0.5f), 0f);
            }
            else
            {
                vector = new Vector3(0f, base.Character.Definition.CharacterCollisionHeight - (base.Character.Definition.CharacterHeadHeight * 0.5f), 0f);
            }
            Vector3 weaponIronsightTranslation = m_weaponIronsightTranslation;
            if (base.Character.CurrentWeapon is MyEngineerToolBase)
            {
                Vector3 toolIronsightTranslation = m_toolIronsightTranslation;
            }
            this.LogicalPositionLocalSpace = vector;
            this.LogicalPositionWorld = Vector3D.Transform(this.LogicalPositionLocalSpace, base.Character.PositionComp.WorldMatrix);
            this.LogicalOrientationWorld = base.Character.ShootDirection;
            this.LogicalCrosshairPoint = this.LogicalPositionWorld + (this.LogicalOrientationWorld * 2000.0);
            if (base.Character.CurrentWeapon != null)
            {
                MyEngineerToolBase currentWeapon = base.Character.CurrentWeapon as MyEngineerToolBase;
                if (currentWeapon != null)
                {
                    currentWeapon.UpdateSensorPosition();
                }
                else
                {
                    MyHandDrill drill = base.Character.CurrentWeapon as MyHandDrill;
                    if (drill != null)
                    {
                        drill.WorldPositionChanged(null);
                    }
                }
            }
        }

        private void UpdateScattering(ref MatrixD weaponAnimMatrix, MyHandItemDefinition handItemDefinition)
        {
            MyEngineerToolBase currentWeapon = base.Character.CurrentWeapon as MyEngineerToolBase;
            bool flag = false;
            if (handItemDefinition.ScatterSpeed > 0f)
            {
                bool hasHitBlock = false;
                if (currentWeapon != null)
                {
                    hasHitBlock = currentWeapon.HasHitBlock;
                }
                flag = this.IsShooting & hasHitBlock;
                if (!flag && (this.m_currentScatterToAnimRatio >= 1f))
                {
                    this.m_currentScatterBlend = 0f;
                }
                else
                {
                    if (this.m_currentScatterBlend == 0f)
                    {
                        this.m_lastScatterPos = Vector3.Zero;
                    }
                    if (this.m_currentScatterBlend == handItemDefinition.ScatterSpeed)
                    {
                        this.m_lastScatterPos = this.m_currentScatterPos;
                        this.m_currentScatterBlend = 0f;
                    }
                    if ((this.m_currentScatterBlend == 0f) || (this.m_currentScatterBlend == handItemDefinition.ScatterSpeed))
                    {
                        this.m_currentScatterPos = new Vector3(MyUtils.GetRandomFloat(-handItemDefinition.ShootScatter.X / 2f, handItemDefinition.ShootScatter.X / 2f), MyUtils.GetRandomFloat(-handItemDefinition.ShootScatter.Y / 2f, handItemDefinition.ShootScatter.Y / 2f), MyUtils.GetRandomFloat(-handItemDefinition.ShootScatter.Z / 2f, handItemDefinition.ShootScatter.Z / 2f));
                    }
                    this.m_currentScatterBlend += 0.01f;
                    if (this.m_currentScatterBlend > handItemDefinition.ScatterSpeed)
                    {
                        this.m_currentScatterBlend = handItemDefinition.ScatterSpeed;
                    }
                    Vector3 vector = Vector3.Lerp(this.m_lastScatterPos, this.m_currentScatterPos, this.m_currentScatterBlend / handItemDefinition.ScatterSpeed);
                    weaponAnimMatrix.Translation += (1f - this.m_currentScatterToAnimRatio) * vector;
                }
                this.m_currentScatterToAnimRatio += flag ? -0.1f : 0.1f;
                if (this.m_currentScatterToAnimRatio > 1f)
                {
                    this.m_currentScatterToAnimRatio = 1f;
                }
                else if (this.m_currentScatterToAnimRatio < 0f)
                {
                    this.m_currentScatterToAnimRatio = 0f;
                }
            }
        }

        public Vector3D LogicalPositionLocalSpace { get; private set; }

        public Vector3D LogicalPositionWorld { get; private set; }

        public Vector3D LogicalOrientationWorld { get; private set; }

        public Vector3D LogicalCrosshairPoint { get; private set; }

        public bool IsShooting { get; private set; }

        public bool ShouldSupressShootAnimation { get; set; }

        public bool IsInIronSight { get; private set; }

        public Vector3D GraphicalPositionWorld { get; private set; }

        public float ArmsIkWeight { get; private set; }
    }
}

