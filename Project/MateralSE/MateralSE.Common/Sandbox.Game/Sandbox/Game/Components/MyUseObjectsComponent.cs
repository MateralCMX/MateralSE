namespace Sandbox.Game.Components
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRageMath;
    using VRageRender.Import;

    [MyComponentBuilder(typeof(MyObjectBuilder_UseObjectsComponent), true)]
    public class MyUseObjectsComponent : MyUseObjectsComponentBase
    {
        [ThreadStatic]
        private static Vector3[] m_detectorVertices;
        [ThreadStatic]
        private static List<HkShape> m_shapes;
        private Dictionary<uint, DetectorData> m_detectorInteractiveObjects = new Dictionary<uint, DetectorData>();
        private Dictionary<string, uint> m_detectorShapeKeys = new Dictionary<string, uint>();
        private List<uint> m_customAddedDetectors = new List<uint>();
        private MyPhysicsBody m_detectorPhysics;
        private MyObjectBuilder_UseObjectsComponent m_objectBuilder;
        private MyUseObjectsComponentDefinition m_definition;

        public override uint AddDetector(string name, Matrix dummyMatrix)
        {
            MyModelDummy dummy;
            string detectorName = name.ToLower();
            string key = "detector_" + detectorName;
            MyModel model = base.Container.Entity.Render.GetModel();
            Dictionary<string, object> customData = null;
            if ((model != null) && model.Dummies.TryGetValue(key, out dummy))
            {
                customData = dummy.CustomData;
            }
            MyModelDummy dummyData = new MyModelDummy();
            dummyData.Name = key;
            dummyData.CustomData = customData;
            dummyData.Matrix = dummyMatrix;
            uint item = this.AddDetector(detectorName, key, dummyData);
            this.m_customAddedDetectors.Add(item);
            return item;
        }

        private unsafe uint AddDetector(string detectorName, string dummyName, MyModelDummy dummyData)
        {
            List<Matrix> list;
            if (!base.m_detectors.TryGetValue(detectorName, out list))
            {
                list = new List<Matrix>();
                base.m_detectors[detectorName] = list;
            }
            Matrix matrix = dummyData.Matrix;
            if (base.Entity is MyCubeBlock)
            {
                float gridScale = (base.Entity as MyCubeBlock).CubeGrid.GridScale;
                Matrix* matrixPtr1 = (Matrix*) ref matrix;
                matrixPtr1.Translation *= gridScale;
                Matrix.Rescale(ref matrix, gridScale);
            }
            list.Add(Matrix.Invert(matrix));
            uint count = (uint) this.m_detectorInteractiveObjects.Count;
            IMyUseObject useObject = this.CreateInteractiveObject(detectorName, dummyName, dummyData, count);
            if (useObject != null)
            {
                this.m_detectorInteractiveObjects.Add(count, new DetectorData(useObject, matrix, detectorName));
                this.m_detectorShapeKeys[detectorName] = count;
            }
            return count;
        }

        private IMyUseObject CreateInteractiveObject(string detectorName, string dummyName, MyModelDummy dummyData, uint shapeKey)
        {
            if (!(base.Container.Entity is MyDoor) || (detectorName != "terminal"))
            {
                return MyUseObjectFactory.CreateUseObject(detectorName, base.Container.Entity, dummyName, dummyData, shapeKey);
            }
            return new MyUseObjectDoorTerminal(base.Container.Entity, dummyName, dummyData, shapeKey);
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            this.m_objectBuilder = builder as MyObjectBuilder_UseObjectsComponent;
        }

        public override IMyUseObject GetInteractiveObject(string detectorName)
        {
            uint num;
            return (this.m_detectorShapeKeys.TryGetValue(detectorName, out num) ? this.GetInteractiveObject(num) : null);
        }

        public override IMyUseObject GetInteractiveObject(uint shapeKey)
        {
            DetectorData data;
            return (this.m_detectorInteractiveObjects.TryGetValue(shapeKey, out data) ? data.UseObject : null);
        }

        public override void GetInteractiveObjects<T>(List<T> objects) where T: class, IMyUseObject
        {
            foreach (KeyValuePair<uint, DetectorData> pair in this.m_detectorInteractiveObjects)
            {
                T useObject = pair.Value.UseObject as T;
                if (useObject != null)
                {
                    objects.Add(useObject);
                }
            }
        }

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            this.m_definition = definition as MyUseObjectsComponentDefinition;
        }

        public override bool IsSerialized() => 
            (this.m_customAddedDetectors.Count > 0);

        public override void LoadDetectorsFromModel()
        {
            base.m_detectors.Clear();
            this.m_detectorInteractiveObjects.Clear();
            if (this.m_detectorPhysics != null)
            {
                this.m_detectorPhysics.Close();
            }
            MyRenderComponentBase base2 = base.Container.Get<MyRenderComponentBase>();
            if (base2.GetModel() != null)
            {
                foreach (KeyValuePair<string, MyModelDummy> pair in base2.GetModel().Dummies)
                {
                    string dummyName = pair.Key.ToLower();
                    if (dummyName.StartsWith("detector_") && (dummyName.Length > "detector_".Length))
                    {
                        char[] separator = new char[] { '_' };
                        string[] strArray = dummyName.Split(separator);
                        if (strArray.Length >= 2)
                        {
                            this.AddDetector(strArray[1], dummyName, pair.Value);
                        }
                    }
                }
            }
            if (this.m_detectorInteractiveObjects.Count > 0)
            {
                this.RecreatePhysics();
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (this.m_definition != null)
            {
                if (this.m_definition.LoadFromModel)
                {
                    this.LoadDetectorsFromModel();
                }
                if (this.m_definition.UseObjectFromModelBBox != null)
                {
                    Matrix matrix = Matrix.CreateScale(base.Entity.PositionComp.LocalAABB.Size) * Matrix.CreateTranslation(base.Entity.PositionComp.LocalAABB.Center);
                    this.AddDetector(this.m_definition.UseObjectFromModelBBox, matrix);
                }
            }
            if (this.m_objectBuilder != null)
            {
                for (int i = 0; i < this.m_objectBuilder.CustomDetectorsCount; i++)
                {
                    if (!base.m_detectors.ContainsKey(this.m_objectBuilder.CustomDetectorsNames[i]))
                    {
                        this.AddDetector(this.m_objectBuilder.CustomDetectorsNames[i], this.m_objectBuilder.CustomDetectorsMatrices[i]);
                    }
                }
            }
            this.RecreatePhysics();
        }

        public override void PositionChanged(MyPositionComponentBase obj)
        {
            if (this.m_detectorPhysics != null)
            {
                this.m_detectorPhysics.OnWorldPositionChanged(obj);
            }
        }

        private void positionComponent_OnPositionChanged(MyPositionComponentBase obj)
        {
            this.m_detectorPhysics.OnWorldPositionChanged(obj);
        }

        public override IMyUseObject RaycastDetectors(Vector3D worldFrom, Vector3D worldTo, out float parameter)
        {
            MyPositionComponentBase base2 = base.Container.Get<MyPositionComponentBase>();
            MatrixD worldMatrixNormalizedInv = base2.WorldMatrixNormalizedInv;
            RayD ray = new RayD(worldFrom, worldTo - worldFrom);
            IMyUseObject useObject = null;
            parameter = float.MaxValue;
            foreach (KeyValuePair<uint, DetectorData> pair in this.m_detectorInteractiveObjects)
            {
                MatrixD matrix = pair.Value.Matrix * base2.WorldMatrix;
                double? nullable = new MyOrientedBoundingBoxD(matrix).Intersects(ref ray);
                if ((nullable != null) && (nullable.Value < ((double) parameter)))
                {
                    parameter = (float) nullable.Value;
                    useObject = pair.Value.UseObject;
                }
            }
            return useObject;
        }

        public override unsafe void RecreatePhysics()
        {
            if (this.m_detectorPhysics != null)
            {
                this.m_detectorPhysics.Close();
                this.m_detectorPhysics = null;
            }
            if (m_shapes == null)
            {
                m_shapes = new List<HkShape>();
            }
            if (m_detectorVertices == null)
            {
                m_detectorVertices = new Vector3[8];
            }
            m_shapes.Clear();
            BoundingBox box = new BoundingBox(-Vector3.One / 2f, Vector3.One / 2f);
            MyPositionComponentBase base2 = base.Container.Get<MyPositionComponentBase>();
            foreach (KeyValuePair<uint, DetectorData> pair in this.m_detectorInteractiveObjects)
            {
                Vector3[] pinned vectorArray;
                try
                {
                    Vector3* vectorPtr;
                    if (((vectorArray = m_detectorVertices) == null) || (vectorArray.Length == 0))
                    {
                        vectorPtr = null;
                    }
                    else
                    {
                        vectorPtr = vectorArray;
                    }
                    box.GetCornersUnsafe(vectorPtr);
                }
                finally
                {
                    vectorArray = null;
                }
                int index = 0;
                while (true)
                {
                    if (index >= 8)
                    {
                        m_shapes.Add((HkShape) new HkConvexVerticesShape(m_detectorVertices, 8, false, 0f));
                        break;
                    }
                    m_detectorVertices[index] = Vector3.Transform(m_detectorVertices[index], pair.Value.Matrix);
                    index++;
                }
            }
            if (m_shapes.Count > 0)
            {
                HkListShape shape = new HkListShape(m_shapes.GetInternalArray<HkShape>(), m_shapes.Count, HkReferencePolicy.TakeOwnership);
                this.m_detectorPhysics = new MyPhysicsBody(base.Container.Entity, RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE);
                HkMassProperties? massProperties = null;
                this.m_detectorPhysics.CreateFromCollisionObject((HkShape) shape, Vector3.Zero, base2.WorldMatrix, massProperties, 15);
                shape.Base.RemoveReference();
            }
        }

        public override void RemoveDetector(uint id)
        {
            if (this.m_detectorInteractiveObjects.ContainsKey(id))
            {
                this.m_detectorShapeKeys.Remove(this.m_detectorInteractiveObjects[id].DetectorName);
                this.m_detectorInteractiveObjects.Remove(id);
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_UseObjectsComponent component = MyComponentFactory.CreateObjectBuilder(this) as MyObjectBuilder_UseObjectsComponent;
            component.CustomDetectorsCount = (uint) this.m_customAddedDetectors.Count;
            int index = 0;
            if (component.CustomDetectorsCount > 0)
            {
                component.CustomDetectorsMatrices = new Matrix[component.CustomDetectorsCount];
                component.CustomDetectorsNames = new string[component.CustomDetectorsCount];
                foreach (uint num2 in this.m_customAddedDetectors)
                {
                    component.CustomDetectorsNames[index] = this.m_detectorInteractiveObjects[num2].DetectorName;
                    component.CustomDetectorsMatrices[index] = this.m_detectorInteractiveObjects[num2].Matrix;
                    index++;
                }
            }
            return component;
        }

        public void SetUseObjectIDs(uint renderId, int instanceId)
        {
            foreach (KeyValuePair<uint, DetectorData> pair in this.m_detectorInteractiveObjects)
            {
                pair.Value.UseObject.SetRenderID(renderId);
                pair.Value.UseObject.SetInstanceID(instanceId);
            }
        }

        public override MyPhysicsComponentBase DetectorPhysics
        {
            get => 
                this.m_detectorPhysics;
            protected set => 
                (this.m_detectorPhysics = value as MyPhysicsBody);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DetectorData
        {
            public IMyUseObject UseObject;
            public VRageMath.Matrix Matrix;
            public string DetectorName;
            public DetectorData(IMyUseObject useObject, VRageMath.Matrix mat, string name)
            {
                this.UseObject = useObject;
                this.Matrix = mat;
                this.DetectorName = name;
            }
        }
    }
}

