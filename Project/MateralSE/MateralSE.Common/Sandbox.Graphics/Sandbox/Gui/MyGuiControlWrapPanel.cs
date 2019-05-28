namespace Sandbox.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyGuiControlWrapPanel : MyGuiControlBase
    {
        private List<MyGuiControlBase> m_controls;
        public Vector2 ItemSize;
        private int m_itemsPerRow;

        public MyGuiControlWrapPanel(Vector2 itemSize) : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            this.m_controls = new List<MyGuiControlBase>();
            this.ArrangeInvisible = false;
            this.ItemSize = itemSize;
        }

        public void Add(MyGuiControlBase control)
        {
            this.m_controls.Add(control);
        }

        public List<MyGuiControlBase> GetControls(bool onlyVisible = true)
        {
            List<MyGuiControlBase> list = new List<MyGuiControlBase>();
            foreach (MyGuiControlBase base2 in this.m_controls)
            {
                if (base2.Visible || !onlyVisible)
                {
                    list.Add(base2);
                }
            }
            return list;
        }

        public override MyGuiControlBase GetMouseOverControl()
        {
            for (int i = this.m_controls.Count - 1; i >= 0; i--)
            {
                if ((this.m_controls[i].Visible && this.m_controls[i].IsHitTestVisible) && this.m_controls[i].IsMouseOver)
                {
                    return this.m_controls[i];
                }
            }
            return null;
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = null;
            base.IsMouseOver = this.CheckMouseOver();
            using (List<MyGuiControlBase>.Enumerator enumerator = this.m_controls.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    base2 = enumerator.Current.HandleInput();
                    if (base2 != null)
                    {
                        break;
                    }
                }
            }
            if (base2 != null)
            {
                base2 = base.HandleInput();
            }
            return base2;
        }

        public override bool IsMouseOverAnyControl()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.m_controls.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGuiControlBase current = enumerator.Current;
                    if (current.IsHitTestVisible && current.IsMouseOver)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void UpdateArrange()
        {
            base.UpdateArrange();
            List<MyGuiControlBase> controls = this.GetControls(true);
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < controls.Count; i++)
            {
                Vector2 vector = new Vector2(base.Margin.Left + ((this.ItemSize.X + this.InnerOffset.X) * num), base.Margin.Top + ((this.ItemSize.Y + this.InnerOffset.Y) * num2)) + base.Position;
                MyGuiControlBase local1 = controls[i];
                local1.Position = vector;
                if ((num + 1) >= this.m_itemsPerRow)
                {
                    num = 0;
                    num2++;
                }
                local1.UpdateArrange();
            }
        }

        public override void UpdateMeasure()
        {
            base.UpdateMeasure();
            float num = ((base.Size.X - base.Margin.Left) - base.Margin.Right) - this.ItemSize.X;
            this.m_itemsPerRow = ((int) (num / (this.ItemSize.X + this.InnerOffset.X))) + 1;
            int num2 = ((this.GetControls(true).Count + this.m_itemsPerRow) - 1) / this.m_itemsPerRow;
            Vector2 vector = new Vector2(base.Size.X, (((num2 * (this.ItemSize.Y + this.InnerOffset.Y)) - this.InnerOffset.Y) + base.Margin.Top) + base.Margin.Bottom);
            base.Size = vector;
        }

        public bool ArrangeInvisible { get; set; }

        public Vector2 InnerOffset { get; set; }
    }
}

