namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using Havok;
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using VRageMath;

    public class MyStructuralIntegrity
    {
        public bool EnabledOnlyForDraw;
        public static float MAX_SI_TENSION = 10f;
        private MyCubeGrid m_cubeGrid;
        private IMyIntegritySimulator m_simulator;
        private int DestructionDelay = 10;
        private int m_destructionDelayCounter;
        private bool m_SISimulated;

        public MyStructuralIntegrity(MyCubeGrid cubeGrid)
        {
            this.m_cubeGrid = cubeGrid;
            this.m_cubeGrid.OnBlockAdded += new Action<MySlimBlock>(this.cubeGrid_OnBlockAdded);
            this.m_cubeGrid.OnBlockRemoved += new Action<MySlimBlock>(this.cubeGrid_OnBlockRemoved);
            this.m_cubeGrid.OnBlockIntegrityChanged += new Action<MySlimBlock>(this.cubeGrid_OnBlockIntegrityChanged);
            this.m_simulator = new MyAdvancedStaticSimulator(this.m_cubeGrid);
            foreach (MySlimBlock block in this.m_cubeGrid.GetBlocks())
            {
                this.cubeGrid_OnBlockAdded(block);
            }
        }

        public void Close()
        {
            this.m_cubeGrid.OnBlockAdded -= new Action<MySlimBlock>(this.cubeGrid_OnBlockAdded);
            this.m_cubeGrid.OnBlockRemoved -= new Action<MySlimBlock>(this.cubeGrid_OnBlockRemoved);
            this.m_cubeGrid.OnBlockIntegrityChanged -= new Action<MySlimBlock>(this.cubeGrid_OnBlockIntegrityChanged);
            this.m_simulator.Close();
        }

        public unsafe void CreateSIDestruction(Vector3D worldCenter)
        {
            HkdFractureImpactDetails details = HkdFractureImpactDetails.Create();
            details.SetBreakingBody(this.m_cubeGrid.Physics.RigidBody);
            details.SetContactPoint((Vector3) this.m_cubeGrid.Physics.WorldToCluster(worldCenter));
            details.SetDestructionRadius(1.5f);
            details.SetBreakingImpulse(MyDestructionConstants.STRENGTH * 10f);
            details.SetParticleVelocity(Vector3.Zero);
            details.SetParticlePosition((Vector3) this.m_cubeGrid.Physics.WorldToCluster(worldCenter));
            details.SetParticleMass(10000f);
            HkdFractureImpactDetails* detailsPtr1 = (HkdFractureImpactDetails*) ref details;
            detailsPtr1.Flag = details.Flag | HkdFractureImpactDetails.Flags.FLAG_DONT_RECURSE;
            if (this.m_cubeGrid.GetPhysicsBody().HavokWorld.DestructionWorld != null)
            {
                MyPhysics.FractureImpactDetails details2 = new MyPhysics.FractureImpactDetails {
                    Details = details,
                    World = this.m_cubeGrid.GetPhysicsBody().HavokWorld,
                    Entity = this.m_cubeGrid,
                    ContactInWorld = worldCenter
                };
                MyPhysics.EnqueueDestruction(details2);
            }
        }

        private void cubeGrid_OnBlockAdded(MySlimBlock block)
        {
            this.m_simulator.Add(block);
        }

        private void cubeGrid_OnBlockIntegrityChanged(MySlimBlock obj)
        {
            this.m_simulator.ForceRecalc();
        }

        private void cubeGrid_OnBlockRemoved(MySlimBlock block)
        {
            this.m_simulator.Remove(block);
        }

        public void DebugDraw()
        {
            this.m_simulator.DebugDraw();
        }

        public void Draw()
        {
            this.m_simulator.Draw();
        }

        public void ForceRecalculation()
        {
            this.m_simulator.ForceRecalc();
        }

        public bool IsConnectionFine(MySlimBlock blockA, MySlimBlock blockB) => 
            this.m_simulator.IsConnectionFine(blockA, blockB);

        public void Update(float deltaTime)
        {
            if (this.m_simulator.Simulate(deltaTime))
            {
                this.m_SISimulated = true;
            }
            if (this.m_destructionDelayCounter > 0)
            {
                this.m_destructionDelayCounter--;
            }
            if ((this.m_SISimulated && ((this.m_destructionDelayCounter == 0) && MyPetaInputComponent.ENABLE_SI_DESTRUCTIONS)) && !this.EnabledOnlyForDraw)
            {
                if (Sync.IsServer)
                {
                    this.m_destructionDelayCounter = this.DestructionDelay;
                    this.m_SISimulated = false;
                    MySlimBlock block = null;
                    float minValue = float.MinValue;
                    foreach (MySlimBlock block2 in this.m_cubeGrid.GetBlocks())
                    {
                        float tension = this.m_simulator.GetTension(block2.Position);
                        if (tension > minValue)
                        {
                            minValue = tension;
                            block = block2;
                        }
                    }
                    Vector3D zero = Vector3D.Zero;
                    if (block != null)
                    {
                        block.ComputeWorldCenter(out zero);
                    }
                    if (minValue > MAX_SI_TENSION)
                    {
                        this.m_SISimulated = true;
                        this.CreateSIDestruction(zero);
                    }
                }
                this.m_cubeGrid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
            }
        }

        public static bool Enabled =>
            (MyFakes.ENABLE_STRUCTURAL_INTEGRITY && ((MySession.Static != null) && MySession.Static.Settings.EnableStructuralSimulation));
    }
}

