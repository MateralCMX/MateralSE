namespace Sandbox.Game.GameSystems
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyFracturedPiecesManager : MySessionComponentBase
    {
        public const int FakePieceLayer = 14;
        public static MyFracturedPiecesManager Static;
        private static float LIFE_OF_CUBIC_PIECE = 300f;
        private Queue<MyFracturedPiece> m_piecesPool = new Queue<MyFracturedPiece>();
        private const int MAX_ALLOC_PER_FRAME = 50;
        private int m_allocatedThisFrame;
        private HashSet<HkdBreakableBody> m_tmpToReturn = new HashSet<HkdBreakableBody>();
        private HashSet<long> m_dbgCreated = new HashSet<long>();
        private HashSet<long> m_dbgRemoved = new HashSet<long>();
        private List<HkBodyCollision> m_rigidList = new List<HkBodyCollision>();
        private int m_addedThisFrame;
        private Queue<Bodies> m_bodyPool = new Queue<Bodies>();
        private const int PREALLOCATE_PIECES = 400;
        private const int PREALLOCATE_BODIES = 400;
        public HashSet<HkRigidBody> m_givenRBs = new HashSet<HkRigidBody>(InstanceComparer<HkRigidBody>.Default);

        private Bodies AllocateBodies()
        {
            Bodies bodies;
            this.m_allocatedThisFrame++;
            bodies.Rigid = HkRigidBody.Allocate();
            bodies.Breakable = HkdBreakableBody.Allocate();
            return bodies;
        }

        private MyFracturedPiece AllocatePiece()
        {
            this.m_allocatedThisFrame++;
            Vector3? position = null;
            position = null;
            position = null;
            MyFracturedPiece piece1 = MyEntities.CreateEntity(new MyDefinitionId(typeof(MyObjectBuilder_FracturedPiece)), false, false, position, position, position) as MyFracturedPiece;
            piece1.Physics = new MyPhysicsBody(MyEntities.CreateEntity(new MyDefinitionId(typeof(MyObjectBuilder_FracturedPiece)), false, false, position, position, position) as MyFracturedPiece, RigidBodyFlag.RBF_DEBRIS);
            piece1.Physics.CanUpdateAccelerations = true;
            return piece1;
        }

        internal void DbgCheck(long createdId, long removedId)
        {
            long num1 = createdId;
            long num2 = removedId;
        }

        public HkdBreakableBody GetBreakableBody(HkdBreakableBodyInfo bodyInfo)
        {
            Bodies bodies = (this.m_bodyPool.Count != 0) ? this.m_bodyPool.Dequeue() : this.AllocateBodies();
            bodies.Breakable.Initialize(bodyInfo, bodies.Rigid);
            return bodies.Breakable;
        }

        public void GetFracturesInBox(ref BoundingBoxD searchBox, List<MyFracturedPiece> output)
        {
            this.m_rigidList.Clear();
            HkShape shape = (HkShape) new HkBoxShape((Vector3) searchBox.HalfExtents);
            try
            {
                Vector3D center = searchBox.Center;
                MyPhysics.GetPenetrationsShape(shape, ref center, ref Quaternion.Identity, this.m_rigidList, 12);
                using (List<HkBodyCollision>.Enumerator enumerator = this.m_rigidList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyFracturedPiece collisionEntity = enumerator.Current.GetCollisionEntity() as MyFracturedPiece;
                        if (collisionEntity != null)
                        {
                            output.Add(collisionEntity);
                        }
                    }
                }
            }
            finally
            {
                this.m_rigidList.Clear();
                shape.RemoveReference();
            }
        }

        public void GetFracturesInSphere(ref BoundingSphereD searchSphere, ref List<MyFracturedPiece> output)
        {
            HkShape shape = (HkShape) new HkSphereShape((float) searchSphere.Radius);
            try
            {
                MyPhysics.GetPenetrationsShape(shape, ref searchSphere.Center, ref Quaternion.Identity, this.m_rigidList, 12);
                using (List<HkBodyCollision>.Enumerator enumerator = this.m_rigidList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyFracturedPiece collisionEntity = enumerator.Current.GetCollisionEntity() as MyFracturedPiece;
                        if (collisionEntity != null)
                        {
                            output.Add(collisionEntity);
                        }
                    }
                }
            }
            finally
            {
                this.m_rigidList.Clear();
                shape.RemoveReference();
            }
        }

        public MyFracturedPiece GetPieceFromPool(long entityId, bool fromServer = false)
        {
            bool isServer = Sync.IsServer;
            MyFracturedPiece piece = (this.m_piecesPool.Count != 0) ? this.m_piecesPool.Dequeue() : this.AllocatePiece();
            if (Sync.IsServer)
            {
                piece.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
            return piece;
        }

        public void InitPools()
        {
            for (int i = 0; i < 400; i++)
            {
                this.m_piecesPool.Enqueue(this.AllocatePiece());
            }
            for (int j = 0; j < 400; j++)
            {
                this.m_bodyPool.Enqueue(this.AllocateBodies());
            }
        }

        public override void LoadData()
        {
            base.LoadData();
            this.InitPools();
            Static = this;
        }

        public void RemoveFracturePiece(MyFracturedPiece piece, float blendTimeSeconds, bool fromServer = false, bool sync = true)
        {
            if (blendTimeSeconds == 0f)
            {
                this.RemoveInternal(piece, fromServer);
            }
        }

        public void RemoveFracturesInBox(ref BoundingBoxD box, float blendTimeSeconds)
        {
            if (Sync.IsServer)
            {
                List<MyFracturedPiece> output = new List<MyFracturedPiece>();
                this.GetFracturesInBox(ref box, output);
                foreach (MyFracturedPiece piece in output)
                {
                    this.RemoveFracturePiece(piece, blendTimeSeconds, false, true);
                }
            }
        }

        public void RemoveFracturesInSphere(Vector3D center, float radius)
        {
            float num = radius * radius;
            foreach (MyEntity entity in MyEntities.GetEntities())
            {
                if (!(entity is MyFracturedPiece))
                {
                    continue;
                }
                if ((radius <= 0f) || ((center - entity.Physics.CenterOfMassWorld).LengthSquared() < num))
                {
                    Static.RemoveFracturePiece(entity as MyFracturedPiece, 2f, false, true);
                }
            }
        }

        private void RemoveInternal(MyFracturedPiece fp, bool fromServer = false)
        {
            if (((fp.Physics != null) && (fp.Physics.RigidBody != null)) && fp.Physics.RigidBody.IsDisposed)
            {
                fp.Physics.BreakableBody = fp.Physics.BreakableBody;
            }
            if (((fp.Physics == null) || (fp.Physics.RigidBody == null)) || fp.Physics.RigidBody.IsDisposed)
            {
                MyEntities.Remove(fp);
            }
            else
            {
                if (!fp.Physics.RigidBody.IsActive)
                {
                    fp.Physics.RigidBody.Activate();
                }
                MyPhysics.RemoveDestructions(fp.Physics.RigidBody);
                HkdBreakableBody breakableBody = fp.Physics.BreakableBody;
                MyPhysicsBody physics = fp.Physics;
                breakableBody.AfterReplaceBody -= new BreakableBodyReplaced(physics.FracturedBody_AfterReplaceBody);
                this.ReturnToPool(breakableBody);
                fp.Physics.Enabled = false;
                MyEntities.Remove(fp);
                fp.Physics.BreakableBody = null;
                fp.Render.ClearModels();
                fp.OriginalBlocks.Clear();
                bool isServer = Sync.IsServer;
                fp.EntityId = 0L;
                fp.Physics.BreakableBody = null;
                this.m_piecesPool.Enqueue(fp);
            }
        }

        public void ReturnToPool(HkdBreakableBody body)
        {
            this.m_tmpToReturn.Add(body);
        }

        private void ReturnToPoolInternal(HkdBreakableBody body)
        {
            HkRigidBody rigidBody = body.GetRigidBody();
            if (rigidBody != null)
            {
                Bodies bodies;
                rigidBody.ContactPointCallbackEnabled = false;
                this.m_givenRBs.Remove(rigidBody);
                foreach (Bodies bodies2 in this.m_bodyPool)
                {
                    if (body != bodies2.Breakable)
                    {
                        bool flag1 = rigidBody == bodies2.Rigid;
                    }
                }
                body.BreakableShape.ClearConnections();
                body.Clear();
                bodies.Rigid = rigidBody;
                bodies.Breakable = body;
                body.InitListener();
                this.m_bodyPool.Enqueue(bodies);
            }
        }

        protected override void UnloadData()
        {
            using (Queue<Bodies>.Enumerator enumerator = this.m_bodyPool.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Breakable.ClearListener();
                }
            }
            this.m_bodyPool.Clear();
            this.m_piecesPool.Clear();
            base.UnloadData();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            foreach (HkdBreakableBody body in this.m_tmpToReturn)
            {
                this.ReturnToPoolInternal(body);
            }
            this.m_tmpToReturn.Clear();
            while ((this.m_bodyPool.Count < 400) && (this.m_allocatedThisFrame < 50))
            {
                this.m_bodyPool.Enqueue(this.AllocateBodies());
            }
            while ((this.m_piecesPool.Count < 400) && (this.m_allocatedThisFrame < 50))
            {
                this.m_piecesPool.Enqueue(this.AllocatePiece());
            }
            this.m_allocatedThisFrame = 0;
        }

        public override bool IsRequiredByGame =>
            MyPerGameSettings.Destruction;

        [StructLayout(LayoutKind.Sequential)]
        private struct Bodies
        {
            public HkRigidBody Rigid;
            public HkdBreakableBody Breakable;
        }
    }
}

