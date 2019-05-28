namespace VRage.Entities.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Definitions.Components;
    using VRage.Game.Components;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.ObjectBuilders.Definitions.Components;
    using VRage.Voxels;
    using VRage.Voxels.DualContouring;
    using VRageMath;

    public class MyVoxelMesherComponent : MyEntityComponentBase
    {
        private List<MyVoxelPostprocessing> m_postprocessingSteps = new List<MyVoxelPostprocessing>();

        public virtual MyMesherResult CalculateMesh(int lod, Vector3I lodVoxelMin, Vector3I lodVoxelMax, MyStorageDataTypeFlags properties = 3, MyVoxelRequestFlags flags = 0, VrVoxelMesh target = null) => 
            MyDualContouringMesher.Static.Calculate(this, lod, lodVoxelMin, lodVoxelMax, properties, flags, target);

        public void Init(MyVoxelMesherComponentDefinition def)
        {
            if (def == null)
            {
                throw new Exception("Definition {0} is not a valid MyVoxelMesherComponentDefinition.");
            }
            foreach (MyObjectBuilder_VoxelPostprocessing postprocessing in def.PostProcessingSteps)
            {
                MyVoxelPostprocessing item = MyVoxelPostprocessing.Factory.CreateInstance(postprocessing.TypeId);
                item.Init(postprocessing);
                this.m_postprocessingSteps.Add(item);
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
        }

        public VRage.Game.Voxels.IMyStorage Storage
        {
            get
            {
                IMyVoxelBase entity = base.Entity as IMyVoxelBase;
                return ((entity == null) ? null : ((VRage.Game.Voxels.IMyStorage) entity.Storage));
            }
        }

        public ListReader<MyVoxelPostprocessing> PostprocessingSteps =>
            this.m_postprocessingSteps;

        public override string ComponentTypeDebugString =>
            "MyVoxelMesherComponent";
    }
}

