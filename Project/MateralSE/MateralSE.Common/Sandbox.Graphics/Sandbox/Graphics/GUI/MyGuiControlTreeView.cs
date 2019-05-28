namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Text;
    using VRageMath;

    internal class MyGuiControlTreeView : MyGuiControlBase, ITreeView
    {
        public bool WholeRowHighlight;
        private Vector4 m_treeBackgroundColor;
        private MyTreeView m_treeView;

        public MyGuiControlTreeView(Vector2 position, Vector2 size, Vector4 backgroundColor, bool canHandleKeyboardActiveControl) : base(new Vector2?(position), new Vector2?(size), nullable, null, null, true, canHandleKeyboardActiveControl, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            base.Visible = true;
            base.Name = "TreeView";
            this.m_treeBackgroundColor = backgroundColor;
            this.m_treeView = new MyTreeView(this, base.GetPositionAbsolute() - (base.Size / 2f), base.Size);
        }

        public MyTreeViewItem AddItem(StringBuilder text, string icon, Vector2 iconSize, string expandIcon, string collapseIcon, Vector2 expandIconSize) => 
            this.m_treeView.AddItem(text, icon, iconSize, expandIcon, collapseIcon, expandIconSize);

        public void ClearItems()
        {
            this.m_treeView.ClearItems();
        }

        public void DeleteItem(MyTreeViewItem item)
        {
            this.m_treeView.DeleteItem(item);
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if (!base.Visible)
            {
                this.ShowToolTip(null);
            }
            else
            {
                this.m_treeView.Layout();
                this.m_treeView.Draw(transitionAlpha);
            }
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
        }

        public void FilterTree(Predicate<MyTreeViewItem> itemFilter)
        {
            MyTreeView.FilterTree(this, itemFilter);
        }

        public MyTreeViewItem GetFocusedItem() => 
            this.m_treeView.FocusedItem;

        public MyTreeViewItem GetItem(int index) => 
            this.m_treeView.GetItem(index);

        public MyTreeViewItem GetItem(StringBuilder name) => 
            this.m_treeView.GetItem(name);

        public int GetItemCount() => 
            this.m_treeView.GetItemCount();

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (((base2 == null) && base.Visible) && this.m_treeView.HandleInput())
            {
                base2 = this;
            }
            return base2;
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            this.m_treeView.SetPosition(base.GetPositionAbsolute() - (base.Size / 2f));
        }

        public void SetSize(Vector2 size)
        {
            base.Size = size;
            this.m_treeView.SetPosition(base.GetPositionAbsolute() - (base.Size / 2f));
            this.m_treeView.SetSize(size);
        }

        public void ShowToolTip(MyToolTips tooltip)
        {
            base.m_showToolTip = false;
            base.m_toolTip = tooltip;
            this.ShowToolTip();
        }
    }
}

