namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlResearchGraph : MyGuiControlBase
    {
        private readonly string simpleTexture;
        private MyGuiStyleDefinition m_styleDef;
        private bool m_itemsLayoutInitialized;
        private MyGuiGridItem m_mouseOverItem;
        private MyGuiGridItem m_selectedItem;
        private Vector2 m_doubleClickFirstPosition;
        private int? m_doubleClickStarted;
        private Vector2 m_mouseDragStartPosition;
        private bool m_isItemDraggingLeft;
        [CompilerGenerated]
        private EventHandler<MySharedButtonsEnum> ItemClicked;
        [CompilerGenerated]
        private EventHandler MouseOverItemChanged;
        [CompilerGenerated]
        private EventHandler SelectedItemChanged;
        [CompilerGenerated]
        private EventHandler ItemDoubleClicked;
        [CompilerGenerated]
        private EventHandler<MyGuiGridItem> ItemDragged;

        public event EventHandler<MySharedButtonsEnum> ItemClicked
        {
            [CompilerGenerated] add
            {
                EventHandler<MySharedButtonsEnum> itemClicked = this.ItemClicked;
                while (true)
                {
                    EventHandler<MySharedButtonsEnum> a = itemClicked;
                    EventHandler<MySharedButtonsEnum> handler3 = (EventHandler<MySharedButtonsEnum>) Delegate.Combine(a, value);
                    itemClicked = Interlocked.CompareExchange<EventHandler<MySharedButtonsEnum>>(ref this.ItemClicked, handler3, a);
                    if (ReferenceEquals(itemClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<MySharedButtonsEnum> itemClicked = this.ItemClicked;
                while (true)
                {
                    EventHandler<MySharedButtonsEnum> source = itemClicked;
                    EventHandler<MySharedButtonsEnum> handler3 = (EventHandler<MySharedButtonsEnum>) Delegate.Remove(source, value);
                    itemClicked = Interlocked.CompareExchange<EventHandler<MySharedButtonsEnum>>(ref this.ItemClicked, handler3, source);
                    if (ReferenceEquals(itemClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler ItemDoubleClicked
        {
            [CompilerGenerated] add
            {
                EventHandler itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    EventHandler a = itemDoubleClicked;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    itemDoubleClicked = Interlocked.CompareExchange<EventHandler>(ref this.ItemDoubleClicked, handler3, a);
                    if (ReferenceEquals(itemDoubleClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    EventHandler source = itemDoubleClicked;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    itemDoubleClicked = Interlocked.CompareExchange<EventHandler>(ref this.ItemDoubleClicked, handler3, source);
                    if (ReferenceEquals(itemDoubleClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<MyGuiGridItem> ItemDragged
        {
            [CompilerGenerated] add
            {
                EventHandler<MyGuiGridItem> itemDragged = this.ItemDragged;
                while (true)
                {
                    EventHandler<MyGuiGridItem> a = itemDragged;
                    EventHandler<MyGuiGridItem> handler3 = (EventHandler<MyGuiGridItem>) Delegate.Combine(a, value);
                    itemDragged = Interlocked.CompareExchange<EventHandler<MyGuiGridItem>>(ref this.ItemDragged, handler3, a);
                    if (ReferenceEquals(itemDragged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<MyGuiGridItem> itemDragged = this.ItemDragged;
                while (true)
                {
                    EventHandler<MyGuiGridItem> source = itemDragged;
                    EventHandler<MyGuiGridItem> handler3 = (EventHandler<MyGuiGridItem>) Delegate.Remove(source, value);
                    itemDragged = Interlocked.CompareExchange<EventHandler<MyGuiGridItem>>(ref this.ItemDragged, handler3, source);
                    if (ReferenceEquals(itemDragged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler MouseOverItemChanged
        {
            [CompilerGenerated] add
            {
                EventHandler mouseOverItemChanged = this.MouseOverItemChanged;
                while (true)
                {
                    EventHandler a = mouseOverItemChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    mouseOverItemChanged = Interlocked.CompareExchange<EventHandler>(ref this.MouseOverItemChanged, handler3, a);
                    if (ReferenceEquals(mouseOverItemChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler mouseOverItemChanged = this.MouseOverItemChanged;
                while (true)
                {
                    EventHandler source = mouseOverItemChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    mouseOverItemChanged = Interlocked.CompareExchange<EventHandler>(ref this.MouseOverItemChanged, handler3, source);
                    if (ReferenceEquals(mouseOverItemChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler SelectedItemChanged
        {
            [CompilerGenerated] add
            {
                EventHandler selectedItemChanged = this.SelectedItemChanged;
                while (true)
                {
                    EventHandler a = selectedItemChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    selectedItemChanged = Interlocked.CompareExchange<EventHandler>(ref this.SelectedItemChanged, handler3, a);
                    if (ReferenceEquals(selectedItemChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler selectedItemChanged = this.SelectedItemChanged;
                while (true)
                {
                    EventHandler source = selectedItemChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    selectedItemChanged = Interlocked.CompareExchange<EventHandler>(ref this.SelectedItemChanged, handler3, source);
                    if (ReferenceEquals(selectedItemChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlResearchGraph() : base(new Vector2?(Vector2.Zero), new Vector2?(Vector2.Zero), new Vector4?(MyGuiConstants.LISTBOX_BACKGROUND_COLOR), null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.simpleTexture = @"Textures\Fake.dds";
            this.ItemBackgroundColorMask = Vector4.One;
            MyGuiBorderThickness thickness = new MyGuiBorderThickness(4f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            MyGuiBorderThickness thickness2 = new MyGuiBorderThickness(2f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            MyGuiStyleDefinition definition1 = new MyGuiStyleDefinition();
            definition1.ItemTexture = MyGuiConstants.TEXTURE_GRID_ITEM;
            definition1.ItemFontNormal = "Blue";
            definition1.ItemFontHighlight = "White";
            definition1.SizeOverride = new Vector2?(MyGuiConstants.TEXTURE_GRID_ITEM.SizeGui * new Vector2(10f, 1f));
            definition1.ItemMargin = thickness2;
            definition1.ItemPadding = thickness;
            definition1.ItemTextScale = 0.6f;
            definition1.FitSizeToItems = true;
            MyGuiStyleDefinition styleDef = definition1;
            this.SetCustomStyleDefinition(styleDef);
        }

        private void CheckMouseOverNode(Vector2 controlMousePosition, GraphNode node)
        {
            foreach (MyGuiGridItem item in node.Items)
            {
                RectangleF ef = new RectangleF(item.Position, this.ItemSize);
                if (ef.Contains(controlMousePosition))
                {
                    this.MouseOverItem = item;
                    break;
                }
            }
            foreach (GraphNode node2 in node.Children)
            {
                this.CheckMouseOverNode(controlMousePosition, node2);
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            this.DrawGraph(backgroundTransitionAlpha);
        }

        private void DrawGraph(float transitionAlpha)
        {
            if (this.m_itemsLayoutInitialized)
            {
                Vector2 position = ((base.GetPositionAbsoluteTopLeft() + this.m_styleDef.BackgroundPaddingSize) + this.m_styleDef.ContentPadding.TopLeftOffset) + this.m_styleDef.ItemMargin.TopLeftOffset;
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    GraphNode node = this.Nodes[i];
                    position = this.DrawNode(node, null, position, transitionAlpha);
                }
            }
        }

        private unsafe Vector2 DrawNode(GraphNode node, GraphNode parentNode, Vector2 position, float transitionAlpha)
        {
            string normal = this.m_styleDef.ItemTexture.Normal;
            string highlight = this.m_styleDef.ItemTexture.Highlight;
            Vector4 sourceColorMask = new Vector4(0.2392157f, 0.2901961f, 0.3215686f, 1f);
            MyGuiManager.DrawSpriteBatch(this.simpleTexture, (position + node.Position) + this.NodeMargin, node.Size, ApplyColorMaskModifiers(sourceColorMask, true, 0.9f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
            if (parentNode != null)
            {
                Vector2 vector2 = parentNode.Position;
                float* singlePtr1 = (float*) ref vector2.X;
                singlePtr1[0] += (this.ItemSize.X / 2f) + this.NodePadding.X;
                vector2.Y = node.Position.Y + (node.Size.Y / 2f);
                Vector2 normalizedSize = new Vector2(node.Position.X, vector2.Y) - vector2;
                normalizedSize.Y = 0.004f;
                float* singlePtr2 = (float*) ref vector2.Y;
                singlePtr2[0] -= normalizedSize.Y;
                MyGuiManager.DrawSpriteBatch(this.simpleTexture, (position + vector2) + this.NodeMargin, normalizedSize, ApplyColorMaskModifiers(sourceColorMask, true, 0.9f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
            }
            for (int i = 0; i < node.Items.Count; i++)
            {
                MyGuiGridItem objB = node.Items[i];
                Vector2 normalizedCoord = position + objB.Position;
                bool enabled = base.Enabled && ((objB != null) ? objB.Enabled : true);
                bool flag2 = false;
                float num4 = 1f;
                flag2 = (MyGuiManager.TotalTimeInMilliseconds - objB.blinkCount) <= 400L;
                if (flag2)
                {
                    num4 = objB.blinkingTransparency();
                }
                Vector4 itemBackgroundColorMask = this.ItemBackgroundColorMask;
                MyGuiManager.DrawSpriteBatch((enabled && ((ReferenceEquals(this.MouseOverItem, objB) || ReferenceEquals(this.SelectedItem, objB)) | flag2)) ? highlight : normal, normalizedCoord, this.ItemSize, ApplyColorMaskModifiers(itemBackgroundColorMask, enabled, 0.9f * num4), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                if (objB.OverlayPercent != 0f)
                {
                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", normalizedCoord, this.ItemSize * new Vector2(objB.OverlayPercent, 1f), ApplyColorMaskModifiers(itemBackgroundColorMask * objB.OverlayColorMask, enabled, transitionAlpha * 0.5f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                }
                if (objB.Icons != null)
                {
                    for (int j = 0; j < objB.Icons.Length; j++)
                    {
                        MyGuiManager.DrawSpriteBatch(objB.Icons[j], normalizedCoord, this.ItemSize, ApplyColorMaskModifiers(itemBackgroundColorMask * objB.IconColorMask, enabled, 0.9f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                    }
                }
                if (!string.IsNullOrWhiteSpace(objB.SubIcon))
                {
                    Vector2 vector6 = new Vector2(this.ItemSize.X - (this.ItemSize.X / 3f), 0f);
                    MyGuiManager.DrawSpriteBatch(objB.SubIcon, normalizedCoord + vector6, this.ItemSize / 3f, ApplyColorMaskModifiers(itemBackgroundColorMask * objB.IconColorMask, enabled, 0.9f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                }
                if (!string.IsNullOrWhiteSpace(objB.SubIcon2))
                {
                    Vector2 vector7 = new Vector2(this.ItemSize.X - (this.ItemSize.X / 3.5f), this.ItemSize.Y - (this.ItemSize.Y / 3.5f));
                    MyGuiManager.DrawSpriteBatch(objB.SubIcon2, normalizedCoord + vector7, this.ItemSize / 3.5f, ApplyColorMaskModifiers(itemBackgroundColorMask * objB.IconColorMask, enabled, 0.9f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                }
            }
            float minValue = float.MinValue;
            float maxValue = float.MaxValue;
            foreach (GraphNode node2 in node.Children)
            {
                this.DrawNode(node2, node, position, transitionAlpha);
                if (node2.Position.Y > minValue)
                {
                    minValue = node2.Position.Y + (node2.Size.Y / 2f);
                }
                if (maxValue > node2.Position.X)
                {
                    maxValue = node2.Position.X;
                }
            }
            if (node.Children.Count != 0)
            {
                Vector2 vector8 = node.Position;
                float* singlePtr3 = (float*) ref vector8.X;
                singlePtr3[0] += (this.ItemSize.X / 2f) + this.NodePadding.X;
                float* singlePtr4 = (float*) ref vector8.Y;
                singlePtr4[0] += node.Size.Y;
                Vector2 normalizedSize = new Vector2(vector8.X, minValue) - vector8;
                normalizedSize.X = 0.003f;
                MyGuiManager.DrawSpriteBatch(this.simpleTexture, (position + vector8) + this.NodeMargin, normalizedSize, ApplyColorMaskModifiers(sourceColorMask, true, 0.9f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
            }
            return position;
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase captureInput = base.HandleInput();
            if (captureInput == null)
            {
                if (!base.Enabled)
                {
                    return captureInput;
                }
                if (!base.IsMouseOver)
                {
                    return captureInput;
                }
                Vector2 controlMousePosition = MyGuiManager.MouseCursorPosition - (((base.GetPositionAbsoluteTopLeft() + this.m_styleDef.BackgroundPaddingSize) + this.m_styleDef.ContentPadding.TopLeftOffset) + this.m_styleDef.ItemMargin.TopLeftOffset);
                MyGuiGridItem mouseOverItem = this.MouseOverItem;
                this.MouseOverItem = null;
                foreach (GraphNode node in this.Nodes)
                {
                    this.CheckMouseOverNode(controlMousePosition, node);
                }
                if (!ReferenceEquals(mouseOverItem, this.MouseOverItem))
                {
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseOver);
                }
                if (captureInput == null)
                {
                    captureInput = this.HandleNewMousePress();
                    this.HandleMouseDrag(ref captureInput, MySharedButtonsEnum.Primary, ref this.m_isItemDraggingLeft);
                }
                if ((this.m_doubleClickStarted != null) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) >= 500f))
                {
                    this.m_doubleClickStarted = null;
                }
            }
            return captureInput;
        }

        private void HandleMouseDrag(ref MyGuiControlBase captureInput, MySharedButtonsEnum button, ref bool isDragging)
        {
            if (MyInput.Static.IsNewButtonPressed(button))
            {
                isDragging = true;
                this.m_mouseDragStartPosition = MyGuiManager.MouseCursorPosition;
            }
            else if (!MyInput.Static.IsButtonPressed(button))
            {
                isDragging = false;
            }
            else if (isDragging && (this.SelectedItem != null))
            {
                if ((MyGuiManager.MouseCursorPosition - this.m_mouseDragStartPosition).Length() != 0f)
                {
                    if (this.ItemDragged != null)
                    {
                        this.ItemDragged(this, this.SelectedItem);
                    }
                    isDragging = false;
                }
                captureInput = this;
            }
        }

        private MyGuiControlBase HandleNewMousePress()
        {
            if (this.MouseOverItem != null)
            {
                if (MyInput.Static.IsAnyNewMouseOrJoystickPressed())
                {
                    this.SelectedItem = this.MouseOverItem;
                    MySharedButtonsEnum none = MySharedButtonsEnum.None;
                    if (MyInput.Static.IsNewPrimaryButtonPressed())
                    {
                        none = MySharedButtonsEnum.Primary;
                    }
                    else if (MyInput.Static.IsNewSecondaryButtonPressed())
                    {
                        none = MySharedButtonsEnum.Secondary;
                    }
                    if (this.ItemClicked != null)
                    {
                        this.ItemClicked(this, none);
                    }
                }
                if (MyInput.Static.IsNewPrimaryButtonPressed())
                {
                    if (this.m_doubleClickStarted == null)
                    {
                        this.m_doubleClickStarted = new int?(MyGuiManager.TotalTimeInMilliseconds);
                        this.m_doubleClickFirstPosition = MyGuiManager.MouseCursorPosition;
                    }
                    else if (((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) <= 500f) && ((this.m_doubleClickFirstPosition - MyGuiManager.MouseCursorPosition).Length() <= 0.005f))
                    {
                        if (this.ItemDoubleClicked != null)
                        {
                            this.ItemDoubleClicked(this, System.EventArgs.Empty);
                            MyGuiSoundManager.PlaySound(GuiSounds.Item);
                        }
                        this.m_doubleClickStarted = null;
                        return this;
                    }
                }
            }
            return null;
        }

        public void InitializeItemsLayout()
        {
            if (((this.m_styleDef != null) && (this.Nodes != null)) && (this.Nodes.Count != 0))
            {
                Vector2 maxPosition = new Vector2();
                Vector2 position = new Vector2();
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    GraphNode node = this.Nodes[i];
                    this.InitializeNode(node, ref position, ref maxPosition);
                    position.Y = ((maxPosition.Y + this.ItemSize.Y) + this.NodePadding.Y) + this.NodeMargin.Y;
                    position.X = 0f;
                }
                Vector2 vector3 = (maxPosition + this.ItemSize) + this.NodePadding;
                Vector2 size = base.Size;
                if (vector3.X > size.X)
                {
                    size.X = vector3.X;
                }
                if (vector3.Y > size.Y)
                {
                    size.Y = vector3.Y;
                }
                base.Size = size;
                this.m_itemsLayoutInitialized = true;
            }
        }

        private unsafe void InitializeNode(GraphNode node, ref Vector2 position, ref Vector2 maxPosition)
        {
            float x = -this.ItemSize.X + this.NodePadding.X;
            float y = this.NodePadding.Y;
            float minValue = float.MinValue;
            for (int i = 0; i < node.Items.Count; i++)
            {
                MyGuiGridItem item = node.Items[i];
                x += this.ItemSize.X;
                if (((position.X + x) + this.ItemSize.X) > base.Size.X)
                {
                    x = this.NodePadding.X;
                    y += this.ItemSize.Y;
                }
                item.Position = position + new Vector2(x, y);
                if (x > minValue)
                {
                    minValue = x;
                }
                if (i == 0)
                {
                    node.Position = (item.Position - this.NodePadding) - this.NodeMargin;
                }
                if (item.Position.X > maxPosition.X)
                {
                    maxPosition.X = item.Position.X;
                }
                if (item.Position.Y > maxPosition.Y)
                {
                    maxPosition.Y = item.Position.Y;
                }
            }
            node.Size = ((((position + new Vector2(minValue, y)) + this.ItemSize) - node.Position) + this.NodePadding) - this.NodeMargin;
            float* singlePtr1 = (float*) ref position.Y;
            singlePtr1[0] += ((this.ItemSize.Y + y) + this.NodePadding.Y) + this.NodeMargin.Y;
            float* singlePtr2 = (float*) ref position.X;
            singlePtr2[0] += this.ItemSize.X;
            foreach (GraphNode node2 in node.Children)
            {
                this.InitializeNode(node2, ref position, ref maxPosition);
            }
            float* singlePtr3 = (float*) ref position.X;
            singlePtr3[0] -= this.ItemSize.X;
        }

        public void InvalidateItemsLayout()
        {
            this.m_itemsLayoutInitialized = false;
        }

        public void SetCustomStyleDefinition(MyGuiStyleDefinition styleDef)
        {
            this.m_styleDef = styleDef;
            base.BorderEnabled = this.m_styleDef.BorderEnabled;
            base.BorderColor = this.m_styleDef.BorderColor;
            if (!this.m_itemsLayoutInitialized)
            {
                this.InitializeItemsLayout();
            }
        }

        public override void ShowToolTip()
        {
            base.ShowToolTip();
            if (this.MouseOverItem != null)
            {
                base.m_toolTip = this.MouseOverItem.ToolTip;
            }
            else
            {
                base.m_toolTip = null;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!this.m_itemsLayoutInitialized)
            {
                this.InitializeItemsLayout();
            }
        }

        public List<GraphNode> Nodes { get; set; }

        public Vector4 ItemBackgroundColorMask { get; set; }

        public Vector2 ItemSize { get; set; }

        public Vector2 NodePadding { get; set; }

        public Vector2 NodeMargin { get; set; }

        public MyGuiGridItem MouseOverItem
        {
            get => 
                this.m_mouseOverItem;
            private set
            {
                if (!ReferenceEquals(this.m_mouseOverItem, value))
                {
                    this.m_mouseOverItem = value;
                    if (this.MouseOverItemChanged != null)
                    {
                        this.MouseOverItemChanged(this, System.EventArgs.Empty);
                    }
                }
            }
        }

        public MyGuiGridItem SelectedItem
        {
            get => 
                this.m_selectedItem;
            private set
            {
                if (!ReferenceEquals(this.m_selectedItem, value))
                {
                    this.m_selectedItem = value;
                    if (this.SelectedItemChanged != null)
                    {
                        this.SelectedItemChanged(this, System.EventArgs.Empty);
                    }
                }
            }
        }

        public class GraphNode
        {
            public GraphNode()
            {
                this.Items = new List<MyGuiGridItem>();
                this.Children = new List<MyGuiControlResearchGraph.GraphNode>();
            }

            public Vector2 Position { get; set; }

            public Vector2 Size { get; set; }

            public string Name { get; set; }

            public List<MyGuiGridItem> Items { get; set; }

            public List<MyGuiControlResearchGraph.GraphNode> Children { get; set; }

            public string UnlockedBy { get; set; }
        }
    }
}

