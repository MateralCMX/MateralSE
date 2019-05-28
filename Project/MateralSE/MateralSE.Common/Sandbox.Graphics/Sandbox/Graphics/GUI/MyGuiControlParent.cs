namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlParent))]
    public class MyGuiControlParent : MyGuiControlBase, IMyGuiControlsParent, IMyGuiControlsOwner
    {
        private MyGuiControls m_controls;

        public MyGuiControlParent() : this(nullable, nullable, nullable2, null)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlParent(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string toolTip = null) : base(position, size, backgroundColor, toolTip, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_controls = new MyGuiControls(this);
        }

        public override void Clear()
        {
            this.Controls.Clear();
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            foreach (MyGuiControlBase base2 in this.Controls.GetVisibleControls())
            {
                if (ReferenceEquals(base2.GetExclusiveInputHandler(), base2))
                {
                    continue;
                }
                if (!(base2 is MyGuiControlGridDragAndDrop))
                {
                    base2.Draw(transitionAlpha * base2.Alpha, backgroundTransitionAlpha * base2.Alpha);
                }
            }
        }

        public override MyGuiControlGridDragAndDrop GetDragAndDropHandlingNow()
        {
            for (int i = 0; i < this.Controls.GetVisibleControls().Count; i++)
            {
                MyGuiControlBase base2 = this.Controls.GetVisibleControls()[i];
                if (base2 is MyGuiControlGridDragAndDrop)
                {
                    MyGuiControlGridDragAndDrop drop = (MyGuiControlGridDragAndDrop) base2;
                    if (drop.IsActive())
                    {
                        return drop;
                    }
                }
            }
            return null;
        }

        public override MyGuiControlBase GetExclusiveInputHandler()
        {
            MyGuiControlBase exclusiveInputHandler = GetExclusiveInputHandler(this.Controls);
            if (exclusiveInputHandler == null)
            {
                exclusiveInputHandler = base.GetExclusiveInputHandler();
            }
            return exclusiveInputHandler;
        }

        internal override MyGuiControlBase GetFocusControl(bool forwardMovement) => 
            this.GetNextFocusControl(this, forwardMovement);

        public override MyGuiControlBase GetMouseOverControl()
        {
            for (int i = this.Controls.GetVisibleControls().Count - 1; i >= 0; i--)
            {
                if (this.Controls.GetVisibleControls()[i].IsHitTestVisible && this.Controls.GetVisibleControls()[i].IsMouseOver)
                {
                    return this.Controls.GetVisibleControls()[i];
                }
            }
            return null;
        }

        public override MyGuiControlBase GetNextFocusControl(MyGuiControlBase currentFocusControl, bool forwardMovement)
        {
            List<MyGuiControlBase> visibleControls = this.Controls.GetVisibleControls();
            int num = visibleControls.Count + base.Elements.Count;
            int index = visibleControls.IndexOf(currentFocusControl);
            if (index == -1)
            {
                index = base.Elements.IndexOf(currentFocusControl);
                if (index != -1)
                {
                    index += visibleControls.Count;
                }
            }
            if (!forwardMovement && (index == -1))
            {
                index = num;
            }
            int num3 = forwardMovement ? (index + 1) : (index - 1);
            int num4 = forwardMovement ? 1 : -1;
            while ((forwardMovement && (num3 < num)) || (!forwardMovement && (num3 >= 0)))
            {
                int num5 = num3;
                if (num5 >= visibleControls.Count)
                {
                    num5 -= visibleControls.Count;
                    if (MyGuiScreenBase.CanHaveFocusRightNow(base.Elements[num5]))
                    {
                        return base.Elements[num5];
                    }
                }
                else
                {
                    MyGuiControlBase control = visibleControls[num5];
                    if (MyGuiScreenBase.CanHaveFocusRightNow(control))
                    {
                        if ((control is MyGuiControlParent) || !control.IsActiveControl)
                        {
                            return control.GetFocusControl(forwardMovement);
                        }
                        return control;
                    }
                }
                num3 += num4;
            }
            return base.Owner.GetNextFocusControl(this, forwardMovement);
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlParent objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_GuiControlParent;
            objectBuilder.Controls = this.Controls.GetObjectBuilder();
            return objectBuilder;
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = null;
            base2 = base.HandleInput();
            base.IsMouseOver = true;
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
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
            return base2;
        }

        public override void HideToolTip()
        {
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.HideToolTip();
                }
            }
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiControlParent parent = builder as MyObjectBuilder_GuiControlParent;
            if (parent.Controls != null)
            {
                this.m_controls.Init(parent.Controls);
            }
        }

        public override bool IsMouseOverAnyControl()
        {
            for (int i = this.Controls.GetVisibleControls().Count - 1; i >= 0; i--)
            {
                if (!this.Controls.GetVisibleControls()[i].IsHitTestVisible && this.Controls.GetVisibleControls()[i].IsMouseOver)
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnRemoving()
        {
            this.Controls.Clear();
            base.OnRemoving();
        }

        public override void ShowToolTip()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ShowToolTip();
                }
            }
        }

        public override void Update()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
            base.Update();
        }

        public override void UpdateArrange()
        {
            base.UpdateArrange();
        }

        public override void UpdateMeasure()
        {
            base.UpdateMeasure();
        }

        public MyGuiControls Controls =>
            this.m_controls;
    }
}

