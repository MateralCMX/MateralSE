namespace Sandbox.Game.Screens
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Input;
    using VRageMath;

    public class MyGuiScreenHighlight : MyGuiScreenBase
    {
        private uint m_closeInFrames;
        private readonly MyGuiControls m_highlightedControls;
        private readonly MyHighlightControl[] m_highlightedControlsData;
        private static readonly Vector2 HIGHLIGHT_TEXTURE_SIZE = new Vector2(MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.LeftCenter.SizeGui.X + MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.RightCenter.SizeGui.X, MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.CenterTop.SizeGui.Y + MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.CenterBottom.SizeGui.Y);
        private static readonly Vector2 HIGHLIGHT_TEXTURE_OFFSET = new Vector2(MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.LeftCenter.SizeGui.X, MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.CenterTop.SizeGui.Y);

        private MyGuiScreenHighlight(MyHighlightControl[] controlsData) : base(new Vector2?(Vector2.Zero), nullable, new Vector2?(Vector2.One * 2.5f), false, null, 0f, 0f)
        {
            this.m_closeInFrames = uint.MaxValue;
            this.m_highlightedControlsData = controlsData;
            this.m_highlightedControls = new MyGuiControls(this);
            foreach (MyHighlightControl control in this.m_highlightedControlsData)
            {
                if (control.CustomToolTips != null)
                {
                    control.CustomToolTips.Highlight = true;
                    Color? color = control.Color;
                    control.CustomToolTips.HighlightColor = (color != null) ? ((Vector4) color.GetValueOrDefault()) : ((Vector4) Color.Yellow);
                }
                this.m_highlightedControls.AddWeak(control.Control);
            }
            base.m_backgroundColor = new Vector4?((Vector4.One * 0.86f).ToSRGB());
            base.m_backgroundFadeColor = (Vector4.One * 0.86f).ToSRGB();
            base.CanBeHidden = false;
            base.CanHaveFocus = true;
            base.m_canShareInput = false;
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = true;
            base.DrawMouseCursor = true;
            base.CloseButtonEnabled = false;
        }

        public override bool CloseScreen()
        {
            this.m_highlightedControls.ClearWeaks();
            return base.CloseScreen();
        }

        public override unsafe bool Draw()
        {
            int num;
            MyHighlightControl[] highlightedControlsData = this.m_highlightedControlsData;
            for (num = 0; num < highlightedControlsData.Length; num++)
            {
                MyHighlightControl control = highlightedControlsData[num];
                MyGuiControlGrid grid = control.Control as MyGuiControlGrid;
                if (grid != null)
                {
                    if (grid.ModalItems == null)
                    {
                        grid.ModalItems = new Dictionary<int, Color>();
                    }
                    else
                    {
                        grid.ModalItems.Clear();
                    }
                    if (control.Indices != null)
                    {
                        foreach (int num3 in control.Indices)
                        {
                            Color yellow;
                            if (control.Color == null)
                            {
                                yellow = Color.Yellow;
                            }
                            else
                            {
                                yellow = control.Color.Value;
                            }
                            grid.ModalItems.Add(num3, yellow);
                        }
                    }
                }
            }
            base.Draw();
            highlightedControlsData = this.m_highlightedControlsData;
            for (num = 0; num < highlightedControlsData.Length; num++)
            {
                MyHighlightControl control1 = highlightedControlsData[num];
                MyGuiControlGrid control = control1.Control as MyGuiControlGrid;
                if ((control != null) && (control.ModalItems != null))
                {
                    control.ModalItems.Clear();
                }
                foreach (MyGuiControlGrid grid3 in control1.Control.Elements)
                {
                    if (grid3 == null)
                    {
                        continue;
                    }
                    if (grid3.ModalItems != null)
                    {
                        grid3.ModalItems.Clear();
                    }
                }
            }
            foreach (MyHighlightControl control2 in this.m_highlightedControlsData)
            {
                if ((base.State == MyGuiScreenState.OPENED) && (control2.CustomToolTips != null))
                {
                    Vector2 positionAbsoluteTopRight = control2.Control.GetPositionAbsoluteTopRight();
                    float* singlePtr1 = (float*) ref positionAbsoluteTopRight.Y;
                    singlePtr1[0] -= control2.CustomToolTips.Size.Y + 0.045f;
                    float* singlePtr2 = (float*) ref positionAbsoluteTopRight.X;
                    singlePtr2[0] -= 0.01f;
                    control2.CustomToolTips.Draw(positionAbsoluteTopRight);
                }
                if (!(control2.Control is MyGuiControlGrid) && !(control2.Control is MyGuiControlGridDragAndDrop))
                {
                    Color yellow;
                    Vector2 size = control2.Control.Size + HIGHLIGHT_TEXTURE_SIZE;
                    Vector2 positionLeftTop = control2.Control.GetPositionAbsoluteTopLeft() - HIGHLIGHT_TEXTURE_OFFSET;
                    if (control2.Color == null)
                    {
                        yellow = Color.Yellow;
                    }
                    else
                    {
                        yellow = control2.Color.Value;
                    }
                    Color colorMask = yellow;
                    Color* colorPtr1 = (Color*) ref colorMask;
                    colorPtr1.A = (byte) (colorMask.A * base.m_transitionAlpha);
                    MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.Draw(positionLeftTop, size, colorMask, 1f);
                    control2.Control.Draw(base.m_transitionAlpha, base.m_backgroundTransition);
                }
            }
            return true;
        }

        public override string GetFriendlyName() => 
            "HighlightScreen";

        public override int GetTransitionClosingTime() => 
            500;

        public override int GetTransitionOpeningTime() => 
            500;

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            this.UniversalInputHandling();
            foreach (MyGuiControlBase base2 in this.m_highlightedControls)
            {
                base2.IsMouseOver = MyGuiControlBase.CheckMouseOver(base2.Size, base2.GetPositionAbsolute(), base2.OriginAlign);
                MyGuiControlBase owner = base2.Owner as MyGuiControlBase;
                while (true)
                {
                    if (owner == null)
                    {
                        if (((this.m_closeInFrames == uint.MaxValue) && base2.IsMouseOver) && MyInput.Static.IsNewLeftMousePressed())
                        {
                            this.m_closeInFrames = 10;
                        }
                        break;
                    }
                    owner.IsMouseOver = MyGuiControlBase.CheckMouseOver(owner.Size, owner.GetPositionAbsolute(), owner.OriginAlign);
                    owner = owner.Owner as MyGuiControlBase;
                }
            }
            base.HandleInput(receivedFocusInThisUpdate);
            if (this.m_closeInFrames == 0)
            {
                this.CloseScreen();
            }
            else if (this.m_closeInFrames < uint.MaxValue)
            {
                this.m_closeInFrames--;
            }
        }

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            this.UniversalInputHandling();
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
        }

        public static void HighlightControl(MyHighlightControl control)
        {
            MyHighlightControl[] controlsData = new MyHighlightControl[] { control };
            HighlightControls(controlsData);
        }

        public static void HighlightControls(MyHighlightControl[] controlsData)
        {
            MyScreenManager.AddScreen(new MyGuiScreenHighlight(controlsData));
        }

        private void UniversalInputHandling()
        {
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                this.CloseScreen();
            }
        }

        public override MyGuiControls Controls =>
            this.m_highlightedControls;

        [StructLayout(LayoutKind.Sequential)]
        public struct MyHighlightControl
        {
            public MyGuiControlBase Control;
            public int[] Indices;
            public VRageMath.Color? Color;
            public MyToolTips CustomToolTips;
        }
    }
}

