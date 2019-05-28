namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Localization;
    using SpaceEngineers.Game.EntityComponents.GameLogic;
    using SpaceEngineers.Game.EntityComponents.Renders;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Graphics;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_WindTurbine))]
    public class MyWindTurbine : MyEnvironmentalPowerProducer
    {
        private int m_nextUpdateRay;
        private float m_effectivity;
        private bool m_paralleRaycastRunning;
        private readonly Action<MyPhysics.HitInfo?> m_onRaycastCompleted;
        private readonly Action<List<MyPhysics.HitInfo>> m_onRaycastCompletedList;
        private List<MyPhysics.HitInfo> m_cachedHitList = new List<MyPhysics.HitInfo>();
        private Action m_updateEffectivity;

        public MyWindTurbine()
        {
            this.m_updateEffectivity = new Action(this.UpdateEffectivity);
            this.m_onRaycastCompleted = new Action<MyPhysics.HitInfo?>(this.OnRaycastCompleted);
            this.m_onRaycastCompletedList = new Action<List<MyPhysics.HitInfo>>(this.OnRaycastCompleted);
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            if (!base.Enabled)
            {
                this.UpdateVisuals();
            }
        }

        protected override void ConstructDetailedInfo(StringBuilder sb)
        {
            base.ConstructDetailedInfo(sb);
            MyStringId id = (this.Effectivity <= 0.95) ? ((this.Effectivity <= 0.6f) ? ((this.Effectivity <= 0f) ? MySpaceTexts.Turbine_WindClearanceNone : MySpaceTexts.Turbine_WindClearancePoor) : MySpaceTexts.Turbine_WindClearanceGood) : MySpaceTexts.Turbine_WindClearanceOptimal;
            sb.AppendFormat(MySpaceTexts.Turbine_WindClearance, id);
        }

        private MyStringHash GetEmissiveState()
        {
            this.CheckIsWorking();
            if (!base.IsWorking)
            {
                return (!base.IsFunctional ? MyCubeBlock.m_emissiveNames.Damaged : MyCubeBlock.m_emissiveNames.Disabled);
            }
            if (!this.GetOrCreateSharedComponent().IsEnabled || (this.Effectivity <= 0f))
            {
                return MyCubeBlock.m_emissiveNames.Warning;
            }
            return MyCubeBlock.m_emissiveNames.Working;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_WindTurbine objectBuilderCubeBlock = (MyObjectBuilder_WindTurbine) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.ImmediateEffectivities = (float[]) this.RayEffectivities.Clone();
            return objectBuilderCubeBlock;
        }

        private MySharedWindComponent GetOrCreateSharedComponent()
        {
            MyEntityComponentContainer components = base.CubeGrid.Components;
            MySharedWindComponent component = components.Get<MySharedWindComponent>();
            if (component == null)
            {
                component = new MySharedWindComponent();
                components.Add<MySharedWindComponent>(component);
            }
            return component;
        }

        public void GetRaycaster(int id, out Vector3D start, out Vector3D end)
        {
            MatrixD worldMatrix = base.WorldMatrix;
            start = worldMatrix.Translation;
            if (id == 0)
            {
                end = start + (this.GetOrCreateSharedComponent().GravityNormal * this.BlockDefinition.OptimalGroundClearance);
            }
            else
            {
                float angle = (6.283185f / ((float) (this.RayEffectivities.Length - 1))) * (id - 1);
                int raycasterSize = this.BlockDefinition.RaycasterSize;
                end = start + (raycasterSize * ((MyMath.FastSin(angle) * worldMatrix.Left) + (MyMath.FastCos(angle) * worldMatrix.Forward)));
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyObjectBuilder_WindTurbine turbine = (MyObjectBuilder_WindTurbine) objectBuilder;
            this.RayEffectivities = turbine.ImmediateEffectivities;
            if (this.RayEffectivities == null)
            {
                this.RayEffectivities = new float[this.BlockDefinition.RaycastersCount];
            }
            base.Init(objectBuilder, cubeGrid);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void InitComponents()
        {
            base.Render = new MyRenderComponentWindTurbine();
            base.InitComponents();
        }

        protected override MyEntitySubpart InstantiateSubpart(MyModelDummy subpartDummy, ref MyEntitySubpart.Data data) => 
            new TurbineSubpart();

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
        }

        public void OnEnvironmentChanged()
        {
            this.UpdateVisuals();
            this.OnProductionChanged();
        }

        private void OnIsWorkingChanged()
        {
            float effectivity = this.Effectivity;
            this.UpdateEffectivity();
            if (this.Effectivity == effectivity)
            {
                this.UpdateVisuals();
            }
        }

        private void OnRaycastCompleted(List<MyPhysics.HitInfo> hitList)
        {
            using (hitList.GetClearToken<MyPhysics.HitInfo>())
            {
                using (List<MyPhysics.HitInfo>.Enumerator enumerator = hitList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyPhysics.HitInfo current = enumerator.Current;
                        if (current.HkHitInfo.Body.Layer == 0x1c)
                        {
                            this.OnRaycastCompleted(new MyPhysics.HitInfo?(current));
                            return;
                        }
                    }
                }
                MyPhysics.HitInfo? hitInfo = null;
                this.OnRaycastCompleted(hitInfo);
            }
        }

        private void OnRaycastCompleted(MyPhysics.HitInfo? hitInfo)
        {
            float num = 1f;
            if (hitInfo != null)
            {
                float hitFraction = hitInfo.Value.HkHitInfo.HitFraction;
                float minRaycasterClearance = this.BlockDefinition.MinRaycasterClearance;
                num = (hitFraction > minRaycasterClearance) ? ((hitFraction - minRaycasterClearance) / (1f - minRaycasterClearance)) : 0f;
            }
            this.RayEffectivities[this.m_nextUpdateRay] = num;
            this.m_nextUpdateRay++;
            if (this.m_nextUpdateRay >= this.BlockDefinition.RaycastersCount)
            {
                this.m_nextUpdateRay = 0;
            }
            MySandboxGame.Static.Invoke(delegate {
                if (!base.MarkedForClose)
                {
                    this.UpdateEffectivity();
                    this.m_paralleRaycastRunning = false;
                }
            }, "Turbine update");
        }

        public override void OnRegisteredToGridSystems()
        {
            base.OnRegisteredToGridSystems();
            this.GetOrCreateSharedComponent().Register(this);
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            this.OnIsWorkingChanged();
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            this.OnIsWorkingChanged();
        }

        public override void OnUnregisteredFromGridSystems()
        {
            base.OnUnregisteredFromGridSystems();
            this.GetOrCreateSharedComponent().Unregister(this);
        }

        public override void RefreshModels(string modelPath, string modelCollisionPath)
        {
            base.RefreshModels(modelPath, modelCollisionPath);
            this.UpdateVisuals();
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.GetOrCreateSharedComponent().Update10();
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            this.GetOrCreateSharedComponent().UpdateWindSpeed();
        }

        private void UpdateEffectivity()
        {
            if (!base.IsWorking)
            {
                this.Effectivity = 0f;
            }
            else
            {
                float num = 0f;
                for (int i = 1; i < this.RayEffectivities.Length; i++)
                {
                    num += this.RayEffectivities[i];
                }
                num = (num / this.BlockDefinition.RaycastersToFullEfficiency) * MathHelper.Lerp(0.5f, 1f, this.RayEffectivities[0]);
                this.Effectivity = Math.Min(1f, num);
            }
        }

        public void UpdateNextRay()
        {
            if (!this.m_paralleRaycastRunning)
            {
                Vector3D vectord;
                Vector3D vectord2;
                this.m_paralleRaycastRunning = true;
                this.GetRaycaster(this.m_nextUpdateRay, out vectord, out vectord2);
                if (this.m_nextUpdateRay != 0)
                {
                    MyPhysics.CastRayParallel(ref vectord, ref vectord2, 0, this.m_onRaycastCompleted);
                }
                else
                {
                    this.m_cachedHitList.AssertEmpty<MyPhysics.HitInfo>();
                    MyPhysics.CastRayParallel(ref vectord, ref vectord2, this.m_cachedHitList, 0x1c, this.m_onRaycastCompletedList);
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.GetOrCreateSharedComponent().UpdateWindSpeed();
        }

        private void UpdateVisuals()
        {
            MyEmissiveColorStateResult result;
            if (!MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, this.GetEmissiveState(), out result))
            {
                result.EmissiveColor = Color.Green;
            }
            float speed = this.CurrentProductionRatio * this.BlockDefinition.TurbineRotationSpeed;
            foreach (TurbineSubpart subpart1 in base.Subparts.Values)
            {
                subpart1.Render.SetSpeed(speed);
                subpart1.Render.SetColor(result.EmissiveColor);
            }
        }

        protected float Effectivity
        {
            get => 
                this.m_effectivity;
            set
            {
                if (this.m_effectivity != value)
                {
                    this.m_effectivity = value;
                    this.OnProductionChanged();
                    this.UpdateVisuals();
                }
            }
        }

        protected override float CurrentProductionRatio =>
            (this.m_effectivity * Math.Min((float) 1f, (float) (this.GetOrCreateSharedComponent().WindSpeed / this.BlockDefinition.OptimalWindSpeed)));

        public MyWindTurbineDefinition BlockDefinition =>
            ((MyWindTurbineDefinition) base.BlockDefinition);

        public float[] RayEffectivities { get; private set; }

        public class TurbineSubpart : MyEntitySubpart
        {
            public override void InitComponents()
            {
                base.Render = new MyRenderComponentWindTurbine.TurbineRenderComponent();
                base.InitComponents();
            }

            public MyWindTurbine Parent =>
                ((MyWindTurbine) base.Parent);

            public MyRenderComponentWindTurbine.TurbineRenderComponent Render =>
                ((MyRenderComponentWindTurbine.TurbineRenderComponent) base.Render);
        }
    }
}

