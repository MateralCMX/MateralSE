namespace Sandbox.AppCode.Game.TransparentGeometry
{
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, Priority=0x3e8)]
    internal class MySunWind : MySessionComponentBase
    {
        public static bool IsActive = false;
        public static bool IsVisible = true;
        public static Vector3D Position;
        private static Vector3D m_initialSunWindPosition;
        private static Vector3D m_directionFromSunNormalized;
        private static PlaneD m_planeMiddle;
        private static PlaneD m_planeFront;
        private static PlaneD m_planeBack;
        private static double m_distanceToSunWind;
        private static Vector3D m_positionOnCameraLine;
        private static int m_timeLastUpdate;
        private static float m_speed;
        private static Vector3D m_rightVector;
        private static Vector3D m_downVector;
        private static float m_strength;
        public static Type[] DoNotIgnoreTheseTypes = new Type[] { typeof(MyVoxelMap) };
        private static MySunWindBillboard[][] m_largeBillboards;
        private static MySunWindBillboardSmall[][] m_smallBillboards;
        private static bool m_smallBillboardsStarted;
        private static List<IMyEntity> m_sunwindEntities = new List<IMyEntity>();
        private static List<HkBodyCollision> m_intersectionLst;
        private static List<MyEntityRayCastPair> m_rayCastQueue = new List<MyEntityRayCastPair>();
        private static int m_computedMaxDistances;
        private static float m_deltaTime;
        private int m_rayCastCounter;
        private List<MyPhysics.HitInfo> m_hitLst = new List<MyPhysics.HitInfo>();

        private static void ComputeMaxDistance(MySunWindBillboardSmall billboard)
        {
            Vector3D vectord = -m_directionFromSunNormalized * 30000.0;
            LineD line = new LineD(((m_directionFromSunNormalized * 30000.0) + billboard.InitialAbsolutePosition) + vectord, billboard.InitialAbsolutePosition + (m_directionFromSunNormalized * 60000.0));
            MyIntersectionResultLineTriangleEx? nullable = MyEntities.GetIntersectionWithLine(ref line, null, null, false, true, true, IntersectionFlags.ALL_TRIANGLES, 0f, true);
            if (nullable != null)
            {
                billboard.MaxDistance = nullable.Value.Triangle.Distance - billboard.Radius;
            }
            else
            {
                billboard.MaxDistance = 60000f;
            }
        }

        private static void ComputeMaxDistances()
        {
            int num = MySunWindConstants.SMALL_BILLBOARDS_SIZE.X * MySunWindConstants.SMALL_BILLBOARDS_SIZE.Y;
            if (m_computedMaxDistances < num)
            {
                for (int i = (int) ((((float) num) / 1f) / 0.01666667f); (m_computedMaxDistances < num) && (i > 0); i--)
                {
                    int index = m_computedMaxDistances % MySunWindConstants.SMALL_BILLBOARDS_SIZE.Y;
                    ComputeMaxDistance(m_smallBillboards[index][m_computedMaxDistances / MySunWindConstants.SMALL_BILLBOARDS_SIZE.X]);
                    m_computedMaxDistances++;
                }
            }
        }

        public override void Draw()
        {
            if (IsActive && IsVisible)
            {
                float num = m_speed * m_deltaTime;
                m_directionFromSunNormalized * num;
                base.Draw();
            }
        }

        public static float GetParticleDustFieldAlpha() => 
            ((float) Math.Pow(MathHelper.Clamp((double) (Math.Abs(m_distanceToSunWind) / 27000.0), (double) 0.0, (double) 1.0), 4.0));

        public static Vector4 GetSunColor()
        {
            float num = ((float) (1.0 - MathHelper.Clamp((double) (Math.Abs(m_distanceToSunWind) / 10000.0), (double) 0.0, (double) 1.0))) * MathHelper.Lerp(3f, 4f, m_strength);
            return (new Vector4(MySector.SunProperties.EnvironmentLight.SunColorRaw, 1f) * (1f + num));
        }

        public static bool IsActiveForHudWarning() => 
            true;

        public override void LoadData()
        {
            MyLog.Default.WriteLine("MySunWind.LoadData() - START");
            MyLog.Default.IncreaseIndent();
            m_intersectionLst = new List<HkBodyCollision>();
            m_largeBillboards = new MySunWindBillboard[MySunWindConstants.LARGE_BILLBOARDS_SIZE.X][];
            int index = 0;
            while (index < MySunWindConstants.LARGE_BILLBOARDS_SIZE.X)
            {
                m_largeBillboards[index] = new MySunWindBillboard[MySunWindConstants.LARGE_BILLBOARDS_SIZE.Y];
                int num2 = 0;
                while (true)
                {
                    if (num2 >= MySunWindConstants.LARGE_BILLBOARDS_SIZE.Y)
                    {
                        index++;
                        break;
                    }
                    m_largeBillboards[index][num2] = new MySunWindBillboard();
                    MySunWindBillboard billboard1 = m_largeBillboards[index][num2];
                    billboard1.Radius = MyUtils.GetRandomFloat(20000f, 35000f);
                    billboard1.InitialAngle = MyUtils.GetRandomRadian();
                    billboard1.RotationSpeed = MyUtils.GetRandomSign() * MyUtils.GetRandomFloat(0.5f, 1.2f);
                    billboard1.Color.X = MyUtils.GetRandomFloat(0.5f, 3f);
                    billboard1.Color.Y = MyUtils.GetRandomFloat(0.5f, 1f);
                    billboard1.Color.Z = MyUtils.GetRandomFloat(0.5f, 1f);
                    billboard1.Color.W = MyUtils.GetRandomFloat(0.5f, 1f);
                    num2++;
                }
            }
            m_smallBillboards = new MySunWindBillboardSmall[MySunWindConstants.SMALL_BILLBOARDS_SIZE.X][];
            int num3 = 0;
            while (num3 < MySunWindConstants.SMALL_BILLBOARDS_SIZE.X)
            {
                m_smallBillboards[num3] = new MySunWindBillboardSmall[MySunWindConstants.SMALL_BILLBOARDS_SIZE.Y];
                int num4 = 0;
                while (true)
                {
                    if (num4 >= MySunWindConstants.SMALL_BILLBOARDS_SIZE.Y)
                    {
                        num3++;
                        break;
                    }
                    m_smallBillboards[num3][num4] = new MySunWindBillboardSmall();
                    MySunWindBillboardSmall small = m_smallBillboards[num3][num4];
                    small.Radius = MyUtils.GetRandomFloat(250f, 500f);
                    small.InitialAngle = MyUtils.GetRandomRadian();
                    small.RotationSpeed = MyUtils.GetRandomSign() * MyUtils.GetRandomFloat(1.4f, 3.5f);
                    small.Color.X = MyUtils.GetRandomFloat(0.5f, 1f);
                    small.Color.Y = MyUtils.GetRandomFloat(0.2f, 0.5f);
                    small.Color.Z = MyUtils.GetRandomFloat(0.2f, 0.5f);
                    small.Color.W = MyUtils.GetRandomFloat(0.1f, 0.5f);
                    small.TailBillboardsCount = MyUtils.GetRandomInt(8, 14);
                    small.TailBillboardsDistance = MyUtils.GetRandomFloat(300f, 450f);
                    small.RadiusScales = new float[small.TailBillboardsCount];
                    int num5 = 0;
                    while (true)
                    {
                        if (num5 >= small.TailBillboardsCount)
                        {
                            num4++;
                            break;
                        }
                        small.RadiusScales[num5] = MyUtils.GetRandomFloat(0.7f, 1f);
                        num5++;
                    }
                }
            }
            MyLog.Default.DecreaseIndent();
            MyLog.Default.WriteLine("MySunWind.LoadData() - END");
        }

        public static void Start()
        {
            IsActive = true;
            m_smallBillboardsStarted = false;
            m_timeLastUpdate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            m_initialSunWindPosition = (MySector.DirectionToSunNormalized * 30000.0) / 2.0;
            m_directionFromSunNormalized = -MySector.DirectionToSunNormalized;
            StopCue();
            m_speed = MyUtils.GetRandomFloat(1300f, 1500f);
            m_strength = MyUtils.GetRandomFloat(0f, 1f);
            m_directionFromSunNormalized.CalculatePerpendicularVector(out m_rightVector);
            m_downVector = MyUtils.Normalize(Vector3D.Cross(m_directionFromSunNormalized, m_rightVector));
            StartBillboards();
            m_computedMaxDistances = 0;
            m_deltaTime = 0f;
            m_sunwindEntities.Clear();
        }

        private static void StartBillboards()
        {
            Vector3D zero;
            int index = 0;
            while (index < MySunWindConstants.LARGE_BILLBOARDS_SIZE.X)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= MySunWindConstants.LARGE_BILLBOARDS_SIZE.Y)
                    {
                        index++;
                        break;
                    }
                    Vector3 vector = new Vector3(MyUtils.GetRandomFloat(-50f, 50f), MyUtils.GetRandomFloat(-50f, 50f), MyUtils.GetRandomFloat(-50f, 50f));
                    Vector3 vector2 = new Vector3((index - MySunWindConstants.LARGE_BILLBOARDS_SIZE_HALF.X) * 7500f, (num2 - MySunWindConstants.LARGE_BILLBOARDS_SIZE_HALF.Y) * 7500f, ((index - MySunWindConstants.LARGE_BILLBOARDS_SIZE_HALF.X) * 7500f) * 0.2f);
                    m_largeBillboards[index][num2].InitialAbsolutePosition = (Vector3) (((m_initialSunWindPosition + (m_rightVector * (vector.X + vector2.X))) + (m_downVector * (vector.Y + vector2.Y))) + ((-1.0 * m_directionFromSunNormalized) * (vector.Z + vector2.Z)));
                    num2++;
                }
            }
            if (MySession.Static.LocalCharacter != null)
            {
                zero = MySession.Static.LocalCharacter.Entity.WorldMatrix.Translation - ((m_directionFromSunNormalized * 30000.0) / 3.0);
            }
            else
            {
                zero = Vector3D.Zero;
            }
            Vector3D vectord = zero;
            int num3 = 0;
            while (num3 < MySunWindConstants.SMALL_BILLBOARDS_SIZE.X)
            {
                int num4 = 0;
                while (true)
                {
                    if (num4 >= MySunWindConstants.SMALL_BILLBOARDS_SIZE.Y)
                    {
                        num3++;
                        break;
                    }
                    Vector2 vector3 = new Vector2(MyUtils.GetRandomFloat(-300f, 300f), MyUtils.GetRandomFloat(-300f, 300f));
                    Vector2 vector4 = new Vector2((num3 - MySunWindConstants.SMALL_BILLBOARDS_SIZE_HALF.X) * 350f, (num4 - MySunWindConstants.SMALL_BILLBOARDS_SIZE_HALF.Y) * 350f);
                    m_smallBillboards[num3][num4].InitialAbsolutePosition = (Vector3) ((vectord + (m_rightVector * (vector3.X + vector4.X))) + (m_downVector * (vector3.Y + vector4.Y)));
                    num4++;
                }
            }
        }

        private static void StopCue()
        {
        }

        protected override void UnloadData()
        {
            MyLog.Default.WriteLine("MySunWind.UnloadData - START");
            MyLog.Default.IncreaseIndent();
            IsActive = false;
            MyLog.Default.DecreaseIndent();
            MyLog.Default.WriteLine("MySunWind.UnloadData - END");
        }

        public override void UpdateBeforeSimulation()
        {
            int num = 0;
            if ((m_rayCastQueue.Count > 0) && ((this.m_rayCastCounter % 20) == 0))
            {
                while ((num < 50) && (m_rayCastQueue.Count > 0))
                {
                    int randomInt = MyUtils.GetRandomInt(m_rayCastQueue.Count - 1);
                    MyEntity entity = m_rayCastQueue[randomInt].Entity;
                    MyEntityRayCastPair local1 = m_rayCastQueue[randomInt];
                    Vector3D position = m_rayCastQueue[randomInt].Position;
                    MyParticleEffect particle = m_rayCastQueue[randomInt].Particle;
                    if (entity is MyCubeGrid)
                    {
                        particle.Stop(true);
                        MyCubeGrid grid = entity as MyCubeGrid;
                        MatrixD worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
                        if (grid.BlocksDestructionEnabled)
                        {
                            grid.Physics.ApplyDeformation(6f, 3f, 3f, (Vector3) Vector3.Transform((Vector3) position, worldMatrixNormalizedInv), Vector3.Normalize(Vector3.Transform((Vector3) m_directionFromSunNormalized, worldMatrixNormalizedInv)), MyDamageType.Environment, 0f, 0f, 0L);
                        }
                        m_rayCastQueue.RemoveAt(randomInt);
                        this.m_hitLst.Clear();
                        break;
                    }
                }
            }
            this.m_rayCastCounter++;
            if (IsActive)
            {
                float num2 = (MySandboxGame.TotalGamePlayTimeInMilliseconds - m_timeLastUpdate) / 1000f;
                m_timeLastUpdate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                if (!MySandboxGame.IsPaused)
                {
                    m_deltaTime += num2;
                    float num3 = m_speed * m_deltaTime;
                    if (num3 >= 60000f)
                    {
                        IsActive = false;
                        StopCue();
                    }
                    else
                    {
                        Vector3D translation;
                        if (MySession.Static.LocalCharacter != null)
                        {
                            translation = MySession.Static.LocalCharacter.Entity.WorldMatrix.Translation;
                        }
                        else
                        {
                            translation = Vector3D.Zero;
                        }
                        Vector3D point = translation;
                        m_planeMiddle = new PlaneD(m_initialSunWindPosition + (m_directionFromSunNormalized * num3), m_directionFromSunNormalized);
                        m_distanceToSunWind = m_planeMiddle.DistanceToPoint(ref point);
                        m_positionOnCameraLine = -m_directionFromSunNormalized * m_distanceToSunWind;
                        Vector3D position = m_positionOnCameraLine + (m_directionFromSunNormalized * 2000.0);
                        Vector3D vectord3 = m_positionOnCameraLine + (m_directionFromSunNormalized * -2000.0);
                        m_planeFront = new PlaneD(position, m_directionFromSunNormalized);
                        m_planeBack = new PlaneD(vectord3, m_directionFromSunNormalized);
                        m_planeFront.DistanceToPoint(ref point);
                        m_planeBack.DistanceToPoint(ref point);
                        int index = 0;
                        while (index < m_sunwindEntities.Count)
                        {
                            if (m_sunwindEntities[index].MarkedForClose)
                            {
                                m_sunwindEntities.RemoveAtFast<IMyEntity>(index);
                                continue;
                            }
                            index++;
                        }
                        Quaternion orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromDir((Vector3) m_directionFromSunNormalized, (Vector3) m_downVector));
                        Vector3 halfExtents = new Vector3(10000f, 10000f, 2000f);
                        MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(position + (m_directionFromSunNormalized * 2500.0), halfExtents, orientation), Color.Red.ToVector3(), 1f, false, false, false);
                        if (this.m_rayCastCounter == 120)
                        {
                            Vector3D translation = position + (m_directionFromSunNormalized * 2500.0);
                            MyPhysics.GetPenetrationsBox(ref halfExtents, ref translation, ref orientation, m_intersectionLst, 15);
                            using (List<HkBodyCollision>.Enumerator enumerator = m_intersectionLst.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                                    if (!(collisionEntity is MyVoxelMap) && !m_sunwindEntities.Contains(collisionEntity))
                                    {
                                        m_sunwindEntities.Add(collisionEntity);
                                    }
                                }
                            }
                            m_intersectionLst.Clear();
                            int num6 = 0;
                            while (true)
                            {
                                if (num6 >= m_sunwindEntities.Count)
                                {
                                    this.m_rayCastCounter = 0;
                                    break;
                                }
                                IMyEntity item = m_sunwindEntities[num6];
                                if (item is MyCubeGrid)
                                {
                                    MyCubeGrid grid2 = item as MyCubeGrid;
                                    BoundingBoxD worldAABB = grid2.PositionComp.WorldAABB;
                                    double num7 = (worldAABB.Center - worldAABB.Min).Length();
                                    double num8 = ((worldAABB.Center - worldAABB.Min) / m_rightVector).AbsMin();
                                    double num9 = ((worldAABB.Center - worldAABB.Min) / m_downVector).AbsMin();
                                    Vector3I vectori = grid2.Max - grid2.Min;
                                    Math.Max(vectori.X, Math.Max(vectori.Y, vectori.Z));
                                    MatrixD worldMatrixNormalizedInv = grid2.PositionComp.WorldMatrixNormalizedInv;
                                    Vector3D vectord6 = (worldAABB.Center - (num8 * m_rightVector)) - (num9 * m_downVector);
                                    int num10 = 0;
                                    while (true)
                                    {
                                        if (num10 >= (num8 * 2.0))
                                        {
                                            m_sunwindEntities.Remove(grid2);
                                            num6--;
                                            break;
                                        }
                                        int num11 = 0;
                                        while (true)
                                        {
                                            if (num11 >= (num9 * 2.0))
                                            {
                                                num10 += (grid2.GridSizeEnum == MyCubeSize.Large) ? 0x19 : 10;
                                                break;
                                            }
                                            Vector3D to = ((vectord6 + (num10 * m_rightVector)) + (num11 * m_downVector)) + (((float) num7) * m_directionFromSunNormalized);
                                            Vector3 vector2 = MyUtils.GetRandomVector3CircleNormalized();
                                            float randomFloat = MyUtils.GetRandomFloat(0f, (grid2.GridSizeEnum == MyCubeSize.Large) ? ((float) 10) : ((float) 5));
                                            to += ((m_rightVector * vector2.X) * randomFloat) + ((m_downVector * vector2.Z) * randomFloat);
                                            LineD ed = new LineD(to - (m_directionFromSunNormalized * ((float) num7)), to);
                                            if (grid2.RayCastBlocks(ed.From, ed.To) != null)
                                            {
                                                ed.From = to - (m_directionFromSunNormalized * 1000.0);
                                                MyPhysics.CastRay(ed.From, ed.To, this.m_hitLst, 0);
                                                this.m_rayCastCounter++;
                                                if ((this.m_hitLst.Count == 0) || !ReferenceEquals(this.m_hitLst[0].HkHitInfo.GetHitEntity(), grid2.Components))
                                                {
                                                    this.m_hitLst.Clear();
                                                }
                                                else
                                                {
                                                    MyParticleEffect effect2;
                                                    MyParticlesManager.TryCreateParticleEffect("Dummy", MatrixD.CreateWorld(this.m_hitLst[0].Position, Vector3D.Forward, Vector3D.Up), out effect2);
                                                    MyEntityRayCastPair pair = new MyEntityRayCastPair {
                                                        Entity = grid2,
                                                        _Ray = ed,
                                                        Position = this.m_hitLst[0].Position,
                                                        Particle = effect2
                                                    };
                                                    m_rayCastQueue.Add(pair);
                                                }
                                            }
                                            num11 += (grid2.GridSizeEnum == MyCubeSize.Large) ? 0x19 : 10;
                                        }
                                    }
                                }
                                else
                                {
                                    m_sunwindEntities.Remove(item);
                                    num6--;
                                }
                                num6++;
                            }
                        }
                        if (m_distanceToSunWind <= 10000.0)
                        {
                            m_smallBillboardsStarted = true;
                        }
                        ComputeMaxDistances();
                        base.UpdateBeforeSimulation();
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyEntityRayCastPair
        {
            public MyEntity Entity;
            public LineD _Ray;
            public Vector3D Position;
            public MyParticleEffect Particle;
        }

        private class MySunWindBillboard
        {
            public Vector4 Color;
            public float Radius;
            public float InitialAngle;
            public float RotationSpeed;
            public Vector3 InitialAbsolutePosition;
        }

        private class MySunWindBillboardSmall : MySunWind.MySunWindBillboard
        {
            public float MaxDistance;
            public int TailBillboardsCount;
            public float TailBillboardsDistance;
            public float[] RadiusScales;
        }
    }
}

