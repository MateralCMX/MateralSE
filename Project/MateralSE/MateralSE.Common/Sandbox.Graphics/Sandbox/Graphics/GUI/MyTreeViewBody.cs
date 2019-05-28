namespace Sandbox.Graphics.GUI
{
    using System;
    using VRageMath;

    internal class MyTreeViewBody : MyTreeViewBase
    {
        private Vector2 m_position;
        private Vector2 m_size;
        private Vector2 m_realSize;

        public MyTreeViewBody(MyTreeView treeView, Vector2 position, Vector2 size)
        {
            base.TreeView = treeView;
            this.m_position = position;
            this.m_size = size;
        }

        public void Draw(float transitionAlpha)
        {
            base.DrawItems(transitionAlpha);
        }

        public Vector2 GetPosition() => 
            this.m_position;

        public Vector2 GetRealSize() => 
            this.m_realSize;

        public Vector2 GetSize() => 
            this.m_size;

        public void Layout(Vector2 scroll)
        {
            this.m_realSize = base.LayoutItems(this.m_position - scroll);
        }

        public void SetPosition(Vector2 position)
        {
            this.m_position = position;
        }

        public void SetSize(Vector2 size)
        {
            this.m_size = size;
        }
    }
}

