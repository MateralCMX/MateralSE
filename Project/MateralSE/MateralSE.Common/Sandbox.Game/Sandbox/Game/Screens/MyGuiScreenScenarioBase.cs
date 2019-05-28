namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenScenarioBase : MyGuiScreenBase
    {
        protected StateEnum m_state;
        protected MyGuiControlTextbox m_nameTextbox;
        protected MyGuiControlTextbox m_descriptionTextbox;
        protected MyGuiControlButton m_okButton;
        protected MyGuiControlButton m_cancelButton;
        protected MyGuiControlTable m_scenarioTable;
        protected MyGuiControlMultilineText m_descriptionBox;
        protected MyLayoutTable m_sideMenuLayout;
        protected MyLayoutTable m_buttonsLayout;
        protected int m_selectedRow;
        protected const float MARGIN_TOP = 0.1f;
        protected const float MARGIN_LEFT = 0.42f;
        protected const string WORKSHOP_PATH_TAG = "workshop";
        private List<Tuple<string, MyWorldInfo>> m_availableSaves;

        public MyGuiScreenScenarioBase() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(CalcSize(null)), false, null, 0f, 0f)
        {
            this.m_availableSaves = new List<Tuple<string, MyWorldInfo>>();
        }

        protected void AddSave(Tuple<string, MyWorldInfo> save)
        {
            this.m_availableSaves.Add(save);
        }

        protected void AddSaves(List<Tuple<string, MyWorldInfo>> saves)
        {
            this.m_availableSaves.AddList<Tuple<string, MyWorldInfo>>(saves);
        }

        protected virtual void BuildControls()
        {
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(this.ScreenCaption, captionTextColor, captionOffset, 0.8f);
            MyGuiControlLabel control = this.MakeLabel(MyCommonTexts.Name);
            MyGuiControlLabel label2 = this.MakeLabel(MyCommonTexts.Description);
            captionOffset = null;
            captionTextColor = null;
            this.m_nameTextbox = new MyGuiControlTextbox(captionOffset, null, 0x80, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_nameTextbox.Enabled = false;
            captionOffset = null;
            captionTextColor = null;
            this.m_descriptionTextbox = new MyGuiControlTextbox(captionOffset, null, 0x1f3f, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_descriptionTextbox.Enabled = false;
            Vector2 vector1 = new Vector2(0f, 0.052f);
            Vector2 topLeft = (-base.m_size.Value / 2f) + new Vector2(0.42f, 0.1f);
            Vector2 size = (base.m_size.Value / 2f) - topLeft;
            float num = size.X * 0.25f;
            float num2 = size.X - num;
            float num3 = 0.052f;
            size.Y = num3 * 5f;
            this.m_sideMenuLayout = new MyLayoutTable(this, topLeft, size);
            float[] widthsPx = new float[] { num, num2 };
            this.m_sideMenuLayout.SetColumnWidthsNormalized(widthsPx);
            float[] heightsPx = new float[] { num3, num3, num3, num3, num3 };
            this.m_sideMenuLayout.SetRowHeightsNormalized(heightsPx);
            this.m_sideMenuLayout.Add(control, MyAlignH.Left, MyAlignV.Top, 0, 0, 1, 1);
            this.m_sideMenuLayout.Add(this.m_nameTextbox, MyAlignH.Left, MyAlignV.Top, 0, 1, 1, 1);
            this.m_sideMenuLayout.Add(label2, MyAlignH.Left, MyAlignV.Top, 1, 0, 1, 1);
            this.m_sideMenuLayout.Add(this.m_descriptionTextbox, MyAlignH.Left, MyAlignV.Top, 1, 1, 1, 1);
            MyGuiControlPanel panel1 = new MyGuiControlPanel();
            panel1.Name = "BriefingPanel";
            panel1.Position = new Vector2(-0.02f, -0.12f);
            panel1.Size = new Vector2(0.43f, 0.422f);
            panel1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            MyGuiControlPanel panel = panel1;
            this.Controls.Add(panel);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(captionOffset, captionOffset, captionTextColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            text1.Name = "BriefingMultilineText";
            text1.Position = new Vector2(-0.009f, -0.115f);
            text1.Size = new Vector2(0.419f, 0.412f);
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_descriptionBox = text1;
            this.Controls.Add(this.m_descriptionBox);
            int count = 2;
            int num5 = 4;
            Vector2 vector3 = new Vector2(0.1875f, 0.05833333f);
            Vector2 vector4 = (base.m_size.Value / 2f) - new Vector2(0.83f, 0.16f);
            Vector2 vector5 = new Vector2(0.01f, 0.01f);
            Vector2 vector6 = new Vector2((vector3.X + vector5.X) * num5, (vector3.Y + vector5.Y) * count);
            this.m_buttonsLayout = new MyLayoutTable(this, vector4, vector6);
            float[] numArray = Enumerable.Repeat<float>(vector3.X + vector5.X, num5).ToArray<float>();
            this.m_buttonsLayout.SetColumnWidthsNormalized(numArray);
            float[] numArray2 = Enumerable.Repeat<float>(vector3.Y + vector5.Y, count).ToArray<float>();
            this.m_buttonsLayout.SetRowHeightsNormalized(numArray2);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_okButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            visibleLinesCount = null;
            this.m_cancelButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Default, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelButtonClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_buttonsLayout.Add(this.m_okButton, MyAlignH.Left, MyAlignV.Top, 1, 2, 1, 1);
            this.m_buttonsLayout.Add(this.m_cancelButton, MyAlignH.Left, MyAlignV.Top, 1, 3, 1, 1);
            this.m_scenarioTable = this.CreateScenarioTable();
            this.Controls.Add(this.m_scenarioTable);
        }

        private static Vector2 CalcSize(MyObjectBuilder_Checkpoint checkpoint)
        {
            float num = (checkpoint == null) ? 1.24f : 0.97f;
            if (checkpoint != null)
            {
                num -= 0.05f;
            }
            return new Vector2((checkpoint == null) ? 0.9f : 0.65f, num - 0.27f);
        }

        protected void ClearSaves()
        {
            this.m_availableSaves.Clear();
        }

        protected virtual MyGuiControlTable CreateScenarioTable()
        {
            MyGuiControlTable table = new MyGuiControlTable {
                Position = new Vector2(-0.42f, -0.4f),
                Size = new Vector2(0.38f, 1.8f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                VisibleRowsCount = 20,
                ColumnsCount = 2
            };
            float[] p = new float[] { 0.085f, 0.905f };
            table.SetCustomColumnWidths(p);
            table.SetColumnName(1, MyTexts.Get(MyCommonTexts.Name));
            table.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            return table;
        }

        public override bool Draw() => 
            ((this.m_state == StateEnum.ListLoaded) ? base.Draw() : false);

        protected virtual void FillList()
        {
            this.m_state = StateEnum.ListLoading;
        }

        protected void FillRight()
        {
            if ((this.m_scenarioTable == null) || (this.m_scenarioTable.SelectedRow == null))
            {
                this.m_nameTextbox.SetText(new StringBuilder(""));
                this.m_descriptionTextbox.SetText(new StringBuilder(""));
            }
            else
            {
                Tuple<string, MyWorldInfo> tuple = this.FindSave(this.m_scenarioTable.SelectedRow);
                this.m_nameTextbox.SetText(new StringBuilder(MyTexts.GetString(tuple.Item2.SessionName)));
                this.m_descriptionTextbox.SetText(new StringBuilder(tuple.Item2.Description));
                this.m_descriptionBox.Text = new StringBuilder(MyTexts.GetString(tuple.Item2.Briefing));
            }
        }

        protected Tuple<string, MyWorldInfo> FindSave(MyGuiControlTable.Row row) => 
            ((Tuple<string, MyWorldInfo>) row.UserData);

        protected virtual MyGuiHighlightTexture GetIcon(Tuple<string, MyWorldInfo> save) => 
            MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL;

        private void LoadSandbox(bool MP)
        {
            MyLog.Default.WriteLine("LoadSandbox() - Start");
            MyGuiControlTable.Row selectedRow = this.m_scenarioTable.SelectedRow;
            if (selectedRow != null)
            {
                Tuple<string, MyWorldInfo> save = this.FindSave(selectedRow);
                if (save != null)
                {
                    this.LoadSandboxInternal(save, MP);
                }
            }
            MyLog.Default.WriteLine("LoadSandbox() - End");
        }

        protected virtual void LoadSandboxInternal(Tuple<string, MyWorldInfo> save, bool MP)
        {
        }

        protected MyGuiControlLabel MakeLabel(MyStringId textEnum)
        {
            Vector2? position = null;
            position = null;
            return new MyGuiControlLabel(position, position, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void OnCancelButtonClick(object sender)
        {
            this.CloseScreen();
        }

        protected void OnOkButtonClick(object sender)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            if ((this.m_nameTextbox.Text.Length < 5) || (this.m_nameTextbox.Text.Length > 0x80))
            {
                MyStringId id = (this.m_nameTextbox.Text.Length >= 5) ? MyCommonTexts.ErrorNameTooLong : MyCommonTexts.ErrorNameTooShort;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(id), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                screen.SkipTransition = true;
                screen.InstantClose = false;
                MyGuiSandbox.AddScreen(screen);
            }
            else if (this.m_descriptionTextbox.Text.Length <= 0x1f3f)
            {
                this.CloseScreen();
                this.LoadSandbox(this.IsOnlineMode);
            }
            else
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.ErrorDescriptionTooLong), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                screen.SkipTransition = true;
                screen.InstantClose = false;
                MyGuiSandbox.AddScreen(screen);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            if (this.m_state == StateEnum.ListNeedsReload)
            {
                this.FillList();
            }
        }

        protected virtual void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            this.m_selectedRow = eventArgs.RowIndex;
            this.FillRight();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.BuildControls();
            this.SetDefaultValues();
        }

        protected void RefreshGameList()
        {
            int? selectedRowIndex = this.m_scenarioTable.SelectedRowIndex;
            int num = (selectedRowIndex != null) ? selectedRowIndex.GetValueOrDefault() : -1;
            this.m_scenarioTable.Clear();
            Color? textColor = null;
            for (int i = 0; i < this.m_availableSaves.Count; i++)
            {
                StringBuilder text = new StringBuilder(this.m_availableSaves[i].Item2.SessionName);
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(this.m_availableSaves[i]);
                row.AddCell(new MyGuiControlTable.Cell(string.Empty, null, null, textColor, new MyGuiHighlightTexture?(this.GetIcon(this.m_availableSaves[i])), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                Color? nullable3 = textColor;
                MyGuiHighlightTexture? icon = null;
                row.AddCell(new MyGuiControlTable.Cell(text, text, null, nullable3, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                this.m_scenarioTable.Add(row);
                if (i == num)
                {
                    this.m_selectedRow = i;
                    this.m_scenarioTable.SelectedRow = row;
                }
            }
            this.m_scenarioTable.SelectedRowIndex = new int?(this.m_selectedRow);
            this.m_scenarioTable.ScrollToSelection();
            this.FillRight();
        }

        protected virtual void SetDefaultValues()
        {
            this.FillRight();
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_state == StateEnum.ListNeedsReload)
            {
                this.FillList();
            }
            this.m_okButton.Enabled = this.m_scenarioTable.SelectedRow != null;
            return base.Update(hasFocus);
        }

        protected abstract MyStringId ScreenCaption { get; }

        protected abstract bool IsOnlineMode { get; }

        protected enum StateEnum
        {
            ListNeedsReload,
            ListLoading,
            ListLoaded
        }
    }
}

