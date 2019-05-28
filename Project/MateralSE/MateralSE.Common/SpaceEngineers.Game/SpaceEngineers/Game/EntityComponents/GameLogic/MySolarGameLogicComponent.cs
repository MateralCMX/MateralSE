namespace SpaceEngineers.Game.EntityComponents.GameLogic
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MySolarGameLogicComponent : MyGameLogicComponent
    {
        private const int NUMBER_OF_PIVOTS = 8;
        [CompilerGenerated]
        private Action OnProductionChanged;
        private float m_maxOutput;
        private Vector3 m_panelOrientation;
        private float m_panelOffset;
        private bool m_isTwoSided;
        private MyFunctionalBlock m_solarBlock;
        private bool m_initialized;
        private byte m_debugCurrentPivot;
        private bool[] m_debugIsPivotInSun = new bool[8];
        private bool m_isBackgroundProcessing;
        private byte m_currentPivot;
        private float m_angleToSun;
        private int m_pivotsInSun;
        private bool[] m_isPivotInSun = new bool[8];
        private List<MyPhysics.HitInfo> m_hitList = new List<MyPhysics.HitInfo>();
        private Vector3D m_to;
        private Vector3D m_from;
        private Action ComputeSunAngleFunc;
        private Action<List<MyPhysics.HitInfo>> OnRayCastCompletedFunc;
        private Action OnSunAngleComputedFunc;

        public event Action OnProductionChanged
        {
            [CompilerGenerated] add
            {
                Action onProductionChanged = this.OnProductionChanged;
                while (true)
                {
                    Action a = onProductionChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onProductionChanged = Interlocked.CompareExchange<Action>(ref this.OnProductionChanged, action3, a);
                    if (ReferenceEquals(onProductionChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onProductionChanged = this.OnProductionChanged;
                while (true)
                {
                    Action source = onProductionChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onProductionChanged = Interlocked.CompareExchange<Action>(ref this.OnProductionChanged, action3, source);
                    if (ReferenceEquals(onProductionChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MySolarGameLogicComponent()
        {
            this.ComputeSunAngleFunc = new Action(this.ComputeSunAngle);
            this.OnSunAngleComputedFunc = new Action(this.OnSunAngleComputed);
            this.OnRayCastCompletedFunc = new Action<List<MyPhysics.HitInfo>>(this.OnRayCastCompleted);
        }

        private void ComputeSunAngle()
        {
            this.m_angleToSun = Vector3.Dot((Vector3) Vector3.Transform(this.m_panelOrientation, this.m_solarBlock.WorldMatrix.GetOrientation()), MySector.DirectionToSunNormalized);
            if (((this.m_angleToSun < 0f) && !this.m_isTwoSided) || !this.m_solarBlock.IsFunctional)
            {
                MySandboxGame.Static.Invoke(this.OnSunAngleComputedFunc, "SolarGamelogic:OnSunAngleComputed");
            }
            else if (MySectorWeatherComponent.IsOnDarkSide(this.m_solarBlock.WorldMatrix.Translation))
            {
                this.m_isPivotInSun.ForEach<bool>(x => x = false);
                this.m_pivotsInSun = 0;
                MySandboxGame.Static.Invoke(this.OnSunAngleComputedFunc, "SolarGamelogic:OnSunAngleComputed");
            }
            else
            {
                this.m_currentPivot = (byte) (this.m_currentPivot % 8);
                MatrixD orientation = this.m_solarBlock.WorldMatrix.GetOrientation();
                float num = (float) this.m_solarBlock.WorldMatrix.Forward.Dot(Vector3.Transform(this.m_panelOrientation, orientation));
                float num2 = (this.m_solarBlock.BlockDefinition.CubeSize == MyCubeSize.Large) ? 2.5f : 0.5f;
                Vector3D vectord = ((this.m_solarBlock.WorldMatrix.Translation + ((((((this.m_currentPivot % 4) - 1.5f) * num2) * num) * (((float) this.m_solarBlock.BlockDefinition.Size.X) / 4f)) * this.m_solarBlock.WorldMatrix.Left)) + ((((((this.m_currentPivot / 4) - 0.5f) * num2) * num) * (((float) this.m_solarBlock.BlockDefinition.Size.Y) / 2f)) * this.m_solarBlock.WorldMatrix.Up)) + ((((num2 * num) * (((float) this.m_solarBlock.BlockDefinition.Size.Z) / 2f)) * Vector3.Transform(this.m_panelOrientation, orientation)) * this.m_panelOffset);
                this.m_from = vectord + (MySector.DirectionToSunNormalized * 100f);
                this.m_to = vectord + ((MySector.DirectionToSunNormalized * this.m_solarBlock.CubeGrid.GridSize) / 4f);
                MyPhysics.CastRayParallel(ref this.m_to, ref this.m_from, this.m_hitList, 15, this.OnRayCastCompletedFunc);
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            null;

        public void Initialize(Vector3 panelOrientation, bool isTwoSided, float panelOffset, MyFunctionalBlock solarBlock)
        {
            this.m_initialized = true;
            this.m_panelOrientation = panelOrientation;
            this.m_isTwoSided = isTwoSided;
            this.m_panelOffset = panelOffset;
            this.m_solarBlock = solarBlock;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        private void OnRayCastCompleted(List<MyPhysics.HitInfo> hits)
        {
            this.m_isPivotInSun[this.m_currentPivot] = true;
            using (List<MyPhysics.HitInfo>.Enumerator enumerator = hits.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IMyEntity hitEntity = enumerator.Current.HkHitInfo.GetHitEntity();
                    if (!ReferenceEquals(hitEntity, this.m_solarBlock.CubeGrid))
                    {
                        this.m_isPivotInSun[this.m_currentPivot] = false;
                    }
                    else
                    {
                        MyCubeGrid grid = hitEntity as MyCubeGrid;
                        Vector3I? nullable = grid.RayCastBlocks(this.m_from, this.m_to);
                        if (nullable == null)
                        {
                            continue;
                        }
                        if (ReferenceEquals(grid.GetCubeBlock(nullable.Value), this.m_solarBlock.SlimBlock))
                        {
                            continue;
                        }
                        this.m_isPivotInSun[this.m_currentPivot] = false;
                    }
                    break;
                }
            }
            this.m_pivotsInSun = 0;
            bool[] isPivotInSun = this.m_isPivotInSun;
            for (int i = 0; i < isPivotInSun.Length; i++)
            {
                if (isPivotInSun[i])
                {
                    this.m_pivotsInSun++;
                }
            }
            MySandboxGame.Static.Invoke(this.OnSunAngleComputedFunc, "SolarGamelogic:OnSunAngleComputed");
        }

        private void OnSunAngleComputed()
        {
            this.m_isBackgroundProcessing = false;
            if (((this.m_angleToSun < 0f) && !this.m_isTwoSided) || !this.m_solarBlock.Enabled)
            {
                this.MaxOutput = 0f;
            }
            else
            {
                float angleToSun = this.m_angleToSun;
                if (angleToSun < 0f)
                {
                    angleToSun = !this.m_isTwoSided ? 0f : Math.Abs(angleToSun);
                }
                angleToSun *= ((float) this.m_pivotsInSun) / 8f;
                this.MaxOutput = angleToSun;
                this.m_debugCurrentPivot = this.m_currentPivot;
                this.m_debugCurrentPivot = (byte) (this.m_debugCurrentPivot + 1);
                for (int i = 0; i < 8; i++)
                {
                    this.m_debugIsPivotInSun[i] = this.m_isPivotInSun[i];
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (this.m_solarBlock.CubeGrid.Physics != null)
            {
                if (!this.m_solarBlock.IsWorking)
                {
                    this.MaxOutput = 0f;
                }
                else if (!this.m_isBackgroundProcessing)
                {
                    this.m_isBackgroundProcessing = true;
                    this.m_currentPivot = this.m_debugCurrentPivot;
                    int index = 0;
                    while (true)
                    {
                        if (index >= 8)
                        {
                            Parallel.Start(this.ComputeSunAngleFunc);
                            break;
                        }
                        this.m_isPivotInSun[index] = this.m_debugIsPivotInSun[index];
                        index++;
                    }
                }
            }
        }

        public float MaxOutput
        {
            get => 
                this.m_maxOutput;
            set
            {
                if (this.m_maxOutput != value)
                {
                    this.m_maxOutput = value;
                    this.OnProductionChanged.InvokeIfNotNull();
                }
            }
        }

        public Vector3 PanelOrientation =>
            this.m_panelOrientation;

        public float PanelOffset =>
            this.m_panelOffset;

        public byte DebugCurrentPivot =>
            this.m_debugCurrentPivot;

        public bool[] DebugIsPivotInSun =>
            this.m_debugIsPivotInSun;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySolarGameLogicComponent.<>c <>9 = new MySolarGameLogicComponent.<>c();
            public static Action<bool> <>9__36_0;

            internal void <ComputeSunAngle>b__36_0(bool x)
            {
                x = false;
            }
        }
    }
}

