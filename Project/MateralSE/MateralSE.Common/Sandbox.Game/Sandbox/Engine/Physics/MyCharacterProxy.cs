namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.Entity;
    using VRageMath;

    public class MyCharacterProxy
    {
        public const float MAX_SPRINT_SPEED = 7f;
        [CompilerGenerated]
        private ContactPointEventHandler ContactPointCallback;
        private bool m_isDynamic;
        private Vector3 m_gravity;
        private bool m_jump;
        private float m_posX;
        private float m_posY;
        private Vector3 m_angularVelocity;
        private float m_speed;
        private float m_maxSpeedRelativeToShip = 7f;
        private int m_airFrameCounter;
        private float m_mass;
        private float m_maxImpulse;
        private float m_maxCharacterSpeedSq;
        private bool m_isCrouching;
        private HkCharacterProxy CharacterProxy;
        private HkSimpleShapePhantom CharacterPhantom;
        private HkShape m_characterShape = HkShape.Empty;
        private HkShape m_crouchShape = HkShape.Empty;
        private HkShape m_characterCollisionShape = HkShape.Empty;
        private MyPhysicsBody m_physicsBody;
        private bool m_flyingStateEnabled;
        private HkRigidBody m_oldRigidBody;

        public event ContactPointEventHandler ContactPointCallback
        {
            [CompilerGenerated] add
            {
                ContactPointEventHandler contactPointCallback = this.ContactPointCallback;
                while (true)
                {
                    ContactPointEventHandler a = contactPointCallback;
                    ContactPointEventHandler handler3 = (ContactPointEventHandler) Delegate.Combine(a, value);
                    contactPointCallback = Interlocked.CompareExchange<ContactPointEventHandler>(ref this.ContactPointCallback, handler3, a);
                    if (ReferenceEquals(contactPointCallback, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ContactPointEventHandler contactPointCallback = this.ContactPointCallback;
                while (true)
                {
                    ContactPointEventHandler source = contactPointCallback;
                    ContactPointEventHandler handler3 = (ContactPointEventHandler) Delegate.Remove(source, value);
                    contactPointCallback = Interlocked.CompareExchange<ContactPointEventHandler>(ref this.ContactPointCallback, handler3, source);
                    if (ReferenceEquals(contactPointCallback, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyCharacterProxy(bool isDynamic, bool isCapsule, float characterWidth, float characterHeight, float crouchHeight, float ladderHeight, float headSize, float headHeight, Vector3 position, Vector3 up, Vector3 forward, float mass, MyPhysicsBody body, bool isOnlyVertical, float maxSlope, float maxImpulse, float maxSpeedRelativeToShip, float? maxForce = new float?(), HkRagdoll ragDoll = null)
        {
            this.m_isDynamic = isDynamic;
            this.m_physicsBody = body;
            this.m_mass = mass;
            this.m_maxImpulse = maxImpulse;
            this.m_maxSpeedRelativeToShip = maxSpeedRelativeToShip;
            if (!isCapsule)
            {
                HkBoxShape shape = new HkBoxShape(new Vector3(characterWidth / 2f, characterHeight / 2f, characterWidth / 2f));
                if (!this.m_isDynamic)
                {
                    this.CharacterPhantom = new HkSimpleShapePhantom((HkShape) shape, 0x12);
                }
                this.m_characterShape = (HkShape) shape;
            }
            else
            {
                this.m_characterShape = CreateCharacterShape(characterHeight, characterWidth, characterHeight + headHeight, headSize, 0f, 0f, false);
                this.m_characterCollisionShape = CreateCharacterShape(characterHeight * 0.9f, characterWidth * 0.9f, (characterHeight * 0.9f) + headHeight, headSize * 0.9f, 0f, 0f, false);
                this.m_crouchShape = CreateCharacterShape(characterHeight, characterWidth, characterHeight + headHeight, headSize, 0f, 1f, false);
                if (!this.m_isDynamic)
                {
                    this.CharacterPhantom = new HkSimpleShapePhantom(this.m_characterShape, 0x12);
                }
            }
            if (!this.m_isDynamic)
            {
                HkCharacterProxyCinfo characterProxyCinfo = new HkCharacterProxyCinfo {
                    StaticFriction = 1f,
                    DynamicFriction = 1f,
                    ExtraDownStaticFriction = 1000f,
                    MaxCharacterSpeedForSolver = 10000f,
                    RefreshManifoldInCheckSupport = true,
                    Up = up,
                    Forward = forward,
                    UserPlanes = 4,
                    MaxSlope = MathHelper.ToRadians(maxSlope),
                    Position = position,
                    CharacterMass = mass,
                    CharacterStrength = 100f,
                    ShapePhantom = this.CharacterPhantom
                };
                this.CharacterProxy = new HkCharacterProxy(characterProxyCinfo);
                characterProxyCinfo.Dispose();
            }
            else
            {
                HkCharacterRigidBodyCinfo characterRigidBodyCinfo = new HkCharacterRigidBodyCinfo {
                    Shape = this.m_characterShape,
                    CrouchShape = this.m_crouchShape,
                    Friction = 0f,
                    MaxSlope = MathHelper.ToRadians(maxSlope),
                    Up = up,
                    Mass = mass,
                    CollisionFilterInfo = 0x12,
                    MaxLinearVelocity = 1000000f,
                    MaxForce = (maxForce != null) ? maxForce.Value : 100000f,
                    AllowedPenetrationDepth = MyFakes.ENABLE_LIMITED_CHARACTER_BODY ? 0.3f : 0.1f,
                    JumpHeight = 0.8f
                };
                float maxCharacterSpeed = MyGridPhysics.ShipMaxLinearVelocity() + this.m_maxSpeedRelativeToShip;
                this.CharacterRigidBody = new HkCharacterRigidBody(characterRigidBodyCinfo, maxCharacterSpeed, body);
                this.m_maxCharacterSpeedSq = maxCharacterSpeed * maxCharacterSpeed;
                this.CharacterRigidBody.GetRigidBody().ContactPointCallbackEnabled = true;
                this.CharacterRigidBody.GetRigidBody().ContactPointCallback -= new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                this.CharacterRigidBody.GetRigidBody().ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                this.CharacterRigidBody.GetRigidBody().ContactPointCallbackDelay = 0;
                Matrix inertiaTensor = this.CharacterRigidBody.GetHitRigidBody().InertiaTensor;
                inertiaTensor.M11 = 1000f;
                inertiaTensor.M22 = 1000f;
                inertiaTensor.M33 = 1000f;
                this.CharacterRigidBody.GetHitRigidBody().InertiaTensor = inertiaTensor;
                characterRigidBodyCinfo.Dispose();
            }
        }

        public void Activate(HkWorld world)
        {
            if (this.CharacterPhantom != null)
            {
                world.AddPhantom(this.CharacterPhantom);
            }
            if (this.CharacterRigidBody != null)
            {
                world.AddCharacterRigidBody(this.CharacterRigidBody);
                if (!float.IsInfinity(this.m_maxImpulse))
                {
                    world.BreakOffPartsUtil.MarkEntityBreakable(this.CharacterRigidBody.GetRigidBody(), this.m_maxImpulse);
                }
            }
        }

        public void ApplyAngularImpulse(Vector3 impulse)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.ApplyAngularImpulse(impulse);
            }
        }

        public void ApplyGravity(Vector3 gravity)
        {
            HkCharacterRigidBody characterRigidBody = this.CharacterRigidBody;
            characterRigidBody.LinearVelocity += gravity * 0.01666667f;
            if (this.CharacterRigidBody.LinearVelocity.LengthSquared() > this.m_maxCharacterSpeedSq)
            {
                Vector3 linearVelocity = this.CharacterRigidBody.LinearVelocity;
                linearVelocity.Normalize();
                linearVelocity *= MyGridPhysics.ShipMaxLinearVelocity() + this.MaxSpeedRelativeToShip;
                this.CharacterRigidBody.LinearVelocity = linearVelocity;
            }
        }

        public void ApplyLinearImpulse(Vector3 impulse)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.ApplyLinearImpulse(impulse);
            }
        }

        public float CharacterFlyingMaxLinearVelocity() => 
            (this.m_maxSpeedRelativeToShip + MyGridPhysics.ShipMaxLinearVelocity());

        public float CharacterWalkingMaxLinearVelocity() => 
            (this.m_maxSpeedRelativeToShip + MyGridPhysics.ShipMaxLinearVelocity());

        public static HkShape CreateCharacterShape(float height, float width, float headHeight, float headSize, float headForwardOffset, float downOffset = 0f, bool capsuleForHead = false)
        {
            HkCapsuleShape shape = new HkCapsuleShape((Vector3.Up * (height - downOffset)) / 2f, (Vector3.Down * height) / 2f, width / 2f);
            if (headSize <= 0f)
            {
                return (HkShape) shape;
            }
            HkConvexShape childShape = !capsuleForHead ? ((HkConvexShape) new HkSphereShape(headSize)) : ((HkConvexShape) new HkCapsuleShape(new Vector3(0f, 0f, -0.3f), new Vector3(0f, 0f, 0.3f), headSize));
            HkShape[] shapes = new HkShape[] { (HkShape) shape, (HkShape) new HkConvexTranslateShape(childShape, ((Vector3.Up * (headHeight - downOffset)) / 2f) + (Vector3.Forward * headForwardOffset), HkReferencePolicy.TakeOwnership) };
            return (HkShape) new HkListShape(shapes, shapes.Length, HkReferencePolicy.TakeOwnership);
        }

        public void Deactivate(HkWorld world)
        {
            if (this.CharacterPhantom != null)
            {
                world.RemovePhantom(this.CharacterPhantom);
            }
            if (this.CharacterRigidBody != null)
            {
                world.RemoveCharacterRigidBody(this.CharacterRigidBody);
            }
        }

        public void Dispose()
        {
            if (this.CharacterProxy != null)
            {
                this.CharacterProxy.Dispose();
                this.CharacterProxy = null;
            }
            if (this.CharacterPhantom != null)
            {
                this.CharacterPhantom.Dispose();
                this.CharacterPhantom = null;
            }
            if (this.CharacterRigidBody != null)
            {
                if (this.CharacterRigidBody.GetRigidBody() != null)
                {
                    this.CharacterRigidBody.GetRigidBody().ContactPointCallback -= new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                }
                this.CharacterRigidBody.Dispose();
                this.CharacterRigidBody = null;
            }
            this.m_characterShape.RemoveReference();
            this.m_characterCollisionShape.RemoveReference();
            this.m_crouchShape.RemoveReference();
        }

        public void EnableFlyingState(bool enable)
        {
            float maxCharacterSpeed = MyGridPhysics.ShipMaxLinearVelocity() + this.m_maxSpeedRelativeToShip;
            this.m_physicsBody.ShapeChangeInProgress = true;
            this.EnableFlyingState(enable, maxCharacterSpeed, MyGridPhysics.ShipMaxLinearVelocity() + this.m_maxSpeedRelativeToShip, 9f);
            this.m_physicsBody.ShapeChangeInProgress = false;
        }

        public void EnableFlyingState(bool enable, float maxCharacterSpeed, float maxFlyingSpeed, float maxAcceleration)
        {
            if (this.m_flyingStateEnabled != enable)
            {
                if (this.CharacterRigidBody != null)
                {
                    this.m_physicsBody.ShapeChangeInProgress = true;
                    this.CharacterRigidBody.EnableFlyingState(enable, maxCharacterSpeed, maxFlyingSpeed, maxAcceleration);
                    this.m_physicsBody.ShapeChangeInProgress = false;
                }
                this.StepSimulation(0.01666667f);
                this.m_flyingStateEnabled = enable;
            }
        }

        public void EnableLadderState(bool enable)
        {
            this.EnableLadderState(enable, MyGridPhysics.ShipMaxLinearVelocity(), 1f);
        }

        public void EnableLadderState(bool enable, float maxCharacterSpeed, float maxAcceleration)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.EnableLadderState(enable, maxCharacterSpeed, maxAcceleration);
            }
        }

        public HkShape GetCollisionShape() => 
            this.m_characterCollisionShape;

        public HkRigidBody GetHitRigidBody() => 
            this.CharacterRigidBody?.GetHitRigidBody();

        public MyPhysicsBody GetPhysicsBody() => 
            ((this.m_physicsBody == null) ? null : this.m_physicsBody);

        public HkEntity GetRigidBody() => 
            this.CharacterRigidBody?.GetRigidBody();

        public Matrix GetRigidBodyTransform() => 
            ((this.CharacterRigidBody == null) ? Matrix.Identity : this.CharacterRigidBody.GetRigidBodyTransform());

        public HkShape GetShape() => 
            this.m_characterShape;

        public HkCharacterStateType GetState()
        {
            if (!this.m_isDynamic)
            {
                return this.CharacterProxy.GetState();
            }
            HkCharacterStateType state = this.CharacterRigidBody.GetState();
            if (state != HkCharacterStateType.HK_CHARACTER_ON_GROUND)
            {
                this.m_airFrameCounter++;
            }
            if (state == HkCharacterStateType.HK_CHARACTER_ON_GROUND)
            {
                this.m_airFrameCounter = 0;
            }
            if ((state == HkCharacterStateType.HK_CHARACTER_IN_AIR) && (this.m_airFrameCounter < 8))
            {
                state = HkCharacterStateType.HK_CHARACTER_ON_GROUND;
            }
            if (this.AtLadder)
            {
                state = HkCharacterStateType.HK_CHARACTER_CLIMBING;
            }
            return state;
        }

        public void GetSupportingEntities(List<VRage.Game.Entity.MyEntity> outEntities)
        {
            if (this.CharacterRigidBody != null)
            {
                using (List<HkRigidBody>.Enumerator enumerator = this.CharacterRigidBody.GetSupportInfo().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyPhysicsBody userObject = (MyPhysicsBody) enumerator.Current.UserObject;
                        if (userObject != null)
                        {
                            VRage.Game.Entity.MyEntity item = (VRage.Game.Entity.MyEntity) userObject.Entity;
                            if ((item != null) && !item.MarkedForClose)
                            {
                                outEntities.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent value)
        {
            if (this.ContactPointCallback != null)
            {
                this.ContactPointCallback(ref value);
            }
        }

        public void SetCollisionFilterInfo(uint info)
        {
            if (this.m_isDynamic)
            {
                this.CharacterRigidBody.SetCollisionFilterInfo(info);
            }
        }

        public void SetForwardAndUp(Vector3 forward, Vector3 up)
        {
            Matrix rigidBodyTransform = this.GetRigidBodyTransform();
            rigidBodyTransform.Up = up;
            rigidBodyTransform.Forward = forward;
            rigidBodyTransform.Right = Vector3.Cross(forward, up);
            this.SetRigidBodyTransform(ref rigidBodyTransform);
        }

        public void SetHardSupportDistance(float distance)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.SetHardSupportDistance(distance);
            }
        }

        public void SetRigidBodyTransform(ref Matrix m)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.SetRigidBodyTransform(ref m);
            }
        }

        public void SetShapeForCrouch(HkWorld world, bool enable)
        {
            if ((this.CharacterRigidBody != null) && (world != null))
            {
                world.Lock();
                this.m_physicsBody.ShapeChangeInProgress = true;
                if (enable)
                {
                    this.CharacterRigidBody.SetShapeForCrouch();
                }
                else
                {
                    this.CharacterRigidBody.SetDefaultShape();
                }
                if (this.m_physicsBody.IsInWorld)
                {
                    world.ReintegrateCharacter(this.CharacterRigidBody);
                }
                this.m_physicsBody.ShapeChangeInProgress = false;
                world.Unlock();
                this.m_isCrouching = enable;
            }
        }

        public void SetState(HkCharacterStateType state)
        {
            if (this.m_isDynamic)
            {
                this.CharacterRigidBody.SetState(state);
            }
            else
            {
                this.CharacterProxy.SetState(state);
            }
        }

        public void SetSupportDistance(float distance)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.SetSupportDistance(distance);
            }
        }

        public void SetSupportedState(bool supported)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.SetPreviousSupportedState(supported);
            }
        }

        public void SkipSimulation(MatrixD mat)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.Position = (Vector3) mat.Translation;
                this.CharacterRigidBody.Forward = (Vector3) mat.Forward;
                this.CharacterRigidBody.Up = (Vector3) mat.Up;
                this.Supported = this.CharacterRigidBody.Supported;
                this.SupportNormal = this.CharacterRigidBody.SupportNormal;
                this.GroundVelocity = this.CharacterRigidBody.GroundVelocity;
            }
        }

        public void Stand()
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.ResetSurfaceVelocity();
            }
        }

        public void StepSimulation(float stepSizeInSeconds)
        {
            if (!this.AtLadder)
            {
                if (this.CharacterProxy != null)
                {
                    this.CharacterProxy.PosX = this.m_posX;
                    this.CharacterProxy.PosY = this.m_posY;
                    this.CharacterProxy.Jump = this.m_jump;
                    this.m_jump = false;
                    this.CharacterProxy.Gravity = this.m_gravity;
                    this.CharacterProxy.StepSimulation(stepSizeInSeconds);
                }
                if (this.CharacterRigidBody != null)
                {
                    this.CharacterRigidBody.PosX = this.m_posX;
                    this.CharacterRigidBody.PosY = this.m_posY;
                    this.CharacterRigidBody.Jump = this.m_jump;
                    this.m_jump = false;
                    this.CharacterRigidBody.Gravity = this.m_gravity;
                    this.CharacterRigidBody.Speed = this.Speed;
                    this.CharacterRigidBody.StepSimulation(stepSizeInSeconds);
                    this.CharacterRigidBody.Elevate = this.Elevate;
                    this.Supported = this.CharacterRigidBody.Supported;
                    this.SupportNormal = this.CharacterRigidBody.SupportNormal;
                    this.GroundVelocity = this.CharacterRigidBody.GroundVelocity;
                    this.GroundAngularVelocity = this.CharacterRigidBody.AngularVelocity;
                }
            }
        }

        public void UpdateSupport(float stepSizeInSeconds)
        {
            if (this.CharacterRigidBody != null)
            {
                this.CharacterRigidBody.UpdateSupport(stepSizeInSeconds);
                this.Supported = this.CharacterRigidBody.Supported;
                this.SupportNormal = this.CharacterRigidBody.SupportNormal;
                this.GroundVelocity = this.CharacterRigidBody.GroundVelocity;
            }
        }

        public HkCharacterRigidBody CharacterRigidBody { get; private set; }

        public Vector3 LinearVelocity
        {
            get => 
                (!this.m_isDynamic ? this.CharacterProxy.LinearVelocity : this.CharacterRigidBody.LinearVelocity);
            set
            {
                if (this.m_isDynamic)
                {
                    this.CharacterRigidBody.LinearVelocity = value;
                }
                else
                {
                    this.CharacterProxy.LinearVelocity = value;
                }
            }
        }

        public Vector3 Forward =>
            (!this.m_isDynamic ? this.CharacterProxy.Forward : this.CharacterRigidBody.Forward);

        public Vector3 Up =>
            (!this.m_isDynamic ? this.CharacterProxy.Up : this.CharacterRigidBody.Up);

        public Vector3 Gravity
        {
            get => 
                this.m_gravity;
            set => 
                (this.m_gravity = value);
        }

        public float Elevate
        {
            get => 
                (!this.m_isDynamic ? 0f : this.CharacterRigidBody.Elevate);
            set
            {
                if (this.m_isDynamic)
                {
                    this.CharacterRigidBody.Elevate = value;
                }
            }
        }

        public bool AtLadder
        {
            get => 
                (this.m_isDynamic && this.CharacterRigidBody.AtLadder);
            set
            {
                if (this.m_isDynamic)
                {
                    this.CharacterRigidBody.AtLadder = value;
                }
            }
        }

        public Vector3 ElevateVector
        {
            get => 
                (!this.m_isDynamic ? Vector3.Zero : this.CharacterRigidBody.ElevateVector);
            set
            {
                if (this.m_isDynamic)
                {
                    this.CharacterRigidBody.ElevateVector = value;
                }
            }
        }

        public Vector3 ElevateUpVector
        {
            get => 
                (!this.m_isDynamic ? Vector3.Zero : this.CharacterRigidBody.ElevateUpVector);
            set
            {
                if (this.m_isDynamic)
                {
                    this.CharacterRigidBody.ElevateUpVector = value;
                }
            }
        }

        public bool Jump
        {
            set => 
                (this.m_jump = value);
        }

        public Vector3 Position
        {
            get => 
                (!this.m_isDynamic ? this.CharacterProxy.Position : this.CharacterRigidBody.Position);
            set
            {
                if (this.m_isDynamic)
                {
                    this.CharacterRigidBody.Position = value;
                }
                else
                {
                    this.CharacterProxy.Position = value;
                }
            }
        }

        public float PosX
        {
            set => 
                (this.m_posX = MathHelper.Clamp(value, -1f, 1f));
        }

        public float PosY
        {
            set => 
                (this.m_posY = MathHelper.Clamp(value, -1f, 1f));
        }

        public Vector3 AngularVelocity
        {
            get => 
                ((this.CharacterRigidBody != null) ? this.CharacterRigidBody.GetAngularVelocity() : this.m_angularVelocity);
            set
            {
                this.m_angularVelocity = value;
                if (this.CharacterRigidBody != null)
                {
                    this.CharacterRigidBody.AngularVelocity = this.m_angularVelocity;
                    this.CharacterRigidBody.SetAngularVelocity(this.m_angularVelocity);
                }
            }
        }

        public float Speed
        {
            get => 
                this.m_speed;
            set => 
                (this.m_speed = value);
        }

        public bool Supported { get; private set; }

        public Vector3 SupportNormal { get; private set; }

        public Vector3 GroundVelocity { get; private set; }

        public Vector3 GroundAngularVelocity { get; private set; }

        public bool IsCrouching =>
            this.m_isCrouching;

        public bool ImmediateSetWorldTransform { get; set; }

        public bool ContactPointCallbackEnabled
        {
            get => 
                ((this.CharacterRigidBody != null) && this.CharacterRigidBody.ContactPointCallbackEnabled);
            set
            {
                if (this.CharacterRigidBody != null)
                {
                    this.CharacterRigidBody.ContactPointCallbackEnabled = value;
                }
            }
        }

        public float Mass =>
            this.m_mass;

        public float MaxSpeedRelativeToShip =>
            this.m_maxSpeedRelativeToShip;

        public float MaxSlope
        {
            get => 
                ((this.CharacterRigidBody == null) ? 0f : this.CharacterRigidBody.MaxSlope);
            set
            {
                if (this.CharacterRigidBody != null)
                {
                    this.CharacterRigidBody.MaxSlope = value;
                }
            }
        }
    }
}

