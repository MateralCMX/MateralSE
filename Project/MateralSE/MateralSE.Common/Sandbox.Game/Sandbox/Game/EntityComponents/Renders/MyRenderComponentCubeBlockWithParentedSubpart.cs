namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox.Game.Components;
    using System;
    using System.Collections.Generic;

    public class MyRenderComponentCubeBlockWithParentedSubpart : MyRenderComponentCubeBlock
    {
        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            this.UpdateChildren();
        }

        protected void UpdateChildren()
        {
            using (List<MyHierarchyComponentBase>.Enumerator enumerator = base.m_cubeBlock.Hierarchy.Children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyParentedSubpartRenderComponent render = enumerator.Current.Entity.Render as MyParentedSubpartRenderComponent;
                    if (render != null)
                    {
                        render.UpdateParent();
                    }
                }
            }
        }
    }
}

