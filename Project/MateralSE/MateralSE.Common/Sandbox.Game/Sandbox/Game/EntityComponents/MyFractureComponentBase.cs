namespace Sandbox.Game.EntityComponents
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Components;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRageMath;

    public abstract class MyFractureComponentBase : MyEntityComponentBase
    {
        protected readonly List<HkdShapeInstanceInfo> m_tmpChildren = new List<HkdShapeInstanceInfo>();
        protected readonly List<HkdShapeInstanceInfo> m_tmpShapeInfos = new List<HkdShapeInstanceInfo>();
        protected readonly List<MyObjectBuilder_FractureComponentBase.FracturedShape> m_tmpShapeList = new List<MyObjectBuilder_FractureComponentBase.FracturedShape>();
        public HkdBreakableShape Shape;

        protected MyFractureComponentBase()
        {
        }

        protected void GetCurrentFracturedShapeList(List<MyObjectBuilder_FractureComponentBase.FracturedShape> shapeList, string[] excludeShapeNames = null)
        {
            GetCurrentFracturedShapeList(this.Shape, shapeList, excludeShapeNames);
        }

        private static bool GetCurrentFracturedShapeList(HkdBreakableShape breakableShape, List<MyObjectBuilder_FractureComponentBase.FracturedShape> shapeList, string[] excludeShapeNames = null)
        {
            MyObjectBuilder_FractureComponentBase.FracturedShape shape2;
            if (!breakableShape.IsValid())
            {
                return false;
            }
            string name = breakableShape.Name;
            bool flag = string.IsNullOrEmpty(name);
            if ((excludeShapeNames != null) && !flag)
            {
                foreach (string str2 in excludeShapeNames)
                {
                    if (name == str2)
                    {
                        return false;
                    }
                }
            }
            if (breakableShape.GetChildrenCount() <= 0)
            {
                if (flag)
                {
                    return false;
                }
                shape2 = new MyObjectBuilder_FractureComponentBase.FracturedShape {
                    Name = name,
                    Fixed = breakableShape.IsFixed()
                };
                shapeList.Add(shape2);
                return true;
            }
            List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
            breakableShape.GetChildren(list);
            bool flag2 = true;
            foreach (HkdShapeInstanceInfo info in list)
            {
                flag2 &= GetCurrentFracturedShapeList(info.Shape, shapeList, excludeShapeNames);
            }
            if (!flag & flag2)
            {
                using (List<HkdShapeInstanceInfo>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        HkdShapeInstanceInfo inst;
                        HkdBreakableShape shape = inst.Shape;
                        if (shape.IsValid())
                        {
                            shapeList.RemoveAll(s => s.Name == inst.ShapeName);
                        }
                    }
                }
                shape2 = new MyObjectBuilder_FractureComponentBase.FracturedShape {
                    Name = name,
                    Fixed = breakableShape.IsFixed()
                };
                shapeList.Add(shape2);
            }
            return flag2;
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            MyRenderComponentFracturedPiece piece = new MyRenderComponentFracturedPiece();
            if (base.Entity.Render.ModelStorage != null)
            {
                piece.ModelStorage = base.Entity.Render.ModelStorage;
            }
            base.Entity.Render.UpdateRenderObject(false, true);
            MyPersistentEntityFlags2 persistentFlags = base.Entity.Render.PersistentFlags;
            Vector3 colorMaskHsv = base.Entity.Render.ColorMaskHsv;
            Dictionary<string, MyTextureChange> textureChanges = base.Entity.Render.TextureChanges;
            base.Entity.Render = piece;
            base.Entity.Render.NeedsDraw = true;
            MyRenderComponentBase render = base.Entity.Render;
            render.PersistentFlags |= persistentFlags | MyPersistentEntityFlags2.CastShadows;
            base.Entity.Render.ColorMaskHsv = colorMaskHsv;
            base.Entity.Render.TextureChanges = textureChanges;
            base.Entity.Render.EnableColorMaskHsv = false;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (this.Shape.IsValid())
            {
                this.Shape.RemoveReference();
            }
        }

        protected abstract void RecreateShape(List<MyObjectBuilder_FractureComponentBase.FracturedShape> shapeList);
        public bool RemoveChildShapes(List<string> shapeNames) => 
            this.RemoveChildShapes(shapeNames.GetInternalArray<string>());

        public virtual bool RemoveChildShapes(string[] shapeNames)
        {
            this.m_tmpShapeList.Clear();
            this.GetCurrentFracturedShapeList(this.m_tmpShapeList, shapeNames);
            this.RecreateShape(this.m_tmpShapeList);
            this.m_tmpShapeList.Clear();
            return false;
        }

        protected void SerializeInternal(MyObjectBuilder_FractureComponentBase ob)
        {
            MyObjectBuilder_FractureComponentBase.FracturedShape shape2;
            if ((!string.IsNullOrEmpty(this.Shape.Name) && !this.Shape.IsCompound()) && (this.Shape.GetChildrenCount() <= 0))
            {
                shape2 = new MyObjectBuilder_FractureComponentBase.FracturedShape {
                    Name = this.Shape.Name
                };
                ob.Shapes.Add(shape2);
            }
            else
            {
                this.Shape.GetChildren(this.m_tmpChildren);
                foreach (HkdShapeInstanceInfo info in this.m_tmpChildren)
                {
                    shape2 = new MyObjectBuilder_FractureComponentBase.FracturedShape {
                        Name = info.ShapeName,
                        Fixed = MyDestructionHelper.IsFixed(info.Shape)
                    };
                    MyObjectBuilder_FractureComponentBase.FracturedShape item = shape2;
                    ob.Shapes.Add(item);
                }
                this.m_tmpChildren.Clear();
            }
        }

        public virtual void SetShape(HkdBreakableShape shape, bool compound)
        {
            if (this.Shape.IsValid())
            {
                this.Shape.RemoveReference();
            }
            this.Shape = shape;
            MyRenderComponentFracturedPiece render = base.Entity.Render as MyRenderComponentFracturedPiece;
            if (render != null)
            {
                render.ClearModels();
                if (!compound)
                {
                    render.AddPiece(shape.Name, Matrix.Identity);
                }
                else
                {
                    shape.GetChildren(this.m_tmpChildren);
                    foreach (HkdShapeInstanceInfo info in this.m_tmpChildren)
                    {
                        if (info.IsValid())
                        {
                            render.AddPiece(info.ShapeName, Matrix.Identity);
                        }
                    }
                    this.m_tmpChildren.Clear();
                }
                render.UpdateRenderObject(true, true);
            }
        }

        public abstract MyPhysicalModelDefinition PhysicalModelDefinition { get; }

        public override string ComponentTypeDebugString =>
            "Fracture";

        [StructLayout(LayoutKind.Sequential)]
        public struct Info
        {
            public MyEntity Entity;
            public HkdBreakableShape Shape;
            public bool Compound;
        }
    }
}

