namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;

    public class MyGuiScreenTriggerBlockDestroyed : MyGuiScreenTrigger
    {
        private MyGuiControlTable m_selectedBlocks;
        private MyGuiControlButton m_buttonPaste;
        private MyGuiControlButton m_buttonDelete;
        private MyGuiControlTextbox m_textboxSingleMessage;
        private MyGuiControlLabel m_labelSingleMessage;
        private MyTriggerBlockDestroyed trigger;
        private static StringBuilder m_tempSb = new StringBuilder();

        public MyGuiScreenTriggerBlockDestroyed(MyTrigger trig) : base(trig, new Vector2(0.5f, 0.8f))
        {
            this.trigger = (MyTriggerBlockDestroyed) trig;
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionBlockDestroyed, captionTextColor, captionOffset, 0.8f);
            MyLayoutTable table = new MyLayoutTable(this);
            table.SetColumnWidthsNormalized(new float[] { 10f, 30f, 3f, 30f, 10f });
            table.SetRowHeightsNormalized(new float[] { 20f, 35f, 6f, 4f, 4f, 5f, 33f });
            this.m_selectedBlocks = new MyGuiControlTable();
            this.m_selectedBlocks.VisibleRowsCount = 8;
            this.m_selectedBlocks.ColumnsCount = 1;
            float[] p = new float[] { 1f };
            this.m_selectedBlocks.SetCustomColumnWidths(p);
            this.m_selectedBlocks.SetColumnName(0, MyTexts.Get(MySpaceTexts.GuiTriggerBlockDestroyed_ColumnName));
            table.AddWithSize(this.m_selectedBlocks, MyAlignH.Left, MyAlignV.Top, 1, 1, 1, 3);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_buttonPaste = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Rectangular, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.GuiTriggerPasteBlocks), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnPasteButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_buttonPaste.SetToolTip(MySpaceTexts.GuiTriggerPasteBlocksTooltip);
            table.AddWithSize(this.m_buttonPaste, MyAlignH.Left, MyAlignV.Top, 2, 1, 1, 1);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_buttonDelete = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Rectangular, captionOffset, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.GuiTriggerDeleteBlocks), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnDeleteButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            table.AddWithSize(this.m_buttonDelete, MyAlignH.Left, MyAlignV.Top, 2, 3, 1, 1);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            this.m_labelSingleMessage = new MyGuiControlLabel(captionOffset, captionOffset, MyTexts.Get(MySpaceTexts.GuiTriggerBlockDestroyedSingleMessage).ToString(), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            table.AddWithSize(this.m_labelSingleMessage, MyAlignH.Left, MyAlignV.Top, 3, 1, 1, 1);
            captionOffset = null;
            captionTextColor = null;
            this.m_textboxSingleMessage = new MyGuiControlTextbox(captionOffset, this.trigger.SingleMessage, 0x55, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            table.AddWithSize(this.m_textboxSingleMessage, MyAlignH.Left, MyAlignV.Top, 4, 1, 1, 3);
            foreach (KeyValuePair<MyTerminalBlock, MyTriggerBlockDestroyed.BlockState> pair in this.trigger.Blocks)
            {
                this.AddRow(pair.Key);
            }
            m_tempSb.Clear().Append(this.trigger.SingleMessage);
            this.m_textboxSingleMessage.SetText(m_tempSb);
        }

        private void AddRow(MyTerminalBlock block)
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(block);
            Color? textColor = null;
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(block.CustomName, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            this.m_selectedBlocks.Add(row);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerBlockDestroyed";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Delete))
            {
                this.m_selectedBlocks.RemoveSelectedRow();
            }
            base.HandleInput(receivedFocusInThisUpdate);
        }

        private void OnDeleteButtonClick(MyGuiControlButton sender)
        {
            this.m_selectedBlocks.RemoveSelectedRow();
        }

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            this.trigger.Blocks.Clear();
            for (int i = 0; i < this.m_selectedBlocks.RowsCount; i++)
            {
                this.trigger.Blocks.Add((MyTerminalBlock) this.m_selectedBlocks.GetRow(i).UserData, MyTriggerBlockDestroyed.BlockState.Ok);
            }
            this.trigger.SingleMessage = this.m_textboxSingleMessage.Text;
            base.OnOkButtonClick(sender);
        }

        private void OnPasteButtonClick(MyGuiControlButton sender)
        {
            foreach (MyTerminalBlock block in MyScenarioBuildingBlock.Clipboard)
            {
                int index = 0;
                while (true)
                {
                    if ((index >= this.m_selectedBlocks.RowsCount) || (this.m_selectedBlocks.GetRow(index).UserData == block))
                    {
                        if (index == this.m_selectedBlocks.RowsCount)
                        {
                            this.AddRow(block);
                        }
                        break;
                    }
                    index++;
                }
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_selectedBlocks.SelectedRowIndex != null)
            {
                int? selectedRowIndex = this.m_selectedBlocks.SelectedRowIndex;
                int rowsCount = this.m_selectedBlocks.RowsCount;
                if ((selectedRowIndex.GetValueOrDefault() < rowsCount) & (selectedRowIndex != null))
                {
                    this.m_buttonDelete.Enabled = true;
                    goto TR_0000;
                }
            }
            this.m_buttonDelete.Enabled = false;
        TR_0000:
            return base.Update(hasFocus);
        }
    }
}

