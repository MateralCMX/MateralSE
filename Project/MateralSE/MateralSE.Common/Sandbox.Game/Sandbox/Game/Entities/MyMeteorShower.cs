namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate), StaticEventOwner]
    internal class MyMeteorShower : MySessionComponentBase
    {
        private static readonly int WAVES_IN_SHOWER = 1;
        private static readonly double HORIZON_ANGLE_FROM_ZENITH_RATIO = Math.Sin(0.35);
        private static readonly double METEOR_BLUR_KOEF = 2.5;
        private static Vector3D m_tgtPos;
        private static Vector3D m_normalSun;
        private static Vector3D m_pltTgtDir;
        private static Vector3D m_mirrorDir;
        private static int m_waveCounter;
        private static List<MyEntity> m_meteorList = new List<MyEntity>();
        private static List<MyEntity> m_tmpEntityList = new List<MyEntity>();
        private static BoundingSphereD? m_currentTarget;
        private static List<BoundingSphereD> m_targetList = new List<BoundingSphereD>();
        private static int m_lastTargetCount;
        private static Vector3 m_downVector;
        private static Vector3 m_rightVector = Vector3.Zero;
        private static int m_meteorcount;
        private static List<MyCubeGrid> m_tmpHitGroup = new List<MyCubeGrid>();
        private static string[] m_enviromentHostilityName = new string[] { "Safe", "MeteorWave", "MeteorWaveCataclysm", "MeteorWaveCataclysmUnreal" };
        private static Vector3D m_meteorHitPos;

        public override void BeforeStart()
        {
            MyGlobalEventBase eventById;
            base.BeforeStart();
            if (Sync.IsServer)
            {
                eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "MeteorWave"));
                if (eventById == null)
                {
                    eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "MeteorWaveCataclysm"));
                }
                if (eventById == null)
                {
                    eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "MeteorWaveCataclysmUnreal"));
                }
                if (((eventById == null) && (MySession.Static.EnvironmentHostility != MyEnvironmentHostilityEnum.SAFE)) && MyFakes.ENABLE_METEOR_SHOWERS)
                {
                    MyGlobalEventBase globalEvent = MyGlobalEventFactory.CreateEvent(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "MeteorWave"));
                    globalEvent.SetActivationTime(CalculateShowerTime(MySession.Static.EnvironmentHostility));
                    MyGlobalEvents.AddGlobalEvent(globalEvent);
                    return;
                }
                if (eventById != null)
                {
                    if (MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.SAFE)
                    {
                        goto TR_0002;
                    }
                    else if (MyFakes.ENABLE_METEOR_SHOWERS)
                    {
                        eventById.Enabled = true;
                        if ((MySession.Static.PreviousEnvironmentHostility != null) && (MySession.Static.EnvironmentHostility != ((MyEnvironmentHostilityEnum) MySession.Static.PreviousEnvironmentHostility.Value)))
                        {
                            MyEnvironmentHostilityEnum? previousEnvironmentHostility = MySession.Static.PreviousEnvironmentHostility;
                            eventById.SetActivationTime(CalculateShowerTime(MySession.Static.EnvironmentHostility, previousEnvironmentHostility.Value, eventById.ActivationTime));
                            previousEnvironmentHostility = null;
                            MySession.Static.PreviousEnvironmentHostility = previousEnvironmentHostility;
                        }
                    }
                    else
                    {
                        goto TR_0002;
                    }
                }
            }
            return;
        TR_0002:
            eventById.Enabled = false;
        }

        public static TimeSpan CalculateShowerTime(MyEnvironmentHostilityEnum hostility)
        {
            double num = 5.0;
            switch (hostility)
            {
                case MyEnvironmentHostilityEnum.NORMAL:
                    num = MathHelper.Max((double) 0.4, (double) (GetActivationTime(hostility, 16.0, 24.0) / ((double) MathHelper.Max(1f, (float) m_lastTargetCount))));
                    break;

                case MyEnvironmentHostilityEnum.CATACLYSM:
                    num = MathHelper.Max((double) 0.4, (double) (GetActivationTime(hostility, 1.0, 1.5) / ((double) MathHelper.Max(1f, (float) m_lastTargetCount))));
                    break;

                case MyEnvironmentHostilityEnum.CATACLYSM_UNREAL:
                    num = GetActivationTime(hostility, 0.10000000149011612, 0.30000001192092896);
                    break;

                default:
                    break;
            }
            return TimeSpan.FromMinutes(num);
        }

        public static TimeSpan CalculateShowerTime(MyEnvironmentHostilityEnum newHostility, MyEnvironmentHostilityEnum oldHostility, TimeSpan oldTime)
        {
            double totalMinutes = oldTime.TotalMinutes;
            double num2 = 1.0;
            if (oldHostility != MyEnvironmentHostilityEnum.SAFE)
            {
                num2 = totalMinutes / GetMaxActivationTime(oldHostility);
            }
            return TimeSpan.FromMinutes(num2 * GetMaxActivationTime(newHostility));
        }

        private static void CheckTargetValid()
        {
            if (m_currentTarget != null)
            {
                m_tmpEntityList.Clear();
                m_tmpEntityList = MyEntities.GetEntitiesInSphere(ref m_currentTarget.Value);
                if (m_tmpEntityList.OfType<MyCubeGrid>().ToList<MyCubeGrid>().Count == 0)
                {
                    m_waveCounter = -1;
                }
                if ((m_waveCounter >= 0) && (MyMusicController.Static != null))
                {
                    foreach (MyEntity entity in m_tmpEntityList)
                    {
                        if (!(entity is MyCharacter))
                        {
                            continue;
                        }
                        if ((MySession.Static != null) && ReferenceEquals(entity as MyCharacter, MySession.Static.LocalCharacter))
                        {
                            MyMusicController.Static.MeteorShowerIncoming();
                            break;
                        }
                    }
                }
                m_tmpEntityList.Clear();
            }
        }

        private static void ClearMeteorList()
        {
            m_meteorList.Clear();
        }

        public override void Draw()
        {
            base.Draw();
            if (MyDebugDrawSettings.DEBUG_DRAW_METEORITS_DIRECTIONS)
            {
                Vector3D correctedDirection = GetCorrectedDirection(MySector.DirectionToSunNormalized);
                MyRenderProxy.DebugDrawPoint(m_meteorHitPos, Color.White, false, false);
                MyRenderProxy.DebugDrawText3D(m_meteorHitPos, "Hit position", Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                MyRenderProxy.DebugDrawLine3D(m_tgtPos, m_tgtPos + (10f * MySector.DirectionToSunNormalized), Color.Yellow, Color.Yellow, false, false);
                MyRenderProxy.DebugDrawText3D(m_tgtPos + (10f * MySector.DirectionToSunNormalized), "Sun direction (sd)", Color.Yellow, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, -1, false);
                MyRenderProxy.DebugDrawLine3D(m_tgtPos, m_tgtPos + (10.0 * correctedDirection), Color.Red, Color.Red, false, false);
                MyRenderProxy.DebugDrawText3D(m_tgtPos + (10.0 * correctedDirection), "Current meteorits direction (cd)", Color.Red, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, -1, false);
                if (MyGravityProviderSystem.IsPositionInNaturalGravity(m_tgtPos, 0.0))
                {
                    MyRenderProxy.DebugDrawLine3D(m_tgtPos, m_tgtPos + (10.0 * m_normalSun), Color.Blue, Color.Blue, false, false);
                    MyRenderProxy.DebugDrawText3D(m_tgtPos + (10.0 * m_normalSun), "Perpendicular to sd and n0 ", Color.Blue, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                    MyRenderProxy.DebugDrawLine3D(m_tgtPos, m_tgtPos + (10.0 * m_pltTgtDir), Color.Green, Color.Green, false, false);
                    MyRenderProxy.DebugDrawText3D(m_tgtPos + (10.0 * m_pltTgtDir), "Dir from center of planet to target (n0)", Color.Green, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                    MyRenderProxy.DebugDrawLine3D(m_tgtPos, m_tgtPos + (10.0 * m_mirrorDir), Color.Purple, Color.Purple, false, false);
                    MyRenderProxy.DebugDrawText3D(m_tgtPos + (10.0 * m_mirrorDir), "Horizon in plane n0 and sd (ho)", Color.Purple, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                }
            }
        }

        public static double GetActivationTime(MyEnvironmentHostilityEnum hostility, double defaultMinMinutes, double defaultMaxMinutes)
        {
            MyGlobalEventDefinition eventDefinition = MyDefinitionManager.Static.GetEventDefinition(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), m_enviromentHostilityName[(int) hostility]));
            if (eventDefinition != null)
            {
                if (eventDefinition.MinActivationTime != null)
                {
                    defaultMinMinutes = eventDefinition.MinActivationTime.Value.TotalMinutes;
                }
                if (eventDefinition.MaxActivationTime != null)
                {
                    defaultMaxMinutes = eventDefinition.MaxActivationTime.Value.TotalMinutes;
                }
            }
            return MyUtils.GetRandomDouble(defaultMinMinutes, defaultMaxMinutes);
        }

        private static Vector3 GetCorrectedDirection(Vector3 direction)
        {
            Vector3 vector = direction;
            if (m_currentTarget != null)
            {
                Vector3D center = m_currentTarget.Value.Center;
                m_tgtPos = center;
                if (!MyGravityProviderSystem.IsPositionInNaturalGravity(center, 0.0))
                {
                    return vector;
                }
                Vector3D vectord2 = -Vector3D.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(center));
                Vector3D vectord3 = Vector3D.Normalize(Vector3D.Cross(vectord2, vector));
                Vector3D normal = Vector3D.Normalize(Vector3D.Cross(vectord3, vectord2));
                m_mirrorDir = normal;
                m_pltTgtDir = vectord2;
                m_normalSun = vectord3;
                double num = vectord2.Dot(vector);
                if (num < -HORIZON_ANGLE_FROM_ZENITH_RATIO)
                {
                    return (Vector3) Vector3D.Reflect(-vector, normal);
                }
                if (num < HORIZON_ANGLE_FROM_ZENITH_RATIO)
                {
                    return (Vector3) Vector3D.Transform(normal, MatrixD.CreateFromAxisAngle(vectord3, -Math.Asin(HORIZON_ANGLE_FROM_ZENITH_RATIO)));
                }
            }
            return vector;
        }

        private static double GetMaxActivationTime(MyEnvironmentHostilityEnum enviroment)
        {
            double totalMinutes = 0.0;
            switch (enviroment)
            {
                case MyEnvironmentHostilityEnum.NORMAL:
                    totalMinutes = 24.0;
                    break;

                case MyEnvironmentHostilityEnum.CATACLYSM:
                    totalMinutes = 1.5;
                    break;

                case MyEnvironmentHostilityEnum.CATACLYSM_UNREAL:
                    totalMinutes = 0.30000001192092896;
                    break;

                default:
                    break;
            }
            MyGlobalEventDefinition eventDefinition = MyDefinitionManager.Static.GetEventDefinition(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), m_enviromentHostilityName[(int) enviroment]));
            if ((eventDefinition != null) && (eventDefinition.MaxActivationTime != null))
            {
                totalMinutes = eventDefinition.MaxActivationTime.Value.TotalMinutes;
            }
            return totalMinutes;
        }

        private static unsafe void GetTargets()
        {
            List<MyCubeGrid> list = MyEntities.GetEntities().OfType<MyCubeGrid>().ToList<MyCubeGrid>();
            for (int i = 0; i < list.Count; i++)
            {
                Vector3I vectori = (Vector3I) ((list[i].Max - list[i].Min) + Vector3I.One);
                if ((vectori.Size < 0x10) || !MySessionComponentTriggerSystem.Static.IsAnyTriggerActive(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
            while (list.Count > 0)
            {
                MyCubeGrid item = list[MyUtils.GetRandomInt(list.Count - 1)];
                m_tmpHitGroup.Add(item);
                list.Remove(item);
                BoundingSphereD worldVolume = item.PositionComp.WorldVolume;
                bool flag = true;
                while (true)
                {
                    if (!flag)
                    {
                        double* numPtr2 = (double*) ref worldVolume.Radius;
                        numPtr2[0] += 150.0;
                        m_targetList.Add(worldVolume);
                        break;
                    }
                    flag = false;
                    foreach (MyCubeGrid grid2 in m_tmpHitGroup)
                    {
                        worldVolume.Include(grid2.PositionComp.WorldVolume);
                    }
                    m_tmpHitGroup.Clear();
                    double* numPtr1 = (double*) ref worldVolume.Radius;
                    numPtr1[0] += 10.0;
                    for (int j = 0; j < list.Count; j++)
                    {
                        BoundingSphereD ed2 = list[j].PositionComp.WorldVolume;
                        if (ed2.Intersects(worldVolume))
                        {
                            flag = true;
                            m_tmpHitGroup.Add(list[j]);
                            list.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
            m_lastTargetCount = m_targetList.Count;
        }

        public override void LoadData()
        {
            m_waveCounter = -1;
            m_lastTargetCount = 0;
            base.LoadData();
        }

        [MyGlobalEventHandler(typeof(MyObjectBuilder_GlobalEventBase), "MeteorWave")]
        public static void MeteorWave(object senderEvent)
        {
            MeteorWaveInternal(senderEvent);
        }

        private static void MeteorWaveInternal(object senderEvent)
        {
            if (MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.SAFE)
            {
                ((MyGlobalEventBase) senderEvent).Enabled = false;
            }
            else if (Sync.IsServer)
            {
                m_waveCounter++;
                if (m_waveCounter == 0)
                {
                    int num1;
                    ClearMeteorList();
                    if (m_targetList.Count == 0)
                    {
                        GetTargets();
                        if (m_targetList.Count == 0)
                        {
                            m_waveCounter = WAVES_IN_SHOWER + 1;
                            RescheduleEvent(senderEvent);
                            return;
                        }
                    }
                    m_currentTarget = new BoundingSphereD?(m_targetList.ElementAt<BoundingSphereD>(MyUtils.GetRandomInt(m_targetList.Count - 1)));
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<BoundingSphereD?>(x => new Action<BoundingSphereD?>(MyMeteorShower.UpdateShowerTarget), m_currentTarget, targetEndpoint, position);
                    m_targetList.Remove(m_currentTarget.Value);
                    m_meteorcount = (int) ((Math.Pow(m_currentTarget.Value.Radius, 2.0) * 3.1415926535897931) / 6000.0);
                    if ((MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM) || (MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM_UNREAL))
                    {
                        num1 = 1;
                    }
                    else
                    {
                        num1 = 8;
                    }
                    m_meteorcount /= num1;
                    m_meteorcount = MathHelper.Clamp(m_meteorcount, 1, 30);
                }
                RescheduleEvent(senderEvent);
                CheckTargetValid();
                if (m_waveCounter >= 0)
                {
                    StartWave();
                }
            }
        }

        private static void RescheduleEvent(object senderEvent)
        {
            if (m_waveCounter <= WAVES_IN_SHOWER)
            {
                TimeSpan time = TimeSpan.FromSeconds((double) ((((float) m_meteorcount) / 5f) + MyUtils.GetRandomFloat(2f, 5f)));
                MyGlobalEvents.RescheduleEvent((MyGlobalEventBase) senderEvent, time);
            }
            else
            {
                TimeSpan time = CalculateShowerTime(MySession.Static.EnvironmentHostility);
                MyGlobalEvents.RescheduleEvent((MyGlobalEventBase) senderEvent, time);
                m_waveCounter = -1;
                m_currentTarget = null;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<BoundingSphereD?>(x => new Action<BoundingSphereD?>(MyMeteorShower.UpdateShowerTarget), m_currentTarget, targetEndpoint, position);
            }
        }

        private static void SetupDirVectors(Vector3 direction)
        {
            if (m_rightVector == Vector3.Zero)
            {
                direction.CalculatePerpendicularVector(out m_rightVector);
                m_downVector = MyUtils.Normalize(Vector3.Cross(direction, m_rightVector));
            }
        }

        public static void StartDebugWave(Vector3 pos)
        {
            int num1;
            m_currentTarget = new BoundingSphereD(pos, 100.0);
            m_meteorcount = (int) ((Math.Pow(m_currentTarget.Value.Radius, 2.0) * 3.1415926535897931) / 3000.0);
            if ((MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM) || (MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM_UNREAL))
            {
                num1 = 1;
            }
            else
            {
                num1 = 8;
            }
            m_meteorcount /= num1;
            m_meteorcount = MathHelper.Clamp(m_meteorcount, 1, 40);
            StartWave();
        }

        private static void StartWave()
        {
            if (m_currentTarget != null)
            {
                Vector3 correctedDirection = GetCorrectedDirection(MySector.DirectionToSunNormalized);
                SetupDirVectors(correctedDirection);
                float randomFloat = MyUtils.GetRandomFloat((float) Math.Min(2, m_meteorcount - 3), (float) (m_meteorcount + 3));
                Vector3 vector2 = MyUtils.GetRandomVector3CircleNormalized();
                float num2 = MyUtils.GetRandomFloat(0f, 1f);
                Vector3D vectord = (Vector3D) ((vector2.X * m_rightVector) + (vector2.Z * m_downVector));
                Vector3D worldPoint = m_currentTarget.Value.Center + (((Math.Pow((double) num2, 0.699999988079071) * m_currentTarget.Value.Radius) * vectord) * METEOR_BLUR_KOEF);
                Vector3D vectord3 = -Vector3D.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(worldPoint));
                if (vectord3 != Vector3D.Zero)
                {
                    MyPhysics.HitInfo? nullable = MyPhysics.CastRay(worldPoint + (vectord3 * 3000.0), worldPoint, 15);
                    if (nullable != null)
                    {
                        worldPoint = nullable.Value.Position;
                    }
                }
                m_meteorHitPos = worldPoint;
                for (int i = 0; i < randomFloat; i++)
                {
                    vector2 = MyUtils.GetRandomVector3CircleNormalized();
                    num2 = MyUtils.GetRandomFloat(0f, 1f);
                    vectord = (Vector3D) ((vector2.X * m_rightVector) + (vector2.Z * m_downVector));
                    worldPoint += (Math.Pow((double) num2, 0.699999988079071) * m_currentTarget.Value.Radius) * vectord;
                    Vector3 vector3 = correctedDirection * (0x7d0 + (100 * i));
                    vector2 = MyUtils.GetRandomVector3CircleNormalized();
                    vectord = (Vector3D) ((vector2.X * m_rightVector) + (vector2.Z * m_downVector));
                    Vector3D position = (worldPoint + vector3) + (((float) Math.Tan((double) MyUtils.GetRandomFloat(0f, 0.1745329f))) * vectord);
                    m_meteorList.Add(MyMeteor.SpawnRandom(position, Vector3.Normalize(worldPoint - position)));
                }
                m_rightVector = Vector3.Zero;
            }
        }

        protected override void UnloadData()
        {
            foreach (MyEntity entity in m_meteorList)
            {
                if (!entity.MarkedForClose)
                {
                    entity.Close();
                }
            }
            m_meteorList.Clear();
            m_currentTarget = null;
            m_targetList.Clear();
            base.UnloadData();
        }

        [Event(null, 0x1fa), Reliable, Broadcast]
        private static void UpdateShowerTarget([Serialize(MyObjectFlags.DefaultZero)] BoundingSphereD? target)
        {
            if (target != null)
            {
                CurrentTarget = new BoundingSphereD(target.Value.Center, target.Value.Radius);
            }
            else
            {
                CurrentTarget = null;
            }
        }

        public static BoundingSphereD? CurrentTarget
        {
            get => 
                m_currentTarget;
            set => 
                (m_currentTarget = value);
        }

        public override bool IsRequiredByGame =>
            (MyPerGameSettings.Game == GameEnum.SE_GAME);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMeteorShower.<>c <>9 = new MyMeteorShower.<>c();
            public static Func<IMyEventOwner, Action<BoundingSphereD?>> <>9__26_0;
            public static Func<IMyEventOwner, Action<BoundingSphereD?>> <>9__33_0;

            internal Action<BoundingSphereD?> <MeteorWaveInternal>b__26_0(IMyEventOwner x) => 
                new Action<BoundingSphereD?>(MyMeteorShower.UpdateShowerTarget);

            internal Action<BoundingSphereD?> <RescheduleEvent>b__33_0(IMyEventOwner x) => 
                new Action<BoundingSphereD?>(MyMeteorShower.UpdateShowerTarget);
        }
    }
}

