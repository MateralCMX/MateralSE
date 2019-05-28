namespace Sandbox.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyGuiControlLayoutGrid : MyGuiControlBase
    {
        private GridLength[] m_columns;
        private GridLength[] m_rows;
        private List<MyGuiControlBase>[,] m_controlTable;

        public MyGuiControlLayoutGrid(GridLength[] columns, GridLength[] rows) : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            this.m_columns = columns;
            this.m_rows = rows;
            this.m_controlTable = new List<MyGuiControlBase>[rows.Length, columns.Length];
        }

        public bool Add(MyGuiControlBase control, int column, int row)
        {
            if (((row < 0) || ((row >= this.m_rows.Length) || (column < 0))) || (column >= this.m_columns.Length))
            {
                return false;
            }
            if (this.m_controlTable[row, column] == null)
            {
                this.m_controlTable[row, column] = new List<MyGuiControlBase>();
            }
            this.m_controlTable[row, column].Add(control);
            return true;
        }

        public List<MyGuiControlBase> GetAllControls()
        {
            List<MyGuiControlBase> list = new List<MyGuiControlBase>();
            int num = 0;
            while (num < this.m_rows.Length)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_columns.Length)
                    {
                        num++;
                        break;
                    }
                    if (this.m_controlTable[num, num2] != null)
                    {
                        foreach (MyGuiControlBase base2 in this.m_controlTable[num, num2])
                        {
                            list.Add(base2);
                        }
                    }
                    num2++;
                }
            }
            return list;
        }

        public List<MyGuiControlBase> GetControlsAt(int column, int row)
        {
            if (((row < 0) || ((row >= this.m_rows.Length) || (column < 0))) || (column >= this.m_columns.Length))
            {
                return null;
            }
            return this.m_controlTable[row, column];
        }

        public MyGuiControlBase GetFirstControlAt(int column, int row)
        {
            if (((row < 0) || ((row >= this.m_rows.Length) || ((column < 0) || (column >= this.m_columns.Length)))) || (this.m_controlTable[row, column].Count <= 0))
            {
                return null;
            }
            return this.m_controlTable[row, column][0];
        }

        public override unsafe void UpdateArrange()
        {
            base.UpdateArrange();
            Vector2 size = base.Size;
            Vector2 zero = Vector2.Zero;
            foreach (GridLength length in this.m_rows)
            {
                if (length.UnitType == GridUnitType.FixValue)
                {
                    float* singlePtr1 = (float*) ref size.Y;
                    singlePtr1[0] -= length.Size;
                }
                else
                {
                    float* singlePtr2 = (float*) ref zero.Y;
                    singlePtr2[0] += length.Size;
                }
            }
            foreach (GridLength length2 in this.m_columns)
            {
                if (length2.UnitType == GridUnitType.FixValue)
                {
                    float* singlePtr3 = (float*) ref size.X;
                    singlePtr3[0] -= length2.Size;
                }
                else
                {
                    float* singlePtr4 = (float*) ref zero.X;
                    singlePtr4[0] += length2.Size;
                }
            }
            Vector2* vectorPtr1 = (Vector2*) new Vector2((zero.X > 0f) ? (size.X / zero.X) : 0f, (zero.Y > 0f) ? (size.Y / zero.Y) : 0f);
            Vector2 position = base.Position;
            int index = 0;
            while (index < this.m_rows.Length)
            {
                position.X = base.PositionX;
                int num3 = 0;
                while (true)
                {
                    Vector2 vector3;
                    if (num3 >= this.m_columns.Length)
                    {
                        if (this.m_rows[index].UnitType == GridUnitType.FixValue)
                        {
                            float* singlePtr7 = (float*) ref position.Y;
                            singlePtr7[0] += this.m_rows[index].Size;
                        }
                        else
                        {
                            float* singlePtr8 = (float*) ref position.Y;
                            singlePtr8[0] += this.m_rows[index].Size * vector3.Y;
                        }
                        index++;
                        break;
                    }
                    if (this.m_controlTable[index, num3] != null)
                    {
                        foreach (MyGuiControlBase base2 in this.m_controlTable[index, num3])
                        {
                            if (base2 != null)
                            {
                                base2.Position = new Vector2(position.X + base2.Margin.Left, position.Y + base2.Margin.Top);
                                base2.UpdateArrange();
                            }
                        }
                    }
                    if (this.m_columns[num3].UnitType == GridUnitType.FixValue)
                    {
                        float* singlePtr5 = (float*) ref position.X;
                        singlePtr5[0] += this.m_columns[num3].Size;
                    }
                    else
                    {
                        float* singlePtr6 = (float*) ref position.X;
                        vectorPtr1 = (Vector2*) ref vector3;
                        singlePtr6[0] += this.m_columns[num3].Size * vector3.X;
                    }
                    num3++;
                }
            }
        }

        public override void UpdateMeasure()
        {
            base.UpdateMeasure();
            int num = 0;
            while (num < this.m_rows.Length)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_columns.Length)
                    {
                        num++;
                        break;
                    }
                    if (this.m_controlTable[num, num2] != null)
                    {
                        foreach (MyGuiControlBase base2 in this.m_controlTable[num, num2])
                        {
                            if (base2 != null)
                            {
                                base2.UpdateMeasure();
                            }
                        }
                    }
                    num2++;
                }
            }
        }
    }
}

