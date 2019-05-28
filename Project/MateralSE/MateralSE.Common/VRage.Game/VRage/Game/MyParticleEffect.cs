namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyParticleEffect
    {
        [CompilerGenerated]
        private EventHandler OnDelete;
        [CompilerGenerated]
        private EventHandler OnUpdate;
        private static readonly int Version;
        private int m_particleID;
        private float m_elapsedTime;
        private string m_name;
        private float m_length = 90f;
        private bool m_isStopped;
        private bool m_isSimulationPaused;
        private bool m_isEmittingStopped;
        private bool m_loop;
        private float m_durationActual;
        private float m_durationMin;
        private float m_durationMax;
        private float m_timer;
        private MatrixD m_worldMatrix = MatrixD.Identity;
        private MatrixD m_lastWorldMatrix;
        public int ParticlesCount;
        private float m_distance;
        private readonly List<IMyParticleGeneration> m_generations = new List<IMyParticleGeneration>();
        private MyConcurrentList<MyParticleEffect> m_instances;
        private readonly List<MyParticleLight> m_particleLights = new List<MyParticleLight>();
        private readonly List<MyParticleSound> m_particleSounds = new List<MyParticleSound>();
        private BoundingBoxD m_AABB;
        private const int GRAVITY_UPDATE_DELAY = 200;
        private int m_updateCounter;
        public bool EnableLods;
        private float m_userEmitterScale;
        private float m_userScale;
        public Vector3 UserAxisScale;
        private uint m_parentID;
        private float m_userBirthMultiplier;
        private float m_userRadiusMultiplier;
        private Vector4 m_userColorMultiplier;
        public bool UserDraw;
        private int m_showOnlyThisGeneration = -1;
        public bool CalculateDeltaMatrix;
        public MatrixD DeltaMatrix;
        public uint RenderCounter;
        private Vector3 m_velocity;
        private bool m_velocitySet;
        private bool m_newLoop;
        private bool m_instantStop;
        private bool m_anyCpuGeneration;

        public event EventHandler OnDelete
        {
            [CompilerGenerated] add
            {
                EventHandler onDelete = this.OnDelete;
                while (true)
                {
                    EventHandler a = onDelete;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onDelete = Interlocked.CompareExchange<EventHandler>(ref this.OnDelete, handler3, a);
                    if (ReferenceEquals(onDelete, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onDelete = this.OnDelete;
                while (true)
                {
                    EventHandler source = onDelete;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onDelete = Interlocked.CompareExchange<EventHandler>(ref this.OnDelete, handler3, source);
                    if (ReferenceEquals(onDelete, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler OnUpdate
        {
            [CompilerGenerated] add
            {
                EventHandler onUpdate = this.OnUpdate;
                while (true)
                {
                    EventHandler a = onUpdate;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onUpdate = Interlocked.CompareExchange<EventHandler>(ref this.OnUpdate, handler3, a);
                    if (ReferenceEquals(onUpdate, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onUpdate = this.OnUpdate;
                while (true)
                {
                    EventHandler source = onUpdate;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onUpdate = Interlocked.CompareExchange<EventHandler>(ref this.OnUpdate, handler3, source);
                    if (ReferenceEquals(onUpdate, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyParticleEffect()
        {
            this.Enabled = true;
        }

        public void AddGeneration(IMyParticleGeneration generation)
        {
            this.m_generations.Add(generation);
            if (this.m_instances != null)
            {
                foreach (MyParticleEffect effect in this.m_instances)
                {
                    IMyParticleGeneration generation2 = generation.CreateInstance(effect);
                    if (generation2 != null)
                    {
                        effect.AddGeneration(generation2);
                    }
                }
            }
        }

        public void AddParticleLight(MyParticleLight particleLight)
        {
            this.m_particleLights.Add(particleLight);
            if (this.m_instances != null)
            {
                foreach (MyParticleEffect effect in this.m_instances)
                {
                    effect.AddParticleLight(particleLight.CreateInstance(effect));
                }
            }
        }

        public void AddParticleSound(MyParticleSound particleSound)
        {
            this.m_particleSounds.Add(particleSound);
            if (this.m_instances != null)
            {
                foreach (MyParticleEffect effect in this.m_instances)
                {
                    effect.AddParticleSound(particleSound.CreateInstance(effect));
                }
            }
        }

        public void Clear()
        {
            this.m_elapsedTime = 0f;
            this.ParticlesCount = 0;
            foreach (IMyParticleGeneration generation in this.m_generations)
            {
                if (generation != null)
                {
                    generation.Clear();
                    continue;
                }
                string msg = "Error: MyParticleGeneration should not be null!";
                MyLog.Default.WriteLine(msg);
            }
            if (this.m_instances != null)
            {
                foreach (MyParticleEffect effect in this.m_instances)
                {
                    if (effect != null)
                    {
                        effect.Clear();
                        continue;
                    }
                    string msg = "Error: MyParticleEffect should not be null!";
                    MyLog.Default.WriteLine(msg);
                }
            }
        }

        public void Close(bool notify, bool forceInstant)
        {
            if (notify && (this.OnDelete != null))
            {
                this.OnDelete(this, null);
            }
            this.Clear();
            foreach (IMyParticleGeneration generation in this.m_generations)
            {
                if (forceInstant || this.m_instantStop)
                {
                    generation.Done();
                }
                else
                {
                    generation.Close();
                }
                generation.Deallocate();
            }
            this.m_generations.Clear();
            foreach (MyParticleLight light in this.m_particleLights)
            {
                if (forceInstant)
                {
                    light.Done();
                }
                else
                {
                    light.Close();
                }
                MyParticlesManager.LightsPool.Deallocate(light);
            }
            this.m_particleLights.Clear();
            foreach (MyParticleSound sound in this.m_particleSounds)
            {
                if (forceInstant)
                {
                    sound.Done();
                }
                else
                {
                    sound.Close();
                }
                MyParticlesManager.SoundsPool.Deallocate(sound);
            }
            this.m_particleSounds.Clear();
            if (this.m_instances != null)
            {
                while (this.m_instances.Count > 0)
                {
                    MyParticlesManager.RemoveParticleEffect(this.m_instances[0], false);
                }
            }
            this.OnDelete = null;
            this.OnUpdate = null;
            this.Tag = null;
        }

        public MyParticleEffect CreateInstance(ref MatrixD effectMatrix, ref Vector3D worldPosition, uint parentId)
        {
            MyParticleEffect effect = null;
            if ((!MyParticlesManager.DISTANCE_CHECK_ENABLE || (this.DistanceMax <= 0f)) || this.m_loop)
            {
                effect = MyParticlesManager.EffectsPool.Allocate(true);
            }
            else
            {
                double num;
                Vector3D translation = MyTransparentGeometry.Camera.Translation;
                Vector3D.DistanceSquared(ref worldPosition, ref translation, out num);
                if (num <= (this.DistanceMax * this.DistanceMax))
                {
                    effect = MyParticlesManager.EffectsPool.Allocate(true);
                }
            }
            if (effect != null)
            {
                effect.Start(this.m_particleID, this.m_name);
                effect.ParentID = parentId;
                effect.Enabled = this.Enabled;
                effect.DistanceMax = this.DistanceMax;
                effect.Length = this.Length;
                effect.Loop = this.m_loop;
                effect.DurationMin = this.m_durationMin;
                effect.DurationMax = this.m_durationMax;
                effect.SetRandomDuration();
                effect.WorldMatrix = effectMatrix;
                effect.m_anyCpuGeneration = false;
                using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IMyParticleGeneration generation = enumerator.Current.CreateInstance(effect);
                        if (generation != null)
                        {
                            effect.AddGeneration(generation);
                            effect.m_anyCpuGeneration |= generation is MyParticleGeneration;
                        }
                    }
                }
                using (List<MyParticleLight>.Enumerator enumerator2 = this.m_particleLights.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        MyParticleLight particleLight = enumerator2.Current.CreateInstance(effect);
                        if (particleLight != null)
                        {
                            effect.AddParticleLight(particleLight);
                        }
                    }
                }
                using (List<MyParticleSound>.Enumerator enumerator3 = this.m_particleSounds.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        MyParticleSound particleSound = enumerator3.Current.CreateInstance(effect);
                        if (particleSound != null)
                        {
                            effect.AddParticleSound(particleSound);
                        }
                    }
                }
                if (this.m_instances == null)
                {
                    this.m_instances = new MyConcurrentList<MyParticleEffect>();
                }
                this.m_instances.Add(effect);
            }
            return effect;
        }

        public void DebugDraw()
        {
            MyRenderProxy.DebugDrawAxis(this.WorldMatrix, 1f, false, false, false);
            foreach (IMyParticleGeneration generation in this.m_generations)
            {
                if (generation is MyParticleGeneration)
                {
                    (generation as MyParticleGeneration).DebugDraw();
                }
            }
            Color color = !this.m_isStopped ? Color.White : Color.Red;
            string[] textArray1 = new string[] { this.Name, "(", this.GetID().ToString(), ") [", this.GetParticlesCount().ToString(), "]" };
            MyRenderProxy.DebugDrawText3D(this.WorldMatrix.Translation, string.Concat(textArray1), color, 0.8f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
            MyRenderProxy.DebugDrawAABB(this.m_AABB, color, 1f, 1f, true, false, false);
        }

        public void Deserialize(XmlReader reader)
        {
            this.m_name = reader.GetAttribute("name");
            Convert.ToInt32(reader.GetAttribute("version"), CultureInfo.InvariantCulture);
            reader.ReadStartElement();
            this.m_particleID = reader.ReadElementContentAsInt();
            this.m_length = reader.ReadElementContentAsFloat();
            if (reader.Name == "LowRes")
            {
                reader.ReadElementContentAsBoolean();
            }
            if (reader.Name == "Scale")
            {
                reader.ReadElementContentAsFloat();
            }
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            while ((reader.NodeType != XmlNodeType.EndElement) && !isEmptyElement)
            {
                if ((reader.Name == "ParticleGeneration") && MyParticlesManager.EnableCPUGenerations)
                {
                    MyParticleGeneration generation;
                    MyParticlesManager.GenerationsPool.AllocateOrCreate(out generation);
                    generation.Start(this);
                    generation.Init();
                    generation.Deserialize(reader);
                    this.AddGeneration(generation);
                    continue;
                }
                if (reader.Name != "ParticleGPUGeneration")
                {
                    reader.Read();
                }
                else
                {
                    MyParticleGPUGeneration generation2;
                    MyParticlesManager.GPUGenerationsPool.AllocateOrCreate(out generation2);
                    generation2.Start(this);
                    generation2.Init();
                    generation2.Deserialize(reader);
                    this.AddGeneration(generation2);
                }
            }
            if (!isEmptyElement)
            {
                reader.ReadEndElement();
            }
            if (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement();
                    while (true)
                    {
                        MyParticleLight light;
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            reader.ReadEndElement();
                            break;
                        }
                        MyParticlesManager.LightsPool.AllocateOrCreate(out light);
                        light.Start(this);
                        light.Init();
                        light.Deserialize(reader);
                        this.AddParticleLight(light);
                    }
                }
            }
            if (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement();
                    while (true)
                    {
                        MyParticleSound sound;
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            reader.ReadEndElement();
                            break;
                        }
                        MyParticlesManager.SoundsPool.AllocateOrCreate(out sound);
                        sound.Start(this);
                        sound.Init();
                        sound.Deserialize(reader);
                        this.AddParticleSound(sound);
                    }
                }
            }
            reader.ReadEndElement();
        }

        public void DeserializeFromObjectBuilder(MyObjectBuilder_ParticleEffect builder)
        {
            this.m_name = builder.Id.SubtypeName;
            this.m_particleID = builder.ParticleId;
            this.m_length = builder.Length;
            this.m_loop = builder.Loop;
            this.m_durationMin = builder.DurationMin;
            this.m_durationMax = builder.DurationMax;
            this.DistanceMax = builder.DistanceMax;
            this.SetRandomDuration();
            foreach (ParticleGeneration generation in builder.ParticleGenerations)
            {
                string generationType = generation.GenerationType;
                if (generationType != "CPU")
                {
                    MyParticleGPUGeneration generation2;
                    if (generationType != "GPU")
                    {
                        continue;
                    }
                    MyParticlesManager.GPUGenerationsPool.AllocateOrCreate(out generation2);
                    generation2.Start(this);
                    generation2.Init();
                    generation2.DeserializeFromObjectBuilder(generation);
                    this.AddGeneration(generation2);
                    continue;
                }
                if (MyParticlesManager.EnableCPUGenerations)
                {
                    MyParticleGeneration generation3;
                    MyParticlesManager.GenerationsPool.AllocateOrCreate(out generation3);
                    generation3.Start(this);
                    generation3.Init();
                    generation3.DeserializeFromObjectBuilder(generation);
                    this.AddGeneration(generation3);
                }
            }
            foreach (ParticleLight light in builder.ParticleLights)
            {
                MyParticleLight light2;
                MyParticlesManager.LightsPool.AllocateOrCreate(out light2);
                light2.Start(this);
                light2.Init();
                light2.DeserializeFromObjectBuilder(light);
                this.AddParticleLight(light2);
            }
            foreach (ParticleSound sound in builder.ParticleSounds)
            {
                MyParticleSound sound2;
                MyParticlesManager.SoundsPool.AllocateOrCreate(out sound2);
                sound2.Start(this);
                sound2.Init();
                sound2.DeserializeFromObjectBuilder(sound);
                this.AddParticleSound(sound2);
            }
        }

        public void Draw(List<MyBillboard> collectedBillboards)
        {
            using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Draw(collectedBillboards);
                }
            }
            if (this.TransformDirty)
            {
                this.m_lastWorldMatrix = this.m_worldMatrix;
                this.TransformDirty = false;
            }
        }

        public MyParticleEffect Duplicate()
        {
            MyParticleEffect effect = MyParticlesManager.EffectsPool.Allocate(false);
            effect.Start(0, this.Name);
            effect.m_length = this.m_length;
            effect.DurationMin = this.m_durationMin;
            effect.DurationMax = this.m_durationMax;
            effect.Loop = this.m_loop;
            using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IMyParticleGeneration generation = enumerator.Current.Duplicate(effect);
                    effect.AddGeneration(generation);
                }
            }
            using (List<MyParticleLight>.Enumerator enumerator2 = this.m_particleLights.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    MyParticleLight particleLight = enumerator2.Current.Duplicate(effect);
                    effect.AddParticleLight(particleLight);
                }
            }
            using (List<MyParticleSound>.Enumerator enumerator3 = this.m_particleSounds.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    MyParticleSound particleSound = enumerator3.Current.Duplicate(effect);
                    effect.AddParticleSound(particleSound);
                }
            }
            return effect;
        }

        public BoundingBoxD GetAABB() => 
            this.m_AABB;

        public MatrixD GetDeltaMatrix()
        {
            MatrixD.Multiply(ref MatrixD.Invert(this.m_lastWorldMatrix), ref this.m_worldMatrix, out this.DeltaMatrix);
            return this.DeltaMatrix;
        }

        public float GetElapsedTime() => 
            this.m_elapsedTime;

        public Vector3 GetEmitterAxisScale() => 
            (this.UserAxisScale * this.UserEmitterScale);

        public float GetEmitterScale() => 
            (this.UserScale * this.UserEmitterScale);

        public List<IMyParticleGeneration> GetGenerations() => 
            this.m_generations;

        public int GetID() => 
            this.m_particleID;

        internal MyConcurrentList<MyParticleEffect> GetInstances() => 
            this.m_instances;

        public string GetName() => 
            this.m_name;

        public List<MyParticleLight> GetParticleLights() => 
            this.m_particleLights;

        public int GetParticlesCount() => 
            this.ParticlesCount;

        public List<MyParticleSound> GetParticleSounds() => 
            this.m_particleSounds;

        public float GetScale() => 
            this.UserScale;

        public void Pause()
        {
            this.m_isSimulationPaused = true;
            this.m_isEmittingStopped = true;
            this.SetDirty();
        }

        public void Play()
        {
            this.m_isSimulationPaused = false;
            this.m_isEmittingStopped = false;
            this.SetDirty();
        }

        public void RemoveGeneration(int index)
        {
            IMyParticleGeneration item = this.m_generations[index];
            this.m_generations.Remove(item);
            item.Close();
            item.Deallocate();
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.RemoveGeneration(index);
                    }
                }
            }
        }

        public void RemoveGeneration(IMyParticleGeneration generation)
        {
            int index = this.m_generations.IndexOf(generation);
            this.RemoveGeneration(index);
        }

        public void RemoveInstance(MyParticleEffect effect)
        {
            if ((this.m_instances != null) && this.m_instances.Contains(effect))
            {
                this.m_instances.Remove(effect);
            }
        }

        public void RemoveParticleLight(int index)
        {
            MyParticleLight item = this.m_particleLights[index];
            this.m_particleLights.Remove(item);
            item.Close();
            MyParticlesManager.LightsPool.Deallocate(item);
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.RemoveParticleLight(index);
                    }
                }
            }
        }

        public void RemoveParticleLight(MyParticleLight particleLight)
        {
            int index = this.m_particleLights.IndexOf(particleLight);
            this.RemoveParticleLight(index);
        }

        public void RemoveParticleSound(int index)
        {
            MyParticleSound item = this.m_particleSounds[index];
            this.m_particleSounds.Remove(item);
            item.Close();
            MyParticlesManager.SoundsPool.Deallocate(item);
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.RemoveParticleSound(index);
                    }
                }
            }
        }

        public void RemoveParticleSound(MyParticleSound particleSound)
        {
            int index = this.m_particleSounds.IndexOf(particleSound);
            this.RemoveParticleSound(index);
        }

        public void Restart()
        {
            this.m_elapsedTime = 0f;
        }

        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("ParticleEffect");
            writer.WriteAttributeString("xsi", "type", null, "MyObjectBuilder_ParticleEffect");
            writer.WriteStartElement("Id");
            writer.WriteElementString("TypeId", "ParticleEffect");
            writer.WriteElementString("SubtypeId", this.Name);
            writer.WriteEndElement();
            writer.WriteElementString("Version", Version.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ParticleId", this.m_particleID.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Length", this.m_length.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("DurationMin", this.m_durationMin.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("DurationMax", this.m_durationMax.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("DistanceMax", this.DistanceMax.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Loop", this.m_loop.ToString(CultureInfo.InvariantCulture).ToLower());
            writer.WriteStartElement("ParticleGenerations");
            using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Serialize(writer);
                }
            }
            writer.WriteEndElement();
            writer.WriteStartElement("ParticleLights");
            using (List<MyParticleLight>.Enumerator enumerator2 = this.m_particleLights.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.Serialize(writer);
                }
            }
            writer.WriteEndElement();
            writer.WriteStartElement("ParticleSounds");
            using (List<MyParticleSound>.Enumerator enumerator3 = this.m_particleSounds.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    enumerator3.Current.Serialize(writer);
                }
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void SetAnimDirty()
        {
            using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.SetAnimDirty();
                }
            }
        }

        public void SetDirty()
        {
            using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.SetDirty();
                }
            }
        }

        public void SetDirtyInstances()
        {
            using (List<IMyParticleGeneration>.Enumerator enumerator = this.m_generations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.SetDirty();
                }
            }
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator2 = this.m_instances.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.SetDirtyInstances();
                    }
                }
            }
        }

        private void SetDurationMax(float duration)
        {
            this.m_durationMax = duration;
            this.SetRandomDuration();
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetDurationMax(duration);
                    }
                }
            }
        }

        private void SetDurationMin(float duration)
        {
            this.m_durationMin = duration;
            this.SetRandomDuration();
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetDurationMin(duration);
                    }
                }
            }
        }

        public void SetID(int id)
        {
            if (this.m_particleID != id)
            {
                this.m_particleID = id;
                MyParticlesLibrary.UpdateParticleEffectID(this.m_particleID);
            }
        }

        private void SetLoop(bool loop)
        {
            this.m_loop = loop;
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetLoop(loop);
                    }
                }
            }
        }

        public void SetName(string name)
        {
            if (this.m_name != name)
            {
                string str = this.m_name;
                this.m_name = name;
                if (str != null)
                {
                    MyParticlesLibrary.UpdateParticleEffectName(str);
                }
            }
        }

        public void SetRandomDuration()
        {
            this.m_durationActual = (this.m_durationMax > this.m_durationMin) ? MyUtils.GetRandomFloat(this.m_durationMin, this.m_durationMax) : this.m_durationMin;
        }

        public void SetShowOnlyThisGeneration(int generationIndex)
        {
            this.m_showOnlyThisGeneration = generationIndex;
            for (int i = 0; i < this.m_generations.Count; i++)
            {
                this.m_generations[i].Show = (generationIndex < 0) || (i == generationIndex);
            }
            if (this.m_instances != null)
            {
                using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetShowOnlyThisGeneration(generationIndex);
                    }
                }
            }
        }

        public void SetShowOnlyThisGeneration(IMyParticleGeneration generation)
        {
            this.SetDirty();
            for (int i = 0; i < this.m_generations.Count; i++)
            {
                if (this.m_generations[i] == generation)
                {
                    this.SetShowOnlyThisGeneration(i);
                    return;
                }
            }
            this.SetShowOnlyThisGeneration(-1);
        }

        public void SetTranslation(Vector3D trans)
        {
            this.TransformDirty = true;
            this.m_worldMatrix.Translation = trans;
        }

        public void Start(int particleID, string particleName)
        {
            this.m_particleID = particleID;
            this.m_name = particleName;
            this.m_parentID = uint.MaxValue;
            this.m_isStopped = false;
            this.m_isEmittingStopped = false;
            this.m_isSimulationPaused = false;
            this.m_distance = 0f;
            this.UserEmitterScale = 1f;
            this.UserBirthMultiplier = 1f;
            this.UserRadiusMultiplier = 1f;
            this.UserScale = 1f;
            this.UserAxisScale = Vector3.One;
            this.UserColorMultiplier = Vector4.One;
            this.UserDraw = false;
            this.Enabled = true;
            this.EnableLods = true;
            this.m_instantStop = false;
            this.m_velocitySet = false;
            this.WorldMatrix = MatrixD.Identity;
            this.DeltaMatrix = MatrixD.Identity;
            this.CalculateDeltaMatrix = false;
            this.RenderCounter = 0;
            this.m_updateCounter = 0;
        }

        public void Stop(bool instant = true)
        {
            this.m_isStopped = true;
            this.m_isEmittingStopped = true;
            this.m_instantStop = instant;
            this.SetDirty();
        }

        public void StopEmitting(float timeout = 0f)
        {
            this.m_isEmittingStopped = true;
            this.m_timer = timeout;
            this.SetDirty();
        }

        public void StopLights()
        {
            foreach (MyParticleLight light in this.m_particleLights)
            {
                if (light.Enabled != null)
                {
                    light.Enabled.SetValue(false);
                }
            }
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.Name, " (", this.ID, ")" };
            return string.Concat(objArray1);
        }

        public bool Update()
        {
            if (!this.Enabled)
            {
                return this.m_isStopped;
            }
            if (this.WorldMatrix == MatrixD.Zero)
            {
                return true;
            }
            if (this.ParentID == uint.MaxValue)
            {
                float num = 100f;
                if ((MyParticlesManager.CalculateGravityInPoint != null) && (this.m_updateCounter == 0))
                {
                    Vector3 vector = MyParticlesManager.CalculateGravityInPoint(this.WorldMatrix.Translation);
                    float num2 = vector.Length();
                    if (num2 > num)
                    {
                        vector = (vector / num2) * num;
                    }
                    this.Gravity = vector;
                }
                this.m_updateCounter++;
                if (this.m_updateCounter > 200)
                {
                    this.m_updateCounter = 0;
                }
            }
            if (this.m_velocitySet)
            {
                Vector3D vectord = this.m_worldMatrix.Translation + (this.m_velocity * 0.01666667f);
                this.m_worldMatrix.Translation = vectord;
                this.TransformDirty = true;
            }
            if (this.m_anyCpuGeneration)
            {
                this.m_distance = ((float) Vector3D.Distance(MyTransparentGeometry.Camera.Translation, this.WorldMatrix.Translation)) / 100f;
                this.ParticlesCount = 0;
                this.m_AABB = BoundingBoxD.CreateInvalid();
                for (int i = 0; i < this.m_generations.Count; i++)
                {
                    if ((this.m_showOnlyThisGeneration < 0) || (i == this.m_showOnlyThisGeneration))
                    {
                        this.m_generations[i].Update();
                        this.m_generations[i].MergeAABB(ref this.m_AABB);
                    }
                }
            }
            if (!MyParticlesManager.Paused)
            {
                using (List<MyParticleLight>.Enumerator enumerator = this.m_particleLights.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Update();
                    }
                }
                using (List<MyParticleSound>.Enumerator enumerator2 = this.m_particleSounds.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.Update(false);
                    }
                }
            }
            if (this.OnUpdate != null)
            {
                this.OnUpdate(this, null);
            }
            return this.UpdateLife();
        }

        public bool UpdateLife()
        {
            this.m_elapsedTime += 0.01666667f;
            if (this.m_timer > 0f)
            {
                this.m_timer -= 0.01666667f;
                if (this.m_timer <= 0f)
                {
                    return true;
                }
            }
            if (this.m_loop && (this.m_elapsedTime >= this.m_durationActual))
            {
                this.m_elapsedTime = 0f;
                this.SetRandomDuration();
            }
            return (!this.m_isStopped ? ((this.m_durationActual > 0f) && (this.m_elapsedTime > this.m_durationActual)) : (this.ParticlesCount == 0));
        }

        public bool TransformDirty { get; private set; }

        public float UserEmitterScale
        {
            get => 
                this.m_userEmitterScale;
            set
            {
                this.m_userEmitterScale = value;
                this.TransformDirty = true;
            }
        }

        public float UserScale
        {
            get => 
                this.m_userScale;
            set
            {
                this.m_userScale = value;
                this.TransformDirty = true;
            }
        }

        public uint ParentID
        {
            get => 
                this.m_parentID;
            set
            {
                this.m_parentID = value;
                this.SetAnimDirty();
            }
        }

        public float UserBirthMultiplier
        {
            get => 
                this.m_userBirthMultiplier;
            set
            {
                if (this.m_userBirthMultiplier != value)
                {
                    this.m_userBirthMultiplier = value;
                    this.SetAnimDirty();
                }
            }
        }

        public float UserRadiusMultiplier
        {
            get => 
                this.m_userRadiusMultiplier;
            set
            {
                this.m_userRadiusMultiplier = value;
                this.SetDirty();
            }
        }

        public Vector4 UserColorMultiplier
        {
            get => 
                this.m_userColorMultiplier;
            set
            {
                this.m_userColorMultiplier = value;
                this.SetDirty();
            }
        }

        [Browsable(false)]
        public int ShowOnlyThisGeneration =>
            this.m_showOnlyThisGeneration;

        public Vector3 Velocity
        {
            get => 
                this.m_velocity;
            set
            {
                this.m_velocity = value;
                this.m_velocitySet = true;
            }
        }

        public Vector3 Gravity { get; private set; }

        public bool Enabled { get; set; }

        public int ID
        {
            get => 
                this.m_particleID;
            set => 
                this.SetID(value);
        }

        public float DistanceMax { get; set; }

        public float Length
        {
            get => 
                this.m_length;
            set
            {
                this.m_length = value;
                if (this.m_instances != null)
                {
                    using (ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, MyParticleEffect, List<MyParticleEffect>.Enumerator> enumerator = this.m_instances.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Length = value;
                        }
                    }
                }
            }
        }

        [Browsable(false)]
        public float Duration =>
            this.m_durationActual;

        public float DurationMin
        {
            get => 
                this.m_durationMin;
            set => 
                this.SetDurationMin(value);
        }

        public float DurationMax
        {
            get => 
                this.m_durationMax;
            set => 
                this.SetDurationMax(value);
        }

        public bool Loop
        {
            get => 
                this.m_loop;
            set => 
                this.SetLoop(value);
        }

        [Browsable(false)]
        public MatrixD WorldMatrix
        {
            get => 
                this.m_worldMatrix;
            set
            {
                if (!value.EqualsFast(ref this.m_worldMatrix, 0.001))
                {
                    this.TransformDirty = true;
                    this.m_worldMatrix = value;
                }
            }
        }

        public string Name
        {
            get => 
                this.m_name;
            set => 
                this.SetName(value);
        }

        [Browsable(false)]
        public float Distance =>
            this.m_distance;

        [Browsable(false)]
        public object Tag { get; set; }

        [Browsable(false)]
        public bool IsStopped =>
            this.m_isStopped;

        public bool IsSimulationPaused =>
            this.m_isSimulationPaused;

        public bool IsEmittingStopped =>
            this.m_isEmittingStopped;
    }
}

