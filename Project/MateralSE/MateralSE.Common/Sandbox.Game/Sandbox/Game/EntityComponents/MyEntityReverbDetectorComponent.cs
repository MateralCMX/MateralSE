namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_EntityReverbDetectorComponent), true)]
    public class MyEntityReverbDetectorComponent : MyEntityComponentBase
    {
        private const float RAYCAST_LENGTH = 25f;
        private const float INFINITY_PENALTY = 50f;
        private const float REVERB_THRESHOLD_SMALL = 3f;
        private const float REVERB_THRESHOLD_MEDIUM = 7f;
        private const float REVERB_THRESHOLD_LARGE = 12f;
        private const int REVERB_NO_OBSTACLE_LIMIT = 3;
        private static Vector3[] m_directions = new Vector3[0x1a];
        private static bool m_systemInitialized = false;
        private static int m_currentReverbPreset = -1;
        private float[] m_detectedLengths;
        private ReverbDetectedType[] m_detectedObjects;
        private MyEntity m_entity;
        private int m_currentDirectionIndex;
        private bool m_componentInitialized;
        private bool m_sendInformationToAudio;

        public float GetDetectedAverage(bool onlyDetected = false)
        {
            float num = 0f;
            int num2 = 0;
            for (int i = 0; i < this.m_detectedLengths.Length; i++)
            {
                if (this.m_detectedLengths[i] >= 0f)
                {
                    num += this.m_detectedLengths[i];
                    num2++;
                }
                else if (!onlyDetected)
                {
                    num += 50f;
                }
            }
            return (!onlyDetected ? (num / ((float) this.m_detectedLengths.Length)) : ((num2 > 0) ? (num / ((float) num2)) : 50f));
        }

        public int GetDetectedNumberOfObjects(ReverbDetectedType type = 2)
        {
            int num = 0;
            for (int i = 0; i < this.m_detectedObjects.Length; i++)
            {
                if (this.m_detectedObjects[i] == type)
                {
                    num++;
                }
            }
            return num;
        }

        public void InitComponent(MyEntity entity, bool sendInformationToAudio)
        {
            int index = 0;
            if (!m_systemInitialized)
            {
                int num2 = -1;
                while (true)
                {
                    if (num2 > 1)
                    {
                        m_systemInitialized = true;
                        break;
                    }
                    int num4 = -1;
                    while (true)
                    {
                        if (num4 > 1)
                        {
                            num2++;
                            break;
                        }
                        int num3 = -1;
                        while (true)
                        {
                            if (num3 > 1)
                            {
                                num4++;
                                break;
                            }
                            if (((num2 != 0) || (num3 != 0)) || (num4 != 0))
                            {
                                m_directions[index] = Vector3.Normalize(new Vector3((float) num2, (float) num3, (float) num4));
                                index++;
                            }
                            num3++;
                        }
                    }
                }
            }
            this.m_entity = entity;
            this.m_detectedLengths = new float[m_directions.Length];
            this.m_detectedObjects = new ReverbDetectedType[m_directions.Length];
            for (index = 0; index < m_directions.Length; index++)
            {
                this.m_detectedLengths[index] = -1f;
                this.m_detectedObjects[index] = ReverbDetectedType.None;
            }
            this.m_sendInformationToAudio = sendInformationToAudio && MyPerGameSettings.UseReverbEffect;
            this.m_componentInitialized = true;
        }

        private static void SetReverb(float distance, int grids, int voxels)
        {
            if (MyAudio.Static != null)
            {
                int num1;
                int num3;
                int num = (m_directions.Length - grids) - voxels;
                int num2 = -1;
                if (!MySession.Static.Settings.RealisticSound)
                {
                    num1 = 1;
                }
                else if ((MySession.Static.LocalCharacter == null) || (MySession.Static.LocalCharacter.AtmosphereDetectorComp == null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = MySession.Static.LocalCharacter.AtmosphereDetectorComp.InShipOrStation ? 1 : ((int) MySession.Static.LocalCharacter.AtmosphereDetectorComp.InAtmosphere);
                }
                if (((num3 != 0) && (distance <= 12f)) && (num <= 3))
                {
                    num2 = (voxels <= grids) ? 0 : 1;
                }
                if (num2 != m_currentReverbPreset)
                {
                    m_currentReverbPreset = num2;
                    if (m_currentReverbPreset <= -1)
                    {
                        MyAudio.Static.ApplyReverb = false;
                        MySessionComponentPlanetAmbientSounds.SetAmbientOn();
                    }
                    else if (m_currentReverbPreset == 0)
                    {
                        MyAudio.Static.ApplyReverb = false;
                        MySessionComponentPlanetAmbientSounds.SetAmbientOff();
                    }
                    else
                    {
                        MyAudio.Static.ApplyReverb = true;
                        MySessionComponentPlanetAmbientSounds.SetAmbientOff();
                    }
                }
            }
        }

        public void Update()
        {
            if (this.Initialized && (this.m_entity != null))
            {
                Vector3 center = (Vector3) this.m_entity.PositionComp.WorldAABB.Center;
                Vector3 to = center + (m_directions[this.m_currentDirectionIndex] * 25f);
                LineD ed = new LineD(center, to);
                MyPhysics.HitInfo? nullable = MyPhysics.CastRay(ed.From, ed.To, 30);
                IMyEntity hitEntity = null;
                Vector3D zero = Vector3D.Zero;
                if (nullable != null)
                {
                    hitEntity = nullable.Value.HkHitInfo.GetHitEntity() as MyEntity;
                    zero = nullable.Value.Position;
                    MyPhysics.HitInfo local1 = nullable.Value;
                }
                if (hitEntity == null)
                {
                    this.m_detectedLengths[this.m_currentDirectionIndex] = -1f;
                    this.m_detectedObjects[this.m_currentDirectionIndex] = ReverbDetectedType.None;
                }
                else
                {
                    int num1;
                    this.m_detectedLengths[this.m_currentDirectionIndex] = Vector3.Distance(center, (Vector3) zero);
                    if ((hitEntity is MyCubeGrid) || (hitEntity is MyCubeBlock))
                    {
                        num1 = 2;
                    }
                    else
                    {
                        num1 = 1;
                    }
                    this.m_detectedObjects[this.m_currentDirectionIndex] = (ReverbDetectedType) num1;
                }
                this.m_currentDirectionIndex++;
                if (this.m_currentDirectionIndex >= m_directions.Length)
                {
                    this.m_currentDirectionIndex = 0;
                    if (this.m_sendInformationToAudio)
                    {
                        this.Grids = this.GetDetectedNumberOfObjects(ReverbDetectedType.Grid);
                        this.Voxels = this.GetDetectedNumberOfObjects(ReverbDetectedType.Voxel);
                        SetReverb(this.GetDetectedAverage(false), this.Grids, this.Voxels);
                    }
                }
            }
        }

        public bool Initialized =>
            (this.m_componentInitialized && m_systemInitialized);

        public static string CurrentReverbPreset =>
            ((m_currentReverbPreset != 1) ? ((m_currentReverbPreset != 0) ? "None (reverb is off)" : "Ship or station") : "Cave");

        public int Voxels { get; private set; }

        public int Grids { get; private set; }

        public override string ComponentTypeDebugString =>
            "EntityReverbDetector";

        public enum ReverbDetectedType
        {
            None,
            Voxel,
            Grid
        }
    }
}

