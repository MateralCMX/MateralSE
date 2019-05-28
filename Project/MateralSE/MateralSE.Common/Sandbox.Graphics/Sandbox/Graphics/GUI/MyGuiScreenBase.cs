namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI.IME;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenBase : IMyGuiControlsParent, IMyGuiControlsOwner
    {
        protected Action OnEnterCallback;
        public Action OnLoadingAction;
        [CompilerGenerated]
        private static Action<string, Vector2, uint> MouseClickEvent;
        [CompilerGenerated]
        private ScreenHandler Closed;
        [CompilerGenerated]
        private VisibleChangedDelegate VisibleChanged;
        [CompilerGenerated]
        private Action<MyGuiScreenBase> DataLoading;
        [CompilerGenerated]
        private Action<MyGuiScreenBase> DataUnloading;
        protected float m_transitionAlpha;
        protected float m_backgroundTransition;
        protected float m_guiTransition;
        private MyGuiControls m_controls;
        protected Vector2 m_position;
        protected Color m_backgroundFadeColor;
        private int m_transitionFrameIndex;
        public static bool EnableSlowTransitionAnimations;
        protected bool m_isTopMostScreen;
        protected bool m_isAlwaysFirst;
        protected bool m_isTopScreen;
        protected bool m_isFirstForUnload;
        protected Vector4? m_backgroundColor;
        protected string m_backgroundTexture;
        protected bool m_canCloseInCloseAllScreenCalls;
        protected Vector2? m_size;
        protected bool m_closeOnEsc;
        private bool m_drawMouseCursor;
        protected bool m_joystickAsMouse;
        protected bool m_defaultJoystickDpadUse;
        protected bool m_defaultJoystickCancelUse;
        protected int m_lastTransitionTime;
        private bool m_isLoaded;
        private object m_isLoadedLock;
        private bool m_firstUpdateServed;
        protected bool m_drawEvenWithoutFocus;
        protected bool m_canShareInput;
        protected bool m_allowUnhidePreviousScreen;
        protected GuiSounds? m_openingCueEnum;
        protected GuiSounds? m_closingCueEnum;
        private MyGuiControlBase m_draggingControl;
        private Vector2 m_draggingControlOffset;
        private StringBuilder m_drawPositionSb;
        protected MyGuiControlGridDragAndDrop m_gridDragAndDropHandlingNow;
        protected MyGuiControlBase m_comboboxHandlingNow;
        protected MyGuiControlBase m_lastHandlingControl;
        private DateTime m_screenCreation;
        private bool m_useAnalytics;
        private MyGuiControlButton m_closeButton;
        public readonly MyGuiControls Elements;
        public bool IsHitTestVisible;
        private MyGuiScreenState m_state;
        private bool m_enabledBackgroundFade;
        private bool m_canBeHidden;
        private bool m_canHideOthers;
        private bool m_canHaveFocus;
        private Vector2 m_closeButtonOffset;
        private bool m_closeButtonEnabled;
        private MyGuiControlButtonStyleEnum m_closeButtonStyle;
        private MyGuiControlBase m_focusedControl;

        public event ScreenHandler Closed
        {
            [CompilerGenerated] add
            {
                ScreenHandler closed = this.Closed;
                while (true)
                {
                    ScreenHandler a = closed;
                    ScreenHandler handler3 = (ScreenHandler) Delegate.Combine(a, value);
                    closed = Interlocked.CompareExchange<ScreenHandler>(ref this.Closed, handler3, a);
                    if (ReferenceEquals(closed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ScreenHandler closed = this.Closed;
                while (true)
                {
                    ScreenHandler source = closed;
                    ScreenHandler handler3 = (ScreenHandler) Delegate.Remove(source, value);
                    closed = Interlocked.CompareExchange<ScreenHandler>(ref this.Closed, handler3, source);
                    if (ReferenceEquals(closed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiScreenBase> DataLoading
        {
            [CompilerGenerated] add
            {
                Action<MyGuiScreenBase> dataLoading = this.DataLoading;
                while (true)
                {
                    Action<MyGuiScreenBase> a = dataLoading;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Combine(a, value);
                    dataLoading = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref this.DataLoading, action3, a);
                    if (ReferenceEquals(dataLoading, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiScreenBase> dataLoading = this.DataLoading;
                while (true)
                {
                    Action<MyGuiScreenBase> source = dataLoading;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Remove(source, value);
                    dataLoading = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref this.DataLoading, action3, source);
                    if (ReferenceEquals(dataLoading, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiScreenBase> DataUnloading
        {
            [CompilerGenerated] add
            {
                Action<MyGuiScreenBase> dataUnloading = this.DataUnloading;
                while (true)
                {
                    Action<MyGuiScreenBase> a = dataUnloading;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Combine(a, value);
                    dataUnloading = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref this.DataUnloading, action3, a);
                    if (ReferenceEquals(dataUnloading, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiScreenBase> dataUnloading = this.DataUnloading;
                while (true)
                {
                    Action<MyGuiScreenBase> source = dataUnloading;
                    Action<MyGuiScreenBase> action3 = (Action<MyGuiScreenBase>) Delegate.Remove(source, value);
                    dataUnloading = Interlocked.CompareExchange<Action<MyGuiScreenBase>>(ref this.DataUnloading, action3, source);
                    if (ReferenceEquals(dataUnloading, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<string, Vector2, uint> MouseClickEvent
        {
            [CompilerGenerated] add
            {
                Action<string, Vector2, uint> mouseClickEvent = MouseClickEvent;
                while (true)
                {
                    Action<string, Vector2, uint> a = mouseClickEvent;
                    Action<string, Vector2, uint> action3 = (Action<string, Vector2, uint>) Delegate.Combine(a, value);
                    mouseClickEvent = Interlocked.CompareExchange<Action<string, Vector2, uint>>(ref MouseClickEvent, action3, a);
                    if (ReferenceEquals(mouseClickEvent, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<string, Vector2, uint> mouseClickEvent = MouseClickEvent;
                while (true)
                {
                    Action<string, Vector2, uint> source = mouseClickEvent;
                    Action<string, Vector2, uint> action3 = (Action<string, Vector2, uint>) Delegate.Remove(source, value);
                    mouseClickEvent = Interlocked.CompareExchange<Action<string, Vector2, uint>>(ref MouseClickEvent, action3, source);
                    if (ReferenceEquals(mouseClickEvent, source))
                    {
                        return;
                    }
                }
            }
        }

        public event VisibleChangedDelegate VisibleChanged
        {
            [CompilerGenerated] add
            {
                VisibleChangedDelegate visibleChanged = this.VisibleChanged;
                while (true)
                {
                    VisibleChangedDelegate a = visibleChanged;
                    VisibleChangedDelegate delegate4 = (VisibleChangedDelegate) Delegate.Combine(a, value);
                    visibleChanged = Interlocked.CompareExchange<VisibleChangedDelegate>(ref this.VisibleChanged, delegate4, a);
                    if (ReferenceEquals(visibleChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                VisibleChangedDelegate visibleChanged = this.VisibleChanged;
                while (true)
                {
                    VisibleChangedDelegate source = visibleChanged;
                    VisibleChangedDelegate delegate4 = (VisibleChangedDelegate) Delegate.Remove(source, value);
                    visibleChanged = Interlocked.CompareExchange<VisibleChangedDelegate>(ref this.VisibleChanged, delegate4, source);
                    if (ReferenceEquals(visibleChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        private MyGuiScreenBase()
        {
            this.m_canCloseInCloseAllScreenCalls = true;
            this.m_closeOnEsc = true;
            this.m_drawMouseCursor = true;
            this.m_joystickAsMouse = true;
            this.m_defaultJoystickDpadUse = true;
            this.m_defaultJoystickCancelUse = true;
            this.m_isLoadedLock = new object();
            this.m_drawPositionSb = new StringBuilder();
            this.m_screenCreation = DateTime.UtcNow;
            this.IsHitTestVisible = true;
            this.m_canBeHidden = true;
            this.m_canHideOthers = true;
            this.m_canHaveFocus = true;
            this.m_closeButtonStyle = MyGuiControlButtonStyleEnum.Close;
        }

        protected MyGuiScreenBase(Vector2? position = new Vector2?(), Vector4? backgroundColor = new Vector4?(), Vector2? size = new Vector2?(), bool isTopMostScreen = false, string backgroundTexture = null, float backgroundTransition = 0f, float guiTransition = 0f)
        {
            this.m_canCloseInCloseAllScreenCalls = true;
            this.m_closeOnEsc = true;
            this.m_drawMouseCursor = true;
            this.m_joystickAsMouse = true;
            this.m_defaultJoystickDpadUse = true;
            this.m_defaultJoystickCancelUse = true;
            this.m_isLoadedLock = new object();
            this.m_drawPositionSb = new StringBuilder();
            this.m_screenCreation = DateTime.UtcNow;
            this.IsHitTestVisible = true;
            this.m_canBeHidden = true;
            this.m_canHideOthers = true;
            this.m_canHaveFocus = true;
            this.m_closeButtonStyle = MyGuiControlButtonStyleEnum.Close;
            this.m_controls = new MyGuiControls(this);
            this.m_backgroundFadeColor = Color.White;
            this.m_backgroundColor = backgroundColor;
            this.m_size = size;
            this.m_isTopMostScreen = isTopMostScreen;
            this.m_allowUnhidePreviousScreen = true;
            this.State = MyGuiScreenState.OPENING;
            this.m_lastTransitionTime = MyGuiManager.TotalTimeInMilliseconds;
            Vector2? nullable = position;
            this.m_position = (nullable != null) ? nullable.GetValueOrDefault() : new Vector2(0.5f, 0.5f);
            this.m_useAnalytics = this.RegisterClicks();
            this.m_backgroundTexture = backgroundTexture;
            this.Elements = new MyGuiControls(this);
            this.m_backgroundTransition = backgroundTransition;
            this.m_guiTransition = guiTransition;
            this.SetDefaultCloseButtonOffset();
            this.CreateCloseButton();
            this.Align = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
        }

        protected MyGuiControlLabel AddCaption(string text, Vector4? captionTextColor = new Vector4?(), Vector2? captionOffset = new Vector2?(), float captionScale = 0.8f)
        {
            Vector2? size = null;
            Vector4? nullable2 = captionTextColor;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(new Vector2(0f, (-this.m_size.Value.Y / 2f) + MyGuiConstants.SCREEN_CAPTION_DELTA_Y) + ((captionOffset != null) ? captionOffset.Value : Vector2.Zero)), size, text, new Vector4?((nullable2 != null) ? nullable2.GetValueOrDefault() : Vector4.One), captionScale, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                Name = "CaptionLabel",
                Font = "ScreenCaption"
            };
            this.Elements.Add(control);
            return control;
        }

        protected MyGuiControlLabel AddCaption(MyStringId textEnum, Vector4? captionTextColor = new Vector4?(), Vector2? captionOffset = new Vector2?(), float captionScale = 0.8f) => 
            this.AddCaption(MyTexts.GetString(textEnum), captionTextColor, captionOffset, captionScale);

        protected unsafe Color ApplyTransitionAlpha(Vector4 color, float transition)
        {
            Vector4 vector = color;
            float* singlePtr1 = (float*) ref vector.W;
            singlePtr1[0] *= transition;
            return new Color(vector);
        }

        private Vector2 CalcCloseButtonPosition()
        {
            Vector2? size;
            if (this.Align == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
            {
                size = this.Size;
                return ((((size != null) ? size.GetValueOrDefault() : Vector2.One) * new Vector2(1f, 0f)) + this.CloseButtonOffset);
            }
            size = this.Size;
            return ((((size != null) ? size.GetValueOrDefault() : Vector2.One) * new Vector2(0.5f, -0.5f)) + this.CloseButtonOffset);
        }

        protected virtual void Canceling()
        {
            this.Cancelled = true;
            if (this.m_closingCueEnum != null)
            {
                MyGuiSoundManager.PlaySound(this.m_closingCueEnum.Value);
            }
            else
            {
                MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
            }
            this.CloseScreen();
        }

        public bool CanCloseInCloseAllScreenCalls() => 
            this.m_canCloseInCloseAllScreenCalls;

        internal static bool CanHaveFocusRightNow(MyGuiControlBase control) => 
            (control.Enabled && (control.Visible && control.CanHaveFocus));

        public bool CanShareInput() => 
            this.m_canShareInput;

        private void closeButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.Canceling();
        }

        public virtual bool CloseScreen()
        {
            if ((this.State == MyGuiScreenState.CLOSING) || (this.State == MyGuiScreenState.CLOSED))
            {
                return false;
            }
            this.State = MyGuiScreenState.CLOSING;
            this.m_lastTransitionTime = MyGuiManager.TotalTimeInMilliseconds;
            return true;
        }

        public virtual void CloseScreenNow()
        {
            if (this.State != MyGuiScreenState.CLOSED)
            {
                this.State = MyGuiScreenState.CLOSED;
                this.OnClosed();
                if (this.Closed != null)
                {
                    this.Closed(this);
                    this.Closed = null;
                }
            }
        }

        private void CreateCloseButton()
        {
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Name = "CloseButton";
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button1.VisualStyle = this.m_closeButtonStyle;
            button1.TextScale = 0f;
            button1.Position = this.CalcCloseButtonPosition();
            button1.Visible = this.CloseButtonEnabled;
            this.m_closeButton = button1;
            this.m_closeButton.ButtonClicked += new Action<MyGuiControlButton>(this.closeButton_OnButtonClick);
            this.Elements.Add(this.m_closeButton);
        }

        public virtual bool Draw()
        {
            if ((this.m_backgroundColor != null) && (this.m_size != null))
            {
                if ((this.m_backgroundTexture == null) && (this.m_size != null))
                {
                    this.m_backgroundTexture = MyGuiManager.GetBackgroundTextureFilenameByAspectRatio(this.m_size.Value);
                }
                MyGuiManager.DrawSpriteBatch(this.m_backgroundTexture, this.m_position, this.m_size.Value, this.ApplyTransitionAlpha(this.m_backgroundColor.Value, (this.m_guiTransition != 0f) ? (this.m_backgroundTransition * this.m_transitionAlpha) : this.m_transitionAlpha), this.Align, false, true);
            }
            if (this.m_guiTransition == 0f)
            {
                this.DrawElements(this.m_transitionAlpha, this.m_transitionAlpha);
                this.DrawControls(this.m_transitionAlpha, this.m_transitionAlpha);
            }
            else
            {
                float transitionAlpha = this.m_guiTransition * this.m_transitionAlpha;
                float backgroundTransitionAlpha = this.m_backgroundTransition * this.m_transitionAlpha;
                this.DrawElements(transitionAlpha, backgroundTransitionAlpha);
                this.DrawControls(transitionAlpha, backgroundTransitionAlpha);
            }
            return true;
        }

        private void DrawControls(float transitionAlpha, float backgroundTransitionAlpha)
        {
            List<MyGuiControlBase> visibleControls = this.Controls.GetVisibleControls();
            for (int i = 0; i < visibleControls.Count; i++)
            {
                MyGuiControlBase objA = visibleControls[i];
                if ((!ReferenceEquals(objA, this.m_comboboxHandlingNow) && !ReferenceEquals(objA, this.m_gridDragAndDropHandlingNow)) && !(objA is MyGuiControlGridDragAndDrop))
                {
                    objA.Draw(transitionAlpha * objA.Alpha, backgroundTransitionAlpha * objA.Alpha);
                }
            }
            if (this.m_comboboxHandlingNow != null)
            {
                this.m_comboboxHandlingNow.Draw(transitionAlpha * this.m_comboboxHandlingNow.Alpha, backgroundTransitionAlpha * this.m_comboboxHandlingNow.Alpha);
            }
            if (this.m_gridDragAndDropHandlingNow != null)
            {
                this.m_gridDragAndDropHandlingNow.Draw(transitionAlpha * this.m_gridDragAndDropHandlingNow.Alpha, backgroundTransitionAlpha * this.m_gridDragAndDropHandlingNow.Alpha);
            }
        }

        private void DrawElements(float transitionAlpha, float backgroundTransitionAlpha)
        {
            foreach (MyGuiControlBase base2 in this.Elements)
            {
                if (base2.Visible)
                {
                    base2.Draw(transitionAlpha * base2.Alpha, backgroundTransitionAlpha * base2.Alpha);
                }
            }
        }

        public virtual void GetControlsUnderMouseCursor(Vector2 position, List<MyGuiControlBase> controls, bool visibleOnly)
        {
            GetControlsUnderMouseCursor(this, position, controls, visibleOnly);
        }

        private static void GetControlsUnderMouseCursor(IMyGuiControlsParent parent, Vector2 position, List<MyGuiControlBase> controls, bool visibleOnly)
        {
            if (visibleOnly)
            {
                foreach (MyGuiControlBase base2 in parent.Controls.GetVisibleControls())
                {
                    if (!IsControlUnderCursor(position, base2))
                    {
                        continue;
                    }
                    controls.Add(base2);
                    base2.GetElementsUnderCursor(position, visibleOnly, controls);
                    IMyGuiControlsParent parent2 = base2 as IMyGuiControlsParent;
                    if (parent2 != null)
                    {
                        GetControlsUnderMouseCursor(parent2, position, controls, visibleOnly);
                    }
                }
            }
            else
            {
                foreach (MyGuiControlBase base3 in parent.Controls)
                {
                    if (!IsControlUnderCursor(position, base3))
                    {
                        continue;
                    }
                    controls.Add(base3);
                    base3.GetElementsUnderCursor(position, visibleOnly, controls);
                    IMyGuiControlsParent parent3 = base3 as IMyGuiControlsParent;
                    if (parent3 != null)
                    {
                        GetControlsUnderMouseCursor(parent3, position, controls, visibleOnly);
                    }
                }
            }
        }

        private MyGuiControlGridDragAndDrop GetDragAndDropHandlingNow()
        {
            for (int i = 0; i < this.Controls.GetVisibleControls().Count; i++)
            {
                MyGuiControlGridDragAndDrop dragAndDropHandlingNow = this.Controls.GetVisibleControls()[i].GetDragAndDropHandlingNow();
                if (dragAndDropHandlingNow != null)
                {
                    return dragAndDropHandlingNow;
                }
            }
            return null;
        }

        public bool GetDrawMouseCursor() => 
            this.m_drawMouseCursor;

        public bool GetDrawScreenEvenWithoutFocus() => 
            this.m_drawEvenWithoutFocus;

        private MyGuiControlBase GetExclusiveInputHandler()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGuiControlBase exclusiveInputHandler = enumerator.Current.GetExclusiveInputHandler();
                    if (exclusiveInputHandler != null)
                    {
                        return exclusiveInputHandler;
                    }
                }
            }
            return null;
        }

        private MyGuiControlBase GetFirstFocusableControl()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGuiControlBase current = enumerator.Current;
                    if (CanHaveFocusRightNow(current))
                    {
                        return current;
                    }
                }
            }
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator2 = this.Elements.GetEnumerator())
            {
                MyGuiControlBase base3;
                while (true)
                {
                    if (enumerator2.MoveNext())
                    {
                        MyGuiControlBase current = enumerator2.Current;
                        if (!CanHaveFocusRightNow(current))
                        {
                            continue;
                        }
                        base3 = current;
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return base3;
            }
        TR_0000:
            return null;
        }

        public abstract string GetFriendlyName();
        public MyGuiControlBase GetMouseOverControl()
        {
            for (int i = this.Controls.GetVisibleControls().Count - 1; i >= 0; i--)
            {
                MyGuiControlBase mouseOverControl = this.Controls.GetVisibleControls()[i].GetMouseOverControl();
                if (mouseOverControl != null)
                {
                    return mouseOverControl;
                }
            }
            return null;
        }

        public MyGuiControlBase GetNextFocusControl(MyGuiControlBase currentFocusControl, bool forwardMovement)
        {
            int num2;
            List<MyGuiControlBase> visibleControls = this.Controls.GetVisibleControls();
            int num = visibleControls.Count + this.Elements.Count;
            if (visibleControls.IndexOf(currentFocusControl) == -1)
            {
                num2 = this.Elements.IndexOf(currentFocusControl) + visibleControls.Count;
            }
            int num3 = forwardMovement ? 1 : -1;
            for (int i = 1; i < num; i++)
            {
                int num5 = num2 + (num3 * i);
                if (forwardMovement)
                {
                    num5 = num5 % num;
                }
                else if (num5 < 0)
                {
                    num5 += num;
                }
                if (num5 < visibleControls.Count)
                {
                    if (CanHaveFocusRightNow(visibleControls[num5]))
                    {
                        return (!(visibleControls[num5] is MyGuiControlParent) ? visibleControls[num5] : visibleControls[num5].GetFocusControl(forwardMovement));
                    }
                }
                else
                {
                    num5 -= visibleControls.Count;
                    if (CanHaveFocusRightNow(this.Elements[num5]) && !ReferenceEquals(this.Elements[num5], this.m_closeButton))
                    {
                        return this.Elements[num5];
                    }
                }
            }
            return null;
        }

        public MyObjectBuilder_GuiScreen GetObjectBuilder()
        {
            MyObjectBuilder_GuiScreen local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_GuiScreen>();
            local1.Controls = this.Controls.GetObjectBuilder();
            local1.BackgroundColor = this.m_backgroundColor;
            local1.BackgroundTexture = this.m_backgroundTexture;
            local1.Size = this.m_size;
            local1.CloseButtonEnabled = this.CloseButtonEnabled;
            local1.CloseButtonOffset = this.CloseButtonOffset;
            return local1;
        }

        public Vector2 GetPosition() => 
            this.m_position;

        public Vector2 GetPositionAbsolute() => 
            this.m_position;

        public Vector2 GetPositionAbsoluteCenter() => 
            this.GetPositionAbsolute();

        public Vector2 GetPositionAbsoluteTopLeft()
        {
            if (this.Size != null)
            {
                return (this.GetPositionAbsolute() - (this.Size.Value * 0.5f));
            }
            return this.GetPositionAbsolute();
        }

        public Vector2? GetSize() => 
            this.m_size;

        public virtual int GetTransitionClosingTime() => 
            (!this.SkipTransition ? 200 : 0);

        public virtual int GetTransitionOpeningTime() => 
            (!this.SkipTransition ? 200 : 0);

        private bool HandleControlsInput(bool receivedFocusInThisUpdate)
        {
            MyGuiControlBase lastHandlingControl = null;
            if (((this.m_lastHandlingControl != null) && this.m_lastHandlingControl.Visible) && (this.m_lastHandlingControl.HandleInput() != null))
            {
                lastHandlingControl = this.m_lastHandlingControl;
            }
            if (((lastHandlingControl == null) && (this.m_gridDragAndDropHandlingNow != null)) && (this.m_gridDragAndDropHandlingNow.HandleInput() != null))
            {
                lastHandlingControl = this.m_gridDragAndDropHandlingNow;
            }
            if (((lastHandlingControl == null) && (this.m_comboboxHandlingNow != null)) && (this.m_comboboxHandlingNow.HandleInput() != null))
            {
                lastHandlingControl = this.m_comboboxHandlingNow;
            }
            MyGuiControlBase objB = null;
            if (lastHandlingControl == null)
            {
                List<MyGuiControlBase> visibleControls = this.Controls.GetVisibleControls();
                for (int i = 0; i < visibleControls.Count; i++)
                {
                    MyGuiControlBase objA = visibleControls[i];
                    if ((!ReferenceEquals(objA, this.m_comboboxHandlingNow) && !ReferenceEquals(objA, this.m_gridDragAndDropHandlingNow)) && objA.CheckMouseOver())
                    {
                        objB = objA;
                        lastHandlingControl = objA.HandleInput();
                        break;
                    }
                }
            }
            if (lastHandlingControl == null)
            {
                List<MyGuiControlBase> visibleControls = this.Controls.GetVisibleControls();
                for (int i = visibleControls.Count - 1; i >= 0; i--)
                {
                    MyGuiControlBase objA = visibleControls[i];
                    if ((!ReferenceEquals(objA, this.m_comboboxHandlingNow) && !ReferenceEquals(objA, this.m_gridDragAndDropHandlingNow)) && !ReferenceEquals(objA, objB))
                    {
                        lastHandlingControl = objA.HandleInput();
                        if (lastHandlingControl != null)
                        {
                            break;
                        }
                    }
                }
            }
            if (lastHandlingControl == null)
            {
                foreach (MyGuiControlBase base6 in this.Elements)
                {
                    if (!base6.Visible)
                    {
                        continue;
                    }
                    if (base6.CanHaveFocus)
                    {
                        lastHandlingControl = base6.HandleInput();
                        if (lastHandlingControl != null)
                        {
                            break;
                        }
                    }
                }
            }
            if (lastHandlingControl != null)
            {
                this.FocusedControl = lastHandlingControl;
            }
            this.m_lastHandlingControl = lastHandlingControl;
            return (lastHandlingControl != null);
        }

        public virtual void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (this.IsLoaded && (this.State == MyGuiScreenState.OPENED))
            {
                bool firstUpdateServed = this.m_firstUpdateServed;
                if (!this.m_firstUpdateServed && (this.FocusedControl == null))
                {
                    if (MyImeProcessor.Instance != null)
                    {
                        MyImeProcessor.Instance.RegisterActiveScreen(this);
                    }
                    this.FocusedControl = this.GetFirstFocusableControl();
                    this.m_firstUpdateServed = true;
                }
                if (!this.HandleControlsInput(receivedFocusInThisUpdate))
                {
                    bool flag = false;
                    if (((MyInput.Static.IsKeyPress(MyKeys.LeftShift) && MyInput.Static.IsNewKeyPressed(MyKeys.Tab)) || (MyInput.Static.IsNewKeyPressed(MyKeys.Up) || (MyInput.Static.IsNewKeyPressed(MyKeys.Left) && !(this.FocusedControl is MyGuiControlSlider)))) || (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_UP, MyControlStateType.NEW_PRESSED, false) && this.m_defaultJoystickDpadUse))
                    {
                        flag = this.HandleKeyboardActiveIndex(false);
                    }
                    else if ((MyInput.Static.IsNewKeyPressed(MyKeys.Tab) || (MyInput.Static.IsNewKeyPressed(MyKeys.Down) || (MyInput.Static.IsNewKeyPressed(MyKeys.Right) && !(this.FocusedControl is MyGuiControlSlider)))) || (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_DOWN, MyControlStateType.NEW_PRESSED, false) && this.m_defaultJoystickDpadUse))
                    {
                        flag = this.HandleKeyboardActiveIndex(true);
                    }
                    else if ((this.m_closeOnEsc && ((MyInput.Static.IsNewKeyPressed(MyKeys.Escape) || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MAIN_MENU, MyControlStateType.NEW_PRESSED, false)) || (this.m_defaultJoystickCancelUse && MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.CANCEL, MyControlStateType.NEW_PRESSED, false)))) && ((MyImeProcessor.Instance == null) || !MyImeProcessor.Instance.IsComposing))
                    {
                        this.Canceling();
                    }
                    if (!flag)
                    {
                        this.HandleUnhandledInput(receivedFocusInThisUpdate);
                    }
                    else if ((this.m_defaultJoystickDpadUse && (this.FocusedControl != null)) && (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_UP, MyControlStateType.NEW_PRESSED, false) || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_DOWN, MyControlStateType.NEW_PRESSED, false)))
                    {
                        Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(this.FocusedControl.GetPositionAbsoluteCenter(), false);
                        MyInput.Static.SetMousePosition((int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y);
                    }
                }
            }
        }

        public virtual bool HandleInputAfterSimulation() => 
            false;

        protected bool HandleKeyboardActiveIndex(bool forwardMovement)
        {
            if (this.FocusedControl == null)
            {
                return false;
            }
            MyGuiControlBase focusControl = this.FocusedControl.GetFocusControl(forwardMovement);
            this.FocusedControl = focusControl;
            return true;
        }

        public virtual void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            if ((this.OnEnterCallback != null) && MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
            {
                this.OnEnterCallback();
            }
        }

        public virtual bool HideScreen()
        {
            if ((this.State == MyGuiScreenState.HIDING) || (this.State == MyGuiScreenState.HIDDEN))
            {
                return false;
            }
            this.State = MyGuiScreenState.HIDING;
            this.m_lastTransitionTime = MyGuiManager.TotalTimeInMilliseconds;
            return true;
        }

        public void HideTooltips()
        {
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.HideToolTip();
                }
            }
        }

        public void Init(MyObjectBuilder_GuiScreen objectBuilder)
        {
            this.m_backgroundColor = objectBuilder.BackgroundColor;
            this.m_backgroundTexture = objectBuilder.BackgroundTexture;
            this.m_size = objectBuilder.Size;
            this.Controls.Init(objectBuilder.Controls);
            this.CloseButtonOffset = objectBuilder.CloseButtonOffset;
            this.CloseButtonEnabled = objectBuilder.CloseButtonEnabled;
        }

        public virtual void InputLost()
        {
        }

        public bool IsAlwaysFirst() => 
            this.m_isAlwaysFirst;

        private static bool IsControlUnderCursor(Vector2 mousePosition, MyGuiControlBase control)
        {
            if (!control.IsHitTestVisible)
            {
                return false;
            }
            Vector2? size = control.GetSize();
            if (size == null)
            {
                return false;
            }
            Vector2 vector = MyUtils.GetCoordCenterFromAligned(control.GetPositionAbsolute(), size.Value, control.OriginAlign) - (size.Value / 2f);
            Vector2 vector2 = MyUtils.GetCoordCenterFromAligned(control.GetPositionAbsolute(), size.Value, control.OriginAlign) + (size.Value / 2f);
            return ((mousePosition.X >= vector.X) && ((mousePosition.X <= vector2.X) && ((mousePosition.Y >= vector.Y) && (mousePosition.Y <= vector2.Y))));
        }

        public bool IsFirstForUnload() => 
            this.m_isFirstForUnload;

        protected bool IsMouseOver()
        {
            if (!this.IsHitTestVisible)
            {
                return false;
            }
            Vector2 zero = Vector2.Zero;
            Vector2 vector2 = Vector2.Zero;
            Vector2 vector3 = ((this.Align == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) ? this.m_position : (this.m_position - (this.m_size.Value / 2f))) + zero;
            Vector2 vector4 = (((this.Align == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) ? this.m_position : (this.m_position - (this.m_size.Value / 2f))) + this.m_size.Value) - vector2;
            return ((MyGuiManager.MouseCursorPosition.X >= vector3.X) && ((MyGuiManager.MouseCursorPosition.X <= vector4.X) && ((MyGuiManager.MouseCursorPosition.Y >= vector3.Y) && (MyGuiManager.MouseCursorPosition.Y <= vector4.Y))));
        }

        public bool IsMouseOverAnyControl()
        {
            List<MyGuiControlBase> visibleControls = this.Controls.GetVisibleControls();
            for (int i = visibleControls.Count - 1; i >= 0; i--)
            {
                if (visibleControls[i].IsHitTestVisible && visibleControls[i].IsMouseOverAnyControl())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsTopMostScreen() => 
            this.m_isTopMostScreen;

        public bool IsTopScreen() => 
            this.m_isTopScreen;

        public virtual void LoadContent()
        {
            this.IsLoaded = true;
            this.m_lastTransitionTime = MyGuiManager.TotalTimeInMilliseconds;
        }

        public virtual void LoadData()
        {
            if (this.DataLoading != null)
            {
                this.DataLoading(this);
            }
        }

        protected static string MakeScreenFilepath(string name) => 
            Path.Combine("Data", "Screens", name + ".gsc");

        protected virtual void OnClosed()
        {
            this.Controls.Clear();
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OnRemoving();
                }
            }
            this.Elements.Clear();
        }

        protected virtual void OnHide()
        {
        }

        public virtual void OnRemoved()
        {
        }

        protected virtual void OnShow()
        {
        }

        public virtual void RecreateControls(bool constructor)
        {
            this.Controls.Clear();
            this.Elements.Clear();
            this.Elements.Add(this.m_closeButton);
            this.FocusedControl = null;
            this.m_firstUpdateServed = false;
            this.m_screenCreation = DateTime.UtcNow;
        }

        public virtual bool RegisterClicks() => 
            false;

        public virtual void RunLoadingAction()
        {
            if (this.OnLoadingAction != null)
            {
                this.OnLoadingAction();
            }
        }

        protected void SetCloseButtonOffset_5_to_4()
        {
            this.CloseButtonOffset = new Vector2(-0.05f, 0.015f);
        }

        protected void SetDefaultCloseButtonOffset()
        {
            this.CloseButtonOffset = new Vector2(-0.013f, 0.015f);
        }

        public virtual bool UnhideScreen()
        {
            if ((this.State == MyGuiScreenState.UNHIDING) || (this.State == MyGuiScreenState.OPENED))
            {
                return false;
            }
            this.State = MyGuiScreenState.UNHIDING;
            this.m_lastTransitionTime = MyGuiManager.TotalTimeInMilliseconds;
            return true;
        }

        public virtual void UnloadContent()
        {
            MyLog.Default.WriteLine("MyGuiScreenBase.UnloadContent - START");
            MyLog.Default.IncreaseIndent();
            this.IsLoaded = false;
            MyLog.Default.DecreaseIndent();
            MyLog.Default.WriteLine("MyGuiScreenBase.UnloadContent - END");
        }

        public virtual void UnloadData()
        {
            if (this.DataUnloading != null)
            {
                this.DataUnloading(this);
            }
        }

        public virtual bool Update(bool hasFocus)
        {
            if (((this.m_useAnalytics & hasFocus) && MyInput.Static.IsNewLeftMousePressed()) && (MouseClickEvent != null))
            {
                MouseClickEvent(this.Name, MyGuiManager.MouseCursorPosition, (uint) (DateTime.UtcNow - this.m_screenCreation).TotalSeconds);
            }
            if (!this.UpdateTransition())
            {
                return false;
            }
            this.UpdateControls();
            return true;
        }

        private void UpdateControls()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().ToList<MyGuiControlBase>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator2 = this.Elements.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.Update();
                }
            }
            this.m_comboboxHandlingNow = this.GetExclusiveInputHandler();
            this.m_gridDragAndDropHandlingNow = this.GetDragAndDropHandlingNow();
        }

        private bool UpdateTransition()
        {
            if (this.m_lastTransitionTime == 0)
            {
                this.m_lastTransitionTime = MyGuiManager.TotalTimeInMilliseconds;
            }
            if ((this.State != MyGuiScreenState.OPENING) && (this.State != MyGuiScreenState.UNHIDING))
            {
                if ((this.State == MyGuiScreenState.CLOSING) || (this.State == MyGuiScreenState.HIDING))
                {
                    int num5 = MyGuiManager.TotalTimeInMilliseconds - this.m_lastTransitionTime;
                    if (num5 < this.GetTransitionClosingTime())
                    {
                        this.m_transitionAlpha = MathHelper.Lerp(1f, 0f, MathHelper.Clamp((float) (((float) num5) / ((float) this.GetTransitionClosingTime())), (float) 0f, (float) 1f));
                    }
                    else
                    {
                        this.m_transitionAlpha = 0f;
                        if (this.State == MyGuiScreenState.CLOSING)
                        {
                            this.CloseScreenNow();
                            return false;
                        }
                        if (this.State == MyGuiScreenState.HIDING)
                        {
                            this.State = MyGuiScreenState.HIDDEN;
                            this.OnHide();
                        }
                    }
                }
            }
            else
            {
                int transitionOpeningTime = this.GetTransitionOpeningTime();
                int num2 = MyGuiManager.TotalTimeInMilliseconds - this.m_lastTransitionTime;
                if (EnableSlowTransitionAnimations)
                {
                    if (num2 == 0)
                    {
                        this.m_transitionFrameIndex = 0;
                    }
                    int transitionFrameIndex = this.m_transitionFrameIndex;
                    this.m_transitionFrameIndex = transitionFrameIndex + 1;
                    num2 = transitionFrameIndex * 2;
                }
                if ((this.State == MyGuiScreenState.OPENING) && (this.m_openingCueEnum != null))
                {
                    MyGuiSoundManager.PlaySound(this.m_openingCueEnum.Value);
                }
                if (num2 >= transitionOpeningTime)
                {
                    this.State = MyGuiScreenState.OPENED;
                    this.m_transitionAlpha = 1f;
                    this.OnShow();
                }
                else
                {
                    float amount = MathHelper.Clamp((float) (((float) num2) / ((float) transitionOpeningTime)), (float) 0f, (float) 1f);
                    amount *= amount;
                    this.m_transitionAlpha = MathHelper.Lerp(0f, 1f, amount);
                }
            }
            return true;
        }

        public Color BackgroundFadeColor
        {
            get
            {
                Color backgroundFadeColor = this.m_backgroundFadeColor;
                Color* colorPtr1 = (Color*) ref backgroundFadeColor;
                colorPtr1.A = (byte) (backgroundFadeColor.A * this.m_transitionAlpha);
                return backgroundFadeColor;
            }
        }

        public MyGuiDrawAlignEnum Align { get; set; }

        public bool SkipTransition { get; set; }

        public bool Cancelled { get; private set; }

        protected bool DrawMouseCursor
        {
            get => 
                this.m_drawMouseCursor;
            set => 
                (this.m_drawMouseCursor = value);
        }

        public bool JoystickAsMouse
        {
            get => 
                this.m_joystickAsMouse;
            set => 
                (this.m_joystickAsMouse = value);
        }

        public MyGuiScreenState State
        {
            get => 
                this.m_state;
            set
            {
                if (this.m_state != value)
                {
                    bool visible = this.Visible;
                    this.m_state = value;
                    if ((this.VisibleChanged != null) && (this.Visible != visible))
                    {
                        this.VisibleChanged(this, this.Visible);
                    }
                    if ((MyImeProcessor.Instance != null) && ReferenceEquals(MyScreenManager.GetScreenWithFocus(), this))
                    {
                        if (value == MyGuiScreenState.OPENED)
                        {
                            MyImeProcessor.Instance.RegisterActiveScreen(this);
                        }
                        if ((value == MyGuiScreenState.CLOSED) || (value == MyGuiScreenState.HIDDEN))
                        {
                            MyImeProcessor.Instance.UnregisterActiveScreen(this);
                        }
                    }
                }
            }
        }

        public bool IsLoaded
        {
            get
            {
                object isLoadedLock = this.m_isLoadedLock;
                lock (isLoadedLock)
                {
                    return this.m_isLoaded;
                }
            }
            set
            {
                object isLoadedLock = this.m_isLoadedLock;
                lock (isLoadedLock)
                {
                    this.m_isLoaded = value;
                }
            }
        }

        public bool EnabledBackgroundFade
        {
            get => 
                this.m_enabledBackgroundFade;
            protected set => 
                (this.m_enabledBackgroundFade = value);
        }

        public bool CanBeHidden
        {
            get => 
                this.m_canBeHidden;
            protected set => 
                (this.m_canBeHidden = value);
        }

        public bool CanHideOthers
        {
            get => 
                this.m_canHideOthers;
            protected set => 
                (this.m_canHideOthers = value);
        }

        public bool CanHaveFocus
        {
            get => 
                this.m_canHaveFocus;
            protected set => 
                (this.m_canHaveFocus = value);
        }

        public virtual MyGuiControls Controls =>
            this.m_controls;

        public Vector4? BackgroundColor
        {
            get => 
                this.m_backgroundColor;
            set => 
                (this.m_backgroundColor = value);
        }

        public Vector2? Size
        {
            get => 
                this.m_size;
            set => 
                (this.m_size = value);
        }

        public bool Visible =>
            (this.State != MyGuiScreenState.HIDDEN);

        public Vector2 CloseButtonOffset
        {
            get => 
                this.m_closeButtonOffset;
            set
            {
                if (this.m_closeButtonOffset != value)
                {
                    this.m_closeButtonOffset = value;
                    if (this.m_closeButton != null)
                    {
                        this.m_closeButton.Position = this.CalcCloseButtonPosition();
                    }
                }
            }
        }

        public bool CloseButtonEnabled
        {
            get => 
                this.m_closeButtonEnabled;
            set
            {
                this.m_closeButtonEnabled = value;
                if (this.m_closeButton != null)
                {
                    this.m_closeButton.Visible = value;
                    this.m_closeButton.Position = this.CalcCloseButtonPosition();
                }
            }
        }

        public MyGuiControlButtonStyleEnum CloseButtonStyle
        {
            get => 
                this.m_closeButtonStyle;
            set
            {
                this.m_closeButtonStyle = value;
                if (this.m_closeButton != null)
                {
                    this.m_closeButton.VisualStyle = value;
                }
            }
        }

        public MyGuiControlBase FocusedControl
        {
            get => 
                this.m_focusedControl;
            set
            {
                if (((value == null) || value.CanHaveFocus) && (!ReferenceEquals(this.m_focusedControl, value) && ((MyImeProcessor.Instance == null) || !MyImeProcessor.Instance.IsComposing)))
                {
                    MyGuiControlBase focusedControl = this.m_focusedControl;
                    this.m_focusedControl = value;
                    if (focusedControl != null)
                    {
                        focusedControl.OnFocusChanged(false);
                    }
                    if (this.m_focusedControl != null)
                    {
                        this.m_focusedControl.OnFocusChanged(true);
                    }
                }
            }
        }

        public string DebugNamePath =>
            this.GetFriendlyName();

        public string Name =>
            this.GetFriendlyName();

        public IMyGuiControlsOwner Owner =>
            null;

        public delegate void ScreenHandler(MyGuiScreenBase source);
    }
}

