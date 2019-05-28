namespace Sandbox.Game.Gui
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using VRage;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalGpsController
    {
        private IMyGuiControlsParent m_controlsParent;
        private MyGuiControlSearchBox m_searchBox;
        private MyGuiControlTable m_tableIns;
        private MyGuiControlLabel m_labelInsName;
        private MyGuiControlTextbox m_panelInsName;
        private MyGuiControlLabel m_labelInsDesc;
        private MyGuiControlTextbox m_panelInsDesc;
        private MyGuiControlLabel m_labelInsX;
        private MyGuiControlTextbox m_xCoord;
        private MyGuiControlLabel m_labelInsY;
        private MyGuiControlTextbox m_yCoord;
        private MyGuiControlLabel m_labelInsZ;
        private MyGuiControlTextbox m_zCoord;
        private MyGuiControlLabel m_labelInsShowOnHud;
        private MyGuiControlCheckbox m_checkInsShowOnHud;
        private MyGuiControlLabel m_labelInsAlwaysVisible;
        private MyGuiControlCheckbox m_checkInsAlwaysVisible;
        private MyGuiControlButton m_buttonAdd;
        private MyGuiControlButton m_buttonAddFromClipboard;
        private MyGuiControlButton m_buttonAddCurrent;
        private MyGuiControlButton m_buttonDelete;
        private MyGuiControlButton m_buttonCopy;
        private MyGuiControlLabel m_labelSaveWarning;
        public static readonly Color ITEM_SHOWN_COLOR = Color.CornflowerBlue;
        private int? m_previousHash;
        private bool m_needsSyncName;
        private bool m_needsSyncDesc;
        private bool m_needsSyncX;
        private bool m_needsSyncY;
        private bool m_needsSyncZ;
        private bool m_nameOk;
        private bool m_xOk;
        private bool m_yOk;
        private bool m_zOk;
        private MyGps m_syncedGps;
        private StringBuilder m_NameBuilder = new StringBuilder();
        private string m_clipboardText;

        private MyGuiControlTable.Row AddToList(MyGps ins)
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(ins);
            StringBuilder text = new StringBuilder(ins.Name);
            string toolTip = text.ToString();
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(text, ins, toolTip, new Color?((ins.DiscardAt != null) ? Color.Gray : (ins.ShowOnHud ? ITEM_SHOWN_COLOR : Color.White)), icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            this.m_tableIns.Add(row);
            return row;
        }

        public void ClearList()
        {
            if (this.m_tableIns != null)
            {
                this.m_tableIns.Clear();
            }
        }

        private void ClearRight()
        {
            this.UnhookSyncEvents();
            StringBuilder source = new StringBuilder("");
            this.m_panelInsName.SetText(source);
            this.m_panelInsDesc.SetText(source);
            this.m_xCoord.SetText(source);
            this.m_yCoord.SetText(source);
            this.m_zCoord.SetText(source);
            this.m_checkInsShowOnHud.IsChecked = false;
            this.m_checkInsAlwaysVisible.IsChecked = false;
            this.m_previousHash = null;
            this.HookSyncEvents();
            this.m_needsSyncName = false;
            this.m_needsSyncDesc = false;
            this.m_needsSyncX = false;
            this.m_needsSyncY = false;
            this.m_needsSyncZ = false;
        }

        public void Close()
        {
            this.trySync();
            if (this.m_tableIns != null)
            {
                this.ClearList();
                this.m_tableIns.ItemSelected -= new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
                this.m_tableIns.ItemDoubleClicked -= new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableDoubleclick);
            }
            this.m_syncedGps = null;
            MySession.Static.Gpss.GpsChanged -= new Action<long, int>(this.OnInsChanged);
            MySession.Static.Gpss.ListChanged -= new Action<long>(this.OnListChanged);
            this.UnhookSyncEvents();
            this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
            this.m_checkInsAlwaysVisible.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkInsAlwaysVisible.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAlwaysVisibleChecked));
            this.m_buttonAdd.ButtonClicked -= new Action<MyGuiControlButton>(this.OnButtonPressedNew);
            this.m_buttonAddFromClipboard.ButtonClicked -= new Action<MyGuiControlButton>(this.OnButtonPressedNewFromClipboard);
            this.m_buttonAddCurrent.ButtonClicked -= new Action<MyGuiControlButton>(this.OnButtonPressedNewFromCurrent);
            this.m_buttonDelete.ButtonClicked -= new Action<MyGuiControlButton>(this.OnButtonPressedDelete);
            this.m_buttonCopy.ButtonClicked -= new Action<MyGuiControlButton>(this.OnButtonPressedCopy);
        }

        private void Delete()
        {
            MySession.Static.Gpss.SendDelete(MySession.Static.LocalPlayerId, ((MyGps) this.m_tableIns.SelectedRow.UserData).GetHashCode());
            this.PopulateList();
            this.enableEditBoxes(false);
            this.m_buttonDelete.Enabled = false;
            this.m_buttonDelete.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Delete_Disabled_ToolTip));
        }

        private void enableEditBoxes(bool enable)
        {
            this.m_panelInsName.Enabled = enable;
            this.m_panelInsDesc.Enabled = enable;
            this.m_xCoord.Enabled = enable;
            this.m_yCoord.Enabled = enable;
            this.m_zCoord.Enabled = enable;
            this.m_checkInsShowOnHud.Enabled = enable;
            this.m_checkInsAlwaysVisible.Enabled = enable;
            this.m_buttonCopy.Enabled = enable;
            if (enable)
            {
                this.m_panelInsName.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewCoord_Name_ToolTip));
                this.m_panelInsDesc.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewCoord_Desc_ToolTip));
                this.m_xCoord.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_X_ToolTip));
                this.m_yCoord.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Y_ToolTip));
                this.m_zCoord.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Z_ToolTip));
                this.m_checkInsShowOnHud.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_ShowOnHud_ToolTip));
                this.m_checkInsAlwaysVisible.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_AlwaysVisible_Tooltip));
                this.m_buttonCopy.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_CopyToClipboard_ToolTip));
            }
            else
            {
                this.m_checkInsShowOnHud.ShowTooltipWhenDisabled = true;
                this.m_checkInsAlwaysVisible.ShowTooltipWhenDisabled = true;
                this.m_panelInsName.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_panelInsDesc.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_xCoord.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_yCoord.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_zCoord.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_checkInsShowOnHud.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_checkInsAlwaysVisible.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
                this.m_buttonCopy.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_SelectGpsEntry));
            }
        }

        private void FillRight()
        {
            bool flag;
            if (this.m_tableIns.SelectedRow != null)
            {
                this.FillRight((MyGps) this.m_tableIns.SelectedRow.UserData);
            }
            else
            {
                this.ClearRight();
            }
            this.m_zOk = flag = true;
            this.m_yOk = flag = flag;
            this.m_nameOk = this.m_xOk = flag;
            this.updateWarningLabel();
        }

        private void FillRight(MyGps ins)
        {
            bool flag;
            this.UnhookSyncEvents();
            this.m_panelInsName.SetText(new StringBuilder(ins.Name));
            this.m_panelInsName.Enabled = !ins.IsContainerGPS;
            this.m_panelInsDesc.SetText(new StringBuilder(ins.Description));
            this.m_xCoord.SetText(new StringBuilder(ins.Coords.X.ToString("F2", CultureInfo.InvariantCulture)));
            this.m_xCoord.Enabled = !ins.IsContainerGPS;
            this.m_yCoord.SetText(new StringBuilder(ins.Coords.Y.ToString("F2", CultureInfo.InvariantCulture)));
            this.m_yCoord.Enabled = !ins.IsContainerGPS;
            this.m_zCoord.SetText(new StringBuilder(ins.Coords.Z.ToString("F2", CultureInfo.InvariantCulture)));
            this.m_zCoord.Enabled = !ins.IsContainerGPS;
            this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
            this.m_checkInsShowOnHud.IsChecked = ins.ShowOnHud;
            this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
            this.m_checkInsAlwaysVisible.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkInsAlwaysVisible.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAlwaysVisibleChecked));
            this.m_checkInsAlwaysVisible.IsChecked = ins.AlwaysVisible;
            this.m_checkInsAlwaysVisible.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkInsAlwaysVisible.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAlwaysVisibleChecked));
            this.m_previousHash = new int?(ins.Hash);
            this.HookSyncEvents();
            this.m_needsSyncName = false;
            this.m_needsSyncDesc = false;
            this.m_needsSyncX = false;
            this.m_needsSyncY = false;
            this.m_needsSyncZ = false;
            this.m_panelInsName.ColorMask = Vector4.One;
            this.m_xCoord.ColorMask = Vector4.One;
            this.m_yCoord.ColorMask = Vector4.One;
            this.m_zCoord.ColorMask = Vector4.One;
            this.m_zOk = flag = true;
            this.m_yOk = flag = flag;
            this.m_nameOk = this.m_xOk = flag;
            this.updateWarningLabel();
        }

        private void HookSyncEvents()
        {
            this.m_panelInsName.TextChanged += new Action<MyGuiControlTextbox>(this.OnNameChanged);
            this.m_panelInsDesc.TextChanged += new Action<MyGuiControlTextbox>(this.OnDescChanged);
            this.m_xCoord.TextChanged += new Action<MyGuiControlTextbox>(this.OnXChanged);
            this.m_yCoord.TextChanged += new Action<MyGuiControlTextbox>(this.OnYChanged);
            this.m_zCoord.TextChanged += new Action<MyGuiControlTextbox>(this.OnZChanged);
        }

        public void Init(IMyGuiControlsParent controlsParent)
        {
            this.m_controlsParent = controlsParent;
            this.m_searchBox = (MyGuiControlSearchBox) this.m_controlsParent.Controls.GetControlByName("SearchIns");
            this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.searchIns_TextChanged);
            this.m_tableIns = (MyGuiControlTable) controlsParent.Controls.GetControlByName("TableINS");
            this.m_tableIns.SetColumnComparison(0, new Comparison<MyGuiControlTable.Cell>(this.TableSortingComparison));
            this.m_tableIns.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_tableIns.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableDoubleclick);
            this.m_buttonAdd = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("buttonAdd");
            this.m_buttonAddCurrent = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("buttonFromCurrent");
            this.m_buttonAddFromClipboard = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("buttonFromClipboard");
            this.m_buttonDelete = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("buttonDelete");
            this.m_buttonAdd.ButtonClicked += new Action<MyGuiControlButton>(this.OnButtonPressedNew);
            this.m_buttonAddFromClipboard.ButtonClicked += new Action<MyGuiControlButton>(this.OnButtonPressedNewFromClipboard);
            this.m_buttonAddCurrent.ButtonClicked += new Action<MyGuiControlButton>(this.OnButtonPressedNewFromCurrent);
            this.m_buttonDelete.ButtonClicked += new Action<MyGuiControlButton>(this.OnButtonPressedDelete);
            this.m_labelInsName = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsName");
            this.m_panelInsName = (MyGuiControlTextbox) controlsParent.Controls.GetControlByName("panelInsName");
            this.m_labelInsDesc = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsDesc");
            this.m_panelInsDesc = (MyGuiControlTextbox) controlsParent.Controls.GetControlByName("textInsDesc");
            this.m_labelInsX = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsX");
            this.m_xCoord = (MyGuiControlTextbox) controlsParent.Controls.GetControlByName("textInsX");
            this.m_labelInsY = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsY");
            this.m_yCoord = (MyGuiControlTextbox) controlsParent.Controls.GetControlByName("textInsY");
            this.m_labelInsZ = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsZ");
            this.m_zCoord = (MyGuiControlTextbox) controlsParent.Controls.GetControlByName("textInsZ");
            this.m_labelInsShowOnHud = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsShowOnHud");
            this.m_checkInsShowOnHud = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("checkInsShowOnHud");
            this.m_checkInsShowOnHud.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_ShowOnHud_ToolTip));
            this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
            this.m_labelInsAlwaysVisible = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("labelInsAlwaysVisible");
            this.m_checkInsAlwaysVisible = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("checkInsAlwaysVisible");
            this.m_checkInsAlwaysVisible.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkInsAlwaysVisible.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnAlwaysVisibleChecked));
            this.m_buttonCopy = (MyGuiControlButton) this.m_controlsParent.Controls.GetControlByName("buttonToClipboard");
            this.m_buttonCopy.ButtonClicked += new Action<MyGuiControlButton>(this.OnButtonPressedCopy);
            this.m_labelSaveWarning = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("TerminalTab_GPS_SaveWarning");
            this.m_labelSaveWarning.Visible = false;
            this.m_panelInsName.ShowTooltipWhenDisabled = true;
            this.m_panelInsDesc.ShowTooltipWhenDisabled = true;
            this.m_xCoord.ShowTooltipWhenDisabled = true;
            this.m_yCoord.ShowTooltipWhenDisabled = true;
            this.m_zCoord.ShowTooltipWhenDisabled = true;
            this.m_checkInsShowOnHud.ShowTooltipWhenDisabled = true;
            this.m_checkInsAlwaysVisible.ShowTooltipWhenDisabled = true;
            this.m_buttonCopy.ShowTooltipWhenDisabled = true;
            this.HookSyncEvents();
            MySession.Static.Gpss.GpsChanged += new Action<long, int>(this.OnInsChanged);
            MySession.Static.Gpss.ListChanged += new Action<long>(this.OnListChanged);
            MySession.Static.Gpss.DiscardOld();
            this.PopulateList();
            this.m_previousHash = null;
            this.enableEditBoxes(false);
            this.m_buttonDelete.Enabled = false;
            this.m_buttonDelete.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Delete_Disabled_ToolTip));
        }

        private bool IsCoordOk(string str)
        {
            double num;
            return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
        }

        public bool IsNameOk(string str) => 
            !str.Contains(":");

        private void OnAlwaysVisibleChecked(MyGuiControlCheckbox sender)
        {
            if (this.m_tableIns.SelectedRow != null)
            {
                MyGps userData = this.m_tableIns.SelectedRow.UserData as MyGps;
                userData.AlwaysVisible = sender.IsChecked;
                userData.ShowOnHud = userData.ShowOnHud || userData.AlwaysVisible;
                this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
                this.m_checkInsShowOnHud.IsChecked = this.m_checkInsShowOnHud.IsChecked || sender.IsChecked;
                this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
                if (!this.trySync())
                {
                    MySession.Static.Gpss.ChangeAlwaysVisible(MySession.Static.LocalPlayerId, userData.Hash, sender.IsChecked);
                }
            }
        }

        private void OnButtonPressedCopy(MyGuiControlButton sender)
        {
            if (this.m_tableIns.SelectedRow != null)
            {
                if (this.trySync())
                {
                    this.m_syncedGps.ToClipboard();
                }
                else
                {
                    ((MyGps) this.m_tableIns.SelectedRow.UserData).ToClipboard();
                }
            }
        }

        private void OnButtonPressedDelete(MyGuiControlButton sender)
        {
            if (this.m_tableIns.SelectedRow != null)
            {
                this.Delete();
            }
        }

        private void OnButtonPressedNew(MyGuiControlButton sender)
        {
            this.trySync();
            MyGps gps = new MyGps {
                Name = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewCoord_Name).ToString(),
                Description = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewCoord_Desc).ToString(),
                Coords = new Vector3D(0.0, 0.0, 0.0),
                ShowOnHud = true
            };
            gps.DiscardAt = null;
            MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
            this.m_searchBox.SearchText = string.Empty;
            this.enableEditBoxes(false);
        }

        private void OnButtonPressedNewFromClipboard(MyGuiControlButton sender)
        {
            Thread thread1 = new Thread(() => this.PasteFromClipboard());
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            thread1.Join();
            MySession.Static.Gpss.ScanText(this.m_clipboardText, MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewFromClipboard_Desc));
            this.m_searchBox.SearchText = string.Empty;
        }

        private unsafe void OnButtonPressedNewFromCurrent(MyGuiControlButton sender)
        {
            this.trySync();
            MyGps gps = new MyGps();
            MySession.Static.Gpss.GetNameForNewCurrent(this.m_NameBuilder);
            gps.Name = this.m_NameBuilder.ToString();
            gps.Description = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewFromCurrent_Desc).ToString();
            Vector3D position = MySession.Static.LocalHumanPlayer.GetPosition();
            Vector3D* vectordPtr1 = (Vector3D*) ref position;
            vectordPtr1->X = Math.Round(position.X, 2);
            Vector3D* vectordPtr2 = (Vector3D*) ref position;
            vectordPtr2->Y = Math.Round(position.Y, 2);
            Vector3D* vectordPtr3 = (Vector3D*) ref position;
            vectordPtr3->Z = Math.Round(position.Z, 2);
            gps.Coords = position;
            gps.ShowOnHud = true;
            gps.DiscardAt = null;
            MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
            this.m_searchBox.SearchText = string.Empty;
            this.enableEditBoxes(false);
        }

        public void OnDelKeyPressed()
        {
            if ((this.m_tableIns.SelectedRow != null) && this.m_tableIns.HasFocus)
            {
                this.Delete();
            }
        }

        public void OnDescChanged(MyGuiControlTextbox sender)
        {
            this.m_needsSyncDesc = true;
        }

        private void OnInsChanged(long id, int hash)
        {
            if (id == MySession.Static.LocalPlayerId)
            {
                this.FillRight();
                for (int i = 0; i < this.m_tableIns.RowsCount; i++)
                {
                    if (((MyGps) this.m_tableIns.GetRow(i).UserData).GetHashCode() == hash)
                    {
                        MyGuiControlTable.Cell cell = this.m_tableIns.GetRow(i).GetCell(0);
                        if (cell == null)
                        {
                            break;
                        }
                        MyGps userData = (MyGps) this.m_tableIns.GetRow(i).UserData;
                        cell.TextColor = new Color?((userData.DiscardAt != null) ? Color.Gray : (userData.ShowOnHud ? ITEM_SHOWN_COLOR : Color.White));
                        cell.Text.Clear().Append(((MyGps) this.m_tableIns.GetRow(i).UserData).Name);
                        return;
                    }
                }
            }
        }

        private void OnListChanged(long id)
        {
            if (id == MySession.Static.LocalPlayerId)
            {
                this.PopulateList();
            }
        }

        public void OnNameChanged(MyGuiControlTextbox sender)
        {
            if (this.m_tableIns.SelectedRow != null)
            {
                this.m_needsSyncName = true;
                if (!this.IsNameOk(sender.Text))
                {
                    this.m_nameOk = false;
                    sender.ColorMask = Color.Red.ToVector4();
                }
                else
                {
                    this.m_nameOk = true;
                    sender.ColorMask = Vector4.One;
                    MyGuiControlTable.Row selectedRow = this.m_tableIns.SelectedRow;
                    MyGuiControlTable.Cell cell = selectedRow.GetCell(0);
                    if (cell != null)
                    {
                        cell.Text.Clear().Append(sender.Text);
                        Color? normalColor = null;
                        normalColor = null;
                        Vector2? offset = null;
                        cell.ToolTip.ToolTips[0] = new MyColoredText(sender.Text, normalColor, normalColor, "White", 0.75f, offset);
                    }
                    this.m_tableIns.SortByColumn(0, 1, true);
                    int index = 0;
                    while (true)
                    {
                        if (index < this.m_tableIns.RowsCount)
                        {
                            if (!ReferenceEquals(selectedRow, this.m_tableIns.GetRow(index)))
                            {
                                index++;
                                continue;
                            }
                            this.m_tableIns.SelectedRowIndex = new int?(index);
                        }
                        this.m_tableIns.ScrollToSelection();
                        break;
                    }
                }
                this.updateWarningLabel();
            }
        }

        private void OnShowOnHudChecked(MyGuiControlCheckbox sender)
        {
            if (this.m_tableIns.SelectedRow != null)
            {
                MyGps userData = this.m_tableIns.SelectedRow.UserData as MyGps;
                userData.ShowOnHud = sender.IsChecked;
                if (!sender.IsChecked && userData.AlwaysVisible)
                {
                    userData.AlwaysVisible = false;
                    this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
                    this.m_checkInsShowOnHud.IsChecked = false;
                    this.m_checkInsShowOnHud.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkInsShowOnHud.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.OnShowOnHudChecked));
                }
                if (!this.trySync())
                {
                    MySession.Static.Gpss.ChangeShowOnHud(MySession.Static.LocalPlayerId, userData.Hash, sender.IsChecked);
                }
            }
        }

        private void OnTableDoubleclick(MyGuiControlTable sender, MyGuiControlTable.EventArgs args)
        {
            if (sender.SelectedRow != null)
            {
                MyGps userData = (MyGps) sender.SelectedRow.UserData;
                userData.ShowOnHud = !userData.ShowOnHud;
                MySession.Static.Gpss.ChangeShowOnHud(MySession.Static.LocalPlayerId, ((MyGps) sender.SelectedRow.UserData).Hash, ((MyGps) sender.SelectedRow.UserData).ShowOnHud);
            }
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs args)
        {
            this.trySync();
            if (sender.SelectedRow != null)
            {
                this.enableEditBoxes(true);
                this.m_buttonDelete.Enabled = true;
                this.m_buttonDelete.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Delete_ToolTip));
                this.FillRight((MyGps) sender.SelectedRow.UserData);
            }
            else
            {
                this.enableEditBoxes(false);
                this.m_buttonDelete.Enabled = false;
                this.m_buttonDelete.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Delete_Disabled_ToolTip));
                this.ClearRight();
            }
        }

        public void OnXChanged(MyGuiControlTextbox sender)
        {
            this.m_needsSyncX = true;
            if (this.IsCoordOk(sender.Text))
            {
                this.m_xOk = true;
                sender.ColorMask = Vector4.One;
            }
            else
            {
                this.m_xOk = false;
                sender.ColorMask = Color.Red.ToVector4();
            }
            this.updateWarningLabel();
        }

        public void OnYChanged(MyGuiControlTextbox sender)
        {
            this.m_needsSyncY = true;
            if (this.IsCoordOk(sender.Text))
            {
                this.m_yOk = true;
                sender.ColorMask = Vector4.One;
            }
            else
            {
                this.m_yOk = false;
                sender.ColorMask = Color.Red.ToVector4();
            }
            this.updateWarningLabel();
        }

        public void OnZChanged(MyGuiControlTextbox sender)
        {
            this.m_needsSyncZ = true;
            if (this.IsCoordOk(sender.Text))
            {
                this.m_zOk = true;
                sender.ColorMask = Vector4.One;
            }
            else
            {
                this.m_zOk = false;
                sender.ColorMask = Color.Red.ToVector4();
            }
            this.updateWarningLabel();
        }

        private void PasteFromClipboard()
        {
            this.m_clipboardText = Clipboard.GetText();
        }

        public void PopulateList()
        {
            this.PopulateList(null);
        }

        public void PopulateList(string searchString)
        {
            object obj2 = (this.m_tableIns.SelectedRow == null) ? null : this.m_tableIns.SelectedRow.UserData;
            this.ClearList();
            if (MySession.Static.Gpss.ExistsForPlayer(MySession.Static.LocalPlayerId))
            {
                foreach (KeyValuePair<int, MyGps> pair in MySession.Static.Gpss[MySession.Static.LocalPlayerId])
                {
                    if (searchString == null)
                    {
                        this.AddToList(pair.Value);
                        continue;
                    }
                    char[] separator = new char[] { ' ' };
                    string str = pair.Value.Name.ToString().ToLower();
                    bool flag = true;
                    string[] strArray = searchString.ToLower().Split(separator);
                    int index = 0;
                    while (true)
                    {
                        if (index < strArray.Length)
                        {
                            string str2 = strArray[index];
                            if (str.Contains(str2.ToLower()))
                            {
                                index++;
                                continue;
                            }
                            flag = false;
                        }
                        if (flag)
                        {
                            this.AddToList(pair.Value);
                        }
                        break;
                    }
                }
            }
            this.m_tableIns.SortByColumn(0, 1, true);
            this.enableEditBoxes(false);
            if (obj2 != null)
            {
                for (int i = 0; i < this.m_tableIns.RowsCount; i++)
                {
                    if (obj2 == this.m_tableIns.GetRow(i).UserData)
                    {
                        this.m_tableIns.SelectedRowIndex = new int?(i);
                        this.enableEditBoxes(true);
                        break;
                    }
                }
            }
            this.m_tableIns.ScrollToSelection();
            this.FillRight();
        }

        private void searchIns_TextChanged(string text)
        {
            this.PopulateList(text);
        }

        private int TableSortingComparison(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b)
        {
            if (((((MyGps) a.UserData).DiscardAt != null) && (((MyGps) b.UserData).DiscardAt != null)) || ((((MyGps) a.UserData).DiscardAt == null) && (((MyGps) b.UserData).DiscardAt == null)))
            {
                return a.Text.CompareToIgnoreCase(b.Text);
            }
            return ((((MyGps) a.UserData).DiscardAt != null) ? 1 : -1);
        }

        private bool trySync()
        {
            MyGps gps;
            if (((this.m_previousHash == null) || (((!this.m_needsSyncName && (!this.m_needsSyncDesc && (!this.m_needsSyncX && !this.m_needsSyncY))) && !this.m_needsSyncZ) || (!MySession.Static.Gpss.ExistsForPlayer(MySession.Static.LocalPlayerId) || (!this.IsNameOk(this.m_panelInsName.Text) || (!this.IsCoordOk(this.m_xCoord.Text) || (!this.IsCoordOk(this.m_yCoord.Text) || !this.IsCoordOk(this.m_zCoord.Text))))))) || !MySession.Static.Gpss[MySession.Static.LocalPlayerId].TryGetValue(this.m_previousHash.Value, out gps))
            {
                return false;
            }
            if (this.m_needsSyncName)
            {
                gps.Name = this.m_panelInsName.Text;
            }
            if (this.m_needsSyncDesc)
            {
                gps.Description = this.m_panelInsDesc.Text;
            }
            StringBuilder result = new StringBuilder();
            Vector3D coords = gps.Coords;
            if (this.m_needsSyncX)
            {
                this.m_xCoord.GetText(result);
                coords.X = Math.Round(double.Parse(result.ToString(), CultureInfo.InvariantCulture), 2);
            }
            result.Clear();
            if (this.m_needsSyncY)
            {
                this.m_yCoord.GetText(result);
                coords.Y = Math.Round(double.Parse(result.ToString(), CultureInfo.InvariantCulture), 2);
            }
            result.Clear();
            if (this.m_needsSyncZ)
            {
                this.m_zCoord.GetText(result);
                coords.Z = Math.Round(double.Parse(result.ToString(), CultureInfo.InvariantCulture), 2);
            }
            gps.Coords = coords;
            this.m_syncedGps = gps;
            MySession.Static.Gpss.SendModifyGps(MySession.Static.LocalPlayerId, gps);
            return true;
        }

        private void UnhookSyncEvents()
        {
            this.m_panelInsName.TextChanged -= new Action<MyGuiControlTextbox>(this.OnNameChanged);
            this.m_panelInsDesc.TextChanged -= new Action<MyGuiControlTextbox>(this.OnDescChanged);
            this.m_xCoord.TextChanged -= new Action<MyGuiControlTextbox>(this.OnXChanged);
            this.m_yCoord.TextChanged -= new Action<MyGuiControlTextbox>(this.OnYChanged);
            this.m_zCoord.TextChanged -= new Action<MyGuiControlTextbox>(this.OnZChanged);
        }

        private void updateWarningLabel()
        {
            if ((!this.m_nameOk || (!this.m_xOk || !this.m_yOk)) || !this.m_zOk)
            {
                this.m_labelSaveWarning.Visible = true;
                this.m_buttonCopy.Enabled = false;
            }
            else
            {
                this.m_labelSaveWarning.Visible = false;
                if (this.m_panelInsName.Enabled)
                {
                    this.m_buttonCopy.Enabled = true;
                }
            }
        }
    }
}

