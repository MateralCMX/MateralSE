namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlTable : MyGuiControlBase
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlTableStyleEnum>() + 1];
        private MyGuiControls m_controls;
        private List<ColumnMetaData> m_columnsMetaData;
        private List<Row> m_rows;
        private Vector2 m_doubleClickFirstPosition;
        private int? m_doubleClickStarted;
        private bool m_mouseOverHeader;
        private int? m_mouseOverColumnIndex;
        private int? m_mouseOverRowIndex;
        private RectangleF m_headerArea;
        private RectangleF m_rowsArea;
        private StyleDefinition m_styleDef;
        private MyVScrollbar m_scrollBar;
        protected int m_visibleRowIndexOffset;
        private int m_lastSortedColumnIdx;
        private float m_textScale;
        private float m_textScaleWithLanguage;
        private int m_sortColumn;
        private SortStateEnum? m_sortColumnState;
        private bool m_headerVisible;
        private int m_columnsCount;
        private int? m_selectedRowIndex;
        private int m_visibleRows;
        private MyGuiControlTableStyleEnum m_visualStyle;
        [CompilerGenerated]
        private Action<MyGuiControlTable, EventArgs> ItemDoubleClicked;
        [CompilerGenerated]
        private Action<MyGuiControlTable, EventArgs> ItemRightClicked;
        [CompilerGenerated]
        private Action<MyGuiControlTable, EventArgs> ItemSelected;
        [CompilerGenerated]
        private Action<MyGuiControlTable, EventArgs> ItemConfirmed;
        [CompilerGenerated]
        private Action<MyGuiControlTable, int> ColumnClicked;
        [CompilerGenerated]
        private Action<Row> ItemMouseOver;

        public event Action<MyGuiControlTable, int> ColumnClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTable, int> columnClicked = this.ColumnClicked;
                while (true)
                {
                    Action<MyGuiControlTable, int> a = columnClicked;
                    Action<MyGuiControlTable, int> action3 = (Action<MyGuiControlTable, int>) Delegate.Combine(a, value);
                    columnClicked = Interlocked.CompareExchange<Action<MyGuiControlTable, int>>(ref this.ColumnClicked, action3, a);
                    if (ReferenceEquals(columnClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTable, int> columnClicked = this.ColumnClicked;
                while (true)
                {
                    Action<MyGuiControlTable, int> source = columnClicked;
                    Action<MyGuiControlTable, int> action3 = (Action<MyGuiControlTable, int>) Delegate.Remove(source, value);
                    columnClicked = Interlocked.CompareExchange<Action<MyGuiControlTable, int>>(ref this.ColumnClicked, action3, source);
                    if (ReferenceEquals(columnClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlTable, EventArgs> ItemConfirmed
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTable, EventArgs> itemConfirmed = this.ItemConfirmed;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> a = itemConfirmed;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Combine(a, value);
                    itemConfirmed = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemConfirmed, action3, a);
                    if (ReferenceEquals(itemConfirmed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTable, EventArgs> itemConfirmed = this.ItemConfirmed;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> source = itemConfirmed;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Remove(source, value);
                    itemConfirmed = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemConfirmed, action3, source);
                    if (ReferenceEquals(itemConfirmed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlTable, EventArgs> ItemDoubleClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTable, EventArgs> itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> a = itemDoubleClicked;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Combine(a, value);
                    itemDoubleClicked = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemDoubleClicked, action3, a);
                    if (ReferenceEquals(itemDoubleClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTable, EventArgs> itemDoubleClicked = this.ItemDoubleClicked;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> source = itemDoubleClicked;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Remove(source, value);
                    itemDoubleClicked = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemDoubleClicked, action3, source);
                    if (ReferenceEquals(itemDoubleClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<Row> ItemMouseOver
        {
            [CompilerGenerated] add
            {
                Action<Row> itemMouseOver = this.ItemMouseOver;
                while (true)
                {
                    Action<Row> a = itemMouseOver;
                    Action<Row> action3 = (Action<Row>) Delegate.Combine(a, value);
                    itemMouseOver = Interlocked.CompareExchange<Action<Row>>(ref this.ItemMouseOver, action3, a);
                    if (ReferenceEquals(itemMouseOver, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Row> itemMouseOver = this.ItemMouseOver;
                while (true)
                {
                    Action<Row> source = itemMouseOver;
                    Action<Row> action3 = (Action<Row>) Delegate.Remove(source, value);
                    itemMouseOver = Interlocked.CompareExchange<Action<Row>>(ref this.ItemMouseOver, action3, source);
                    if (ReferenceEquals(itemMouseOver, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlTable, EventArgs> ItemRightClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTable, EventArgs> itemRightClicked = this.ItemRightClicked;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> a = itemRightClicked;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Combine(a, value);
                    itemRightClicked = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemRightClicked, action3, a);
                    if (ReferenceEquals(itemRightClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTable, EventArgs> itemRightClicked = this.ItemRightClicked;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> source = itemRightClicked;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Remove(source, value);
                    itemRightClicked = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemRightClicked, action3, source);
                    if (ReferenceEquals(itemRightClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlTable, EventArgs> ItemSelected
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTable, EventArgs> itemSelected = this.ItemSelected;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> a = itemSelected;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Combine(a, value);
                    itemSelected = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemSelected, action3, a);
                    if (ReferenceEquals(itemSelected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTable, EventArgs> itemSelected = this.ItemSelected;
                while (true)
                {
                    Action<MyGuiControlTable, EventArgs> source = itemSelected;
                    Action<MyGuiControlTable, EventArgs> action3 = (Action<MyGuiControlTable, EventArgs>) Delegate.Remove(source, value);
                    itemSelected = Interlocked.CompareExchange<Action<MyGuiControlTable, EventArgs>>(ref this.ItemSelected, action3, source);
                    if (ReferenceEquals(itemSelected, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlTable()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition1.RowTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition1.HeaderTextureHighlight = @"Textures\GUI\Controls\item_highlight_light.dds";
            definition1.RowFontNormal = "Blue";
            definition1.RowFontHighlight = "White";
            definition1.HeaderFontNormal = "White";
            definition1.HeaderFontHighlight = "White";
            definition1.TextScale = 0.8f;
            definition1.RowHeight = 40f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            MyGuiBorderThickness thickness = new MyGuiBorderThickness {
                Left = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 2f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition1.Padding = thickness;
            thickness = new MyGuiBorderThickness {
                Left = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition1.ScrollbarMargin = thickness;
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.Texture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            definition2.RowTextureHighlight = @"Textures\GUI\Controls\item_highlight_dark.dds";
            definition2.HeaderTextureHighlight = @"Textures\GUI\Controls\item_highlight_light.dds";
            definition2.RowFontNormal = "White";
            definition2.RowFontHighlight = "White";
            definition2.HeaderFontNormal = "White";
            definition2.HeaderFontHighlight = "White";
            definition2.TextScale = 0.8f;
            definition2.RowHeight = 40f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            thickness = new MyGuiBorderThickness {
                Left = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition2.Padding = thickness;
            thickness = new MyGuiBorderThickness {
                Left = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Right = 1f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                Top = 3f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                Bottom = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
            };
            definition2.ScrollbarMargin = thickness;
            m_styles[1] = definition2;
        }

        public MyGuiControlTable() : base(nullable, nullable, nullable2, null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_sortColumn = -1;
            this.m_headerVisible = true;
            this.m_columnsCount = 1;
            this.m_visibleRows = 1;
            Vector2? nullable = null;
            nullable = null;
            this.m_scrollBar = new MyVScrollbar(this);
            this.m_scrollBar.ValueChanged += new Action<MyScrollbar>(this.verticalScrollBar_ValueChanged);
            this.m_rows = new List<Row>();
            this.m_columnsMetaData = new List<ColumnMetaData>();
            this.VisualStyle = MyGuiControlTableStyleEnum.Default;
            this.m_controls = new MyGuiControls(null);
            base.Name = "Table";
        }

        public void Add(Row row)
        {
            this.m_rows.Add(row);
            this.RefreshScrollbar();
        }

        public void Clear()
        {
            using (List<Row>.Enumerator enumerator = this.m_rows.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (Cell cell in enumerator.Current.Cells)
                    {
                        if (cell.Control != null)
                        {
                            cell.Control.OnRemoving();
                            cell.Control.Clear();
                        }
                    }
                }
            }
            this.m_rows.Clear();
            this.SelectedRowIndex = null;
            this.RefreshScrollbar();
        }

        private static int Compare(int columnIdx, Comparison<Cell> comparison, Row a, Row b)
        {
            Cell x = (a.Cells.Count > columnIdx) ? a.Cells[columnIdx] : null;
            return comparison(x, (b.Cells.Count > columnIdx) ? b.Cells[columnIdx] : null);
        }

        private int ComputeColumnIndexFromPosition(Vector2 normalizedPosition)
        {
            normalizedPosition -= base.GetPositionAbsoluteTopLeft();
            float num = (normalizedPosition.X - this.m_rowsArea.Position.X) / this.m_rowsArea.Size.X;
            int num2 = 0;
            while ((num2 < this.m_columnsMetaData.Count) && (num >= this.m_columnsMetaData[num2].Width))
            {
                num -= this.m_columnsMetaData[num2].Width;
                num2++;
            }
            return num2;
        }

        private int ComputeRowIndexFromPosition(Vector2 normalizedPosition)
        {
            normalizedPosition -= base.GetPositionAbsoluteTopLeft();
            return (((int) ((normalizedPosition.Y - this.m_rowsArea.Position.Y) / this.RowHeight)) + this.m_visibleRowIndexOffset);
        }

        private unsafe void DebugDraw()
        {
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_headerArea.Position, this.m_headerArea.Size, Color.Cyan, 1);
            MyGuiManager.DrawBorders(base.GetPositionAbsoluteTopLeft() + this.m_rowsArea.Position, this.m_rowsArea.Size, Color.White, 1);
            Vector2 topLeftPosition = base.GetPositionAbsoluteTopLeft() + this.m_headerArea.Position;
            for (int i = 0; i < this.m_columnsMetaData.Count; i++)
            {
                ColumnMetaData data = this.m_columnsMetaData[i];
                Vector2 size = new Vector2(data.Width * this.m_rowsArea.Size.X, this.m_headerArea.Height);
                MyGuiManager.DrawBorders(topLeftPosition, size, Color.Yellow, 1);
                float* singlePtr1 = (float*) ref topLeftPosition.X;
                singlePtr1[0] += data.Width * this.m_headerArea.Width;
            }
            this.m_scrollBar.DebugDraw();
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            float rowHeight = this.RowHeight;
            int visibleRowsCount = this.VisibleRowsCount;
            this.m_styleDef.Texture.Draw(positionAbsoluteTopLeft, base.Size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, backgroundTransitionAlpha), 1f);
            if (this.HeaderVisible)
            {
                this.DrawHeader(transitionAlpha);
            }
            this.DrawRows(transitionAlpha);
            this.m_scrollBar.Draw(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha));
            Vector2 positionAbsoluteTopRight = base.GetPositionAbsoluteTopRight();
            float* singlePtr1 = (float*) ref positionAbsoluteTopRight.X;
            singlePtr1[0] -= this.m_styleDef.ScrollbarMargin.HorizontalSum + this.m_scrollBar.Size.X;
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Controls\scrollable_list_line.dds", positionAbsoluteTopRight, new Vector2(0.0012f, base.Size.Y), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
        }

        private unsafe void DrawHeader(float transitionAlpha)
        {
            Vector2 vector = base.GetPositionAbsoluteTopLeft() + this.m_headerArea.Position;
            MyGuiManager.DrawSpriteBatch(this.m_styleDef.HeaderTextureHighlight, new Vector2(vector.X + 0.001f, vector.Y), new Vector2(this.m_headerArea.Size.X - 0.001f, this.m_headerArea.Size.Y), Color.White, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
            for (int i = 0; i < this.m_columnsMetaData.Count; i++)
            {
                ColumnMetaData data = this.m_columnsMetaData[i];
                string headerFontNormal = this.m_styleDef.HeaderFontNormal;
                if ((this.m_mouseOverColumnIndex != null) && (this.m_mouseOverColumnIndex.Value == i))
                {
                    headerFontNormal = this.m_styleDef.HeaderFontHighlight;
                }
                Vector2 size = new Vector2(data.Width * this.m_rowsArea.Size.X, this.m_headerArea.Height);
                Vector2 normalizedCoord = MyUtils.GetCoordAlignedFromCenter((vector + (0.5f * size)) + new Vector2(0.01f, 0f), size, data.HeaderTextAlign);
                MyGuiManager.DrawString(headerFontNormal, data.Name, normalizedCoord, this.TextScaleWithLanguage, new Color?(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha)), data.HeaderTextAlign, false, size.X);
                float* singlePtr1 = (float*) ref vector.X;
                singlePtr1[0] += data.Width * this.m_headerArea.Width;
            }
        }

        private unsafe void DrawRows(float transitionAlpha)
        {
            Vector2 normalizedCoord = base.GetPositionAbsoluteTopLeft() + this.m_rowsArea.Position;
            int num = 0;
            while (true)
            {
                if (num < this.VisibleRowsCount)
                {
                    int num2 = num + this.m_visibleRowIndexOffset;
                    if (num2 < this.m_rows.Count)
                    {
                        if (num2 >= 0)
                        {
                            int num1;
                            int num7;
                            if ((this.m_mouseOverRowIndex != null) && (this.m_mouseOverRowIndex.Value == num2))
                            {
                                num1 = 1;
                            }
                            else if (this.SelectedRowIndex != null)
                            {
                                num1 = (int) (this.SelectedRowIndex.Value == num2);
                            }
                            else
                            {
                                num1 = 0;
                            }
                            string rowFontNormal = this.m_styleDef.RowFontNormal;
                            if (num7 != 0)
                            {
                                MyGuiManager.DrawSpriteBatch(this.m_styleDef.RowTextureHighlight, normalizedCoord, new Vector2(this.m_rowsArea.Size.X, this.RowHeight), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                                rowFontNormal = this.m_styleDef.RowFontHighlight;
                            }
                            Row row = this.m_rows[num2];
                            if (row != null)
                            {
                                Vector2 topLeft = normalizedCoord;
                                for (int i = 0; (i < this.ColumnsCount) && (i < row.Cells.Count); i++)
                                {
                                    Cell cell = row.Cells[i];
                                    ColumnMetaData data = this.m_columnsMetaData[i];
                                    Vector2 size = new Vector2(data.Width * this.m_rowsArea.Size.X, this.RowHeight);
                                    if ((cell != null) && (cell.Control != null))
                                    {
                                        MyUtils.GetCoordAlignedFromTopLeft(topLeft, size, cell.IconOriginAlign);
                                        cell.Control.Position = topLeft + (size * 0.5f);
                                        cell.Control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
                                        cell.Control.Draw(transitionAlpha, transitionAlpha);
                                    }
                                    else if ((cell != null) && (cell.Text != null))
                                    {
                                        float num4 = 0f;
                                        float x = 0.01f;
                                        if (cell.Icon != null)
                                        {
                                            Vector2 vector5 = MyUtils.GetCoordAlignedFromTopLeft(topLeft, size, cell.IconOriginAlign);
                                            MyGuiHighlightTexture texture = cell.Icon.Value;
                                            Vector2 vector6 = Vector2.Min(texture.SizeGui, size) / texture.SizeGui;
                                            float f = Math.Min(vector6.X, vector6.Y);
                                            num4 = texture.SizeGui.X;
                                            MyGuiManager.DrawSpriteBatch(base.HasHighlight ? texture.Highlight : texture.Normal, vector5 + new Vector2(x, 0f), texture.SizeGui * f, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), cell.IconOriginAlign, false, true);
                                            if (f.IsValid())
                                            {
                                                x = 0.02f;
                                            }
                                        }
                                        Vector2 vector4 = MyUtils.GetCoordAlignedFromCenter((topLeft + (0.5f * size)) + new Vector2(x, 0f), size, data.TextAlign);
                                        float* singlePtr1 = (float*) ref vector4.X;
                                        singlePtr1[0] += num4;
                                        MyGuiManager.DrawString(rowFontNormal, cell.Text, vector4, this.TextScaleWithLanguage, (cell.TextColor != null) ? cell.TextColor : new Color?(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha)), data.TextAlign, false, size.X - 0.02f);
                                    }
                                    float* singlePtr2 = (float*) ref topLeft.X;
                                    singlePtr2[0] += size.X;
                                }
                            }
                            float* singlePtr3 = (float*) ref normalizedCoord.Y;
                            singlePtr3[0] += this.RowHeight;
                        }
                        num++;
                        continue;
                    }
                }
                return;
            }
        }

        public Row Find(Predicate<Row> match) => 
            this.m_rows.Find(match);

        public int FindIndex(Predicate<Row> match) => 
            this.m_rows.FindIndex(match);

        public int FindIndexByUserData(ref object data, EqualUserData equals)
        {
            int num = -1;
            using (List<Row>.Enumerator enumerator = this.m_rows.GetEnumerator())
            {
                while (true)
                {
                    int num2;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Row current = enumerator.Current;
                    num++;
                    if (current.UserData == null)
                    {
                        if (data != null)
                        {
                            continue;
                        }
                        num2 = num;
                    }
                    else if (equals == null)
                    {
                        if (current.UserData != data)
                        {
                            continue;
                        }
                        num2 = num;
                    }
                    else
                    {
                        if (!equals(current.UserData, data))
                        {
                            continue;
                        }
                        num2 = num;
                    }
                    return num2;
                }
            }
            return -1;
        }

        public int FindRow(Row row) => 
            this.m_rows.IndexOf(row);

        public Row GetRow(int index) => 
            this.m_rows[index];

        public static StyleDefinition GetVisualStyle(MyGuiControlTableStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase captureInput = base.HandleInput();
            if (captureInput == null)
            {
                if (!base.Enabled)
                {
                    return null;
                }
                if ((this.m_scrollBar != null) && this.m_scrollBar.HandleInput())
                {
                    captureInput = this;
                }
                this.HandleMouseOver();
                this.HandleNewMousePress(ref captureInput);
                using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
                {
                    while (enumerator.MoveNext() && (enumerator.Current.HandleInput() == null))
                    {
                    }
                }
                if ((this.m_doubleClickStarted != null) && ((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) >= 500f))
                {
                    this.m_doubleClickStarted = null;
                }
                if (!base.HasFocus)
                {
                    return captureInput;
                }
                if (((this.SelectedRowIndex != null) && MyInput.Static.IsNewKeyPressed(MyKeys.Enter)) && (this.ItemConfirmed != null))
                {
                    captureInput = this;
                    EventArgs args = new EventArgs {
                        RowIndex = this.SelectedRowIndex.Value
                    };
                    this.ItemConfirmed(this, args);
                }
            }
            return captureInput;
        }

        private void HandleMouseOver()
        {
            if (this.m_rowsArea.Contains(MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft()))
            {
                this.m_mouseOverRowIndex = new int?(this.ComputeRowIndexFromPosition(MyGuiManager.MouseCursorPosition));
                this.m_mouseOverColumnIndex = new int?(this.ComputeColumnIndexFromPosition(MyGuiManager.MouseCursorPosition));
                this.m_mouseOverHeader = false;
            }
            else if (this.m_headerArea.Contains(MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft()))
            {
                this.m_mouseOverRowIndex = null;
                this.m_mouseOverColumnIndex = new int?(this.ComputeColumnIndexFromPosition(MyGuiManager.MouseCursorPosition));
                this.m_mouseOverHeader = true;
            }
            else
            {
                this.m_mouseOverRowIndex = null;
                this.m_mouseOverColumnIndex = null;
                this.m_mouseOverHeader = false;
            }
        }

        private void HandleNewMousePress(ref MyGuiControlBase captureInput)
        {
            EventArgs args;
            bool flag = this.m_rowsArea.Contains(MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft());
            MyMouseButtonsEnum none = MyMouseButtonsEnum.None;
            if (MyInput.Static.IsNewPrimaryButtonPressed())
            {
                none = MyMouseButtonsEnum.Left;
            }
            else if (MyInput.Static.IsNewSecondaryButtonPressed())
            {
                none = MyMouseButtonsEnum.Right;
            }
            else if (MyInput.Static.IsNewMiddleMousePressed())
            {
                none = MyMouseButtonsEnum.Middle;
            }
            else if (MyInput.Static.IsNewXButton1MousePressed())
            {
                none = MyMouseButtonsEnum.XButton1;
            }
            else if (MyInput.Static.IsNewXButton2MousePressed())
            {
                none = MyMouseButtonsEnum.XButton2;
            }
            if (MyInput.Static.IsAnyNewMouseOrJoystickPressed() & flag)
            {
                this.SelectedRowIndex = new int?(this.ComputeRowIndexFromPosition(MyGuiManager.MouseCursorPosition));
                captureInput = this;
                if (this.ItemSelected != null)
                {
                    args = new EventArgs {
                        RowIndex = this.SelectedRowIndex.Value,
                        MouseButton = none
                    };
                    this.ItemSelected(this, args);
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                }
            }
            if (MyInput.Static.IsNewPrimaryButtonPressed())
            {
                if (this.m_mouseOverHeader)
                {
                    SortStateEnum? sortState = null;
                    this.SortByColumn(this.m_mouseOverColumnIndex.Value, sortState, true);
                    if (this.ColumnClicked != null)
                    {
                        this.ColumnClicked(this, this.m_mouseOverColumnIndex.Value);
                    }
                }
                else if (flag)
                {
                    if (this.m_doubleClickStarted == null)
                    {
                        this.m_doubleClickStarted = new int?(MyGuiManager.TotalTimeInMilliseconds);
                        this.m_doubleClickFirstPosition = MyGuiManager.MouseCursorPosition;
                    }
                    else if (((MyGuiManager.TotalTimeInMilliseconds - this.m_doubleClickStarted.Value) <= 500f) && ((this.m_doubleClickFirstPosition - MyGuiManager.MouseCursorPosition).Length() <= 0.005f))
                    {
                        if ((this.ItemDoubleClicked != null) && (this.SelectedRowIndex != null))
                        {
                            args = new EventArgs {
                                RowIndex = this.SelectedRowIndex.Value,
                                MouseButton = none
                            };
                            this.ItemDoubleClicked(this, args);
                        }
                        this.m_doubleClickStarted = null;
                        captureInput = this;
                    }
                }
            }
        }

        public void Insert(int index, Row row)
        {
            this.m_rows.Insert(index, row);
            this.RefreshScrollbar();
        }

        private bool IsValidRowIndex(int? index) => 
            ((index != null) && ((0 <= index.Value) && (index.Value < this.m_rows.Count)));

        public void MoveSelectedRowBottom()
        {
            if (this.SelectedRow != null)
            {
                Row selectedRow = this.SelectedRow;
                this.RemoveSelectedRow();
                this.m_rows.Add(selectedRow);
                this.SelectedRowIndex = new int?(this.RowsCount - 1);
            }
        }

        public void MoveSelectedRowDown()
        {
            if (this.SelectedRow != null)
            {
                int? nullable2;
                int? nullable1;
                int? selectedRowIndex = this.SelectedRowIndex;
                if (selectedRowIndex != null)
                {
                    nullable1 = new int?(selectedRowIndex.GetValueOrDefault() + 1);
                }
                else
                {
                    nullable2 = null;
                    nullable1 = nullable2;
                }
                if (this.IsValidRowIndex(nullable1))
                {
                    int? nullable3;
                    Row row = this.m_rows[this.SelectedRowIndex.Value + 1];
                    this.m_rows[this.SelectedRowIndex.Value + 1] = this.m_rows[this.SelectedRowIndex.Value];
                    this.m_rows[this.SelectedRowIndex.Value] = row;
                    selectedRowIndex = this.SelectedRowIndex;
                    if (selectedRowIndex != null)
                    {
                        nullable3 = new int?(selectedRowIndex.GetValueOrDefault() + 1);
                    }
                    else
                    {
                        nullable2 = null;
                        nullable3 = nullable2;
                    }
                    this.SelectedRowIndex = nullable3;
                }
            }
        }

        public void MoveSelectedRowTop()
        {
            if (this.SelectedRow != null)
            {
                Row selectedRow = this.SelectedRow;
                this.RemoveSelectedRow();
                this.m_rows.Insert(0, selectedRow);
                this.SelectedRowIndex = 0;
            }
        }

        public void MoveSelectedRowUp()
        {
            if (this.SelectedRow != null)
            {
                int? nullable2;
                int? nullable1;
                int? selectedRowIndex = this.SelectedRowIndex;
                if (selectedRowIndex != null)
                {
                    nullable1 = new int?(selectedRowIndex.GetValueOrDefault() - 1);
                }
                else
                {
                    nullable2 = null;
                    nullable1 = nullable2;
                }
                if (this.IsValidRowIndex(nullable1))
                {
                    int? nullable3;
                    Row row = this.m_rows[this.SelectedRowIndex.Value - 1];
                    this.m_rows[this.SelectedRowIndex.Value - 1] = this.m_rows[this.SelectedRowIndex.Value];
                    this.m_rows[this.SelectedRowIndex.Value] = row;
                    selectedRowIndex = this.SelectedRowIndex;
                    if (selectedRowIndex != null)
                    {
                        nullable3 = new int?(selectedRowIndex.GetValueOrDefault() - 1);
                    }
                    else
                    {
                        nullable2 = null;
                        nullable3 = nullable2;
                    }
                    this.SelectedRowIndex = nullable3;
                }
            }
        }

        public void MoveToNextRow()
        {
            if (this.m_rows.Count != 0)
            {
                if (this.SelectedRowIndex == null)
                {
                    this.SelectedRowIndex = 0;
                }
                else
                {
                    int num = Math.Min((int) (this.SelectedRowIndex.Value + 1), (int) (this.m_rows.Count - 1));
                    if (num != this.SelectedRowIndex.Value)
                    {
                        this.SelectedRowIndex = new int?(num);
                        EventArgs args = new EventArgs {
                            RowIndex = this.SelectedRowIndex.Value,
                            MouseButton = MyMouseButtonsEnum.Left
                        };
                        this.ItemSelected(this, args);
                        this.ScrollToSelection();
                    }
                }
            }
        }

        public void MoveToPreviousRow()
        {
            if (this.m_rows.Count != 0)
            {
                if (this.SelectedRowIndex == null)
                {
                    this.SelectedRowIndex = 0;
                }
                else
                {
                    int num = Math.Max(this.SelectedRowIndex.Value - 1, 0);
                    if (num != this.SelectedRowIndex.Value)
                    {
                        this.SelectedRowIndex = new int?(num);
                        EventArgs args = new EventArgs {
                            RowIndex = this.SelectedRowIndex.Value,
                            MouseButton = MyMouseButtonsEnum.Left
                        };
                        this.ItemSelected(this, args);
                        this.ScrollToSelection();
                    }
                }
            }
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.m_scrollBar.HasHighlight = base.HasHighlight;
        }

        protected override void OnOriginAlignChanged()
        {
            base.OnOriginAlignChanged();
            this.RefreshInternals();
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            this.RefreshInternals();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshInternals();
        }

        private void RefreshInternals()
        {
            while (this.m_columnsMetaData.Count < this.ColumnsCount)
            {
                this.m_columnsMetaData.Add(new ColumnMetaData());
            }
            Vector2 minSizeGui = this.m_styleDef.Texture.MinSizeGui;
            base.Size = Vector2.Clamp(new Vector2(base.Size.X, (this.RowHeight * (this.VisibleRowsCount + 1)) + minSizeGui.Y), minSizeGui, this.m_styleDef.Texture.MaxSizeGui);
            this.m_headerArea.Position = new Vector2(this.m_styleDef.Padding.Left, this.m_styleDef.Padding.Top);
            this.m_headerArea.Size = new Vector2(base.Size.X - ((this.m_styleDef.Padding.Left + this.m_styleDef.ScrollbarMargin.HorizontalSum) + this.m_scrollBar.Size.X), this.RowHeight);
            this.m_rowsArea.Position = this.m_headerArea.Position + (this.HeaderVisible ? new Vector2(0f, this.RowHeight) : Vector2.Zero);
            this.m_rowsArea.Size = new Vector2(this.m_headerArea.Size.X, this.RowHeight * this.VisibleRowsCount);
            this.RefreshScrollbar();
        }

        private void RefreshScrollbar()
        {
            this.m_scrollBar.Visible = this.m_rows.Count > this.VisibleRowsCount;
            this.m_scrollBar.Init((float) this.m_rows.Count, (float) this.VisibleRowsCount);
            Vector2 vector = base.Size * new Vector2(0.5f, -0.5f);
            MyGuiBorderThickness scrollbarMargin = this.m_styleDef.ScrollbarMargin;
            Vector2 position = new Vector2(vector.X - (scrollbarMargin.Right + this.m_scrollBar.Size.X), vector.Y + scrollbarMargin.Top);
            this.m_scrollBar.Layout(position, base.Size.Y - (scrollbarMargin.Top + scrollbarMargin.Bottom));
            this.m_scrollBar.ChangeValue(0f);
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = GetVisualStyle(this.VisualStyle);
            this.RowHeight = this.m_styleDef.RowHeight;
            this.TextScale = this.m_styleDef.TextScale;
            this.RefreshInternals();
        }

        public void Remove(Row row)
        {
            int index = this.m_rows.IndexOf(row);
            if (index != -1)
            {
                this.m_rows.RemoveAt(index);
                if (((this.SelectedRowIndex != null) && (this.SelectedRowIndex.Value != index)) && (this.SelectedRowIndex.Value > index))
                {
                    this.SelectedRowIndex = new int?(this.SelectedRowIndex.Value - 1);
                }
            }
        }

        public void Remove(Predicate<Row> match)
        {
            int index = this.m_rows.FindIndex(match);
            if (index != -1)
            {
                this.m_rows.RemoveAt(index);
                if (((this.SelectedRowIndex != null) && (this.SelectedRowIndex.Value != index)) && (this.SelectedRowIndex.Value > index))
                {
                    this.SelectedRowIndex = new int?(this.SelectedRowIndex.Value - 1);
                }
            }
        }

        public void RemoveSelectedRow()
        {
            if ((this.SelectedRowIndex != null) && (this.SelectedRowIndex.Value < this.m_rows.Count))
            {
                this.m_rows.RemoveAt(this.SelectedRowIndex.Value);
                int? selectedRowIndex = this.SelectedRowIndex;
                if (!this.IsValidRowIndex(new int?(selectedRowIndex.Value)))
                {
                    selectedRowIndex = null;
                    this.SelectedRowIndex = selectedRowIndex;
                }
                this.RefreshScrollbar();
            }
        }

        public void ScrollToSelection()
        {
            if (this.SelectedRow == null)
            {
                this.m_visibleRowIndexOffset = 0;
            }
            else
            {
                int num = this.SelectedRowIndex.Value;
                if (num > ((this.m_visibleRowIndexOffset + this.VisibleRowsCount) - 1))
                {
                    this.m_scrollBar.Value = (num - this.VisibleRowsCount) + 1;
                }
                if (num < this.m_visibleRowIndexOffset)
                {
                    this.m_scrollBar.Value = num;
                }
            }
        }

        public void SetColumnAlign(int colIdx, MyGuiDrawAlignEnum align = 1)
        {
            this.m_columnsMetaData[colIdx].TextAlign = align;
        }

        public void SetColumnComparison(int colIdx, Comparison<Cell> ascendingComparison)
        {
            this.m_columnsMetaData[colIdx].AscendingComparison = ascendingComparison;
        }

        public void SetColumnName(int colIdx, StringBuilder name)
        {
            this.m_columnsMetaData[colIdx].Name.Clear().AppendStringBuilder(name);
        }

        public void SetCustomColumnWidths(float[] p)
        {
            for (int i = 0; i < this.ColumnsCount; i++)
            {
                this.m_columnsMetaData[i].Width = p[i];
            }
        }

        public void SetHeaderColumnAlign(int colIdx, MyGuiDrawAlignEnum align = 1)
        {
            this.m_columnsMetaData[colIdx].HeaderTextAlign = align;
        }

        public override void ShowToolTip()
        {
            MyToolTips toolTip = base.m_toolTip;
            if ((this.m_mouseOverRowIndex != null) && this.m_rows.IsValidIndex<Row>(this.m_mouseOverRowIndex.Value))
            {
                Row row = this.m_rows[this.m_mouseOverRowIndex.Value];
                if (row.Cells.IsValidIndex<Cell>(this.m_mouseOverColumnIndex.Value))
                {
                    Cell cell = row.Cells[this.m_mouseOverColumnIndex.Value];
                    if (cell.ToolTip != null)
                    {
                        base.m_toolTip = cell.ToolTip;
                    }
                }
                if (this.ItemMouseOver != null)
                {
                    this.ItemMouseOver(row);
                }
            }
            using (List<MyGuiControlBase>.Enumerator enumerator = this.Controls.GetVisibleControls().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ShowToolTip();
                }
            }
            base.ShowToolTip();
            base.m_toolTip = toolTip;
        }

        public void Sort(bool switchSort = true)
        {
            if (this.m_sortColumn != -1)
            {
                SortStateEnum? sortState = null;
                this.SortByColumn(this.m_sortColumn, sortState, switchSort);
            }
        }

        public void SortByColumn(int columnIdx, SortStateEnum? sortState = new SortStateEnum?(), bool switchSort = true)
        {
            columnIdx = MathHelper.Clamp(columnIdx, 0, this.m_columnsMetaData.Count - 1);
            this.m_sortColumn = columnIdx;
            this.m_sortColumnState = (sortState != null) ? new SortStateEnum?(sortState.Value) : this.m_sortColumnState;
            ColumnMetaData data = this.m_columnsMetaData[columnIdx];
            SortStateEnum enum2 = data.SortState;
            this.m_columnsMetaData[this.m_lastSortedColumnIdx].SortState = SortStateEnum.Unsorted;
            Comparison<Cell> comparison = data.AscendingComparison;
            if (comparison != null)
            {
                SortStateEnum enum3 = enum2;
                if (switchSort)
                {
                    enum3 = (enum2 == SortStateEnum.Ascending) ? SortStateEnum.Descending : SortStateEnum.Ascending;
                }
                if (sortState != null)
                {
                    enum3 = sortState.Value;
                }
                else if (this.m_sortColumnState != null)
                {
                    enum3 = this.m_sortColumnState.Value;
                }
                Row item = null;
                if (this.IgnoreFirstRowForSort && (this.m_rows.Count > 0))
                {
                    item = this.m_rows[0];
                    this.m_rows.RemoveAt(0);
                }
                List<Row> collection = (from r in this.m_rows
                    where !r.IsGlobalSortEnabled
                    select r).ToList<Row>();
                foreach (Row row2 in collection)
                {
                    this.m_rows.Remove(row2);
                }
                if (enum3 == SortStateEnum.Ascending)
                {
                    this.m_rows.Sort((a, b) => Compare(columnIdx, comparison, a, b));
                    collection.Sort((a, b) => Compare(columnIdx, comparison, a, b));
                }
                else
                {
                    this.m_rows.Sort((a, b) => Compare(columnIdx, comparison, b, a));
                    collection.Sort((a, b) => Compare(columnIdx, comparison, b, a));
                }
                if (item != null)
                {
                    this.m_rows.Insert(0, item);
                }
                this.m_rows.InsertRange(0, collection);
                this.m_lastSortedColumnIdx = columnIdx;
                data.SortState = enum3;
                this.SelectedRowIndex = null;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!base.IsMouseOver)
            {
                this.m_mouseOverColumnIndex = null;
                this.m_mouseOverRowIndex = null;
                this.m_mouseOverHeader = false;
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Down))
            {
                this.MoveToNextRow();
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Up))
            {
                this.MoveToPreviousRow();
            }
        }

        private void verticalScrollBar_ValueChanged(MyScrollbar scrollbar)
        {
            this.m_visibleRowIndexOffset = (int) scrollbar.Value;
        }

        public MyGuiControls Controls =>
            this.m_controls;

        public MyVScrollbar ScrollBar =>
            this.m_scrollBar;

        public bool HeaderVisible
        {
            get => 
                this.m_headerVisible;
            set
            {
                this.m_headerVisible = value;
                this.RefreshInternals();
            }
        }

        public int ColumnsCount
        {
            get => 
                this.m_columnsCount;
            set
            {
                this.m_columnsCount = value;
                this.RefreshInternals();
            }
        }

        public int? SelectedRowIndex
        {
            get => 
                this.m_selectedRowIndex;
            set => 
                (this.m_selectedRowIndex = value);
        }

        public Row SelectedRow
        {
            get
            {
                if (this.IsValidRowIndex(this.SelectedRowIndex))
                {
                    return this.m_rows[this.SelectedRowIndex.Value];
                }
                return null;
            }
            set
            {
                int index = this.m_rows.IndexOf(value);
                if (index >= 0)
                {
                    this.m_selectedRowIndex = new int?(index);
                }
            }
        }

        public int VisibleRowsCount
        {
            get => 
                this.m_visibleRows;
            set
            {
                this.m_visibleRows = value;
                this.RefreshInternals();
            }
        }

        public bool IgnoreFirstRowForSort { get; set; }

        public float RowHeight { get; private set; }

        public MyGuiControlTableStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public float TextScale
        {
            get => 
                this.m_textScale;
            private set
            {
                this.m_textScale = value;
                this.TextScaleWithLanguage = value * MyGuiManager.LanguageTextScale;
            }
        }

        public float TextScaleWithLanguage
        {
            get => 
                this.m_textScaleWithLanguage;
            private set => 
                (this.m_textScaleWithLanguage = value);
        }

        public int RowsCount =>
            this.m_rows.Count;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiControlTable.<>c <>9 = new MyGuiControlTable.<>c();
            public static Func<MyGuiControlTable.Row, bool> <>9__123_0;

            internal bool <SortByColumn>b__123_0(MyGuiControlTable.Row r) => 
                !r.IsGlobalSortEnabled;
        }

        public class Cell
        {
            public readonly StringBuilder Text;
            public readonly object UserData;
            public readonly MyToolTips ToolTip;
            public MyGuiHighlightTexture? Icon;
            public readonly MyGuiDrawAlignEnum IconOriginAlign;
            public Color? TextColor;
            public MyGuiControlBase Control;
            public Sandbox.Graphics.GUI.MyGuiControlTable.Row Row;
            private StringBuilder text;
            private StringBuilder toolTip;

            public Cell(string text = null, object userData = null, string toolTip = null, Color? textColor = new Color?(), MyGuiHighlightTexture? icon = new MyGuiHighlightTexture?(), MyGuiDrawAlignEnum iconOriginAlign = 6)
            {
                if (text != null)
                {
                    this.Text = new StringBuilder().Append(text);
                }
                if (toolTip != null)
                {
                    this.ToolTip = new MyToolTips(toolTip);
                }
                this.UserData = userData;
                this.Icon = icon;
                this.IconOriginAlign = iconOriginAlign;
                this.TextColor = textColor;
            }

            public Cell(StringBuilder text, object userData = null, string toolTip = null, Color? textColor = new Color?(), MyGuiHighlightTexture? icon = new MyGuiHighlightTexture?(), MyGuiDrawAlignEnum iconOriginAlign = 6)
            {
                if (text != null)
                {
                    this.Text = new StringBuilder().AppendStringBuilder(text);
                }
                if (toolTip != null)
                {
                    this.ToolTip = new MyToolTips(toolTip);
                }
                this.UserData = userData;
                this.Icon = icon;
                this.IconOriginAlign = iconOriginAlign;
                this.TextColor = textColor;
            }

            public virtual void Update()
            {
            }
        }

        private class ColumnMetaData
        {
            public StringBuilder Name = new StringBuilder();
            public float Width;
            public Comparison<MyGuiControlTable.Cell> AscendingComparison;
            public MyGuiControlTable.SortStateEnum SortState = MyGuiControlTable.SortStateEnum.Unsorted;
            public MyGuiDrawAlignEnum TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            public MyGuiDrawAlignEnum HeaderTextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
        }

        public delegate bool EqualUserData(object first, object second);

        [StructLayout(LayoutKind.Sequential)]
        public struct EventArgs
        {
            public int RowIndex;
            public MyMouseButtonsEnum MouseButton;
        }

        public class Row
        {
            internal readonly List<MyGuiControlTable.Cell> Cells;
            public readonly object UserData;

            public Row(object userData = null)
            {
                this.IsGlobalSortEnabled = true;
                this.UserData = userData;
                this.Cells = new List<MyGuiControlTable.Cell>();
            }

            public void AddCell(MyGuiControlTable.Cell cell)
            {
                this.Cells.Add(cell);
                cell.Row = this;
            }

            public MyGuiControlTable.Cell GetCell(int cell) => 
                this.Cells[cell];

            public void Update()
            {
                using (List<MyGuiControlTable.Cell>.Enumerator enumerator = this.Cells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Update();
                    }
                }
            }

            public bool IsGlobalSortEnabled { get; set; }
        }

        public enum SortStateEnum
        {
            Unsorted,
            Ascending,
            Descending
        }

        public class StyleDefinition
        {
            public string HeaderFontHighlight;
            public string HeaderFontNormal;
            public string HeaderTextureHighlight;
            public MyGuiBorderThickness Padding;
            public string RowFontHighlight;
            public string RowFontNormal;
            public float RowHeight;
            public string RowTextureHighlight;
            public float TextScale;
            public MyGuiBorderThickness ScrollbarMargin;
            public MyGuiCompositeTexture Texture;
        }
    }
}

