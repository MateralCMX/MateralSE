namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenLoadSubscribedWorld : MyGuiScreenBase
    {
        private MyGuiControlTable m_worldsTable;
        private MyGuiControlButton m_loadButton;
        private MyGuiControlButton m_openInWorkshopButton;
        private MyGuiControlButton m_refreshButton;
        private MyGuiControlButton m_browseWorkshopButton;
        private MyGuiControlButton m_copyButton;
        private MyGuiControlButton m_currentButton;
        private int m_selectedRow;
        private bool m_listNeedsReload;
        private List<MyWorkshopItem> m_subscribedWorlds;
        private MyGuiControlTextbox m_searchBox;
        private MyGuiControlLabel m_searchBoxLabel;
        private MyGuiControlButton m_searchClear;
        private MyGuiControlRotatingWheel m_loadingWheel;

        public MyGuiScreenLoadSubscribedWorld() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.878f, 0.97f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            this.m_listNeedsReload = true;
            this.RecreateControls(true);
        }

        private void AddHeaders()
        {
            this.m_worldsTable.SetColumnName(0, MyTexts.Get(MyCommonTexts.Name));
        }

        private IMyAsyncResult beginAction() => 
            new LoadListResult();

        private IMyAsyncResult beginActionLoadSaves() => 
            new MyLoadWorldInfoListResult(null);

        private void CopyWorldAndGoToLoadScreen()
        {
            MyGuiControlTable.Row selectedRow = this.m_worldsTable.SelectedRow;
            if ((selectedRow != null) && (selectedRow.UserData is MyWorkshopItem))
            {
                MyStringId? cancelText = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginActionLoadSaves), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endActionLoadSaves), null));
            }
        }

        private void CreateAndLoadFromSubscribedWorld()
        {
            MyGuiControlTable.Row selectedRow = this.m_worldsTable.SelectedRow;
            if ((selectedRow != null) && (selectedRow.UserData is MyWorkshopItem))
            {
                MyStringId? cancelText = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginActionLoadSaves), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endActionLoadSaves), null));
            }
        }

        private void endAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            this.m_listNeedsReload = false;
            LoadListResult result2 = (LoadListResult) result;
            this.m_subscribedWorlds = result2.SubscribedWorlds;
            this.RefreshGameList();
            screen.CloseScreen();
            this.m_loadingWheel.Visible = false;
        }

        private void endActionLoadSaves(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            screen.CloseScreen();
            this.m_loadingWheel.Visible = false;
            MyWorkshopItem userData = this.m_worldsTable.SelectedRow.UserData as MyWorkshopItem;
            if (Directory.Exists(MyLocalCache.GetSessionSavesPath(MyUtils.StripInvalidChars(userData.Title), false, false)))
            {
                this.OverwriteWorldDialog();
            }
            else
            {
                MyWorkshop.CreateWorldInstanceAsync(userData, MyWorkshop.MyWorkshopPathInfo.CreateWorldInfo(), false, delegate (bool success, string sessionPath) {
                    if (success)
                    {
                        this.OnSuccess(sessionPath);
                    }
                });
            }
        }

        private void FillList()
        {
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginAction), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endAction), null));
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenLoadSubscribedWorld";

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, string toolTip, Action<MyGuiControlButton> onClick)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            StringBuilder builder = MyTexts.Get(text);
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, toolTip, builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, MyStringId toolTip, Action<MyGuiControlButton> onClick)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            StringBuilder builder = MyTexts.Get(text);
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(toolTip), builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private void OnBackClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void OnBrowseWorkshopClick(MyGuiControlButton obj)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_WORLDS, "Steam Workshop", false);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyAnalyticsHelper.ReportActivityEnd(null, "show_workshop");
        }

        private void OnCopyClick(MyGuiControlButton sender)
        {
            this.m_currentButton = this.m_copyButton;
            this.CopyWorldAndGoToLoadScreen();
        }

        private void OnLoadClick(MyGuiControlButton sender)
        {
            this.m_currentButton = this.m_loadButton;
            this.CreateAndLoadFromSubscribedWorld();
        }

        private void OnOpenInWorkshopClick(MyGuiControlButton obj)
        {
            MyGuiControlTable.Row selectedRow = this.m_worldsTable.SelectedRow;
            if (selectedRow != null)
            {
                MyWorkshopItem userData = selectedRow.UserData as MyWorkshopItem;
                if (userData != null)
                {
                    MyGuiSandbox.OpenUrlWithFallback(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, userData.Id), "Steam Workshop", false);
                }
            }
        }

        private void OnOverwriteWorld(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyWorkshop.CreateWorldInstanceAsync(this.m_worldsTable.SelectedRow.UserData as MyWorkshopItem, MyWorkshop.MyWorkshopPathInfo.CreateWorldInfo(), true, delegate (bool success, string sessionPath) {
                    if (success)
                    {
                        this.OnSuccess(sessionPath);
                    }
                });
            }
        }

        private void OnRefreshClick(MyGuiControlButton obj)
        {
            if (!this.m_listNeedsReload)
            {
                this.m_listNeedsReload = true;
                this.FillList();
            }
        }

        private void OnSearchClear(MyGuiControlButton sender)
        {
            this.m_searchBox.Text = "";
            this.RefreshGameList();
        }

        private void OnSearchTextChange(MyGuiControlTextbox box)
        {
            this.RefreshGameList();
        }

        protected override void OnShow()
        {
            base.OnShow();
            MyAnalyticsHelper.ReportActivityStart(null, "show_workshop", string.Empty, "GUI", string.Empty, true);
            if (this.m_listNeedsReload)
            {
                this.FillList();
            }
        }

        private void OnSuccess(string sessionPath)
        {
            if (ReferenceEquals(this.m_currentButton, this.m_copyButton))
            {
                MyGuiScreenLoadSandbox sandbox1 = new MyGuiScreenLoadSandbox();
                MyGuiSandbox.AddScreen(new MyGuiScreenLoadSandbox());
            }
            else if (ReferenceEquals(this.m_currentButton, this.m_loadButton))
            {
                MyOnlineModeEnum? onlineMode = null;
                MySessionLoader.LoadSingleplayerSession(sessionPath, null, null, onlineMode, 0);
            }
            this.m_currentButton = null;
        }

        private void OnTableItemConfirmedOrDoubleClick(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            this.m_currentButton = this.m_loadButton;
            this.CreateAndLoadFromSubscribedWorld();
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            this.m_selectedRow = eventArgs.RowIndex;
        }

        private void OverwriteWorldDialog()
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(ReferenceEquals(this.m_currentButton, this.m_loadButton) ? MyCommonTexts.MessageBoxTextWorldExistsDownloadOverwrite : MyCommonTexts.MessageBoxTextWorldExistsOverwrite), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnOverwriteWorld), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            MyGuiControlButton button;
            base.RecreateControls(constructor);
            MyGuiControlScreenSwitchPanel panel1 = new MyGuiControlScreenSwitchPanel(this, MyTexts.Get(MyCommonTexts.WorkshopScreen_Description));
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MyCommonTexts.ScreenMenuButtonCampaign, captionTextColor, captionOffset, 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.872f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.872f, 0f, captionTextColor);
            this.Controls.Add(control);
            float y = 0.216f;
            float num2 = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            float num3 = 15f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float num4 = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float x = 93f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(x, y + 0.199f);
            Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
            Vector2 vector3 = (-base.m_size.Value / 2f) + new Vector2((x + minSizeGui.X) + num3, y);
            Vector2 vector4 = (base.m_size.Value / 2f) - vector3;
            float* singlePtr1 = (float*) ref vector4.X;
            singlePtr1[0] -= num4;
            float* singlePtr2 = (float*) ref vector4.Y;
            singlePtr2[0] -= num2;
            this.m_searchBoxLabel = new MyGuiControlLabel();
            this.m_searchBoxLabel.Text = MyTexts.Get(MyCommonTexts.Search).ToString() + ":";
            this.m_searchBoxLabel.Position = new Vector2(-0.188f, -0.244f);
            this.Controls.Add(this.m_searchBoxLabel);
            captionTextColor = null;
            this.m_searchBox = new MyGuiControlTextbox(new Vector2(0.382f, -0.247f), null, 0x200, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_searchBox.TextChanged += new Action<MyGuiControlTextbox>(this.OnSearchTextChange);
            this.m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_searchBox.Size = new Vector2(0.56f - this.m_searchBoxLabel.Size.X, 1f);
            this.Controls.Add(this.m_searchBox);
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = this.m_searchBox.Position + new Vector2(-0.027f, 0.004f);
            button1.Size = new Vector2(0.045f, 0.05666667f);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Close;
            button1.ActivateOnMouseRelease = true;
            this.m_searchClear = button1;
            this.m_searchClear.ButtonClicked += new Action<MyGuiControlButton>(this.OnSearchClear);
            this.Controls.Add(this.m_searchClear);
            this.m_worldsTable = new MyGuiControlTable();
            this.m_worldsTable.Position = vector3 + new Vector2(0.0055f, 0.065f);
            this.m_worldsTable.Size = new Vector2((1075f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) * 0.852f, 0.15f);
            this.m_worldsTable.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_worldsTable.ColumnsCount = 1;
            this.m_worldsTable.VisibleRowsCount = 15;
            this.m_worldsTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_worldsTable.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            this.m_worldsTable.ItemConfirmed += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemConfirmedOrDoubleClick);
            float[] p = new float[] { 1f };
            this.m_worldsTable.SetCustomColumnWidths(p);
            this.m_worldsTable.SetColumnComparison(0, (a, b) => ((StringBuilder) a.UserData).CompareToIgnoreCase((StringBuilder) b.UserData));
            this.Controls.Add(this.m_worldsTable);
            Vector2 vector5 = vector + (minSizeGui * 0.5f);
            Vector2 vector6 = MyGuiConstants.MENU_BUTTONS_POSITION_DELTA;
            this.m_copyButton = button = this.MakeButton(vector5 + (vector6 * -3.21f), MyCommonTexts.ScreenLoadSubscribedWorldCopyWorld, MyCommonTexts.ToolTipWorkshopCopyWorld, new Action<MyGuiControlButton>(this.OnCopyClick));
            this.Controls.Add(button);
            this.m_openInWorkshopButton = button = this.MakeButton(vector5 + (vector6 * -2.21f), MyCommonTexts.ScreenLoadSubscribedWorldOpenInWorkshop, MyCommonTexts.ToolTipWorkshopOpenInWorkshop, new Action<MyGuiControlButton>(this.OnOpenInWorkshopClick));
            this.Controls.Add(button);
            this.m_browseWorkshopButton = button = this.MakeButton(vector5 + (vector6 * -1.21f), MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop, string.Format(MyTexts.GetString(MyCommonTexts.ToolTipWorkshopBrowseWorkshop), MySession.Platform), new Action<MyGuiControlButton>(this.OnBrowseWorkshopClick));
            this.Controls.Add(button);
            this.m_refreshButton = button = this.MakeButton(vector5 + (vector6 * -0.21f), MyCommonTexts.ScreenLoadSubscribedWorldRefresh, MyCommonTexts.ToolTipWorkshopRefresh, new Action<MyGuiControlButton>(this.OnRefreshClick));
            this.Controls.Add(button);
            this.m_loadButton = button = this.MakeButton(new Vector2(0f, 0f) - new Vector2(-0.109f, (-base.m_size.Value.Y / 2f) + 0.071f), MyCommonTexts.ScreenLoadSubscribedWorldCopyAndLoad, MyCommonTexts.ToolTipWorkshopCopyAndLoad, new Action<MyGuiControlButton>(this.OnLoadClick));
            this.Controls.Add(button);
            captionOffset = null;
            this.m_loadingWheel = new MyGuiControlRotatingWheel(new Vector2((base.m_size.Value.X / 2f) - 0.077f, (-base.m_size.Value.Y / 2f) + 0.108f), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, captionOffset, 1.5f);
            this.Controls.Add(this.m_loadingWheel);
            this.m_loadingWheel.Visible = false;
            this.m_loadButton.DrawCrossTextureWhenDisabled = false;
            this.m_openInWorkshopButton.DrawCrossTextureWhenDisabled = false;
            base.CloseButtonEnabled = true;
        }

        private void RefreshGameList()
        {
            this.m_worldsTable.Clear();
            this.AddHeaders();
            if (this.m_subscribedWorlds != null)
            {
                for (int i = 0; i < this.m_subscribedWorlds.Count; i++)
                {
                    MyGuiControlTable.Row row = new MyGuiControlTable.Row(this.m_subscribedWorlds[i]);
                    MyWorkshopItem local1 = this.m_subscribedWorlds[i];
                    StringBuilder userData = new StringBuilder(local1.Title);
                    if (this.SearchFilterTest(userData.ToString()))
                    {
                        Color? textColor = null;
                        MyGuiHighlightTexture? icon = null;
                        row.AddCell(new MyGuiControlTable.Cell(userData.ToString(), userData, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                        textColor = null;
                        icon = null;
                        row.AddCell(new MyGuiControlTable.Cell(null, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                        this.m_worldsTable.Add(row);
                    }
                }
            }
            this.m_worldsTable.SelectedRowIndex = null;
        }

        public override bool RegisterClicks() => 
            true;

        private bool SearchFilterTest(string testString)
        {
            if ((this.m_searchBox.Text != null) && (this.m_searchBox.Text.Length != 0))
            {
                char[] separator = new char[] { ' ' };
                string str = testString.ToLower();
                foreach (string str2 in this.m_searchBox.Text.Split(separator))
                {
                    if (!str.Contains(str2.ToLower()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_worldsTable.SelectedRow != null)
            {
                this.m_loadButton.Enabled = true;
                this.m_copyButton.Enabled = true;
                this.m_openInWorkshopButton.Enabled = true;
            }
            else
            {
                this.m_loadButton.Enabled = false;
                this.m_copyButton.Enabled = false;
                this.m_openInWorkshopButton.Enabled = false;
            }
            return base.Update(hasFocus);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenLoadSubscribedWorld.<>c <>9 = new MyGuiScreenLoadSubscribedWorld.<>c();
            public static Comparison<MyGuiControlTable.Cell> <>9__16_0;

            internal int <RecreateControls>b__16_0(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                ((StringBuilder) a.UserData).CompareToIgnoreCase(((StringBuilder) b.UserData));
        }

        private class LoadListResult : IMyAsyncResult
        {
            public List<MyWorkshopItem> SubscribedWorlds;

            public LoadListResult()
            {
                this.Task = Parallel.Start(() => this.LoadListAsync(out this.SubscribedWorlds));
            }

            private void LoadListAsync(out List<MyWorkshopItem> list)
            {
                List<MyWorkshopItem> results = new List<MyWorkshopItem>();
                if (!MyWorkshop.GetSubscribedWorldsBlocking(results))
                {
                    list = null;
                }
                else
                {
                    list = results;
                    List<MyWorkshopItem> list3 = new List<MyWorkshopItem>();
                    if (MyWorkshop.GetSubscribedScenariosBlocking(list3) && (list3.Count > 0))
                    {
                        list.InsertRange(list.Count, list3);
                    }
                }
            }

            public bool IsCompleted =>
                this.Task.IsComplete;

            public ParallelTasks.Task Task { get; private set; }
        }
    }
}

