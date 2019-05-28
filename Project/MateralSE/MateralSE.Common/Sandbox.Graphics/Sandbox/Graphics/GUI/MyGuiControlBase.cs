namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiControlBase : IMyGuiControlsOwner
    {
        private float m_alpha = 1f;
        private const bool DEBUG_CONTROL_FOCUS = false;
        public static bool DEBUG_CONTROL_BORDERS;
        private bool m_isMouseOver;
        private bool m_isMouseOverInPrevious;
        private bool m_canPlaySoundOnMouseOver = true;
        private int m_showToolTipDelay;
        private bool m_canHaveFocus;
        private Vector2 m_minSize = Vector2.Zero;
        private Vector2 m_maxSize = Vector2.PositiveInfinity;
        private string m_name;
        protected bool m_mouseButtonPressed;
        protected bool m_showToolTip;
        protected internal MyToolTips m_toolTip;
        protected Vector2 m_toolTipPosition;
        [CompilerGenerated]
        private Action<MyGuiControlBase, NameChangedArgs> NameChanged;
        public readonly MyGuiControls Elements;
        private Thickness m_margin;
        private Vector2 m_position;
        private Vector2 m_size;
        [CompilerGenerated]
        private Action<MyGuiControlBase> SizeChanged;
        private Vector4 m_colorMask;
        public MyGuiCompositeTexture BackgroundTexture;
        public Vector4 BorderColor;
        public bool BorderEnabled;
        public bool DrawWhilePaused;
        public bool SkipForMouseTest;
        private bool m_enabled;
        public bool ShowTooltipWhenDisabled;
        public bool IsHitTestVisible = true;
        public bool IsActiveControl;
        private MyGuiDrawAlignEnum m_originAlign;
        private bool m_visible;
        [CompilerGenerated]
        private VisibleChangedDelegate VisibleChanged;
        public MyGuiControlHighlightType HighlightType;
        [CompilerGenerated]
        private Action<MyGuiControlBase> HightlightChanged;
        private bool m_hasHighlight;
        [CompilerGenerated]
        private Action<MyGuiControlBase, bool> FocusChanged;

        public event Action<MyGuiControlBase, bool> FocusChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlBase, bool> focusChanged = this.FocusChanged;
                while (true)
                {
                    Action<MyGuiControlBase, bool> a = focusChanged;
                    Action<MyGuiControlBase, bool> action3 = (Action<MyGuiControlBase, bool>) Delegate.Combine(a, value);
                    focusChanged = Interlocked.CompareExchange<Action<MyGuiControlBase, bool>>(ref this.FocusChanged, action3, a);
                    if (ReferenceEquals(focusChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlBase, bool> focusChanged = this.FocusChanged;
                while (true)
                {
                    Action<MyGuiControlBase, bool> source = focusChanged;
                    Action<MyGuiControlBase, bool> action3 = (Action<MyGuiControlBase, bool>) Delegate.Remove(source, value);
                    focusChanged = Interlocked.CompareExchange<Action<MyGuiControlBase, bool>>(ref this.FocusChanged, action3, source);
                    if (ReferenceEquals(focusChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlBase> HightlightChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlBase> hightlightChanged = this.HightlightChanged;
                while (true)
                {
                    Action<MyGuiControlBase> a = hightlightChanged;
                    Action<MyGuiControlBase> action3 = (Action<MyGuiControlBase>) Delegate.Combine(a, value);
                    hightlightChanged = Interlocked.CompareExchange<Action<MyGuiControlBase>>(ref this.HightlightChanged, action3, a);
                    if (ReferenceEquals(hightlightChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlBase> hightlightChanged = this.HightlightChanged;
                while (true)
                {
                    Action<MyGuiControlBase> source = hightlightChanged;
                    Action<MyGuiControlBase> action3 = (Action<MyGuiControlBase>) Delegate.Remove(source, value);
                    hightlightChanged = Interlocked.CompareExchange<Action<MyGuiControlBase>>(ref this.HightlightChanged, action3, source);
                    if (ReferenceEquals(hightlightChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlBase, NameChangedArgs> NameChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlBase, NameChangedArgs> nameChanged = this.NameChanged;
                while (true)
                {
                    Action<MyGuiControlBase, NameChangedArgs> a = nameChanged;
                    Action<MyGuiControlBase, NameChangedArgs> action3 = (Action<MyGuiControlBase, NameChangedArgs>) Delegate.Combine(a, value);
                    nameChanged = Interlocked.CompareExchange<Action<MyGuiControlBase, NameChangedArgs>>(ref this.NameChanged, action3, a);
                    if (ReferenceEquals(nameChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlBase, NameChangedArgs> nameChanged = this.NameChanged;
                while (true)
                {
                    Action<MyGuiControlBase, NameChangedArgs> source = nameChanged;
                    Action<MyGuiControlBase, NameChangedArgs> action3 = (Action<MyGuiControlBase, NameChangedArgs>) Delegate.Remove(source, value);
                    nameChanged = Interlocked.CompareExchange<Action<MyGuiControlBase, NameChangedArgs>>(ref this.NameChanged, action3, source);
                    if (ReferenceEquals(nameChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlBase> SizeChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlBase> sizeChanged = this.SizeChanged;
                while (true)
                {
                    Action<MyGuiControlBase> a = sizeChanged;
                    Action<MyGuiControlBase> action3 = (Action<MyGuiControlBase>) Delegate.Combine(a, value);
                    sizeChanged = Interlocked.CompareExchange<Action<MyGuiControlBase>>(ref this.SizeChanged, action3, a);
                    if (ReferenceEquals(sizeChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlBase> sizeChanged = this.SizeChanged;
                while (true)
                {
                    Action<MyGuiControlBase> source = sizeChanged;
                    Action<MyGuiControlBase> action3 = (Action<MyGuiControlBase>) Delegate.Remove(source, value);
                    sizeChanged = Interlocked.CompareExchange<Action<MyGuiControlBase>>(ref this.SizeChanged, action3, source);
                    if (ReferenceEquals(sizeChanged, source))
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

        protected MyGuiControlBase(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? colorMask = new Vector4?(), string toolTip = null, MyGuiCompositeTexture backgroundTexture = null, bool isActiveControl = true, bool canHaveFocus = false, bool allowFocusingElements = false, MyGuiControlHighlightType highlightType = 2, MyGuiDrawAlignEnum originAlign = 4)
        {
            this.m_canPlaySoundOnMouseOver = true;
            this.Name = base.GetType().Name;
            this.Visible = true;
            this.m_enabled = true;
            Vector2? nullable = position;
            this.m_position = (nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero;
            this.m_canHaveFocus = canHaveFocus;
            nullable = size;
            this.m_size = (nullable != null) ? nullable.GetValueOrDefault() : Vector2.One;
            Vector4? nullable2 = colorMask;
            this.m_colorMask = (nullable2 != null) ? nullable2.GetValueOrDefault() : Vector4.One;
            this.BackgroundTexture = backgroundTexture;
            this.IsActiveControl = isActiveControl;
            this.HighlightType = highlightType;
            this.m_originAlign = originAlign;
            this.BorderSize = 1;
            this.BorderColor = Vector4.One;
            this.BorderEnabled = false;
            this.DrawWhilePaused = true;
            this.Elements = new MyGuiControls(this);
            this.AllowFocusingElements = allowFocusingElements;
            if (toolTip != null)
            {
                this.m_toolTip = new MyToolTips(toolTip);
            }
        }

        public static unsafe Color ApplyColorMaskModifiers(Vector4 sourceColorMask, bool enabled, float transitionAlpha)
        {
            Vector4 vector = sourceColorMask;
            if (!enabled)
            {
                float* singlePtr1 = (float*) ref vector.X;
                singlePtr1[0] *= MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER.X;
                float* singlePtr2 = (float*) ref vector.Y;
                singlePtr2[0] *= MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER.Y;
                float* singlePtr3 = (float*) ref vector.Z;
                singlePtr3[0] *= MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER.Z;
                float* singlePtr4 = (float*) ref vector.W;
                singlePtr4[0] *= MyGuiConstants.DISABLED_CONTROL_COLOR_MASK_MULTIPLIER.W;
            }
            return new Color(vector * transitionAlpha);
        }

        public virtual bool CheckMouseOver()
        {
            RectangleF ef = new RectangleF(this.GetPositionAbsoluteTopLeft(), this.m_size);
            RectangleF result = new RectangleF(0f, 0f, 0f, 0f);
            MyGuiControlBase owner = this.Owner as MyGuiControlBase;
            bool flag = true;
            while ((owner != null) & flag)
            {
                flag &= owner.IsMouseOver;
                Vector2 positionAbsoluteTopLeft = owner.GetPositionAbsoluteTopLeft();
                RectangleF ef3 = new RectangleF(positionAbsoluteTopLeft, owner.m_size);
                if (!owner.SkipForMouseTest && (!RectangleF.Intersect(ref ef, ref ef3, out result) || !IsPointInside(MyGuiManager.MouseCursorPosition, result.Size, result.Position, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)))
                {
                    return false;
                }
                owner = owner.Owner as MyGuiControlBase;
            }
            return (flag && CheckMouseOver(this.Size, this.GetPositionAbsolute(), this.OriginAlign));
        }

        public static bool CheckMouseOver(Vector2 size, Vector2 position, MyGuiDrawAlignEnum originAlign) => 
            IsPointInside(MyGuiManager.MouseCursorPosition, size, position, originAlign);

        public virtual void Clear()
        {
        }

        protected virtual void ClearEvents()
        {
            this.SizeChanged = null;
            this.VisibleChanged = null;
            this.NameChanged = null;
        }

        public virtual void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            this.DrawBackground(backgroundTransitionAlpha);
            this.DrawElements(transitionAlpha, backgroundTransitionAlpha);
            this.DrawBorder(transitionAlpha);
        }

        protected void DrawBackground(float transitionAlpha)
        {
            if ((this.BackgroundTexture != null) && (this.ColorMask.W > 0f))
            {
                this.BackgroundTexture.Draw(this.GetPositionAbsoluteTopLeft(), this.Size, ApplyColorMaskModifiers(this.ColorMask, this.Enabled, transitionAlpha), 1f);
            }
        }

        protected void DrawBorder(float transitionAlpha)
        {
            if (DEBUG_CONTROL_BORDERS)
            {
                float num = ((float) (MyGuiManager.TotalTimeInMilliseconds % 0x1388)) / 5000f;
                Color color = new Vector3((this.PositionY + num) % 1f, (this.PositionX / 2f) + 0.5f, 1f).HSVtoColor();
                MyGuiManager.DrawBorders(this.GetPositionAbsoluteTopLeft(), this.Size, color, 1);
            }
            else if (this.BorderEnabled)
            {
                Color color = ApplyColorMaskModifiers(this.BorderColor * this.ColorMask, this.Enabled, transitionAlpha);
                MyGuiManager.DrawBorders(this.GetPositionAbsoluteTopLeft(), this.Size, color, this.BorderSize);
            }
        }

        protected virtual void DrawElements(float transitionAlpha, float backgroundTransitionAlpha)
        {
            foreach (MyGuiControlBase base2 in this.Elements.GetVisibleControls())
            {
                if (!ReferenceEquals(base2.GetExclusiveInputHandler(), base2))
                {
                    base2.Draw(transitionAlpha * base2.Alpha, backgroundTransitionAlpha * base2.Alpha);
                }
            }
        }

        public virtual MyGuiControlGridDragAndDrop GetDragAndDropHandlingNow() => 
            null;

        public void GetElementsUnderCursor(Vector2 position, bool visibleOnly, List<MyGuiControlBase> controls)
        {
            if (visibleOnly)
            {
                foreach (MyGuiControlBase base2 in this.Elements.GetVisibleControls())
                {
                    if (IsPointInside(position, base2.Size, base2.GetPositionAbsolute(), base2.OriginAlign))
                    {
                        base2.GetElementsUnderCursor(position, visibleOnly, controls);
                        controls.Add(base2);
                    }
                }
            }
            else
            {
                foreach (MyGuiControlBase base3 in this.Elements)
                {
                    if (IsPointInside(position, base3.Size, base3.GetPositionAbsolute(), base3.OriginAlign))
                    {
                        base3.GetElementsUnderCursor(position, visibleOnly, controls);
                        controls.Add(base3);
                    }
                }
            }
        }

        public virtual MyGuiControlBase GetExclusiveInputHandler() => 
            GetExclusiveInputHandler(this.Elements);

        public static MyGuiControlBase GetExclusiveInputHandler(MyGuiControls controls)
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = controls.GetVisibleControls().GetEnumerator())
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

        internal virtual MyGuiControlBase GetFocusControl(bool forwardMovement) => 
            (!this.AllowFocusingElements ? ((this.Owner == null) ? this : this.Owner.GetNextFocusControl(this, forwardMovement)) : this.GetNextFocusControl(this, forwardMovement));

        public virtual string GetMouseCursorTexture()
        {
            bool isMouseOver = this.IsMouseOver;
            return MyGuiManager.GetMouseCursorTexture();
        }

        public virtual MyGuiControlBase GetMouseOverControl() => 
            (!this.IsMouseOver ? null : this);

        public virtual MyGuiControlBase GetNextFocusControl(MyGuiControlBase currentFocusControl, bool forwardMovement)
        {
            int index = this.Elements.IndexOf(currentFocusControl);
            if ((index == -1) && !forwardMovement)
            {
                index = this.Elements.Count;
            }
            int num2 = forwardMovement ? (index + 1) : (index - 1);
            int num3 = forwardMovement ? 1 : -1;
            while ((forwardMovement && (num2 < this.Elements.Count)) || (!forwardMovement && (num2 >= 0)))
            {
                if (MyGuiScreenBase.CanHaveFocusRightNow(this.Elements[num2]))
                {
                    return this.Elements[num2];
                }
                num2 += num3;
            }
            return this.Owner.GetNextFocusControl(this, forwardMovement);
        }

        public virtual MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlBase base1 = MyGuiControlsFactory.CreateObjectBuilder(this);
            base1.Position = this.m_position;
            base1.Size = this.Size;
            base1.Name = this.Name;
            base1.BackgroundColor = this.ColorMask;
            base1.ControlTexture = (this.BackgroundTexture != null) ? this.BackgroundTexture.Center.Texture : null;
            MyObjectBuilder_GuiControlBase local1 = base1;
            local1.OriginAlign = this.OriginAlign;
            return local1;
        }

        public Vector2 GetPositionAbsolute() => 
            ((this.Owner == null) ? this.m_position : (this.Owner.GetPositionAbsoluteCenter() + this.m_position));

        public Vector2 GetPositionAbsoluteBottomLeft() => 
            (this.GetPositionAbsoluteTopLeft() + new Vector2(0f, this.Size.Y));

        public Vector2 GetPositionAbsoluteBottomRight() => 
            (this.GetPositionAbsoluteTopLeft() + this.Size);

        public Vector2 GetPositionAbsoluteCenter() => 
            MyUtils.GetCoordCenterFromAligned(this.GetPositionAbsolute(), this.Size, this.OriginAlign);

        public Vector2 GetPositionAbsoluteCenterLeft() => 
            (this.GetPositionAbsoluteTopLeft() + new Vector2(0f, this.Size.Y * 0.5f));

        public Vector2 GetPositionAbsoluteTopLeft() => 
            MyUtils.GetCoordTopLeftFromAligned(this.GetPositionAbsolute(), this.Size, this.OriginAlign);

        public Vector2 GetPositionAbsoluteTopRight() => 
            (this.GetPositionAbsoluteTopLeft() + new Vector2(this.Size.X, 0f));

        public Vector2? GetSize() => 
            new Vector2?(this.Size);

        protected MyGuiScreenBase GetTopMostOwnerScreen()
        {
            MyGuiScreenBase base2;
            try
            {
                IMyGuiControlsOwner owner = this.Owner;
                while (true)
                {
                    if (owner is MyGuiScreenBase)
                    {
                        base2 = owner as MyGuiScreenBase;
                        break;
                    }
                    owner = ((MyGuiControlBase) owner).Owner;
                }
            }
            catch (NullReferenceException)
            {
                MyLog.Default.WriteLine("NullReferenceException in " + this.DebugNamePath + " trying to reach top most owner.");
                base2 = null;
            }
            return base2;
        }

        public virtual MyGuiControlBase HandleInput()
        {
            bool isMouseOver = this.IsMouseOver;
            this.IsMouseOver = this.CheckMouseOver();
            if (this.IsActiveControl)
            {
                this.m_mouseButtonPressed = this.IsMouseOver && MyInput.Static.IsPrimaryButtonPressed();
                if ((this.IsMouseOver && (!isMouseOver && this.Enabled)) && this.CanPlaySoundOnMouseOver)
                {
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseOver);
                }
            }
            if (!(this.IsMouseOver & isMouseOver))
            {
                this.m_showToolTip = false;
            }
            else if (!this.m_showToolTip)
            {
                this.m_showToolTipDelay = MyGuiManager.TotalTimeInMilliseconds + MyGuiConstants.SHOW_CONTROL_TOOLTIP_DELAY;
                this.m_showToolTip = true;
            }
            return null;
        }

        protected MyGuiControlBase HandleInputElements()
        {
            MyGuiControlBase base2 = null;
            MyGuiControlBase[] baseArray = this.Elements.GetVisibleControls().ToArray();
            int index = baseArray.Length - 1;
            while (true)
            {
                if (index >= 0)
                {
                    base2 = baseArray[index].HandleInput();
                    if (base2 == null)
                    {
                        index--;
                        continue;
                    }
                }
                return base2;
            }
        }

        public virtual void HideToolTip()
        {
            this.m_showToolTip = false;
        }

        public virtual void Init(MyObjectBuilder_GuiControlBase builder)
        {
            this.m_position = builder.Position;
            this.Size = builder.Size;
            this.Name = builder.Name;
            if (builder.BackgroundColor != Vector4.One)
            {
                this.ColorMask = builder.BackgroundColor;
            }
            if (builder.ControlTexture != null)
            {
                MyGuiSizedTexture texture = new MyGuiSizedTexture {
                    Texture = builder.ControlTexture
                };
                MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
                texture1.Center = texture;
                this.BackgroundTexture = texture1;
            }
            this.OriginAlign = builder.OriginAlign;
        }

        public virtual bool IsMouseOverAnyControl() => 
            this.IsMouseOver;

        protected bool IsMouseOverOrKeyboardActive()
        {
            MyGuiScreenBase topMostOwnerScreen = this.GetTopMostOwnerScreen();
            if (topMostOwnerScreen != null)
            {
                MyGuiScreenState state = topMostOwnerScreen.State;
                if ((state <= MyGuiScreenState.OPENED) || (state == MyGuiScreenState.UNHIDING))
                {
                    return (this.IsMouseOver || this.HasFocus);
                }
            }
            return false;
        }

        public static bool IsPointInside(Vector2 queryPoint, Vector2 size, Vector2 position, MyGuiDrawAlignEnum originAlign)
        {
            Vector2 vector = MyUtils.GetCoordCenterFromAligned(position, size, originAlign) - (size / 2f);
            Vector2 vector2 = MyUtils.GetCoordCenterFromAligned(position, size, originAlign) + (size / 2f);
            return ((queryPoint.X >= vector.X) && ((queryPoint.X <= vector2.X) && ((queryPoint.Y >= vector.Y) && (queryPoint.Y <= vector2.Y))));
        }

        protected virtual void OnColorMaskChanged()
        {
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ColorMask = this.ColorMask;
                }
            }
        }

        protected virtual void OnEnabledChanged()
        {
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = this.m_enabled;
                }
            }
        }

        internal virtual void OnFocusChanged(bool focus)
        {
            if (this.FocusChanged != null)
            {
                this.FocusChanged(this, focus);
            }
        }

        protected virtual void OnHasHighlightChanged()
        {
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.HasHighlight = this.HasHighlight;
                }
            }
        }

        protected virtual void OnOriginAlignChanged()
        {
        }

        protected virtual void OnPositionChanged()
        {
        }

        public virtual void OnRemoving()
        {
            if (this.HasFocus)
            {
                this.GetTopMostOwnerScreen().FocusedControl = null;
            }
            this.Elements.Clear();
            this.Owner = null;
            this.ClearEvents();
        }

        protected virtual void OnSizeChanged()
        {
            if (this.SizeChanged != null)
            {
                this.SizeChanged(this);
            }
        }

        protected virtual void OnVisibleChanged()
        {
            if (this.VisibleChanged != null)
            {
                this.VisibleChanged(this, this.m_visible);
            }
        }

        public static void ReadIfHasValue(ref Color target, Vector4? source)
        {
            if (source != null)
            {
                target = new Color(source.Value);
            }
        }

        public static void ReadIfHasValue<T>(ref T target, T? source) where T: struct
        {
            if (source != null)
            {
                target = source.Value;
            }
        }

        public void SetToolTip(MyToolTips toolTip)
        {
            this.m_toolTip = toolTip;
        }

        public void SetToolTip(string text)
        {
            this.SetToolTip(new MyToolTips(text));
        }

        public void SetToolTip(MyStringId text)
        {
            this.SetToolTip(MyTexts.GetString(text));
        }

        protected virtual bool ShouldHaveHighlight()
        {
            if (this.HighlightType == MyGuiControlHighlightType.CUSTOM)
            {
                return this.HasHighlight;
            }
            if ((!this.Enabled || (this.HighlightType == MyGuiControlHighlightType.NEVER)) || !this.IsMouseOverOrKeyboardActive())
            {
                return false;
            }
            return (((this.HighlightType == MyGuiControlHighlightType.WHEN_ACTIVE) || ((this.HighlightType == MyGuiControlHighlightType.WHEN_CURSOR_OVER) && this.IsMouseOver)) || this.HasFocus);
        }

        public virtual void ShowToolTip()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Elements.GetVisibleControls().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ShowToolTip();
                }
            }
            if ((this.m_showToolTip && ((this.Enabled || this.ShowTooltipWhenDisabled) && ((MyGuiManager.TotalTimeInMilliseconds > this.m_showToolTipDelay) && (this.m_toolTip != null)))) && this.m_toolTip.HasContent)
            {
                this.m_toolTipPosition = MyGuiManager.MouseCursorPosition;
                if (CheckMouseOver(this.Size, this.GetPositionAbsolute(), this.OriginAlign))
                {
                    this.m_toolTip.Draw(this.m_toolTipPosition);
                }
                else
                {
                    this.m_showToolTip = false;
                }
            }
        }

        public override string ToString() => 
            this.DebugNamePath;

        public virtual void Update()
        {
            this.HasHighlight = this.ShouldHaveHighlight();
            using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = this.Elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
        }

        public virtual void UpdateArrange()
        {
        }

        public virtual void UpdateMeasure()
        {
        }

        public float Alpha
        {
            get => 
                this.m_alpha;
            set => 
                (this.m_alpha = value);
        }

        public string Name
        {
            get => 
                this.m_name;
            set
            {
                if (this.m_name != value)
                {
                    string name = this.m_name;
                    this.m_name = value;
                    if (this.NameChanged != null)
                    {
                        NameChangedArgs args = new NameChangedArgs {
                            OldName = name
                        };
                        this.NameChanged(this, args);
                    }
                }
            }
        }

        public IMyGuiControlsOwner Owner { get; private set; }

        public MyToolTips Tooltips =>
            this.m_toolTip;

        public Vector2 Position
        {
            get => 
                this.m_position;
            set
            {
                if (this.m_position != value)
                {
                    this.m_position = value;
                    this.OnPositionChanged();
                }
            }
        }

        public Thickness Margin
        {
            get => 
                this.m_margin;
            set => 
                (this.m_margin = value);
        }

        public float PositionY
        {
            get => 
                this.m_position.Y;
            set
            {
                if (this.m_position.Y != value)
                {
                    this.m_position.Y = value;
                    this.OnPositionChanged();
                }
            }
        }

        public float PositionX
        {
            get => 
                this.m_position.X;
            set
            {
                if (this.m_position.X != value)
                {
                    this.m_position.X = value;
                    this.OnPositionChanged();
                }
            }
        }

        public Vector2 Size
        {
            get => 
                this.m_size;
            set
            {
                Vector2 vector1 = Vector2.Clamp(value, this.MinSize, this.MaxSize);
                value = vector1;
                if (this.m_size != value)
                {
                    this.m_size = value;
                    this.OnSizeChanged();
                }
            }
        }

        public Vector2 MinSize
        {
            get => 
                this.m_minSize;
            protected set
            {
                if (this.m_minSize != value)
                {
                    this.m_minSize = value;
                    this.Size = this.m_size;
                }
            }
        }

        public Vector2 MaxSize
        {
            get => 
                this.m_maxSize;
            protected set
            {
                if (this.m_maxSize != value)
                {
                    this.m_maxSize = value;
                    this.Size = this.m_size;
                }
            }
        }

        public Vector4 ColorMask
        {
            get => 
                this.m_colorMask;
            set
            {
                if (this.m_colorMask != value)
                {
                    this.m_colorMask = value;
                    this.OnColorMaskChanged();
                }
            }
        }

        public int BorderSize { get; set; }

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set
            {
                if (this.m_enabled != value)
                {
                    this.m_enabled = value;
                    this.OnEnabledChanged();
                }
            }
        }

        public MyGuiDrawAlignEnum OriginAlign
        {
            get => 
                this.m_originAlign;
            set
            {
                if (this.m_originAlign != value)
                {
                    this.m_originAlign = value;
                    this.OnOriginAlignChanged();
                }
            }
        }

        public bool Visible
        {
            get => 
                this.m_visible;
            set
            {
                if (this.m_visible != value)
                {
                    this.m_visible = value;
                    this.OnVisibleChanged();
                }
            }
        }

        public bool HasHighlight
        {
            get => 
                this.m_hasHighlight;
            set
            {
                if (this.m_hasHighlight != value)
                {
                    this.m_hasHighlight = value;
                    this.OnHasHighlightChanged();
                    if (this.HightlightChanged != null)
                    {
                        this.HightlightChanged(this);
                    }
                }
            }
        }

        public bool HasFocus =>
            ReferenceEquals(MyScreenManager.FocusedControl, this);

        public bool IsMouseOver
        {
            get => 
                this.m_isMouseOver;
            set => 
                (this.m_isMouseOver = value);
        }

        public bool CanHaveFocus
        {
            get => 
                this.m_canHaveFocus;
            set => 
                (this.m_canHaveFocus = value);
        }

        public bool CanPlaySoundOnMouseOver
        {
            get => 
                this.m_canPlaySoundOnMouseOver;
            set => 
                (this.m_canPlaySoundOnMouseOver = value);
        }

        protected bool AllowFocusingElements { get; set; }

        public object UserData { get; set; }

        public string DebugNamePath =>
            Path.Combine((this.Owner != null) ? this.Owner.DebugNamePath : "null", this.Name);

        public class Friend
        {
            protected static void SetOwner(MyGuiControlBase control, IMyGuiControlsOwner owner)
            {
                control.Owner = owner;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NameChangedArgs
        {
            public string OldName;
        }
    }
}

