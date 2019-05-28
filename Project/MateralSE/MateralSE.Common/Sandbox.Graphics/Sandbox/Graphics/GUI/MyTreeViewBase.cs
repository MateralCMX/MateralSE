namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using VRageMath;

    internal class MyTreeViewBase : ITreeView
    {
        private List<MyTreeViewItem> m_items = new List<MyTreeViewItem>();
        public MyTreeView TreeView;

        public MyTreeViewItem AddItem(StringBuilder text, string icon, Vector2 iconSize, string expandIcon, string collapseIcon, Vector2 expandIconSize)
        {
            MyTreeViewItem item = new MyTreeViewItem(text, icon, iconSize, expandIcon, collapseIcon, expandIconSize) {
                TreeView = this.TreeView
            };
            this.m_items.Add(item);
            item.Parent = this;
            return item;
        }

        public void ClearItems()
        {
            foreach (MyTreeViewItem local1 in this.m_items)
            {
                local1.TreeView = null;
                local1.ClearItems();
            }
            this.m_items.Clear();
        }

        public void DeleteItem(MyTreeViewItem item)
        {
            if (this.m_items.Remove(item))
            {
                item.TreeView = null;
                item.ClearItems();
            }
        }

        public void DrawItems(float transitionAlpha)
        {
            foreach (MyTreeViewItem item in this.m_items)
            {
                item.Draw(transitionAlpha);
                if (item.IsExpanded)
                {
                    item.DrawItems(transitionAlpha);
                }
            }
        }

        public int GetIndex(MyTreeViewItem item) => 
            this.m_items.IndexOf(item);

        public MyTreeViewItem GetItem(int index) => 
            this.m_items[index];

        public MyTreeViewItem GetItem(StringBuilder name) => 
            this.m_items.Find(a => ReferenceEquals(a.Text, name));

        public int GetItemCount() => 
            this.m_items.Count;

        public bool HandleInput(bool hasKeyboardActiveControl)
        {
            bool flag = false;
            foreach (MyTreeViewItem item in this.m_items)
            {
                flag = flag || item.HandleInputEx(hasKeyboardActiveControl);
                if (item.IsExpanded)
                {
                    flag = flag || item.HandleInput(hasKeyboardActiveControl);
                }
            }
            return flag;
        }

        public Vector2 LayoutItems(Vector2 origin)
        {
            float num = 0f;
            float y = 0f;
            using (List<MyTreeViewItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Vector2 vector = enumerator.Current.LayoutItem(origin + new Vector2(0f, y));
                    num = Math.Max(num, vector.X);
                    y += vector.Y;
                }
            }
            return new Vector2(num, y);
        }

        public MyTreeViewItem this[int i] =>
            this.m_items[i];
    }
}

