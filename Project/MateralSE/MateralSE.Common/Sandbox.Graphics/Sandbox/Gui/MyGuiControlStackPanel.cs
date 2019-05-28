namespace Sandbox.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlStackPanel : MyGuiControlBase
    {
        private List<MyGuiControlBase> m_controls;

        public MyGuiControlStackPanel() : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            this.m_controls = new List<MyGuiControlBase>();
            this.ArrangeInvisible = false;
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

        public override unsafe void UpdateArrange()
        {
            base.UpdateArrange();
            Vector2 vector = (base.OriginAlign != MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) ? base.Position : (base.Position - new Vector2(0f, base.Size.Y / 2f));
            for (int i = 0; i < this.m_controls.Count; i++)
            {
                MyGuiControlBase base2 = this.m_controls[i];
                if (base2.Visible || this.ArrangeInvisible)
                {
                    if (this.Orientation == MyGuiOrientation.Horizontal)
                    {
                        float* singlePtr1 = (float*) ref vector.X;
                        singlePtr1[0] += base2.Margin.Left;
                        base2.Position = new Vector2(vector.X, vector.Y + base2.Margin.Top);
                        float* singlePtr2 = (float*) ref vector.X;
                        singlePtr2[0] += base2.Margin.Right + base2.Size.X;
                    }
                    else
                    {
                        float* singlePtr3 = (float*) ref vector.Y;
                        singlePtr3[0] += base2.Margin.Top;
                        base2.Position = new Vector2(base.PositionX + base2.Margin.Left, vector.Y);
                        float* singlePtr4 = (float*) ref vector.Y;
                        singlePtr4[0] += base2.Margin.Bottom + base2.Size.Y;
                    }
                    base2.UpdateArrange();
                }
            }
        }

        public override unsafe void UpdateMeasure()
        {
            base.UpdateMeasure();
            Vector2 vector = new Vector2();
            for (int i = 0; i < this.m_controls.Count; i++)
            {
                MyGuiControlBase base2 = this.m_controls[i];
                if (base2.Visible || this.ArrangeInvisible)
                {
                    base2.UpdateMeasure();
                    if (this.Orientation == MyGuiOrientation.Horizontal)
                    {
                        Vector2* vectorPtr1 = (Vector2*) ref vector;
                        vectorPtr1->Y = Math.Max(vector.Y, (base2.Size.Y + base2.Margin.Top) + base2.Margin.Bottom);
                        float* singlePtr1 = (float*) ref vector.X;
                        singlePtr1[0] += (base2.Size.X + base2.Margin.Left) + base2.Margin.Right;
                    }
                    else
                    {
                        Vector2* vectorPtr2 = (Vector2*) ref vector;
                        vectorPtr2->X = Math.Max(vector.X, (base2.Size.X + base2.Margin.Right) + base2.Margin.Left);
                        float* singlePtr2 = (float*) ref vector.Y;
                        singlePtr2[0] += (base2.Size.Y + base2.Margin.Top) + base2.Margin.Bottom;
                    }
                }
            }
            base.Size = vector;
        }

        public MyGuiOrientation Orientation { get; set; }

        public bool ArrangeInvisible { get; set; }
    }
}

