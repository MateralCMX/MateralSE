namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRageMath;

    public static class MyPhysicsExtensions
    {
        [ThreadStatic]
        private static List<IMyEntity> m_entityList;

        public static bool ActivateIfNeeded(this MyPhysicsComponentBase body)
        {
            if (body.IsActive || body.IsStatic)
            {
                return false;
            }
            body.ForceActivate();
            return true;
        }

        public static List<IMyEntity> GetAllEntities(this HkEntity hkEntity)
        {
            MyPhysicsBody body = hkEntity.GetBody();
            if (body != null)
            {
                EntityList.Add(body.Entity);
                foreach (MyPhysicsBody body2 in body.WeldInfo.Children)
                {
                    EntityList.Add(body2.Entity);
                }
            }
            return EntityList;
        }

        [IteratorStateMachine(typeof(<GetAllShapes>d__26))]
        public static IEnumerable<HkShape> GetAllShapes(this HkShape shape)
        {
            <GetAllShapes>d__26 d__1 = new <GetAllShapes>d__26(-2);
            d__1.<>3__shape = shape;
            return d__1;
        }

        public static MyPhysicsBody GetBody(this HkEntity hkEntity) => 
            ((hkEntity != null) ? (hkEntity.UserObject as MyPhysicsBody) : null);

        public static IMyEntity GetCollisionEntity(this HkBodyCollision collision) => 
            ((collision.Body != null) ? collision.Body.GetEntity(0) : null);

        public static float GetConvexRadius(this HkWorld.HitInfo hitInfo)
        {
            if (hitInfo.Body == null)
            {
                return 0f;
            }
            HkShape shape = hitInfo.Body.GetShape();
            int index = 0;
            while (true)
            {
                if (index < HkWorld.HitInfo.ShapeKeyCount)
                {
                    uint shapeKey = hitInfo.GetShapeKey(index);
                    if ((uint.MaxValue != shapeKey) && shape.IsContainer())
                    {
                        index++;
                        continue;
                    }
                }
                if (((shape.ShapeType == HkShapeType.ConvexTransform) || (shape.ShapeType == HkShapeType.ConvexTranslate)) || (shape.ShapeType == HkShapeType.Transform))
                {
                    shape = shape.GetContainer().GetShape(0);
                }
                if ((shape.ShapeType == HkShapeType.Sphere) || (shape.ShapeType == HkShapeType.Capsule))
                {
                    return 0f;
                }
                return (shape.IsConvex ? shape.ConvexRadius : HkConvexShape.DefaultConvexRadius);
            }
        }

        public static IMyEntity GetEntity(this HkEntity hkEntity, uint shapeKey)
        {
            MyPhysicsBody body = hkEntity.GetBody();
            if (body != null)
            {
                if (shapeKey == 0)
                {
                    return body.Entity;
                }
                if (shapeKey > body.WeldInfo.Children.Count)
                {
                    return body.Entity;
                }
                HkShape shape = body.RigidBody.GetShape().GetContainer().GetShape(shapeKey);
                if (shape.IsValid)
                {
                    body = HkRigidBody.FromShape(shape).GetBody();
                }
            }
            return body?.Entity;
        }

        public static Vector3 GetFixedPosition(this MyPhysics.HitInfo hitInfo)
        {
            Vector3 position = (Vector3) hitInfo.Position;
            float convexRadius = hitInfo.HkHitInfo.GetConvexRadius();
            if (convexRadius != 0f)
            {
                position += -hitInfo.HkHitInfo.Normal * convexRadius;
            }
            return position;
        }

        public static MyGridContactInfo.ContactFlags GetFlags(this HkContactPointProperties cp) => 
            ((MyGridContactInfo.ContactFlags) cp.UserData.AsUint);

        public static IMyEntity GetHitEntity(this HkWorld.HitInfo hitInfo) => 
            hitInfo.Body.GetEntity(hitInfo.GetShapeKey(0));

        public static unsafe HkShape GetHitShape(this HkShape shape, ref HkContactPointEvent contactEvent, int bodyIndex, bool checkMissingKeys = true, bool ImNotSureThatShapeKeysAreStillValid = false)
        {
            uint* numPtr = (uint*) stackalloc byte[0x10];
            int num = 0;
            int index = 0;
            while (true)
            {
                if (index < 4)
                {
                    numPtr[index] = contactEvent.GetShapeKey(bodyIndex, index);
                    if (numPtr[index] != uint.MaxValue)
                    {
                        num++;
                        index++;
                        continue;
                    }
                }
                int num3 = num - 1;
                while (true)
                {
                    if (num3 >= 0)
                    {
                        uint shapeKey = numPtr[num3];
                        if (!shape.IsContainer())
                        {
                            shape = HkShape.Empty;
                        }
                        else
                        {
                            HkShapeContainerIterator container = shape.GetContainer();
                            if (container.IsValid)
                            {
                                if (!ImNotSureThatShapeKeysAreStillValid || container.IsShapeKeyValid(shapeKey))
                                {
                                    shape = container.GetShape(shapeKey);
                                }
                                else
                                {
                                    shape = HkShape.Empty;
                                }
                                if (shape.IsZero)
                                {
                                    bool flag1 = checkMissingKeys;
                                    return HkShape.Empty;
                                }
                                num3--;
                                continue;
                            }
                            shape = HkShape.Empty;
                        }
                    }
                    bool isZero = shape.IsZero;
                    return shape;
                }
            }
        }

        public static unsafe uint GetHitTriangleMaterial(this MyVoxelPhysicsBody voxelBody, ref HkContactPointEvent contactEvent, int bodyIndex)
        {
            uint* numPtr = (uint*) stackalloc byte[0x10];
            int num = 0;
            int index = 0;
            while (true)
            {
                if (index < 4)
                {
                    numPtr[index] = contactEvent.GetShapeKey(bodyIndex, index);
                    if (numPtr[index] != uint.MaxValue)
                    {
                        num++;
                        index++;
                        continue;
                    }
                }
                if (num == 2)
                {
                    HkShape shape2 = voxelBody.GetShape();
                    if (!shape2.IsZero && (shape2.ShapeType == HkShapeType.BvTree))
                    {
                        shape2 = shape2.GetContainer().GetShape(numPtr[1]);
                        if (!shape2.IsZero && (shape2.ShapeType == HkShapeType.BvCompressedMesh))
                        {
                            uint userData = shape2.UserData;
                            if (userData != uint.MaxValue)
                            {
                                return userData;
                            }
                        }
                    }
                }
                HkShape shape = voxelBody.GetShape().GetHitShape(ref contactEvent, bodyIndex, false, true);
                return (!shape.IsZero ? shape.UserData : uint.MaxValue);
            }
        }

        public static IMyEntity GetOtherEntity(this HkCollisionEvent eventInfo, IMyEntity sourceEntity)
        {
            bool flag;
            return eventInfo.GetOtherEntity(sourceEntity, out flag);
        }

        public static IMyEntity GetOtherEntity(this HkContactPointEvent eventInfo, IMyEntity sourceEntity)
        {
            bool flag;
            return eventInfo.Base.GetOtherEntity(sourceEntity, out flag);
        }

        public static IMyEntity GetOtherEntity(this HkCollisionEvent eventInfo, IMyEntity sourceEntity, out bool AisThis)
        {
            MyPhysicsBody physicsBody = eventInfo.GetPhysicsBody(0);
            MyPhysicsBody body2 = eventInfo.GetPhysicsBody(1);
            IMyEntity objB = physicsBody?.Entity;
            IMyEntity entity = body2?.Entity;
            if (ReferenceEquals(sourceEntity, objB))
            {
                AisThis = true;
                return entity;
            }
            AisThis = false;
            return objB;
        }

        public static IMyEntity GetOtherEntity(this HkContactPointEvent eventInfo, IMyEntity sourceEntity, out bool AisThis) => 
            eventInfo.Base.GetOtherEntity(sourceEntity, out AisThis);

        public static MyPhysicsBody GetOtherPhysicsBody(this HkContactPointEvent eventInfo, IMyEntity sourceEntity)
        {
            MyPhysicsBody physicsBody = eventInfo.GetPhysicsBody(0);
            MyPhysicsBody body2 = eventInfo.GetPhysicsBody(1);
            IMyEntity objB = physicsBody?.Entity;
            if (body2 != null)
            {
                IMyEntity entity = body2.Entity;
            }
            return (ReferenceEquals(sourceEntity, objB) ? body2 : physicsBody);
        }

        public static MyPhysicsBody GetPhysicsBody(this HkCollisionEvent eventInfo, int index)
        {
            HkRigidBody rigidBody = eventInfo.GetRigidBody(index);
            if (rigidBody == null)
            {
                return null;
            }
            MyPhysicsBody body = rigidBody.GetBody();
            if (body != null)
            {
                bool isWelded = body.IsWelded;
            }
            return body;
        }

        public static MyPhysicsBody GetPhysicsBody(this HkContactPointEvent eventInfo, int index) => 
            eventInfo.Base.GetPhysicsBody(index);

        [IteratorStateMachine(typeof(<GetShapeKeys>d__29))]
        public static IEnumerable<uint> GetShapeKeys(this HkShape shape)
        {
            <GetShapeKeys>d__29 d__1 = new <GetShapeKeys>d__29(-2);
            d__1.<>3__shape = shape;
            return d__1;
        }

        public static IMyEntity GetSingleEntity(this HkEntity hkEntity)
        {
            MyPhysicsBody body = hkEntity.GetBody();
            return body?.Entity;
        }

        public static bool HasFlag(this HkContactPointProperties cp, MyGridContactInfo.ContactFlags flag) => 
            ((((MyGridContactInfo.ContactFlags) cp.UserData.AsUint) & flag) != ((MyGridContactInfo.ContactFlags) 0));

        public static bool IsInWorldWelded(this MyPhysicsBody body) => 
            (body.IsInWorld || ((body.WeldInfo.Parent != null) && body.WeldInfo.Parent.IsInWorld));

        public static bool IsInWorldWelded(this MyPhysicsComponentBase body) => 
            ((body != null) && ((body is MyPhysicsBody) && ((MyPhysicsBody) body).IsInWorldWelded()));

        public static void SetFlag(this HkContactPointProperties cp, MyGridContactInfo.ContactFlags flag)
        {
            MyGridContactInfo.ContactFlags flags = ((MyGridContactInfo.ContactFlags) cp.UserData.AsUint) | flag;
            cp.UserData = HkContactUserData.UInt((uint) flags);
        }

        public static void SetInBodySpace(this HkBallAndSocketConstraintData data, Vector3 pivotA, Vector3 pivotB, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(pivotA, bodyA.WeldInfo.Transform);
                pivotA = vector1;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector2 = Vector3.Transform(pivotB, bodyB.WeldInfo.Transform);
                pivotB = vector2;
            }
            data.SetInBodySpaceInternal(ref pivotA, ref pivotB);
        }

        public static void SetInBodySpace(this HkFixedConstraintData data, Matrix pivotA, Matrix pivotB, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if ((bodyA != null) && bodyA.IsWelded)
            {
                pivotA *= bodyA.WeldInfo.Transform;
            }
            if ((bodyB != null) && bodyB.IsWelded)
            {
                pivotB *= bodyB.WeldInfo.Transform;
            }
            data.SetInBodySpaceInternal(ref pivotA, ref pivotB);
        }

        public static void SetInBodySpace(this HkRopeConstraintData data, Vector3 pivotA, Vector3 pivotB, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(pivotA, bodyA.WeldInfo.Transform);
                pivotA = vector1;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector2 = Vector3.Transform(pivotB, bodyB.WeldInfo.Transform);
                pivotB = vector2;
            }
            data.SetInBodySpaceInternal(ref pivotA, ref pivotB);
        }

        public static void SetInBodySpace(this HkHingeConstraintData data, Vector3 posA, Vector3 posB, Vector3 axisA, Vector3 axisB, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(posA, bodyA.WeldInfo.Transform);
                posA = vector1;
                Vector3 vector2 = Vector3.TransformNormal(axisA, bodyA.WeldInfo.Transform);
                axisA = vector2;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector3 = Vector3.Transform(posB, bodyB.WeldInfo.Transform);
                posB = vector3;
                Vector3 vector4 = Vector3.TransformNormal(axisB, bodyB.WeldInfo.Transform);
                axisB = vector4;
            }
            data.SetInBodySpaceInternal(ref posA, ref posB, ref axisA, ref axisB);
        }

        public static void SetInBodySpace(this HkCogWheelConstraintData data, Vector3 pivotA, Vector3 rotationA, float radius1, Vector3 pivotB, Vector3 rotationB, float radius2, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(pivotA, bodyA.WeldInfo.Transform);
                pivotA = vector1;
                Vector3 vector2 = Vector3.TransformNormal(rotationA, bodyA.WeldInfo.Transform);
                rotationA = vector2;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector3 = Vector3.Transform(pivotB, bodyB.WeldInfo.Transform);
                pivotB = vector3;
                Vector3 vector4 = Vector3.TransformNormal(rotationB, bodyB.WeldInfo.Transform);
                rotationB = vector4;
            }
            data.SetInBodySpaceInternal(ref pivotA, ref rotationA, radius1, ref pivotB, ref rotationB, radius2);
        }

        public static void SetInBodySpace(this HkCustomWheelConstraintData data, Vector3 posA, Vector3 posB, Vector3 axisA, Vector3 axisB, Vector3 suspension, Vector3 steering, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(posA, bodyA.WeldInfo.Transform);
                posA = vector1;
                Vector3 vector2 = Vector3.TransformNormal(axisA, bodyA.WeldInfo.Transform);
                axisA = vector2;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector3 = Vector3.Transform(posB, bodyB.WeldInfo.Transform);
                posB = vector3;
                Vector3 vector4 = Vector3.TransformNormal(axisB, bodyB.WeldInfo.Transform);
                axisB = vector4;
                Vector3 vector5 = Vector3.TransformNormal(suspension, bodyB.WeldInfo.Transform);
                suspension = vector5;
                Vector3 vector6 = Vector3.TransformNormal(steering, bodyB.WeldInfo.Transform);
                steering = vector6;
            }
            data.SetInBodySpaceInternal(ref posA, ref posB, ref axisA, ref axisB, ref suspension, ref steering);
        }

        public static void SetInBodySpace(this HkLimitedHingeConstraintData data, Vector3 posA, Vector3 posB, Vector3 axisA, Vector3 axisB, Vector3 axisAPerp, Vector3 axisBPerp, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(posA, bodyA.WeldInfo.Transform);
                posA = vector1;
                Vector3 vector2 = Vector3.TransformNormal(axisA, bodyA.WeldInfo.Transform);
                axisA = vector2;
                Vector3 vector3 = Vector3.TransformNormal(axisAPerp, bodyA.WeldInfo.Transform);
                axisAPerp = vector3;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector4 = Vector3.Transform(posB, bodyB.WeldInfo.Transform);
                posB = vector4;
                Vector3 vector5 = Vector3.TransformNormal(axisB, bodyB.WeldInfo.Transform);
                axisB = vector5;
                Vector3 vector6 = Vector3.TransformNormal(axisBPerp, bodyB.WeldInfo.Transform);
                axisBPerp = vector6;
            }
            data.SetInBodySpaceInternal(ref posA, ref posB, ref axisA, ref axisB, ref axisAPerp, ref axisBPerp);
        }

        public static void SetInBodySpace(this HkPrismaticConstraintData data, Vector3 posA, Vector3 posB, Vector3 axisA, Vector3 axisB, Vector3 axisAPerp, Vector3 axisBPerp, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(posA, bodyA.WeldInfo.Transform);
                posA = vector1;
                Vector3 vector2 = Vector3.TransformNormal(axisA, bodyA.WeldInfo.Transform);
                axisA = vector2;
                Vector3 vector3 = Vector3.TransformNormal(axisAPerp, bodyA.WeldInfo.Transform);
                axisAPerp = vector3;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector4 = Vector3.Transform(posB, bodyB.WeldInfo.Transform);
                posB = vector4;
                Vector3 vector5 = Vector3.TransformNormal(axisB, bodyB.WeldInfo.Transform);
                axisB = vector5;
                Vector3 vector6 = Vector3.TransformNormal(axisBPerp, bodyB.WeldInfo.Transform);
                axisBPerp = vector6;
            }
            data.SetInBodySpaceInternal(ref posA, ref posB, ref axisA, ref axisB, ref axisAPerp, ref axisBPerp);
        }

        public static void SetInBodySpace(this HkWheelConstraintData data, Vector3 posA, Vector3 posB, Vector3 axisA, Vector3 axisB, Vector3 suspension, Vector3 steering, MyPhysicsBody bodyA, MyPhysicsBody bodyB)
        {
            if (bodyA.IsWelded)
            {
                Vector3 vector1 = Vector3.Transform(posA, bodyA.WeldInfo.Transform);
                posA = vector1;
                Vector3 vector2 = Vector3.TransformNormal(axisA, bodyA.WeldInfo.Transform);
                axisA = vector2;
            }
            if (bodyB.IsWelded)
            {
                Vector3 vector3 = Vector3.Transform(posB, bodyB.WeldInfo.Transform);
                posB = vector3;
                Vector3 vector4 = Vector3.TransformNormal(axisB, bodyB.WeldInfo.Transform);
                axisB = vector4;
                Vector3 vector5 = Vector3.TransformNormal(suspension, bodyB.WeldInfo.Transform);
                suspension = vector5;
                Vector3 vector6 = Vector3.TransformNormal(steering, bodyB.WeldInfo.Transform);
                steering = vector6;
            }
            data.SetInBodySpaceInternal(ref posA, ref posB, ref axisA, ref axisB, ref suspension, ref steering);
        }

        private static List<IMyEntity> EntityList
        {
            get
            {
                if (m_entityList == null)
                {
                    m_entityList = new List<IMyEntity>();
                }
                return m_entityList;
            }
        }

        [CompilerGenerated]
        private sealed class <GetAllShapes>d__26 : IEnumerable<HkShape>, IEnumerable, IEnumerator<HkShape>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private HkShape <>2__current;
            private int <>l__initialThreadId;
            private HkShape shape;
            public HkShape <>3__shape;
            private HkShapeContainerIterator <iterator>5__2;
            private IEnumerator<HkShape> <>7__wrap2;

            [DebuggerHidden]
            public <GetAllShapes>d__26(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = System.Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap2 != null)
                {
                    this.<>7__wrap2.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            if (!this.shape.IsContainer())
                            {
                                this.<>2__current = this.shape;
                                this.<>1__state = 2;
                                return true;
                            }
                            else
                            {
                                this.<iterator>5__2 = this.shape.GetContainer();
                            }
                            break;

                        case 1:
                            this.<>1__state = -3;
                            goto TR_0006;

                        case 2:
                            this.<>1__state = -1;
                            return false;

                        default:
                            return false;
                    }
                    goto TR_0009;
                TR_0006:
                    if (this.<>7__wrap2.MoveNext())
                    {
                        HkShape current = this.<>7__wrap2.Current;
                        this.<>2__current = current;
                        this.<>1__state = 1;
                        flag = true;
                    }
                    else
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap2 = null;
                        this.<iterator>5__2.Next();
                        goto TR_0009;
                    }
                    return flag;
                TR_0009:
                    while (true)
                    {
                        if (this.<iterator>5__2.CurrentShapeKey != uint.MaxValue)
                        {
                            this.<>7__wrap2 = this.<iterator>5__2.CurrentValue.GetAllShapes().GetEnumerator();
                            this.<>1__state = -3;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    }
                    goto TR_0006;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<HkShape> IEnumerable<HkShape>.GetEnumerator()
            {
                MyPhysicsExtensions.<GetAllShapes>d__26 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != System.Environment.CurrentManagedThreadId))
                {
                    d__ = new MyPhysicsExtensions.<GetAllShapes>d__26(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.shape = this.<>3__shape;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<Havok.HkShape>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            HkShape IEnumerator<HkShape>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <GetShapeKeys>d__29 : IEnumerable<uint>, IEnumerable, IEnumerator<uint>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private uint <>2__current;
            private int <>l__initialThreadId;
            private HkShape shape;
            public HkShape <>3__shape;
            private HkShapeContainerIterator <it>5__2;

            [DebuggerHidden]
            public <GetShapeKeys>d__29(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = System.Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    if (!this.shape.IsContainer())
                    {
                        goto TR_0001;
                    }
                    else
                    {
                        this.<it>5__2 = this.shape.GetContainer();
                    }
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    this.<it>5__2.Next();
                }
                if (this.<it>5__2.IsValid)
                {
                    this.<>2__current = this.<it>5__2.CurrentShapeKey;
                    this.<>1__state = 1;
                    return true;
                }
                this.<it>5__2 = new HkShapeContainerIterator();
            TR_0001:
                return false;
            }

            [DebuggerHidden]
            IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
            {
                MyPhysicsExtensions.<GetShapeKeys>d__29 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != System.Environment.CurrentManagedThreadId))
                {
                    d__ = new MyPhysicsExtensions.<GetShapeKeys>d__29(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.shape = this.<>3__shape;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.UInt32>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            uint IEnumerator<uint>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

