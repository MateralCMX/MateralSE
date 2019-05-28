namespace Sandbox.Definitions
{
    using Sandbox;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_PrefabDefinition), (Type) null)]
    public class MyPrefabDefinition : MyDefinitionBase
    {
        private MyObjectBuilder_CubeGrid[] m_cubeGrids;
        private VRageMath.BoundingSphere m_boundingSphere;
        private VRageMath.BoundingBox m_boundingBox;
        public string PrefabPath;
        public bool Initialized;

        protected override void Init(MyObjectBuilder_DefinitionBase baseBuilder)
        {
            base.Init(baseBuilder);
            MyObjectBuilder_PrefabDefinition definition = baseBuilder as MyObjectBuilder_PrefabDefinition;
            this.PrefabPath = definition.PrefabPath;
            this.Initialized = false;
        }

        public void InitLazy(MyObjectBuilder_DefinitionBase baseBuilder)
        {
            MyObjectBuilder_PrefabDefinition definition = baseBuilder as MyObjectBuilder_PrefabDefinition;
            if ((definition.CubeGrid != null) || (definition.CubeGrids != null))
            {
                if (definition.CubeGrid == null)
                {
                    this.m_cubeGrids = definition.CubeGrids;
                }
                else
                {
                    this.m_cubeGrids = new MyObjectBuilder_CubeGrid[] { definition.CubeGrid };
                }
                this.m_boundingSphere = new VRageMath.BoundingSphere(Vector3.Zero, float.MinValue);
                this.m_boundingBox = VRageMath.BoundingBox.CreateInvalid();
                MyObjectBuilder_CubeGrid[] cubeGrids = this.m_cubeGrids;
                int index = 0;
                while (index < cubeGrids.Length)
                {
                    Matrix identity;
                    MyObjectBuilder_CubeGrid grid = cubeGrids[index];
                    VRageMath.BoundingBox box = grid.CalculateBoundingBox();
                    if (grid.PositionAndOrientation == null)
                    {
                        identity = Matrix.Identity;
                    }
                    else
                    {
                        identity = (Matrix) grid.PositionAndOrientation.Value.GetMatrix();
                    }
                    this.m_boundingBox.Include(box.Transform(identity));
                    index++;
                }
                this.m_boundingSphere = VRageMath.BoundingSphere.CreateFromBoundingBox(this.m_boundingBox);
                cubeGrids = this.m_cubeGrids;
                for (index = 0; index < cubeGrids.Length; index++)
                {
                    MyObjectBuilder_CubeGrid grid1 = cubeGrids[index];
                    grid1.CreatePhysics = true;
                    grid1.XMirroxPlane = null;
                    grid1.YMirroxPlane = null;
                    grid1.ZMirroxPlane = null;
                }
                this.Initialized = true;
            }
        }

        public MyObjectBuilder_CubeGrid[] CubeGrids
        {
            get
            {
                if (!this.Initialized)
                {
                    MyDefinitionManager.Static.ReloadPrefabsFromFile(this.PrefabPath);
                }
                return this.m_cubeGrids;
            }
        }

        public VRageMath.BoundingSphere BoundingSphere
        {
            get
            {
                if (!this.Initialized)
                {
                    MyDefinitionManager.Static.ReloadPrefabsFromFile(this.PrefabPath);
                }
                return this.m_boundingSphere;
            }
        }

        public VRageMath.BoundingBox BoundingBox
        {
            get
            {
                if (!this.Initialized)
                {
                    MyDefinitionManager.Static.ReloadPrefabsFromFile(this.PrefabPath);
                }
                return this.m_boundingBox;
            }
        }
    }
}

