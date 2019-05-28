namespace Sandbox.Game.World
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyAudioComponent : MySessionComponentBase
    {
        private static readonly int MIN_SOUND_DELAY_IN_FRAMES = 30;
        private static readonly MyStringId m_startCue = MyStringId.GetOrCompute("Start");
        private static readonly int POOL_CAPACITY = 50;
        public static readonly double MaxDistanceOfBlockEmitterFromPlayer = 2500.0;
        public static ConcurrentDictionary<long, int> ContactSoundsPool = new ConcurrentDictionary<long, int>();
        public static MyConcurrentHashSet<long> ContactSoundsThisFrame = new MyConcurrentHashSet<long>();
        private static MyConcurrentQueue<MyEntity3DSoundEmitter> m_singleUseEmitterPool = new MyConcurrentQueue<MyEntity3DSoundEmitter>(POOL_CAPACITY);
        private static List<MyEntity3DSoundEmitter> m_borrowedEmitters = new List<MyEntity3DSoundEmitter>();
        private static List<MyEntity3DSoundEmitter> m_emittersToRemove = new List<MyEntity3DSoundEmitter>();
        private static Dictionary<string, MyEntity3DSoundEmitter> m_emitterLibrary = new Dictionary<string, MyEntity3DSoundEmitter>();
        private static List<string> m_emitterLibraryToRemove = new List<string>();
        private static int m_currentEmitters;
        private static FastResourceLock m_emittersLock = new FastResourceLock();
        private static MyCueId m_nullCueId = new MyCueId(MyStringHash.NullOrEmpty);
        private static FastResourceLock m_contactSoundLock = new FastResourceLock();
        private int m_updateCounter;
        private List<VRage.Game.Entity.MyEntity> m_detectedGrids;
        private static MyStringId m_destructionSound = MyStringId.GetOrCompute("Destruction");

        private static void CheckEmitters()
        {
            for (int i = 0; i < m_borrowedEmitters.Count; i++)
            {
                MyEntity3DSoundEmitter item = m_borrowedEmitters[i];
                if ((item != null) && !item.IsPlaying)
                {
                    m_emittersToRemove.Add(item);
                }
            }
        }

        private static void CleanUpEmitters()
        {
            for (int i = 0; i < m_emittersToRemove.Count; i++)
            {
                MyEntity3DSoundEmitter instance = m_emittersToRemove[i];
                if (instance != null)
                {
                    instance.Entity = null;
                    instance.SoundId = m_nullCueId;
                    m_singleUseEmitterPool.Enqueue(instance);
                    m_borrowedEmitters.Remove(instance);
                }
            }
            m_emittersToRemove.Clear();
        }

        public static MyEntity3DSoundEmitter CreateNewLibraryEmitter(string id, VRage.Game.Entity.MyEntity entity = null)
        {
            if (m_emitterLibrary.ContainsKey(id))
            {
                return null;
            }
            m_emitterLibrary.Add(id, new MyEntity3DSoundEmitter(entity, (entity != null) && (entity is MyCubeBlock), 1f));
            return m_emitterLibrary[id];
        }

        private static void emitter_StoppedPlaying(MyEntity3DSoundEmitter emitter)
        {
            if (emitter != null)
            {
                m_emittersToRemove.Add(emitter);
            }
        }

        public static MyEntity3DSoundEmitter GetLibraryEmitter(string id) => 
            (!m_emitterLibrary.ContainsKey(id) ? null : m_emitterLibrary[id]);

        private static float GetMass(MyPhysicsComponentBase body)
        {
            if (body == null)
            {
                return 0f;
            }
            MyGridPhysics physics = body as MyGridPhysics;
            if ((physics == null) || (physics.Shape == null))
            {
                return body.Mass;
            }
            if (physics.Shape.MassProperties == null)
            {
                return 0f;
            }
            return physics.Shape.MassProperties.Value.Mass;
        }

        public static void PlayContactSound(ContactPointWrapper wrap, IMyEntity sourceEntity)
        {
            IMyEntity topMostParent = sourceEntity.GetTopMostParent(null);
            MyPhysicsBody bodyA = wrap.bodyA;
            MyPhysicsBody bodyB = wrap.bodyB;
            if ((!topMostParent.MarkedForClose && (!topMostParent.Closed && (bodyA != null))) && (bodyB != null))
            {
                ContactSoundsThisFrame.Add(sourceEntity.EntityId);
                IMyEntity entityB = ReferenceEquals(sourceEntity, wrap.entityA) ? wrap.entityB : wrap.entityA;
                if (!Sync.IsServer || (MyMultiplayer.Static == null))
                {
                    PlayContactSoundInternal(sourceEntity, entityB, wrap.WorldPosition, wrap.normal, Math.Abs(wrap.separatingVelocity));
                }
                else
                {
                    VRage.Game.Entity.MyEntity entity3 = sourceEntity as VRage.Game.Entity.MyEntity;
                    if (entity3 != null)
                    {
                        Vector3 localPosition = (Vector3) (wrap.WorldPosition - entity3.PositionComp.WorldMatrix.Translation);
                        entity3.UpdateSoundContactPoint(entityB.EntityId, localPosition, wrap.normal, wrap.normal * wrap.separatingVelocity, Math.Abs(wrap.separatingVelocity));
                    }
                }
            }
        }

        public static bool PlayContactSound(long entityId, MyStringId strID, Vector3D position, MyStringHash materialA, MyStringHash materialB, float volume = 1f, Func<bool> canHear = null, VRage.Game.Entity.MyEntity surfaceEntity = null, float separatingVelocity = 0f)
        {
            MySoundPair pair;
            MySoundPair pair1;
            VRage.Game.Entity.MyEntity entity = null;
            if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                goto TR_0000;
            }
            else if ((MyMaterialPropertiesHelper.Static != null) && (MySession.Static != null))
            {
                float mass = GetMass(entity.Physics);
                if ((entity.Physics != null) && (!entity.Physics.IsStatic || (mass != 0f)))
                {
                    pair1 = MyMaterialPropertiesHelper.Static.GetCollisionCueWithMass(strID, materialA, materialB, ref volume, new float?(mass), separatingVelocity);
                    goto TR_0028;
                }
            }
            else
            {
                goto TR_0000;
            }
            pair1 = MyMaterialPropertiesHelper.Static.GetCollisionCue(strID, materialA, materialB);
            goto TR_0028;
        TR_0000:
            return false;
        TR_0028:
            pair = pair1;
            if (pair != null)
            {
                MyCueId soundId = pair.SoundId;
                if (MyAudio.Static != null)
                {
                    int inVoid;
                    if ((separatingVelocity > 0f) && (separatingVelocity < 0.5f))
                    {
                        return false;
                    }
                    if (pair.SoundId.IsNull || !MyAudio.Static.SourceIsCloseEnoughToPlaySound((Vector3) (position - MySector.MainCamera.Position), pair.SoundId, 0f))
                    {
                        return false;
                    }
                    MyEntity3DSoundEmitter emitter = TryGetSoundEmitter();
                    if (emitter == null)
                    {
                        return false;
                    }
                    ContactSoundsPool.TryAdd(entityId, MySession.Static.GameplayFrameCounter);
                    Action<MyEntity3DSoundEmitter> poolRemove = null;
                    poolRemove = delegate (MyEntity3DSoundEmitter e) {
                        int num;
                        ContactSoundsPool.TryRemove(entityId, out num);
                        emitter.StoppedPlaying -= poolRemove;
                    };
                    emitter.StoppedPlaying += poolRemove;
                    if (MySession.Static.Settings.RealisticSound && MyFakes.ENABLE_NEW_SOUNDS)
                    {
                        Action<MyEntity3DSoundEmitter> remove = null;
                        remove = delegate (MyEntity3DSoundEmitter e) {
                            emitter.EmitterMethods[0].Remove(canHear, false);
                            emitter.StoppedPlaying -= remove;
                        };
                        emitter.EmitterMethods[0].Add(canHear);
                        emitter.StoppedPlaying += remove;
                    }
                    if ((!MySession.Static.Settings.RealisticSound || (!MyFakes.ENABLE_NEW_SOUNDS || (MySession.Static.LocalCharacter == null))) || (MySession.Static.LocalCharacter.AtmosphereDetectorComp == null))
                    {
                        inVoid = 0;
                    }
                    else
                    {
                        inVoid = (int) MySession.Static.LocalCharacter.AtmosphereDetectorComp.InVoid;
                    }
                    bool flag = (bool) inVoid;
                    if ((surfaceEntity == null) || flag)
                    {
                        emitter.Entity = entity;
                    }
                    else
                    {
                        emitter.Entity = surfaceEntity;
                    }
                    emitter.SetPosition(new Vector3D?(position));
                    bool? nullable = null;
                    emitter.PlaySound(pair, false, false, false, false, false, nullable);
                    if (emitter.Sound != null)
                    {
                        emitter.Sound.SetVolume(emitter.Sound.Volume * volume);
                    }
                    if (flag && (surfaceEntity != null))
                    {
                        MyEntity3DSoundEmitter emitter2 = TryGetSoundEmitter();
                        if (emitter2 == null)
                        {
                            return false;
                        }
                        Action<MyEntity3DSoundEmitter> action1 = null;
                        action1 = delegate (MyEntity3DSoundEmitter e) {
                            emitter2.EmitterMethods[0].Remove(canHear, false);
                            emitter2.StoppedPlaying -= action1;
                        };
                        emitter2.EmitterMethods[0].Add(canHear);
                        emitter2.StoppedPlaying += action1;
                        emitter2.Entity = surfaceEntity;
                        emitter2.SetPosition(new Vector3D?(position));
                        nullable = null;
                        emitter2.PlaySound(pair, false, false, false, false, false, nullable);
                        if (emitter2.Sound != null)
                        {
                            emitter2.Sound.SetVolume(emitter2.Sound.Volume * volume);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        internal static void PlayContactSoundInternal(IMyEntity entityA, IMyEntity entityB, Vector3D position, Vector3 normal, float separatingSpeed)
        {
            MyPhysicsBody physics = entityA.Physics as MyPhysicsBody;
            MyPhysicsBody body = entityB.Physics as MyPhysicsBody;
            if ((physics != null) && (body != null))
            {
                MyStringHash materialAt = physics.GetMaterialAt(position + (normal * 0.1f));
                MyStringHash materialB = body.GetMaterialAt(position - (normal * 0.1f));
                bool flag = (body.Entity is MyVoxelBase) || ReferenceEquals(body.Entity.Physics, null);
                float mass = GetMass(physics);
                float num2 = GetMass(body);
                bool flag2 = !physics.Entity.Physics.IsStatic || (mass > 0f);
                bool flag3 = !body.Entity.Physics.IsStatic || (num2 > 0f);
                flag = ((!flag && (physics.Entity.Physics != null)) & flag2) && (!flag3 || (mass < num2));
                float volume = (Math.Abs(separatingSpeed) >= 10f) ? 1f : (0.5f + (Math.Abs(separatingSpeed) / 20f));
                Func<bool> canHear = delegate {
                    if (MySession.Static.ControlledEntity == null)
                    {
                        return false;
                    }
                    VRage.Game.Entity.MyEntity topMostParent = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null);
                    return ReferenceEquals(topMostParent, entityA) || ReferenceEquals(topMostParent, entityB);
                };
                using (m_contactSoundLock.AcquireExclusiveUsing())
                {
                    if (flag)
                    {
                        PlayContactSound(entityA.EntityId, m_startCue, position, materialAt, materialB, volume, canHear, (VRage.Game.Entity.MyEntity) entityB, separatingSpeed);
                    }
                    else
                    {
                        PlayContactSound(entityA.EntityId, m_startCue, position, materialB, materialAt, volume, canHear, (VRage.Game.Entity.MyEntity) entityA, separatingSpeed);
                    }
                }
            }
        }

        public static void PlayDestructionSound(MySlimBlock b)
        {
            MyPhysicalMaterialDefinition physicalMaterial = null;
            MySoundPair pair;
            if (b.FatBlock is MyCompoundCubeBlock)
            {
                MyCompoundCubeBlock fatBlock = b.FatBlock as MyCompoundCubeBlock;
                if (fatBlock.GetBlocksCount() > 0)
                {
                    physicalMaterial = fatBlock.GetBlocks()[0].BlockDefinition.PhysicalMaterial;
                }
            }
            else if (!(b.FatBlock is MyFracturedBlock))
            {
                physicalMaterial = b.BlockDefinition.PhysicalMaterial;
            }
            else
            {
                MyCubeBlockDefinition definition2;
                if (MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>((b.FatBlock as MyFracturedBlock).OriginalBlocks[0], out definition2))
                {
                    physicalMaterial = definition2.PhysicalMaterial;
                }
            }
            if ((physicalMaterial != null) && (physicalMaterial.GeneralSounds.TryGetValue(m_destructionSound, out pair) && !pair.SoundId.IsNull))
            {
                MyEntity3DSoundEmitter emitter = TryGetSoundEmitter();
                if (emitter != null)
                {
                    Vector3D vectord;
                    b.ComputeWorldCenter(out vectord);
                    emitter.SetPosition(new Vector3D?(vectord));
                    bool? nullable = null;
                    emitter.PlaySound(pair, false, false, false, false, false, nullable);
                }
            }
        }

        public static void PlayDestructionSound(MyFracturedPiece fp)
        {
            MySoundPair pair;
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(fp.OriginalBlocks[0]);
            if ((cubeBlockDefinition != null) && (cubeBlockDefinition.PhysicalMaterial.GeneralSounds.TryGetValue(m_destructionSound, out pair) && !pair.SoundId.IsNull))
            {
                MyEntity3DSoundEmitter emitter = TryGetSoundEmitter();
                if (emitter != null)
                {
                    emitter.SetPosition(new Vector3D?(fp.PositionComp.GetPosition()));
                    bool? nullable = null;
                    emitter.PlaySound(pair, false, false, false, false, false, nullable);
                }
            }
        }

        public static void RemoveLibraryEmitter(string id)
        {
            if (m_emitterLibrary.ContainsKey(id))
            {
                m_emitterLibrary[id].StopSound(true, true);
                m_emitterLibraryToRemove.Add(id);
            }
        }

        public static bool ShouldPlayContactSound(long entityId, HkContactPointEvent.Type eventType)
        {
            int num;
            int gameplayFrameCounter = MySession.Static.GameplayFrameCounter;
            bool flag = ContactSoundsPool.TryGetValue(entityId, out num);
            if (((eventType != HkContactPointEvent.Type.Manifold) || ContactSoundsThisFrame.Contains(entityId)) || (flag && ((num + MIN_SOUND_DELAY_IN_FRAMES) > gameplayFrameCounter)))
            {
                return false;
            }
            if (flag)
            {
                ContactSoundsPool.TryRemove(entityId, out num);
            }
            return true;
        }

        public static MyEntity3DSoundEmitter TryGetSoundEmitter()
        {
            MyEntity3DSoundEmitter instance = null;
            using (m_emittersLock.AcquireExclusiveUsing())
            {
                if (m_currentEmitters >= POOL_CAPACITY)
                {
                    CheckEmitters();
                }
                if (m_emittersToRemove.Count > 0)
                {
                    CleanUpEmitters();
                }
                if (!m_singleUseEmitterPool.TryDequeue(out instance) && (m_currentEmitters < POOL_CAPACITY))
                {
                    instance = new MyEntity3DSoundEmitter(null, false, 1f);
                    instance.StoppedPlaying += new Action<MyEntity3DSoundEmitter>(MyAudioComponent.emitter_StoppedPlaying);
                    instance.CanPlayLoopSounds = false;
                    m_currentEmitters++;
                }
                if (instance != null)
                {
                    m_borrowedEmitters.Add(instance);
                }
            }
            return instance;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            using (Dictionary<string, MyEntity3DSoundEmitter>.ValueCollection.Enumerator enumerator = m_emitterLibrary.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.StopSound(true, true);
                }
            }
            CleanUpEmitters();
            m_emitterLibrary.Clear();
            m_borrowedEmitters.Clear();
            m_singleUseEmitterPool.Clear();
            m_emitterLibraryToRemove.Clear();
            m_currentEmitters = 0;
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            ContactSoundsThisFrame.Clear();
            this.m_updateCounter++;
            if (((this.m_updateCounter % 100) == 0) && (MySession.Static.LocalCharacter != null))
            {
                foreach (string str in m_emitterLibraryToRemove)
                {
                    if (m_emitterLibrary.ContainsKey(str))
                    {
                        m_emitterLibrary[str].StopSound(true, true);
                        m_emitterLibrary.Remove(str);
                    }
                }
                m_emitterLibraryToRemove.Clear();
                using (Dictionary<string, MyEntity3DSoundEmitter>.ValueCollection.Enumerator enumerator2 = m_emitterLibrary.Values.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.Update();
                    }
                }
            }
        }
    }
}

