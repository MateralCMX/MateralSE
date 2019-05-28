namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Input;
    using VRageMath;

    internal class MyTreeViewItemDragAndDrop : MyGuiControlBase
    {
        private bool m_frameBackDragging;
        public EventHandler Drop;

        public MyTreeViewItemDragAndDrop() : base(new Vector2?(Vector2.Zero), nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
        }

        public override bool CheckMouseOver() => 
            this.m_frameBackDragging;

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            if (this.Dragging)
            {
                Vector2 vector = this.StartDragPosition - this.DraggedItem.GetPosition();
                this.DraggedItem.DrawDraged(MyGuiManager.MouseCursorPosition - vector, transitionAlpha);
            }
            this.m_frameBackDragging = this.Dragging;
        }

        public override MyGuiControlBase HandleInput()
        {
            if (this.DraggedItem != null)
            {
                if (MyInput.Static.IsLeftMousePressed())
                {
                    if (MyGuiManager.GetScreenSizeFromNormalizedSize(this.StartDragPosition - MyGuiManager.MouseCursorPosition, false).LengthSquared() > 16f)
                    {
                        this.Dragging = this.m_frameBackDragging = true;
                    }
                }
                else
                {
                    if ((this.Drop != null) && this.Dragging)
                    {
                        this.Drop(this, System.EventArgs.Empty);
                    }
                    this.Dragging = false;
                    this.DraggedItem = null;
                }
            }
            return base.HandleInput();
        }

        public bool HandleInput(MyTreeViewItem treeViewItem)
        {
            bool flag = false;
            if (((this.DraggedItem == null) && (MyGUIHelper.Contains(treeViewItem.GetPosition(), treeViewItem.GetSize(), MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y) && treeViewItem.TreeView.Contains(MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y))) && MyInput.Static.IsNewLeftMousePressed())
            {
                this.Dragging = false;
                this.DraggedItem = treeViewItem;
                this.StartDragPosition = MyGuiManager.MouseCursorPosition;
                flag = true;
            }
            return flag;
        }

        public void Init(MyTreeViewItem item, Vector2 startDragPosition)
        {
            this.Dragging = false;
            this.DraggedItem = item;
            this.StartDragPosition = startDragPosition;
        }

        public bool Dragging { get; set; }

        public Vector2 StartDragPosition { get; set; }

        public MyTreeViewItem DraggedItem { get; set; }
    }
}

