namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlGridDragAndDrop : MyGuiControlBase
    {
        private MyGuiGridItem m_draggingGridItem;
        private List<MyGuiControlListbox> m_listboxesToDrop;
        private MyDragAndDropInfo m_draggingFrom;
        private Vector4 m_textColor;
        private float m_textScale;
        private Vector2 m_textOffset;
        private bool m_supportIcon;
        private MyDropHandleType? m_currentDropHandleType;
        private MySharedButtonsEnum? m_dragButton;
        private List<MyGuiControlBase> m_dropToControls;
        [CompilerGenerated]
        private OnItemDropped ItemDropped;

        public event OnItemDropped ItemDropped
        {
            [CompilerGenerated] add
            {
                OnItemDropped itemDropped = this.ItemDropped;
                while (true)
                {
                    OnItemDropped a = itemDropped;
                    OnItemDropped dropped3 = (OnItemDropped) Delegate.Combine(a, value);
                    itemDropped = Interlocked.CompareExchange<OnItemDropped>(ref this.ItemDropped, dropped3, a);
                    if (ReferenceEquals(itemDropped, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                OnItemDropped itemDropped = this.ItemDropped;
                while (true)
                {
                    OnItemDropped source = itemDropped;
                    OnItemDropped dropped3 = (OnItemDropped) Delegate.Remove(source, value);
                    itemDropped = Interlocked.CompareExchange<OnItemDropped>(ref this.ItemDropped, dropped3, source);
                    if (ReferenceEquals(itemDropped, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlGridDragAndDrop(Vector4 backgroundColor, Vector4 textColor, float textScale, Vector2 textOffset, bool supportIcon) : base(new Vector2(0f, 0f), new Vector2?(MyGuiConstants.DRAG_AND_DROP_SMALL_SIZE), new Vector4?(backgroundColor), null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_dropToControls = new List<MyGuiControlBase>();
            this.m_textColor = textColor;
            this.m_textScale = textScale;
            this.m_textOffset = textOffset;
            this.m_supportIcon = supportIcon;
            this.DrawBackgroundTexture = true;
        }

        public override bool CheckMouseOver() => 
            this.IsActive();

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if (this.IsActive())
            {
                if (this.DrawBackgroundTexture)
                {
                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", MyGuiManager.MouseCursorPosition, base.Size, ApplyColorMaskModifiers(base.ColorMask * new Color(50, 0x42, 70, 0xff).ToVector4(), true, backgroundTransitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
                }
                Vector2 normalizedCoord = MyGuiManager.MouseCursorPosition - (base.Size / 2f);
                float* singlePtr1 = (float*) ref (normalizedCoord + this.m_textOffset).Y;
                singlePtr1[0] += base.Size.Y / 2f;
                if (this.m_supportIcon && (this.m_draggingGridItem.Icons != null))
                {
                    for (int i = 0; i < this.m_draggingGridItem.Icons.Length; i++)
                    {
                        MyGuiManager.DrawSpriteBatch(this.m_draggingGridItem.Icons[i], normalizedCoord, base.Size, ApplyColorMaskModifiers(base.ColorMask, true, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    }
                }
                this.ShowToolTip();
            }
        }

        public void Drop()
        {
            if (this.IsActive())
            {
                MyDragAndDropInfo info = null;
                this.m_dropToControls.Clear();
                MyScreenManager.GetControlsUnderMouseCursor(this.m_dropToControls, true);
                foreach (MyGuiControlGrid grid in this.m_dropToControls)
                {
                    if (grid == null)
                    {
                        continue;
                    }
                    if (grid.Enabled && ((grid.MouseOverIndex >= 0) && (grid.MouseOverIndex < grid.MaxItemCount)))
                    {
                        info = new MyDragAndDropInfo {
                            Grid = grid,
                            ItemIndex = grid.MouseOverIndex
                        };
                        break;
                    }
                }
                MyDragAndDropEventArgs eventArgs = new MyDragAndDropEventArgs();
                eventArgs.DragFrom = this.m_draggingFrom;
                eventArgs.DropTo = info;
                eventArgs.Item = this.m_draggingGridItem;
                eventArgs.DragButton = this.m_dragButton.Value;
                this.ItemDropped(this, eventArgs);
                this.Stop();
            }
        }

        public override MyGuiControlGridDragAndDrop GetDragAndDropHandlingNow() => 
            this;

        private void HandleButtonClickDrop(MySharedButtonsEnum button, ref MyGuiControlBase captureInput)
        {
            if (MyInput.Static.IsNewButtonPressed(button))
            {
                this.HandleDropingItem();
                captureInput = this;
            }
        }

        private void HandleButtonPressedDrop(MySharedButtonsEnum button, ref MyGuiControlBase captureInput)
        {
            if (MyInput.Static.IsButtonPressed(button))
            {
                captureInput = this;
            }
            else
            {
                this.HandleDropingItem();
            }
        }

        private void HandleDropingItem()
        {
            if (this.IsActive())
            {
                this.Drop();
            }
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase captureInput = base.HandleInput();
            if ((captureInput == null) && this.IsActive())
            {
                MyDropHandleType type = this.m_currentDropHandleType.Value;
                if (type == MyDropHandleType.MouseClick)
                {
                    this.HandleButtonClickDrop(this.m_dragButton.Value, ref captureInput);
                }
                else if (type == MyDropHandleType.MouseRelease)
                {
                    this.HandleButtonPressedDrop(this.m_dragButton.Value, ref captureInput);
                }
            }
            return null;
        }

        public bool IsActive() => 
            ((this.m_draggingGridItem != null) && ((this.m_draggingFrom != null) && ((this.m_currentDropHandleType != null) && (this.m_dragButton != null))));

        public bool IsEmptySpace() => 
            ((this.m_dropToControls != null) && (this.m_dropToControls.Count == 0));

        public override void ShowToolTip()
        {
            if ((this.IsActive() && (base.m_toolTip != null)) && base.m_toolTip.HasContent)
            {
                base.m_toolTipPosition = MyGuiManager.MouseCursorPosition;
                base.m_toolTip.Draw(base.m_toolTipPosition);
            }
        }

        public void StartDragging(MyDropHandleType dropHandleType, MySharedButtonsEnum dragButton, MyGuiGridItem draggingItem, MyDragAndDropInfo draggingFrom, bool includeTooltip = true)
        {
            this.m_currentDropHandleType = new MyDropHandleType?(dropHandleType);
            this.m_dragButton = new MySharedButtonsEnum?(dragButton);
            this.m_draggingGridItem = draggingItem;
            this.m_draggingFrom = draggingFrom;
            this.m_toolTip = includeTooltip ? draggingItem.ToolTip : null;
        }

        public void Stop()
        {
            this.m_draggingFrom = null;
            this.m_draggingGridItem = null;
            this.m_currentDropHandleType = null;
            this.m_dragButton = null;
        }

        public bool DrawBackgroundTexture { get; set; }
    }
}

