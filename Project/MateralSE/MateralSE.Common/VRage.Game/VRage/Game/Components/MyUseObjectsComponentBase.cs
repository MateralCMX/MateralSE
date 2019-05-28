namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.Entity.UseObject;
    using VRageMath;

    [MyComponentType(typeof(MyUseObjectsComponentBase))]
    public abstract class MyUseObjectsComponentBase : MyEntityComponentBase
    {
        protected Dictionary<string, List<Matrix>> m_detectors = new Dictionary<string, List<Matrix>>();

        protected MyUseObjectsComponentBase()
        {
        }

        public abstract uint AddDetector(string name, Matrix matrix);
        public virtual void ClearPhysics()
        {
            if (this.DetectorPhysics != null)
            {
                this.DetectorPhysics.Close();
            }
        }

        public ListReader<Matrix> GetDetectors(string detectorName)
        {
            List<Matrix> list = null;
            this.m_detectors.TryGetValue(detectorName, out list);
            if ((list == null) || (list.Count == 0))
            {
                return ListReader<Matrix>.Empty;
            }
            return new ListReader<Matrix>(list);
        }

        public abstract IMyUseObject GetInteractiveObject(string detectorName);
        public abstract IMyUseObject GetInteractiveObject(uint shapeKey);
        public abstract void GetInteractiveObjects<T>(List<T> objects) where T: class, IMyUseObject;
        public abstract void LoadDetectorsFromModel();
        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (this.DetectorPhysics != null)
            {
                this.DetectorPhysics.Activate();
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            this.ClearPhysics();
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            if (this.DetectorPhysics != null)
            {
                this.DetectorPhysics.Deactivate();
            }
        }

        public abstract void PositionChanged(MyPositionComponentBase obj);
        public string RaycastDetectors(Vector3D worldFrom, Vector3D worldTo)
        {
            MatrixD worldMatrixNormalizedInv = base.Container.Get<MyPositionComponentBase>().WorldMatrixNormalizedInv;
            Vector3D position = Vector3D.Transform(worldFrom, worldMatrixNormalizedInv);
            Vector3D vectord2 = Vector3D.Transform(worldTo, worldMatrixNormalizedInv);
            BoundingBox box = new BoundingBox(-Vector3.One, Vector3.One);
            string key = null;
            float maxValue = float.MaxValue;
            foreach (KeyValuePair<string, List<Matrix>> pair in this.m_detectors)
            {
                foreach (Matrix matrix in pair.Value)
                {
                    Vector3 vector = (Vector3) Vector3D.Transform(position, matrix);
                    Vector3 direction = (Vector3) Vector3D.Transform(vectord2, matrix);
                    float? nullable = box.Intersects(new Ray(vector, direction));
                    if ((nullable != null) && (nullable.Value < maxValue))
                    {
                        maxValue = nullable.Value;
                        key = pair.Key;
                    }
                }
            }
            return key;
        }

        public abstract IMyUseObject RaycastDetectors(Vector3D worldFrom, Vector3D worldTo, out float parameter);
        public abstract void RecreatePhysics();
        public abstract void RemoveDetector(uint id);

        public abstract MyPhysicsComponentBase DetectorPhysics { get; protected set; }

        public override string ComponentTypeDebugString =>
            "Use Objects";
    }
}

