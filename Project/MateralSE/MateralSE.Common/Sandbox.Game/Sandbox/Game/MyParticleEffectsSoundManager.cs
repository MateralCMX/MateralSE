namespace Sandbox.Game
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    public static class MyParticleEffectsSoundManager
    {
        private static List<EffectSoundEmitter> m_soundEmitters = new List<EffectSoundEmitter>();
        private static short UpdateCount = 0;

        public static void UpdateEffects()
        {
            UpdateCount = (short) (UpdateCount + 1);
            int index = 0;
            while (index < m_soundEmitters.Count)
            {
                m_soundEmitters[index].Updated = false;
                index++;
            }
            using (MyParticlesManager.SoundsPool.ActiveLock.Acquire())
            {
                foreach (MyParticleSound sound in MyParticlesManager.SoundsPool.ActiveWithoutLock)
                {
                    bool flag = true;
                    index = 0;
                    while (true)
                    {
                        if (index < m_soundEmitters.Count)
                        {
                            if (m_soundEmitters[index].ParticleSoundId != sound.ParticleSoundId)
                            {
                                index++;
                                continue;
                            }
                            m_soundEmitters[index].Updated = true;
                            m_soundEmitters[index].Emitter.CustomVolume = new float?(m_soundEmitters[index].OriginalVolume * sound.CurrentVolume);
                            m_soundEmitters[index].Emitter.CustomMaxDistance = new float?(sound.CurrentRange);
                            flag = false;
                            if (!m_soundEmitters[index].Emitter.Loop && sound.NewLoop)
                            {
                                sound.NewLoop = false;
                                bool? nullable = null;
                                m_soundEmitters[index].Emitter.PlaySound(m_soundEmitters[index].SoundPair, false, false, false, false, false, nullable);
                            }
                        }
                        if ((flag && sound.Enabled) && (sound.Position != Vector3.Zero))
                        {
                            MySoundPair objA = new MySoundPair((string) sound.SoundName, true);
                            if (!ReferenceEquals(objA, MySoundPair.Empty))
                            {
                                m_soundEmitters.Add(new EffectSoundEmitter(sound.ParticleSoundId, sound.Position, objA));
                            }
                        }
                        break;
                    }
                }
            }
            for (index = 0; index < m_soundEmitters.Count; index++)
            {
                if (!m_soundEmitters[index].Updated)
                {
                    m_soundEmitters[index].Emitter.StopSound(true, true);
                    m_soundEmitters.RemoveAt(index);
                    index--;
                }
                else if (UpdateCount == 100)
                {
                    m_soundEmitters[index].Emitter.Update();
                }
            }
            if (UpdateCount >= 100)
            {
                UpdateCount = 0;
            }
        }

        private class EffectSoundEmitter
        {
            public readonly uint ParticleSoundId;
            public bool Updated;
            public MyEntity3DSoundEmitter Emitter;
            public MySoundPair SoundPair;
            public float OriginalVolume;

            public EffectSoundEmitter(uint id, Vector3 position, MySoundPair sound)
            {
                this.ParticleSoundId = id;
                this.Updated = true;
                MyEntity entity = null;
                if (MyFakes.ENABLE_NEW_SOUNDS && MySession.Static.Settings.RealisticSound)
                {
                    List<MyEntity> result = new List<MyEntity>();
                    Vector3D center = (MySession.Static.LocalCharacter != null) ? MySession.Static.LocalCharacter.PositionComp.GetPosition() : MySector.MainCamera.Position;
                    BoundingSphereD sphere = new BoundingSphereD(center, 2.0);
                    MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, result, MyEntityQueryType.Both);
                    float maxValue = float.MaxValue;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= result.Count)
                        {
                            result.Clear();
                            break;
                        }
                        MyCubeBlock block = result[num2] as MyCubeBlock;
                        if ((block != null) && (Vector3.DistanceSquared((Vector3) center, (Vector3) block.PositionComp.GetPosition()) < maxValue))
                        {
                            entity = block;
                        }
                        num2++;
                    }
                }
                this.Emitter = new MyEntity3DSoundEmitter(entity, false, 1f);
                this.Emitter.SetPosition(new Vector3D?(position));
                if (sound == null)
                {
                    sound = MySoundPair.Empty;
                }
                bool? nullable = null;
                this.Emitter.PlaySound(sound, false, false, false, false, false, nullable);
                this.OriginalVolume = (this.Emitter.Sound == null) ? 1f : this.Emitter.Sound.Volume;
                this.Emitter.Update();
                this.SoundPair = sound;
            }
        }
    }
}

