namespace VRage.Entities.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.ObjectBuilders.Definitions.Components;
    using VRage.ObjectBuilders.Voxels;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    [VoxelPostprocessing(typeof(MyObjectBuilder_VoxelPostprocessingDecimate), true)]
    public class MyDecimatePostprocessing : MyVoxelPostprocessing
    {
        [ThreadStatic]
        private static VrDecimatePostprocessing m_instance;
        private List<Settings> m_perLodSettings = new List<Settings>();

        public override bool Get(int lod, out VrPostprocessing postprocess)
        {
            if (m_instance == null)
            {
                m_instance = new VrDecimatePostprocessing();
            }
            int num = this.m_perLodSettings.BinaryIntervalSearch<Settings>(x => (x.FromLod <= lod)) - 1;
            if (num == -1)
            {
                postprocess = null;
                return false;
            }
            Settings settings = this.m_perLodSettings[num];
            m_instance.FeatureAngle = settings.FeatureAngle;
            m_instance.EdgeThreshold = settings.EdgeThreshold;
            m_instance.PlaneThreshold = settings.PlaneThreshold;
            m_instance.IgnoreEdges = settings.IgnoreEdges;
            postprocess = m_instance;
            return true;
        }

        protected internal override void Init(MyObjectBuilder_VoxelPostprocessing builder)
        {
            base.Init(builder);
            int num = -1;
            foreach (MyObjectBuilder_VoxelPostprocessingDecimate.Settings settings in ((MyObjectBuilder_VoxelPostprocessingDecimate) builder).LodSettings)
            {
                if (settings.FromLod <= num)
                {
                    MyLog.Default.Error("Decimation lod sets must have strictly ascending lod indices.", Array.Empty<object>());
                    continue;
                }
                this.m_perLodSettings.Add(new Settings(settings));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Settings
        {
            public int FromLod;
            public float FeatureAngle;
            public float EdgeThreshold;
            public float PlaneThreshold;
            public bool IgnoreEdges;
            public Settings(MyObjectBuilder_VoxelPostprocessingDecimate.Settings obSettings)
            {
                this.FromLod = obSettings.FromLod;
                this.FeatureAngle = MathHelper.ToRadians(obSettings.FeatureAngle);
                this.EdgeThreshold = obSettings.EdgeThreshold;
                this.PlaneThreshold = obSettings.PlaneThreshold;
                this.IgnoreEdges = obSettings.IgnoreEdges;
            }
        }
    }
}

