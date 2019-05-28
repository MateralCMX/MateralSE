namespace VRage.Entities.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.ObjectBuilders;
    using VRage.ObjectBuilders.Definitions.Components;
    using VRage.Voxels;

    public abstract class MyVoxelPostprocessing
    {
        private static MyObjectFactory<VoxelPostprocessingAttribute, MyVoxelPostprocessing> m_objectFactory = new MyObjectFactory<VoxelPostprocessingAttribute, MyVoxelPostprocessing>();

        static MyVoxelPostprocessing()
        {
            m_objectFactory.RegisterFromCreatedObjectAssembly();
        }

        protected MyVoxelPostprocessing()
        {
        }

        public abstract bool Get(int lod, out VrPostprocessing postprocess);
        protected internal virtual void Init(MyObjectBuilder_VoxelPostprocessing builder)
        {
            this.UseForPhysics = builder.ForPhysics;
        }

        public static MyObjectFactory<VoxelPostprocessingAttribute, MyVoxelPostprocessing> Factory =>
            m_objectFactory;

        public bool UseForPhysics { get; set; }
    }
}

