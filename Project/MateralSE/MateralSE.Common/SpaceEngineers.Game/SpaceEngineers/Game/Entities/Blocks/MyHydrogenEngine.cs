namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.EntityComponents.Renders;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_HydrogenEngine))]
    public class MyHydrogenEngine : MyGasFueledPowerProducer
    {
        private bool m_renderAnimationEnabled = true;
        private List<MyPistonSubpart> m_pistons = new List<MyPistonSubpart>();
        private List<MyRotatingSubpartSubpart> m_rotatingSubparts = new List<MyRotatingSubpartSubpart>();

        protected override string GetDefaultEmissiveParts(byte index) => 
            ((index == 0) ? "Emissive2" : null);

        public override void InitComponents()
        {
            base.Render = new MyRenderComponentHydrogenEngine();
            base.InitComponents();
        }

        protected override MyEntitySubpart InstantiateSubpart(MyModelDummy subpartDummy, ref MyEntitySubpart.Data data)
        {
            string name = data.Name;
            if (!name.Contains("Piston"))
            {
                if (!name.Contains("Propeller") && !name.Contains("Camshaft"))
                {
                    return base.InstantiateSubpart(subpartDummy, ref data);
                }
                MyRotatingSubpartSubpart subpart2 = new MyRotatingSubpartSubpart();
                this.m_rotatingSubparts.Add(subpart2);
                return subpart2;
            }
            MyPistonSubpart item = new MyPistonSubpart();
            float num = 0f;
            float[] pistonAnimationOffsets = this.BlockDefinition.PistonAnimationOffsets;
            if ((pistonAnimationOffsets != null) && (pistonAnimationOffsets.Length != 0))
            {
                num = pistonAnimationOffsets[this.m_pistons.Count % pistonAnimationOffsets.Length];
            }
            item.Render.AnimationOffset = num;
            this.m_pistons.Add(item);
            return item;
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            this.UpdateVisuals();
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            this.UpdateVisuals();
        }

        public override void RefreshModels(string modelPath, string modelCollisionPath)
        {
            this.m_pistons.Clear();
            this.m_rotatingSubparts.Clear();
            base.RefreshModels(modelPath, modelCollisionPath);
            this.UpdateVisuals();
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (!Sync.IsDedicated)
            {
                bool flag = Vector3D.DistanceSquared(MySector.MainCamera.Position, base.PositionComp.GetPosition()) < this.BlockDefinition.AnimationVisibilityDistanceSq;
                if (flag != this.m_renderAnimationEnabled)
                {
                    this.m_renderAnimationEnabled = flag;
                    this.UpdateVisuals();
                }
            }
        }

        private void UpdateVisuals()
        {
            float speed = 0f;
            if (this.m_renderAnimationEnabled && base.IsWorking)
            {
                speed = this.BlockDefinition.AnimationSpeed;
            }
            using (List<MyPistonSubpart>.Enumerator enumerator = this.m_pistons.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Render.SetSpeed(speed);
                }
            }
            using (List<MyRotatingSubpartSubpart>.Enumerator enumerator2 = this.m_rotatingSubparts.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.Render.SetSpeed(speed);
                }
            }
        }

        public MyHydrogenEngineDefinition BlockDefinition =>
            ((MyHydrogenEngineDefinition) base.BlockDefinition);

        public MyRenderComponentHydrogenEngine Render =>
            ((MyRenderComponentHydrogenEngine) base.Render);

        private class MyPistonSubpart : MyEntitySubpart
        {
            public override void InitComponents()
            {
                base.Render = new MyRenderComponentHydrogenEngine.MyPistonRenderComponent();
                base.InitComponents();
            }

            public MyRenderComponentHydrogenEngine.MyPistonRenderComponent Render =>
                ((MyRenderComponentHydrogenEngine.MyPistonRenderComponent) base.Render);
        }

        private class MyRotatingSubpartSubpart : MyEntitySubpart
        {
            public override void InitComponents()
            {
                base.Render = new MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent();
                base.InitComponents();
            }

            public MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent Render =>
                ((MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent) base.Render);
        }
    }
}

