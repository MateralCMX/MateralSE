namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Entities.UseObject;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyEntityType(typeof(MyObjectBuilder_FloatingObject), true)]
    public class MyFloatingObject : MyEntity, IMyUseObject, IMyUsableEntity, IMyDestroyableObject, IMyFloatingObject, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEventProxy, IMyEventOwner, IMySyncedEntity
    {
        private static MyStringHash m_explosives = MyStringHash.GetOrCompute("Explosives");
        public static MyObjectBuilder_Ore ScrapBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Scrap");
        private StringBuilder m_displayedText = new StringBuilder();
        public MyPhysicalInventoryItem Item;
        private int m_modelVariant;
        public MyVoxelMaterialDefinition VoxelMaterial;
        public long CreationTime;
        private float m_health = 100f;
        private MyEntity3DSoundEmitter m_soundEmitter;
        public int m_lastTimePlayedSound;
        public float ClosestDistanceToAnyPlayerSquared = -1f;
        public int NumberOfFramesInsideVoxel;
        public const int NUMBER_OF_FRAMES_INSIDE_VOXEL_TO_REMOVE = 5;
        public long SyncWaitCounter;
        private DateTime lastTimeSound = DateTime.MinValue;
        private Vector3 m_smoothGravity;
        private Vector3 m_smoothGravityDir;
        private List<Vector3> m_supportNormals;
        public VRage.Sync.Sync<MyFixedPoint, SyncDirection.FromServer> Amount;
        private HkEasePenetrationAction m_easeCollisionForce;
        private TimeSpan m_timeFromSpawn;

        public MyFloatingObject()
        {
            this.WasRemovedFromWorld = false;
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
            this.m_lastTimePlayedSound = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            base.Render = new MyRenderComponentFloatingObject();
            this.SyncType = SyncHelpers.Compose(this, 0);
        }

        protected override void Closing()
        {
            MyFloatingObjects.UnregisterFloatingObject(this);
            base.Closing();
        }

        public bool DoDamage(float damage, MyStringHash damageType, bool sync, long attackerId)
        {
            if (base.MarkedForClose)
            {
                return false;
            }
            if (sync)
            {
                if (!Sync.IsServer)
                {
                    return false;
                }
                MySyncDamage.DoDamageSynced(this, damage, damageType, attackerId);
                return true;
            }
            MyDamageInformation information = new MyDamageInformation(false, damage, damageType, attackerId);
            if (this.UseDamageSystem)
            {
                MyDamageSystem.Static.RaiseBeforeDamageApplied(this, ref information);
            }
            MyObjectBuilderType typeId = this.Item.Content.TypeId;
            if ((typeId == typeof(MyObjectBuilder_Ore)) || (typeId == typeof(MyObjectBuilder_Ingot)))
            {
                if (this.Item.Amount >= 1)
                {
                    if (Sync.IsServer)
                    {
                        MyFloatingObjects.RemoveFloatingObject(this, (MyFixedPoint) information.Amount);
                    }
                }
                else
                {
                    MyParticleEffect effect;
                    if (MyParticlesManager.TryCreateParticleEffect("Smoke_Construction", base.WorldMatrix, out effect))
                    {
                        effect.UserScale = 0.4f;
                    }
                    if (Sync.IsServer)
                    {
                        MyFloatingObjects.RemoveFloatingObject(this);
                    }
                }
            }
            else
            {
                this.m_health -= 10f * information.Amount;
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseAfterDamageApplied(this, information);
                }
                if (this.m_health < 0f)
                {
                    MyParticleEffect effect2;
                    MyPhysicalItemDefinition definition2;
                    if (MyParticlesManager.TryCreateParticleEffect("Smoke_Construction", base.WorldMatrix, out effect2))
                    {
                        effect2.UserScale = 0.4f;
                    }
                    if (Sync.IsServer)
                    {
                        MyFloatingObjects.RemoveFloatingObject(this);
                    }
                    if ((this.Item.Content.SubtypeId == m_explosives) && Sync.IsServer)
                    {
                        BoundingSphere sphere = new BoundingSphere((Vector3) base.WorldMatrix.Translation, (((float) this.Item.Amount) * 0.01f) + 0.5f);
                        MyExplosionInfo explosionInfo = new MyExplosionInfo {
                            PlayerDamage = 0f,
                            Damage = 800f,
                            ExplosionType = MyExplosionTypeEnum.WARHEAD_EXPLOSION_15,
                            ExplosionSphere = sphere,
                            LifespanMiliseconds = 700,
                            HitEntity = this,
                            ParticleScale = 1f,
                            OwnerEntity = this,
                            Direction = new Vector3?((Vector3) base.WorldMatrix.Forward),
                            VoxelExplosionCenter = sphere.Center,
                            ExplosionFlags = MyExplosionFlags.APPLY_DEFORMATION | MyExplosionFlags.CREATE_SHRAPNELS | MyExplosionFlags.CREATE_PARTICLE_EFFECT | MyExplosionFlags.CREATE_DECALS | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE | MyExplosionFlags.AFFECT_VOXELS | MyExplosionFlags.CREATE_DEBRIS,
                            VoxelCutoutScale = 0.5f,
                            PlaySound = true,
                            ApplyForceAndDamage = true,
                            ObjectsRemoveDelayInMiliseconds = 40
                        };
                        if (this.Physics != null)
                        {
                            explosionInfo.Velocity = this.Physics.LinearVelocity;
                        }
                        MyExplosions.AddExplosion(ref explosionInfo, true);
                    }
                    if (MyFakes.ENABLE_SCRAP && Sync.IsServer)
                    {
                        if (this.Item.Content.SubtypeId == ScrapBuilder.SubtypeId)
                        {
                            return true;
                        }
                        if (this.Item.Content.GetId().TypeId == typeof(MyObjectBuilder_Component))
                        {
                            MyComponentDefinition componentDefinition = MyDefinitionManager.Static.GetComponentDefinition((this.Item.Content as MyObjectBuilder_Component).GetId());
                            if (MyRandom.Instance.NextFloat() < componentDefinition.DropProbability)
                            {
                                MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(this.Item.Amount * 0.8f, ScrapBuilder, 1f), base.PositionComp.GetPosition(), base.WorldMatrix.Forward, base.WorldMatrix.Up, null, null);
                            }
                        }
                    }
                    if (((this.ItemDefinition != null) && ((this.ItemDefinition.DestroyedPieceId != null) && Sync.IsServer)) && MyDefinitionManager.Static.TryGetPhysicalItemDefinition(this.ItemDefinition.DestroyedPieceId.Value, out definition2))
                    {
                        MyFloatingObjects.Spawn(definition2, base.WorldMatrix.Translation, base.WorldMatrix.Forward, base.WorldMatrix.Up, this.ItemDefinition.DestroyedPieces, 1f);
                    }
                    if (this.UseDamageSystem)
                    {
                        MyDamageSystem.Static.RaiseDestroyed(this, information);
                    }
                }
            }
            return true;
        }

        private MyStringHash EvaluatePhysicsMaterial(MyStringHash originalMaterial) => 
            ((this.VoxelMaterial != null) ? MyMaterialType.ROCK : originalMaterial);

        private void FormatDisplayName(StringBuilder outputBuffer, MyPhysicalInventoryItem item)
        {
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Content);
            outputBuffer.Clear().Append(physicalItemDefinition.DisplayNameText);
            if (this.Item.Amount != 1)
            {
                outputBuffer.Append(" (");
                MyGuiControlInventoryOwner.FormatItemAmount(item, outputBuffer);
                outputBuffer.Append(")");
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_FloatingObject objectBuilder = (MyObjectBuilder_FloatingObject) base.GetObjectBuilder(copy);
            objectBuilder.Item = this.Item.GetObjectBuilder();
            objectBuilder.ModelVariant = this.m_modelVariant;
            return objectBuilder;
        }

        protected virtual HkShape GetPhysicsShape(float mass, float scale, out HkMassProperties massProperties)
        {
            HkShapeType sphere;
            if (base.Model == null)
            {
                MyLog.Default.WriteLine("Invalid floating object model: " + this.Item.GetDefinitionId());
            }
            if (this.VoxelMaterial != null)
            {
                sphere = HkShapeType.Sphere;
                massProperties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(base.Model.BoundingSphere.Radius * scale, mass);
            }
            else
            {
                sphere = HkShapeType.Box;
                Vector3 vector = (Vector3) ((2f * (base.Model.BoundingBox.Max - base.Model.BoundingBox.Min)) / 2f);
                massProperties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(vector * scale, mass);
                massProperties.Mass = mass;
                massProperties.CenterOfMass = base.Model.BoundingBox.Center;
            }
            return MyDebris.Static.GetDebrisShape(base.Model, sphere, scale);
        }

        public bool HasConstraints() => 
            this.Physics.RigidBody.HasConstraints();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyObjectBuilder_FloatingObject obj2 = objectBuilder as MyObjectBuilder_FloatingObject;
            if (obj2.Item.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException("MyPhysicalInventoryItem.Amount", $"Creating floating object with invalid amount: {obj2.Item.Amount}x '{obj2.Item.PhysicalContent.GetId()}'");
            }
            base.Init(objectBuilder);
            this.Item = new MyPhysicalInventoryItem(obj2.Item);
            this.m_modelVariant = obj2.ModelVariant;
            this.Amount.SetLocalValue(this.Item.Amount);
            this.Amount.ValueChanged += delegate (SyncBase x) {
                this.Item.Amount = this.Amount.Value;
                this.UpdateInternalState();
            };
            this.InitInternal();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            this.UseDamageSystem = true;
            MyPhysicalItemDefinition definition = null;
            this.ItemDefinition = MyDefinitionManager.Static.TryGetPhysicalItemDefinition(this.Item.GetDefinitionId(), out definition) ? definition : null;
            this.m_timeFromSpawn = MySession.Static.ElapsedPlayTime;
            this.m_smoothGravity = this.Physics.RigidBody.Gravity;
            this.m_smoothGravityDir = this.m_smoothGravity;
            this.m_smoothGravityDir.Normalize();
            this.m_supportNormals = new List<Vector3>();
            this.m_supportNormals.Capacity = 3;
            if (!Sync.IsServer)
            {
                this.Physics.RigidBody.UpdateMotionType(HkMotionType.Fixed);
            }
        }

        private void InitInternal()
        {
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(this.Item.Content);
            this.m_health = physicalItemDefinition.Health;
            this.VoxelMaterial = null;
            if (physicalItemDefinition.VoxelMaterial != MyStringHash.NullOrEmpty)
            {
                this.VoxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(physicalItemDefinition.VoxelMaterial.String);
            }
            else if (this.Item.Content is MyObjectBuilder_Ore)
            {
                string subtypeName = physicalItemDefinition.Id.SubtypeName;
                string materialName = (this.Item.Content as MyObjectBuilder_Ore).GetMaterialName();
                bool flag = (this.Item.Content as MyObjectBuilder_Ore).HasMaterialName();
                foreach (MyVoxelMaterialDefinition definition2 in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
                {
                    if ((flag && (materialName == definition2.Id.SubtypeName)) || (!flag && (subtypeName == definition2.MinedOre)))
                    {
                        this.VoxelMaterial = definition2;
                        break;
                    }
                }
            }
            if ((this.VoxelMaterial != null) && (this.VoxelMaterial.DamagedMaterial != MyStringHash.NullOrEmpty))
            {
                this.VoxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.VoxelMaterial.DamagedMaterial.ToString());
            }
            string model = physicalItemDefinition.Model;
            if (physicalItemDefinition.HasModelVariants)
            {
                int length = physicalItemDefinition.Models.Length;
                this.m_modelVariant = this.m_modelVariant % length;
                model = physicalItemDefinition.Models[this.m_modelVariant];
            }
            else if ((this.Item.Content is MyObjectBuilder_Ore) && (this.VoxelMaterial != null))
            {
                float num5 = 50f;
                model = MyDebris.GetAmountBasedDebrisVoxel(Math.Max((float) this.Item.Amount, num5));
            }
            float scale = 0.7f;
            this.FormatDisplayName(this.m_displayedText, this.Item);
            float? nullable = null;
            this.Init(this.m_displayedText, model, null, nullable, null);
            HkMassProperties massProperties = new HkMassProperties();
            float mass = MathHelper.Clamp((float) ((MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(physicalItemDefinition.Mass) : physicalItemDefinition.Mass) * ((float) this.Item.Amount)), (float) 3f, (float) 100000f);
            HkShape shape = this.GetPhysicsShape(mass, scale, out massProperties);
            massProperties.Mass = mass;
            Matrix identity = Matrix.Identity;
            if (this.Physics != null)
            {
                this.Physics.Close();
            }
            this.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_DEBRIS);
            int collisionFilter = (mass > MyPerGameSettings.MinimumLargeShipCollidableMass) ? 0x17 : 10;
            this.Physics.LinearDamping = 0.1f;
            this.Physics.AngularDamping = 2f;
            if (!shape.IsConvex || (shape.ShapeType == HkShapeType.Sphere))
            {
                this.Physics.CreateFromCollisionObject(shape, Vector3.Zero, MatrixD.Identity, new HkMassProperties?(massProperties), collisionFilter);
                this.Physics.Enabled = true;
            }
            else
            {
                HkConvexTransformShape shape2 = new HkConvexTransformShape((HkConvexShape) shape, ref identity, HkReferencePolicy.None);
                this.Physics.CreateFromCollisionObject((HkShape) shape2, Vector3.Zero, MatrixD.Identity, new HkMassProperties?(massProperties), collisionFilter);
                this.Physics.Enabled = true;
                shape2.Base.RemoveReference();
            }
            this.Physics.Friction = 2f;
            this.Physics.MaterialType = this.EvaluatePhysicsMaterial(physicalItemDefinition.PhysicalMaterial);
            this.Physics.PlayCollisionCueEnabled = true;
            this.Physics.RigidBody.ContactSoundCallbackEnabled = true;
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            this.Physics.RigidBody.SetProperty(HkCharacterRigidBody.FLOATING_OBJECT, 0f);
            this.Physics.RigidBody.CenterOfMassLocal = Vector3.Zero;
            HkMassChangerUtil.Create(this.Physics.RigidBody, 0x10200, 1f, 0f);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            MyFloatingObjects.RegisterFloatingObject(this);
        }

        [Event(null, 0x321), Reliable, Server]
        private void OnClosedRequest()
        {
            if ((MySession.Static.CreativeMode || MyEventContext.Current.IsLocallyInvoked) || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                base.Close();
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        public void OnDestroy()
        {
        }

        public void RefreshDisplayName()
        {
            this.FormatDisplayName(this.m_displayedText, this.Item);
        }

        public void RemoveUsers(bool local)
        {
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent e)
        {
            if ((e.EventType == HkContactPointEvent.Type.Manifold) && (this.Physics.RigidBody.GetShape().ShapeType == HkShapeType.Sphere))
            {
                Vector3 item = e.ContactPoint.Position - this.Physics.RigidBody.Position;
                if (item.Normalize() > 0.001f)
                {
                    this.m_supportNormals.Add(item);
                }
            }
        }

        UseActionResult IMyUsableEntity.CanUse(UseActionEnum actionEnum, Sandbox.Game.Entities.IMyControllableEntity user) => 
            (base.MarkedForClose ? UseActionResult.Closed : UseActionResult.OK);

        public void SendCloseRequest()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyFloatingObject>(this, x => new Action(x.OnClosedRequest), targetEndpoint);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(base.PositionComp.GetPosition());
            if (this.Physics.RigidBody.GetShape().ShapeType != HkShapeType.Sphere)
            {
                this.Physics.RigidBody.Gravity = vector;
            }
            else
            {
                this.m_smoothGravity = (this.m_smoothGravity * 0.5f) + (vector * 0.5f);
                this.m_smoothGravityDir = this.m_smoothGravity;
                this.m_smoothGravityDir.Normalize();
                bool flag = false;
                foreach (Vector3 vector2 in this.m_supportNormals)
                {
                    if (vector2.Dot(this.m_smoothGravityDir) > 0.8f)
                    {
                        flag = true;
                        break;
                    }
                }
                this.m_supportNormals.Clear();
                if (!flag)
                {
                    this.Physics.RigidBody.Gravity = this.m_smoothGravity;
                }
                else if (this.Physics.RigidBody.Gravity.Length() > 0.01f)
                {
                    HkRigidBody rigidBody = this.Physics.RigidBody;
                    rigidBody.Gravity *= 0.99f;
                }
            }
        }

        public void UpdateInternalState()
        {
            if (this.Item.Amount <= 0)
            {
                base.Close();
            }
            else
            {
                base.Render.UpdateRenderObject(false, true);
                this.InitInternal();
                this.Physics.Activate();
                base.InScene = true;
                base.Render.UpdateRenderObject(true, true);
                MyHud.Notifications.ReloadTexts();
            }
        }

        MyActionDescription IMyUseObject.GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description;
            if (!MySandboxGame.Config.ControlsHints)
            {
                description = new MyActionDescription {
                    Text = MyCommonTexts.CustomText,
                    IsTextControlHint = false
                };
                description.FormatParams = new object[] { this.m_displayedText };
                return description;
            }
            if (actionEnum == UseActionEnum.Manipulate)
            {
                MyInput.Static.GetGameControl(MyControlsSpace.USE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                description = new MyActionDescription {
                    Text = MyCommonTexts.NotificationPickupObject
                };
                description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]", this.m_displayedText };
                description.IsTextControlHint = true;
                description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]", this.m_displayedText };
                return description;
            }
            if (actionEnum != UseActionEnum.PickUp)
            {
                return new MyActionDescription();
            }
            MyInput.Static.GetGameControl(MyControlsSpace.PICK_UP).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
            description = new MyActionDescription {
                Text = MyCommonTexts.NotificationPickupObject
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.PICK_UP) + "]", this.m_displayedText };
            description.IsTextControlHint = true;
            description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.PICK_UP).ToString() + "]", this.m_displayedText };
            return description;
        }

        bool IMyUseObject.HandleInput() => 
            false;

        void IMyUseObject.OnSelectionLost()
        {
        }

        void IMyUseObject.SetInstanceID(int id)
        {
        }

        void IMyUseObject.SetRenderID(uint id)
        {
        }

        void IMyUseObject.Use(UseActionEnum actionEnum, VRage.ModAPI.IMyEntity entity)
        {
            MyCharacter thisEntity = entity as MyCharacter;
            if (!base.MarkedForClose)
            {
                MyFixedPoint amount = MyFixedPoint.Min(this.Item.Amount, thisEntity.GetInventory(0).ComputeAmountThatFits(this.Item.Content.GetId(), 0f, 0f));
                if (amount == 0)
                {
                    if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimePlayedSound) > 0x9c4)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudVocInventoryFull);
                        this.m_lastTimePlayedSound = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    }
                    MyHud.Stats.GetStat<MyStatPlayerInventoryFull>().InventoryFull = true;
                }
                else
                {
                    if (amount > 0)
                    {
                        if (ReferenceEquals(MySession.Static.ControlledEntity, thisEntity) && ((this.lastTimeSound == DateTime.MinValue) || ((DateTime.UtcNow - this.lastTimeSound).TotalMilliseconds > 500.0)))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.PlayTakeItem);
                            this.lastTimeSound = DateTime.UtcNow;
                        }
                        thisEntity.GetInventory(0).PickupItem(this, amount);
                    }
                    MyHud.Notifications.ReloadTexts();
                }
            }
        }

        bool IMyDestroyableObject.DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId) => 
            this.DoDamage(damage, damageType, sync, attackerId);

        void IMyDestroyableObject.OnDestroy()
        {
            this.OnDestroy();
        }

        public bool WasRemovedFromWorld { get; set; }

        public MyPhysicalItemDefinition ItemDefinition { get; private set; }

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public VRage.Sync.SyncType SyncType { get; set; }

        VRage.ModAPI.IMyEntity IMyUseObject.Owner =>
            this;

        MyModelDummy IMyUseObject.Dummy =>
            null;

        float IMyUseObject.InteractiveDistance =>
            MyConstants.FLOATING_OBJ_INTERACTIVE_DISTANCE;

        MatrixD IMyUseObject.ActivationMatrix
        {
            get
            {
                if (base.PositionComp == null)
                {
                    return MatrixD.Zero;
                }
                return (Matrix.CreateScale(base.PositionComp.LocalAABB.Size) * base.WorldMatrix);
            }
        }

        MatrixD IMyUseObject.WorldMatrix =>
            base.WorldMatrix;

        uint IMyUseObject.RenderObjectID =>
            ((base.Render.RenderObjectIDs.Length == 0) ? uint.MaxValue : base.Render.RenderObjectIDs[0]);

        int IMyUseObject.InstanceID =>
            -1;

        bool IMyUseObject.ShowOverlay =>
            false;

        UseActionEnum IMyUseObject.SupportedActions =>
            (MyFakes.ENABLE_SEPARATE_USE_AND_PICK_UP_KEY ? UseActionEnum.PickUp : UseActionEnum.Manipulate);

        UseActionEnum IMyUseObject.PrimaryAction =>
            (MyFakes.ENABLE_SEPARATE_USE_AND_PICK_UP_KEY ? UseActionEnum.PickUp : UseActionEnum.Manipulate);

        UseActionEnum IMyUseObject.SecondaryAction =>
            UseActionEnum.None;

        bool IMyUseObject.ContinuousUsage =>
            true;

        bool IMyUseObject.PlayIndicatorSound =>
            false;

        public float Integrity =>
            this.m_health;

        public bool UseDamageSystem { get; private set; }

        float IMyDestroyableObject.Integrity =>
            this.Integrity;

        bool IMyDestroyableObject.UseDamageSystem =>
            this.UseDamageSystem;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFloatingObject.<>c <>9 = new MyFloatingObject.<>c();
            public static Func<MyFloatingObject, Action> <>9__98_0;

            internal Action <SendCloseRequest>b__98_0(MyFloatingObject x) => 
                new Action(x.OnClosedRequest);
        }
    }
}

