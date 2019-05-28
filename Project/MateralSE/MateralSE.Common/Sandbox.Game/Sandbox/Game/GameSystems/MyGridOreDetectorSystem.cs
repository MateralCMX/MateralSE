namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.Components;

    public class MyGridOreDetectorSystem
    {
        private readonly MyCubeGrid m_cubeGrid;
        private readonly HashSet<RegisteredOreDetectorData> m_oreDetectors = new HashSet<RegisteredOreDetectorData>();

        public MyGridOreDetectorSystem(MyCubeGrid cubeGrid)
        {
            this.m_cubeGrid = cubeGrid;
            this.m_cubeGrid.OnFatBlockAdded += new Action<MyCubeBlock>(this.CubeGridOnOnFatBlockAdded);
            this.m_cubeGrid.OnFatBlockRemoved += new Action<MyCubeBlock>(this.CubeGridOnOnFatBlockRemoved);
        }

        private void CubeGridOnOnFatBlockAdded(MyCubeBlock block)
        {
            MyOreDetectorComponent component;
            IMyComponentOwner<MyOreDetectorComponent> owner = block as IMyComponentOwner<MyOreDetectorComponent>;
            if ((owner != null) && owner.GetComponent(out component))
            {
                this.m_oreDetectors.Add(new RegisteredOreDetectorData(block, component));
            }
        }

        private void CubeGridOnOnFatBlockRemoved(MyCubeBlock block)
        {
            MyOreDetectorComponent component;
            IMyComponentOwner<MyOreDetectorComponent> owner = block as IMyComponentOwner<MyOreDetectorComponent>;
            if ((owner != null) && owner.GetComponent(out component))
            {
                this.m_oreDetectors.Remove(new RegisteredOreDetectorData(block, component));
            }
        }

        public HashSetReader<RegisteredOreDetectorData> OreDetectors =>
            new HashSetReader<RegisteredOreDetectorData>(this.m_oreDetectors);

        [StructLayout(LayoutKind.Sequential)]
        public struct RegisteredOreDetectorData
        {
            public readonly MyCubeBlock Block;
            public readonly MyOreDetectorComponent Component;
            public RegisteredOreDetectorData(MyCubeBlock block, MyOreDetectorComponent comp)
            {
                this = new MyGridOreDetectorSystem.RegisteredOreDetectorData();
                this.Block = block;
                this.Component = comp;
            }

            public override int GetHashCode() => 
                this.Block.EntityId.GetHashCode();
        }
    }
}

