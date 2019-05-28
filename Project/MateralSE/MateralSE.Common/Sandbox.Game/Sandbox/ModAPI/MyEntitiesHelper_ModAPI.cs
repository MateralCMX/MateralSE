namespace Sandbox.ModAPI
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Scripting;
    using VRage.Utils;
    using VRageMath;

    public class MyEntitiesHelper_ModAPI : IMyEntities
    {
        private List<MyEntity> m_entityList = new List<MyEntity>();

        event Action IMyEntities.OnCloseAll
        {
            add
            {
                MyEntities.OnCloseAll += value;
            }
            remove
            {
                MyEntities.OnCloseAll -= value;
            }
        }

        event Action<IMyEntity> IMyEntities.OnEntityAdd
        {
            add
            {
                MyEntities.OnEntityAdd += this.GetDelegate(value);
            }
            remove
            {
                MyEntities.OnEntityAdd -= this.GetDelegate(value);
            }
        }

        event Action<IMyEntity, string, string> IMyEntities.OnEntityNameSet
        {
            add
            {
                MyEntities.OnEntityNameSet += this.GetDelegate(value);
            }
            remove
            {
                MyEntities.OnEntityNameSet -= this.GetDelegate(value);
            }
        }

        event Action<IMyEntity> IMyEntities.OnEntityRemove
        {
            add
            {
                MyEntities.OnEntityRemove += this.GetDelegate(value);
            }
            remove
            {
                MyEntities.OnEntityRemove -= this.GetDelegate(value);
            }
        }

        private Action<MyEntity> GetDelegate(Action<IMyEntity> value) => 
            ((Action<MyEntity>) Delegate.CreateDelegate(typeof(Action<MyEntity>), value.Target, value.Method));

        private Action<MyEntity, string, string> GetDelegate(Action<IMyEntity, string, string> value) => 
            ((Action<MyEntity, string, string>) Delegate.CreateDelegate(typeof(Action<MyEntity, string, string>), value.Target, value.Method));

        void IMyEntities.AddEntity(IMyEntity entity, bool insertIntoScene)
        {
            if (entity is MyEntity)
            {
                MyEntities.Add(entity as MyEntity, insertIntoScene);
            }
        }

        IMyEntity IMyEntities.CreateFromObjectBuilder(MyObjectBuilder_EntityBase objectBuilder) => 
            MyEntities.CreateFromObjectBuilder(objectBuilder, false);

        IMyEntity IMyEntities.CreateFromObjectBuilderAndAdd(MyObjectBuilder_EntityBase objectBuilder) => 
            MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, false);

        IMyEntity IMyEntities.CreateFromObjectBuilderNoinit(MyObjectBuilder_EntityBase objectBuilder) => 
            MyEntities.CreateFromObjectBuilderNoinit(objectBuilder);

        IMyEntity IMyEntities.CreateFromObjectBuilderParallel(MyObjectBuilder_EntityBase objectBuilder, bool addToScene, Action<IMyEntity> completionCallback)
        {
            Vector3D? relativeOffset = null;
            return MyEntities.CreateFromObjectBuilderParallel(objectBuilder, addToScene, (Action<MyEntity>) completionCallback, null, null, relativeOffset, false, false);
        }

        void IMyEntities.EnableEntityBoundingBoxDraw(IMyEntity entity, bool enable, VRageMath.Vector4? color, float lineWidth, Vector3? inflateAmount)
        {
            if (entity is MyEntity)
            {
                if (ReferenceEquals(Thread.CurrentThread, MyUtils.MainThread))
                {
                    MyStringId? lineMaterial = null;
                    MyEntities.EnableEntityBoundingBoxDraw(entity as MyEntity, enable, color, lineWidth, inflateAmount, lineMaterial);
                }
                else
                {
                    MyModWatchdog.ReportIncorrectBehaviour(MyCommonTexts.ModRuleViolation_EngineParallelAccess);
                    MySandboxGame.Static.Invoke(delegate {
                        MyStringId? lineMaterial = null;
                        MyEntities.EnableEntityBoundingBoxDraw(entity as MyEntity, enable, color, lineWidth, inflateAmount, lineMaterial);
                    }, "EnableEntityBoundingBoxDraw");
                }
            }
        }

        bool IMyEntities.EntityExists(long entityId) => 
            MyEntities.EntityExists(entityId);

        bool IMyEntities.EntityExists(long? entityId) => 
            ((entityId != null) && MyEntities.EntityExists(entityId.Value));

        bool IMyEntities.EntityExists(string name) => 
            MyEntities.EntityExists(name);

        bool IMyEntities.Exist(IMyEntity entity) => 
            ((entity is MyEntity) && MyEntities.Exist(entity as MyEntity));

        Vector3D? IMyEntities.FindFreePlace(Vector3D basePos, float radius, int maxTestCount, int testsPerDistance, float stepSize) => 
            MyEntities.FindFreePlace(basePos, radius, maxTestCount, testsPerDistance, stepSize);

        List<IMyEntity> IMyEntities.GetElementsInBox(ref BoundingBoxD boundingBox)
        {
            this.m_entityList.Clear();
            MyEntities.GetElementsInBox(ref boundingBox, this.m_entityList);
            List<IMyEntity> list = new List<IMyEntity>(this.m_entityList.Count);
            foreach (MyEntity entity in this.m_entityList)
            {
                list.Add(entity);
            }
            return list;
        }

        void IMyEntities.GetEntities(HashSet<IMyEntity> entities, Func<IMyEntity, bool> collect)
        {
            foreach (MyEntity entity in MyEntities.GetEntities())
            {
                if ((collect == null) || collect(entity))
                {
                    entities.Add(entity);
                }
            }
        }

        List<IMyEntity> IMyEntities.GetEntitiesInAABB(ref BoundingBoxD boundingBox)
        {
            List<MyEntity> entitiesInAABB = MyEntities.GetEntitiesInAABB(ref boundingBox, false);
            List<IMyEntity> list2 = new List<IMyEntity>(entitiesInAABB.Count);
            foreach (MyEntity entity in entitiesInAABB)
            {
                list2.Add(entity);
            }
            entitiesInAABB.Clear();
            return list2;
        }

        List<IMyEntity> IMyEntities.GetEntitiesInSphere(ref BoundingSphereD boundingSphere)
        {
            List<MyEntity> entitiesInSphere = MyEntities.GetEntitiesInSphere(ref boundingSphere);
            List<IMyEntity> list2 = new List<IMyEntity>(entitiesInSphere.Count);
            foreach (MyEntity entity in entitiesInSphere)
            {
                list2.Add(entity);
            }
            entitiesInSphere.Clear();
            return list2;
        }

        IMyEntity IMyEntities.GetEntity(Func<IMyEntity, bool> match)
        {
            using (ConcurrentEnumerator<SpinLockRef.Token, MyEntity, HashSet<MyEntity>.Enumerator> enumerator = MyEntities.GetEntities().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyEntity current = enumerator.Current;
                    if (match(current))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        IMyEntity IMyEntities.GetEntityById(long entityId) => 
            (MyEntities.EntityExists(entityId) ? MyEntities.GetEntityById(entityId, false) : null);

        IMyEntity IMyEntities.GetEntityById(long? entityId) => 
            ((entityId != null) ? MyEntities.GetEntityById(entityId.Value, false) : null);

        IMyEntity IMyEntities.GetEntityByName(string name) => 
            MyEntities.GetEntityByName(name);

        [Obsolete]
        void IMyEntities.GetInflatedPlayerBoundingBox(ref BoundingBox playerBox, float inflation)
        {
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            MyEntities.GetInflatedPlayerBoundingBox(ref xd, inflation);
            playerBox = (BoundingBox) xd;
        }

        void IMyEntities.GetInflatedPlayerBoundingBox(ref BoundingBoxD playerBox, float inflation)
        {
            MyEntities.GetInflatedPlayerBoundingBox(ref playerBox, inflation);
        }

        IMyEntity IMyEntities.GetIntersectionWithSphere(ref BoundingSphereD sphere) => 
            MyEntities.GetIntersectionWithSphere(ref sphere);

        IMyEntity IMyEntities.GetIntersectionWithSphere(ref BoundingSphereD sphere, IMyEntity ignoreEntity0, IMyEntity ignoreEntity1) => 
            MyEntities.GetIntersectionWithSphere(ref sphere, ignoreEntity0 as MyEntity, ignoreEntity1 as MyEntity);

        List<IMyEntity> IMyEntities.GetIntersectionWithSphere(ref BoundingSphereD sphere, IMyEntity ignoreEntity0, IMyEntity ignoreEntity1, bool ignoreVoxelMaps, bool volumetricTest)
        {
            this.m_entityList.Clear();
            MyEntities.GetIntersectionWithSphere(ref sphere, ignoreEntity0 as MyEntity, ignoreEntity1 as MyEntity, ignoreVoxelMaps, volumetricTest, ref this.m_entityList);
            List<IMyEntity> list = new List<IMyEntity>(this.m_entityList.Count);
            foreach (MyEntity entity in this.m_entityList)
            {
                list.Add(entity);
            }
            return list;
        }

        IMyEntity IMyEntities.GetIntersectionWithSphere(ref BoundingSphereD sphere, IMyEntity ignoreEntity0, IMyEntity ignoreEntity1, bool ignoreVoxelMaps, bool volumetricTest, bool excludeEntitiesWithDisabledPhysics, bool ignoreFloatingObjects, bool ignoreHandWeapons) => 
            MyEntities.GetIntersectionWithSphere(ref sphere, ignoreEntity0 as MyEntity, ignoreEntity1 as MyEntity, ignoreVoxelMaps, volumetricTest, excludeEntitiesWithDisabledPhysics, ignoreFloatingObjects, ignoreHandWeapons);

        List<IMyEntity> IMyEntities.GetTopMostEntitiesInBox(ref BoundingBoxD boundingBox)
        {
            this.m_entityList.Clear();
            MyEntities.GetTopMostEntitiesInBox(ref boundingBox, this.m_entityList, MyEntityQueryType.Both);
            List<IMyEntity> list = new List<IMyEntity>(this.m_entityList.Count);
            foreach (MyEntity entity in this.m_entityList)
            {
                list.Add(entity);
            }
            return list;
        }

        List<IMyEntity> IMyEntities.GetTopMostEntitiesInSphere(ref BoundingSphereD boundingSphere)
        {
            List<MyEntity> topMostEntitiesInSphere = MyEntities.GetTopMostEntitiesInSphere(ref boundingSphere);
            List<IMyEntity> list2 = new List<IMyEntity>(topMostEntitiesInSphere.Count);
            foreach (MyEntity entity in topMostEntitiesInSphere)
            {
                list2.Add(entity);
            }
            topMostEntitiesInSphere.Clear();
            return list2;
        }

        [Obsolete]
        bool IMyEntities.IsInsideVoxel(Vector3 pos, Vector3 hintPosition, out Vector3 lastOutsidePos)
        {
            Vector3D vectord;
            bool flag1 = MyEntities.IsInsideVoxel(pos, hintPosition, out vectord);
            lastOutsidePos = (Vector3) vectord;
            return flag1;
        }

        bool IMyEntities.IsInsideVoxel(Vector3D pos, Vector3D hintPosition, out Vector3D lastOutsidePos) => 
            MyEntities.IsInsideVoxel(pos, hintPosition, out lastOutsidePos);

        bool IMyEntities.IsInsideWorld(Vector3D pos) => 
            MyEntities.IsInsideWorld(pos);

        bool IMyEntities.IsNameExists(IMyEntity entity, string name) => 
            ((entity is MyEntity) && MyEntities.IsNameExists(entity as MyEntity, name));

        bool IMyEntities.IsRaycastBlocked(Vector3D pos, Vector3D target) => 
            MyEntities.IsRaycastBlocked(pos, target);

        bool IMyEntities.IsSpherePenetrating(ref BoundingSphereD bs) => 
            MyEntities.IsSpherePenetrating(ref bs);

        bool IMyEntities.IsTypeHidden(System.Type type) => 
            MyEntities.IsTypeHidden(type);

        bool IMyEntities.IsVisible(IMyEntity entity) => 
            ((IMyEntities) this).IsTypeHidden(entity.GetType());

        bool IMyEntities.IsWorldLimited() => 
            MyEntities.IsWorldLimited();

        void IMyEntities.MarkForClose(IMyEntity entity)
        {
            if (entity is MyEntity)
            {
                MyEntities.Close(entity as MyEntity);
            }
        }

        void IMyEntities.RegisterForDraw(IMyEntity entity)
        {
            MyEntity entity2 = entity as MyEntity;
            if (entity2 != null)
            {
                MyEntities.RegisterForDraw(entity2);
            }
        }

        void IMyEntities.RegisterForUpdate(IMyEntity entity)
        {
            MyEntity e = entity as MyEntity;
            if (e != null)
            {
                if (ReferenceEquals(Thread.CurrentThread, MyUtils.MainThread))
                {
                    MyEntities.RegisterForUpdate(e);
                }
                else
                {
                    MyModWatchdog.ReportIncorrectBehaviour(MyCommonTexts.ModRuleViolation_EngineParallelAccess);
                    MySandboxGame.Static.Invoke(() => MyEntities.RegisterForUpdate(e), "RegisterForUpdate");
                }
            }
        }

        void IMyEntities.RemapObjectBuilder(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyEntities.RemapObjectBuilder(objectBuilder);
        }

        void IMyEntities.RemapObjectBuilderCollection(IEnumerable<MyObjectBuilder_EntityBase> objectBuilders)
        {
            MyEntities.RemapObjectBuilderCollection(objectBuilders);
        }

        void IMyEntities.RemoveEntity(IMyEntity entity)
        {
            MyEntities.Remove(entity as MyEntity);
        }

        void IMyEntities.RemoveFromClosedEntities(IMyEntity entity)
        {
            if (entity is MyEntity)
            {
                MyEntities.RemoveFromClosedEntities(entity as MyEntity);
            }
        }

        void IMyEntities.RemoveName(IMyEntity entity)
        {
            if (!string.IsNullOrEmpty(entity.Name))
            {
                MyEntities.m_entityNameDictionary.Remove<string, MyEntity>(entity.Name);
            }
        }

        void IMyEntities.SetEntityName(IMyEntity entity, bool possibleRename)
        {
            if (entity is MyEntity)
            {
                MyEntities.SetEntityName(entity as MyEntity, possibleRename);
            }
        }

        void IMyEntities.SetTypeHidden(System.Type type, bool hidden)
        {
            MyEntities.SetTypeHidden(type, hidden);
        }

        bool IMyEntities.TryGetEntityById(long id, out IMyEntity entity)
        {
            MyEntity entity2;
            bool flag1 = MyEntities.TryGetEntityById(id, out entity2, false);
            entity = entity2;
            return flag1;
        }

        bool IMyEntities.TryGetEntityById(long? id, out IMyEntity entity)
        {
            entity = null;
            bool flag = false;
            if (id != null)
            {
                MyEntity entity2;
                flag = MyEntities.TryGetEntityById(id.Value, out entity2, false);
                entity = entity2;
            }
            return flag;
        }

        bool IMyEntities.TryGetEntityByName(string name, out IMyEntity entity)
        {
            MyEntity entity2;
            bool flag1 = MyEntities.TryGetEntityByName(name, out entity2);
            entity = entity2;
            return flag1;
        }

        void IMyEntities.UnhideAllTypes()
        {
            MyEntities.UnhideAllTypes();
        }

        void IMyEntities.UnregisterForDraw(IMyEntity entity)
        {
            MyEntity entity2 = entity as MyEntity;
            if (entity2 != null)
            {
                MyEntities.UnregisterForDraw(entity2);
            }
        }

        void IMyEntities.UnregisterForUpdate(IMyEntity entity, bool immediate)
        {
            MyEntity e = entity as MyEntity;
            if (e != null)
            {
                if (ReferenceEquals(Thread.CurrentThread, MyUtils.MainThread))
                {
                    MyEntities.UnregisterForUpdate(e, immediate);
                }
                else
                {
                    MyModWatchdog.ReportIncorrectBehaviour(MyCommonTexts.ModRuleViolation_EngineParallelAccess);
                    MySandboxGame.Static.Invoke(() => MyEntities.UnregisterForUpdate(e, immediate), "UnregisterForUpdate");
                }
            }
        }

        float IMyEntities.WorldHalfExtent() => 
            MyEntities.WorldHalfExtent();

        float IMyEntities.WorldSafeHalfExtent() => 
            MyEntities.WorldSafeHalfExtent();
    }
}

