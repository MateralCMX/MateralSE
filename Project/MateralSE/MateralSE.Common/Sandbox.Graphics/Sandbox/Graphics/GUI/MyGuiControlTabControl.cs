namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlTabControl))]
    public class MyGuiControlTabControl : MyGuiControlParent
    {
        [CompilerGenerated]
        private Action OnPageChanged;
        private Dictionary<int, MyGuiControlTabPage> m_pages;
        private string m_selectedTexture;
        private string m_unSelectedTexture;
        private int m_selectedPage;
        private Vector2 m_tabButtonSize;
        private float m_tabButtonScale;

        public event Action OnPageChanged
        {
            [CompilerGenerated] add
            {
                Action onPageChanged = this.OnPageChanged;
                while (true)
                {
                    Action a = onPageChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onPageChanged = Interlocked.CompareExchange<Action>(ref this.OnPageChanged, action3, a);
                    if (ReferenceEquals(onPageChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onPageChanged = this.OnPageChanged;
                while (true)
                {
                    Action source = onPageChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onPageChanged = Interlocked.CompareExchange<Action>(ref this.OnPageChanged, action3, source);
                    if (ReferenceEquals(onPageChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlTabControl() : this(nullable, nullable, nullable2)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlTabControl(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? colorMask = new Vector4?()) : base(position, size, colorMask, null)
        {
            this.m_pages = new Dictionary<int, MyGuiControlTabPage>();
            this.m_tabButtonScale = 1f;
            this.RefreshInternals();
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            MyGuiCompositeTexture texture = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL;
            MyGuiCompositeTexture texture2 = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_HIGHLIGHT;
            int count = this.m_pages.Count;
            int num = 0;
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            foreach (int num2 in this.m_pages.Keys)
            {
                MyGuiControlTabPage tabSubControl = this.GetTabSubControl(num2);
                if (tabSubControl.IsTabVisible)
                {
                    bool flag = (this.GetMouseOverTab() == num2) || (this.SelectedPage == num2);
                    bool enabled = base.Enabled && tabSubControl.Enabled;
                    Color color = ApplyColorMaskModifiers(base.ColorMask, enabled, transitionAlpha);
                    string font = (enabled & flag) ? "White" : "Blue";
                    ((enabled & flag) ? texture2 : texture).Draw(positionAbsoluteTopLeft, this.TabButtonSize, ApplyColorMaskModifiers(base.ColorMask, enabled, transitionAlpha), this.m_tabButtonScale);
                    StringBuilder text = tabSubControl.Text;
                    if (text != null)
                    {
                        MyGuiDrawAlignEnum drawAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
                        MyGuiManager.DrawString(font, text, positionAbsoluteTopLeft + (this.TabButtonSize / 2f), tabSubControl.TextScale, new Color?(color), drawAlign, false, float.PositiveInfinity);
                    }
                    float* singlePtr1 = (float*) ref positionAbsoluteTopLeft.X;
                    singlePtr1[0] += this.TabButtonSize.X;
                    num++;
                }
            }
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
        }

        public override MyGuiControlGridDragAndDrop GetDragAndDropHandlingNow() => 
            ((this.m_selectedPage <= -1) ? null : this.m_pages[this.m_selectedPage].GetDragAndDropHandlingNow());

        private unsafe int GetMouseOverTab()
        {
            int count = this.m_pages.Keys.Count;
            int num = 0;
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            using (Dictionary<int, MyGuiControlTabPage>.Enumerator enumerator = this.m_pages.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<int, MyGuiControlTabPage> current = enumerator.Current;
                    if (current.Value.IsTabVisible)
                    {
                        int key = current.Key;
                        Vector2 vector2 = positionAbsoluteTopLeft;
                        Vector2 vector3 = vector2 + this.TabButtonSize;
                        if (((MyGuiManager.MouseCursorPosition.X < vector2.X) || ((MyGuiManager.MouseCursorPosition.X > vector3.X) || (MyGuiManager.MouseCursorPosition.Y < vector2.Y))) || (MyGuiManager.MouseCursorPosition.Y > vector3.Y))
                        {
                            float* singlePtr1 = (float*) ref positionAbsoluteTopLeft.X;
                            singlePtr1[0] += this.TabButtonSize.X;
                            num++;
                            continue;
                        }
                        return key;
                    }
                }
            }
            return -1;
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder() => 
            ((MyObjectBuilder_GuiControlTabControl) base.GetObjectBuilder());

        public MyGuiControlTabPage GetTabSubControl(int key)
        {
            if (!this.m_pages.ContainsKey(key))
            {
                Vector2? position = new Vector2?(this.TabPosition);
                Vector2? size = new Vector2?(this.TabSize);
                Vector4? color = new Vector4?(base.ColorMask);
                MyGuiControlTabPage page1 = new MyGuiControlTabPage(key, position, size, color, 1f);
                page1.Visible = false;
                page1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_pages[key] = page1;
                base.Controls.Add(this.m_pages[key]);
            }
            return this.m_pages[key];
        }

        public override MyGuiControlBase HandleInput()
        {
            int mouseOverTab = this.GetMouseOverTab();
            if (((mouseOverTab == -1) || !this.GetTabSubControl(mouseOverTab).Enabled) || !MyInput.Static.IsNewPrimaryButtonPressed())
            {
                return base.HandleInput();
            }
            MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
            this.SelectedPage = mouseOverTab;
            return this;
        }

        private void HideTabs()
        {
            foreach (KeyValuePair<int, MyGuiControlTabPage> pair in this.m_pages)
            {
                pair.Value.Visible = false;
            }
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiControlTabControl control1 = (MyObjectBuilder_GuiControlTabControl) builder;
            this.RecreatePages();
            this.HideTabs();
            this.SelectedPage = 0;
        }

        public void MoveToNextTab()
        {
            if (this.m_pages.Count != 0)
            {
                int selectedPage = this.SelectedPage;
                int key = 0x7fffffff;
                int num3 = 0x7fffffff;
                foreach (KeyValuePair<int, MyGuiControlTabPage> pair in this.m_pages)
                {
                    num3 = Math.Min(num3, pair.Key);
                    if ((pair.Key > selectedPage) && (pair.Key < key))
                    {
                        key = pair.Key;
                    }
                }
                this.SelectedPage = (key != 0x7fffffff) ? key : num3;
            }
        }

        public void MoveToPreviousTab()
        {
            if (this.m_pages.Count != 0)
            {
                int selectedPage = this.SelectedPage;
                int key = -2147483648;
                int num3 = -2147483648;
                foreach (KeyValuePair<int, MyGuiControlTabPage> pair in this.m_pages)
                {
                    num3 = Math.Max(num3, pair.Key);
                    if ((pair.Key < selectedPage) && (pair.Key > key))
                    {
                        key = pair.Key;
                    }
                }
                this.SelectedPage = (key != -2147483648) ? key : num3;
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshInternals();
        }

        public void RecreatePages()
        {
            this.m_pages.Clear();
            foreach (MyGuiControlTabPage page in base.Controls)
            {
                page.Visible = false;
                this.m_pages.Add(page.PageKey, page);
            }
            this.RefreshInternals();
        }

        private void RefreshInternals()
        {
            Vector2 vector = MyGuiConstants.TEXTURE_BUTTON_DEFAULT_NORMAL.MinSizeGui * this.m_tabButtonScale;
            this.TabButtonSize = new Vector2(Math.Min(base.Size.X / ((float) this.m_pages.Count), vector.X), vector.Y);
            this.TabPosition = (base.Size * -0.5f) + new Vector2(0f, this.TabButtonSize.Y);
            this.TabSize = base.Size - new Vector2(0f, this.TabButtonSize.Y);
            this.RefreshPageParameters();
        }

        private void RefreshPageParameters()
        {
            foreach (MyGuiControlTabPage local1 in this.m_pages.Values)
            {
                local1.Position = this.TabPosition;
                local1.Size = this.TabSize;
                local1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            }
        }

        public override void ShowToolTip()
        {
            int mouseOverTab = this.GetMouseOverTab();
            using (Dictionary<int, MyGuiControlTabPage>.Enumerator enumerator = this.m_pages.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<int, MyGuiControlTabPage> current = enumerator.Current;
                    if ((current.Key == mouseOverTab) && (current.Value.m_toolTip != null))
                    {
                        current.Value.m_toolTip.Draw(MyGuiManager.MouseCursorPosition);
                        return;
                    }
                }
            }
            base.ShowToolTip();
        }

        public int SelectedPage
        {
            get => 
                this.m_selectedPage;
            set
            {
                if (this.m_pages.ContainsKey(this.m_selectedPage))
                {
                    this.m_pages[this.m_selectedPage].Visible = false;
                }
                this.m_selectedPage = value;
                if (this.OnPageChanged != null)
                {
                    this.OnPageChanged();
                }
                if (this.m_pages.ContainsKey(this.m_selectedPage))
                {
                    this.m_pages[this.m_selectedPage].Visible = true;
                }
            }
        }

        public Vector2 TabButtonSize
        {
            get => 
                this.m_tabButtonSize;
            private set => 
                (this.m_tabButtonSize = value);
        }

        public float TabButtonScale
        {
            get => 
                this.m_tabButtonScale;
            set
            {
                this.m_tabButtonScale = value;
                this.RefreshInternals();
            }
        }

        public Vector2 TabPosition { get; private set; }

        public Vector2 TabSize { get; private set; }
    }
}

