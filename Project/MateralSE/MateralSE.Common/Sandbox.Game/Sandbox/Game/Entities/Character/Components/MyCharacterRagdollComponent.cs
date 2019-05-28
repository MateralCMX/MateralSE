namespace Sandbox.Game.Entities.Character.Components
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    public class MyCharacterRagdollComponent : MyCharacterComponent
    {
        public MyRagdollMapper RagdollMapper;
        private IMyGunObject<MyDeviceBase> m_previousWeapon;
        private MyPhysicsBody m_previousPhysics;
        private Vector3D m_lastPosition;
        private int m_gravityTimer;
        private const int GRAVITY_DELAY = 300;
        public float Distance;

        private void ActivateJetpackRagdoll()
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            if ((((((this.RagdollMapper != null) && (base.Character.Physics != null)) && (base.Character.Physics.Ragdoll != null)) && MyPerGameSettings.EnableRagdollModels) && MyPerGameSettings.EnableRagdollInJetpack) && (base.Character.GetPhysicsBody().HavokWorld != null))
            {
                string[] strArray;
                List<string> list = new List<string>();
                if (base.Character.CurrentWeapon != null)
                {
                    if (base.Character.Definition.RagdollPartialSimulations.TryGetValue("Jetpack_Weapon", out strArray))
                    {
                        list.AddArray<string>(strArray);
                    }
                    else
                    {
                        list.Add("Ragdoll_SE_rig_LThigh001");
                        list.Add("Ragdoll_SE_rig_LCalf001");
                        list.Add("Ragdoll_SE_rig_LFoot001");
                        list.Add("Ragdoll_SE_rig_RThigh001");
                        list.Add("Ragdoll_SE_rig_RCalf001");
                        list.Add("Ragdoll_SE_rig_RFoot001");
                    }
                }
                else if (base.Character.Definition.RagdollPartialSimulations.TryGetValue("Jetpack", out strArray))
                {
                    list.AddArray<string>(strArray);
                }
                else
                {
                    list.Add("Ragdoll_SE_rig_LUpperarm001");
                    list.Add("Ragdoll_SE_rig_LForearm001");
                    list.Add("Ragdoll_SE_rig_LPalm001");
                    list.Add("Ragdoll_SE_rig_RUpperarm001");
                    list.Add("Ragdoll_SE_rig_RForearm001");
                    list.Add("Ragdoll_SE_rig_RPalm001");
                    list.Add("Ragdoll_SE_rig_LThigh001");
                    list.Add("Ragdoll_SE_rig_LCalf001");
                    list.Add("Ragdoll_SE_rig_LFoot001");
                    list.Add("Ragdoll_SE_rig_RThigh001");
                    list.Add("Ragdoll_SE_rig_RCalf001");
                    list.Add("Ragdoll_SE_rig_RFoot001");
                }
                if (base.Character.Physics.Enabled)
                {
                    List<int> dynamicRigidBodies = new List<int>();
                    foreach (string str in list)
                    {
                        dynamicRigidBodies.Add(this.RagdollMapper.BodyIndex(str));
                    }
                    if (!base.Character.Physics.IsRagdollModeActive)
                    {
                        base.Character.Physics.SwitchToRagdollMode(false, 1);
                    }
                    if (base.Character.Physics.IsRagdollModeActive)
                    {
                        this.RagdollMapper.ActivatePartialSimulation(dynamicRigidBodies);
                    }
                    this.RagdollMapper.SetVelocities(false, false);
                    if (!MyFakes.ENABLE_JETPACK_RAGDOLL_COLLISIONS)
                    {
                        base.Character.Physics.DisableRagdollBodiesCollisions();
                    }
                }
            }
        }

        private void CheckChangesOnCharacter()
        {
            MyCharacter character = base.Character;
            if (MyPerGameSettings.EnableRagdollInJetpack)
            {
                if (!ReferenceEquals(character.Physics, this.m_previousPhysics))
                {
                    this.UpdateCharacterPhysics();
                    this.m_previousPhysics = character.Physics;
                }
                if (Sync.IsServer)
                {
                    goto TR_001B;
                }
                else if (character.ClosestParentId == 0)
                {
                    goto TR_001B;
                }
                else
                {
                    this.DeactivateJetpackRagdoll();
                }
            }
        TR_0004:
            if ((character.IsDead && !this.IsRagdollActivated) && character.Physics.Enabled)
            {
                this.InitDeadBodyPhysics();
            }
            return;
        TR_001B:
            if (!ReferenceEquals(character.CurrentWeapon, this.m_previousWeapon))
            {
                this.DeactivateJetpackRagdoll();
                this.ActivateJetpackRagdoll();
                this.m_previousWeapon = character.CurrentWeapon;
            }
            MyCharacterJetpackComponent jetpackComp = character.JetpackComp;
            MyCharacterMovementEnum currentMovementState = character.GetCurrentMovementState();
            if ((((jetpackComp == null) || !jetpackComp.TurnedOn) || (currentMovementState != MyCharacterMovementEnum.Flying)) && ((currentMovementState != MyCharacterMovementEnum.Falling) || !character.Physics.Enabled))
            {
                if ((this.RagdollMapper != null) && this.RagdollMapper.IsPartiallySimulated)
                {
                    this.DeactivateJetpackRagdoll();
                }
            }
            else if (!this.IsRagdollActivated || !this.RagdollMapper.IsActive)
            {
                this.DeactivateJetpackRagdoll();
                this.ActivateJetpackRagdoll();
            }
            if (this.IsRagdollActivated && (character.Physics.Ragdoll != null))
            {
                bool isDead = character.IsDead;
                using (List<HkRigidBody>.Enumerator enumerator = character.Physics.Ragdoll.RigidBodies.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.EnableDeactivation = isDead;
                    }
                }
            }
            goto TR_0004;
        }

        private void DeactivateJetpackRagdoll()
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            if (((((this.RagdollMapper != null) && (base.Character.Physics != null)) && (base.Character.Physics.Ragdoll != null)) && MyPerGameSettings.EnableRagdollModels) && MyPerGameSettings.EnableRagdollInJetpack)
            {
                if (this.RagdollMapper.IsPartiallySimulated)
                {
                    this.RagdollMapper.DeactivatePartialSimulation();
                }
                if (base.Character.Physics.IsRagdollModeActive)
                {
                    base.Character.Physics.CloseRagdollMode();
                }
            }
        }

        public void InitDeadBodyPhysics()
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            MyPhysicsBody physics = base.Character.Physics;
            if (physics.IsRagdollModeActive)
            {
                physics.CloseRagdollMode();
            }
            MyRagdollMapper ragdollMapper = this.RagdollMapper;
            if (ragdollMapper.IsActive)
            {
                ragdollMapper.Deactivate();
            }
            physics.SwitchToRagdollMode(true, 1);
            ragdollMapper.Activate();
            ragdollMapper.SetRagdollToKeyframed();
            ragdollMapper.UpdateRagdollPose();
            ragdollMapper.SetRagdollToDynamic();
        }

        public bool InitRagdoll()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("RagdollComponent.InitRagdoll");
            }
            if (base.Character.Physics.Ragdoll != null)
            {
                base.Character.Physics.CloseRagdollMode();
                base.Character.Physics.Ragdoll.ResetToRigPose();
                base.Character.Physics.Ragdoll.SetToKeyframed();
                return true;
            }
            base.Character.Physics.Ragdoll = new HkRagdoll();
            bool flag = false;
            if ((base.Character.Model.HavokData != null) && (base.Character.Model.HavokData.Length != 0))
            {
                try
                {
                    flag = base.Character.Physics.Ragdoll.LoadRagdollFromBuffer(base.Character.Model.HavokData);
                }
                catch (Exception)
                {
                    base.Character.Physics.CloseRagdoll();
                    base.Character.Physics.Ragdoll = null;
                }
            }
            else if (base.Character.Definition.RagdollDataFile != null)
            {
                string path = Path.Combine(MyFileSystem.ContentPath, base.Character.Definition.RagdollDataFile);
                if (File.Exists(path))
                {
                    flag = base.Character.Physics.Ragdoll.LoadRagdollFromFile(path);
                }
            }
            if (base.Character.Definition.RagdollRootBody != string.Empty)
            {
                base.Character.Physics.Ragdoll.SetRootBody(base.Character.Definition.RagdollRootBody);
            }
            if (!flag)
            {
                base.Character.Physics.Ragdoll.Dispose();
                base.Character.Physics.Ragdoll = null;
            }
            using (List<HkRigidBody>.Enumerator enumerator = base.Character.Physics.Ragdoll.RigidBodies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UserObject = base.Character;
                }
            }
            if ((base.Character.Physics.Ragdoll != null) && MyPerGameSettings.Destruction)
            {
                base.Character.Physics.Ragdoll.SetToDynamic();
                HkMassProperties properties = new HkMassProperties();
                foreach (HkRigidBody body in base.Character.Physics.Ragdoll.RigidBodies)
                {
                    properties.Mass = MyDestructionHelper.MassToHavok(body.Mass);
                    properties.InertiaTensor = Matrix.CreateScale((float) 0.04f) * body.InertiaTensor;
                    body.SetMassProperties(ref properties);
                }
                base.Character.Physics.Ragdoll.SetToKeyframed();
            }
            if ((base.Character.Physics.Ragdoll != null) && MyFakes.ENABLE_RAGDOLL_DEFAULT_PROPERTIES)
            {
                base.Character.Physics.SetRagdollDefaults();
            }
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("RagdollComponent.InitRagdoll - FINISHED");
            }
            return flag;
        }

        public void InitRagdollMapper()
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            if ((base.Character.AnimationController.CharacterBones.Length != 0) && ((base.Character.Physics != null) && (base.Character.Physics.Ragdoll != null)))
            {
                this.RagdollMapper = new MyRagdollMapper(base.Character, base.Character.AnimationController.CharacterBones);
                this.RagdollMapper.Init(base.Character.Definition.RagdollBonesMappings);
            }
        }

        public override void OnAddedToContainer()
        {
            if (Sandbox.Engine.Platform.Game.IsDedicated || !MyFakes.ENABLE_RAGDOLL)
            {
                base.Container.Remove<MyCharacterRagdollComponent>();
            }
            else
            {
                bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
                base.OnAddedToContainer();
                base.NeedsUpdateSimulation = true;
                base.NeedsUpdateBeforeSimulation = true;
                base.NeedsUpdateBeforeSimulation100 = true;
                if (((base.Character.Physics == null) || (!MyPerGameSettings.EnableRagdollModels || (base.Character.Model.HavokData == null))) || (base.Character.Model.HavokData.Length == 0))
                {
                    base.Container.Remove<MyCharacterRagdollComponent>();
                }
                else if (this.InitRagdoll() && (base.Character.Definition.RagdollBonesMappings.Count > 1))
                {
                    this.InitRagdollMapper();
                }
            }
        }

        public override void Simulate()
        {
            if (this.Distance <= MyFakes.ANIMATION_UPDATE_DISTANCE)
            {
                base.Simulate();
                if (!base.Character.IsDead || ((this.RagdollMapper.Ragdoll != null) && this.RagdollMapper.Ragdoll.IsSimulationActive))
                {
                    this.SimulateRagdoll();
                }
                this.UpdateCharacterBones();
                if ((base.Character.Physics == null) || (base.Character.Physics.Ragdoll == null))
                {
                    this.IsRagdollMoving = true;
                }
                else
                {
                    double num = Vector3D.DistanceSquared(this.m_lastPosition, base.Character.Physics.Ragdoll.WorldMatrix.Translation);
                    this.IsRagdollMoving = num > 9.9999997473787516E-05;
                }
                this.CheckChangesOnCharacter();
            }
        }

        private void SimulateRagdoll()
        {
            if ((MyPerGameSettings.EnableRagdollModels && ((base.Character.Physics != null) && (this.RagdollMapper != null))) && (((base.Character.Physics.Ragdoll != null) && base.Character.Physics.Ragdoll.InWorld) && this.RagdollMapper.IsActive))
            {
                try
                {
                    this.RagdollMapper.UpdateRagdollAfterSimulation();
                    if (!base.Character.IsCameraNear)
                    {
                        bool flag1 = MyFakes.ENABLE_PERMANENT_SIMULATIONS_COMPUTATION;
                    }
                }
                finally
                {
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            bool isDead;
            base.UpdateBeforeSimulation();
            this.UpdateRagdoll();
            if (base.Character.Physics == null)
            {
                goto TR_0004;
            }
            else if (base.Character.Physics.Ragdoll == null)
            {
                goto TR_0004;
            }
            else if (!base.Character.Physics.Ragdoll.InWorld)
            {
                goto TR_0004;
            }
            else
            {
                if (base.Character.Physics.Ragdoll.IsKeyframed && !this.RagdollMapper.IsPartiallySimulated)
                {
                    goto TR_0004;
                }
                if (this.IsRagdollMoving || (this.m_gravityTimer > 0))
                {
                    Vector3 vector = MyGravityProviderSystem.CalculateTotalGravityInPoint(base.Character.PositionComp.WorldAABB.Center) + (base.Character.GetPhysicsBody().HavokWorld.Gravity * MyPerGameSettings.CharacterGravityMultiplier);
                    isDead = base.Character.IsDead;
                    if (isDead)
                    {
                        foreach (HkRigidBody body in base.Character.Physics.Ragdoll.RigidBodies)
                        {
                            if (!body.IsFixedOrKeyframed)
                            {
                                body.ApplyForce(0.01666667f, vector * body.Mass);
                            }
                        }
                    }
                    else
                    {
                        vector *= MyFakes.RAGDOLL_GRAVITY_MULTIPLIER;
                        Vector3.ClampToSphere(ref vector, 500f);
                        foreach (HkRigidBody body2 in base.Character.Physics.Ragdoll.RigidBodies)
                        {
                            if (!body2.IsFixedOrKeyframed)
                            {
                                body2.ApplyForce(0.01666667f, vector);
                            }
                        }
                    }
                }
                else
                {
                    goto TR_0004;
                }
            }
            if (!this.IsRagdollMoving)
            {
                this.m_gravityTimer--;
            }
            else
            {
                this.m_gravityTimer = 300;
                if (isDead)
                {
                    this.m_gravityTimer /= 5;
                }
            }
        TR_0004:
            if (((base.Character.Physics != null) && (base.Character.Physics.Ragdoll != null)) && this.IsRagdollMoving)
            {
                this.m_lastPosition = base.Character.Physics.Ragdoll.WorldMatrix.Translation;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if ((Sync.IsServer && base.Character.IsDead) && MyFakes.ENABLE_RAGDOLL_CLIENT_SYNC)
            {
                this.RagdollMapper.SyncRigidBodiesTransforms(base.Character.WorldMatrix);
            }
        }

        private void UpdateCharacterBones()
        {
            if (((this.RagdollMapper != null) && (this.RagdollMapper.Ragdoll != null)) && this.RagdollMapper.Ragdoll.InWorld)
            {
                this.RagdollMapper.UpdateCharacterPose(1f, 0f);
                this.RagdollMapper.DebugDraw(base.Character.WorldMatrix);
                MyCharacterBone[] characterBones = base.Character.AnimationController.CharacterBones;
                for (int i = 0; i < characterBones.Length; i++)
                {
                    MyCharacterBone bone = characterBones[i];
                    bone.ComputeBoneTransform();
                    base.Character.BoneRelativeTransforms[i] = bone.RelativeTransform;
                }
            }
        }

        public void UpdateCharacterPhysics()
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            this.InitRagdoll();
            if ((base.Character.Definition.RagdollBonesMappings.Count > 1) && (base.Character.Physics.Ragdoll != null))
            {
                this.InitRagdollMapper();
            }
        }

        private void UpdateRagdoll()
        {
            if (((((((base.Character.Physics != null) && (base.Character.Physics.Ragdoll != null)) && (this.RagdollMapper != null)) && MyPerGameSettings.EnableRagdollModels) && (this.Distance <= MyFakes.ANIMATION_UPDATE_DISTANCE)) && (this.RagdollMapper.IsActive && base.Character.Physics.IsRagdollModeActive)) && (this.RagdollMapper.IsKeyFramed || this.RagdollMapper.IsPartiallySimulated))
            {
                this.RagdollMapper.UpdateRagdollPosition();
                this.RagdollMapper.SetVelocities(true, true);
                this.RagdollMapper.SetLimitedVelocities();
                this.RagdollMapper.DebugDraw(base.Character.WorldMatrix);
            }
        }

        public bool IsRagdollMoving { get; set; }

        public bool IsRagdollActivated =>
            ((base.Character.Physics != null) ? base.Character.Physics.IsRagdollModeActive : false);

        public override string ComponentTypeDebugString =>
            "Character Ragdoll Component";
    }
}

