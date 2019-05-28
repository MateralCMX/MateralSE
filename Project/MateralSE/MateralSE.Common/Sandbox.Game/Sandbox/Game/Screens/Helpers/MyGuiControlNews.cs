namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.Game.News;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlNews : MyGuiControlBase
    {
        private static StringBuilder m_stringCache = new StringBuilder(100);
        private List<MyNewsEntry> m_news;
        private int m_currentEntryIndex;
        private StateEnum m_state;
        private MyGuiControlLabel m_labelTitle;
        private MyGuiControlLabel m_labelDate;
        private MyGuiControlSeparatorList m_separator;
        private MyGuiControlMultilineText m_textNewsEntry;
        private MyGuiControlPanel m_backgroundPanel;
        private MyGuiControlPanel m_backgroundPanel_BlueLine;
        private MyGuiControlPanel m_bottomPanel;
        private MyGuiControlLabel m_labelPages;
        private MyGuiControlButton m_buttonNext;
        private MyGuiControlButton m_buttonPrev;
        private MyGuiControlMultilineText m_textError;
        private MyGuiControlRotatingWheel m_wheelLoading;
        private Task m_downloadNewsTask;
        private MyNews m_downloadedNews;
        private XmlSerializer m_newsSerializer;
        private bool m_downloadedNewsOK;
        private bool m_downloadedNewsFinished;
        private bool m_pauseGame;
        private static readonly char[] m_trimArray = new char[] { ' ', '\r', '\r', '\n' };
        private static readonly char[] m_splitArray = new char[] { '\r', '\n' };

        public MyGuiControlNews() : base(position, position, colorMask, null, null, true, false, true, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            this.m_news = new List<MyNewsEntry>();
            position = null;
            position = null;
            colorMask = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, null, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label1.Name = "Title";
            this.m_labelTitle = label1;
            position = null;
            position = null;
            colorMask = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(position, position, null, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            label2.Name = "Date";
            this.m_labelDate = label2;
            MyGuiControlSeparatorList list1 = new MyGuiControlSeparatorList();
            list1.Name = "Separator";
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.m_separator = list1;
            position = null;
            position = null;
            colorMask = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(position, position, colorMask, "Blue", 0.68f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            text1.Name = "NewsEntry";
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_textNewsEntry = text1;
            this.m_textNewsEntry.OnLinkClicked += new LinkClicked(this.OnLinkClicked);
            MyGuiControlPanel panel1 = new MyGuiControlPanel();
            panel1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            panel1.ColorMask = new VRageMath.Vector4(1f, 1f, 1f, 0f);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_NEWS_PAGING_BACKGROUND;
            panel1.Name = "BottomPanel";
            this.m_bottomPanel = panel1;
            position = null;
            position = null;
            colorMask = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(position, position, new StringBuilder("{0}/{1}  ").ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM);
            label3.Name = "Pages";
            this.m_labelPages = label3;
            position = null;
            position = null;
            colorMask = null;
            visibleLinesCount = null;
            MyGuiControlButton button1 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.ArrowLeft, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, MyTexts.GetString(MyCommonTexts.PreviousNews), null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, b => this.UpdateCurrentEntryIndex(-1), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            button1.Name = "Previous";
            this.m_buttonPrev = button1;
            position = null;
            position = null;
            colorMask = null;
            visibleLinesCount = null;
            MyGuiControlButton button2 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.ArrowRight, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, MyTexts.GetString(MyCommonTexts.NextNews), null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, b => this.UpdateCurrentEntryIndex(1), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            button2.Name = "Next";
            this.m_buttonNext = button2;
            position = null;
            position = null;
            colorMask = null;
            visibleLinesCount = null;
            textPadding = null;
            MyGuiControlMultilineText text2 = new MyGuiControlMultilineText(position, position, colorMask, "Red", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            text2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            text2.Name = "Error";
            this.m_textError = text2;
            MyGuiControlCompositePanel panel2 = new MyGuiControlCompositePanel();
            panel2.ColorMask = new VRageMath.Vector4(1f, 1f, 1f, 0.8f);
            panel2.BackgroundTexture = MyGuiConstants.TEXTURE_NEWS_BACKGROUND;
            this.m_backgroundPanel = panel2;
            MyGuiControlCompositePanel panel3 = new MyGuiControlCompositePanel();
            panel3.ColorMask = new VRageMath.Vector4(1f, 1f, 1f, 1f);
            panel3.BackgroundTexture = MyGuiConstants.TEXTURE_NEWS_BACKGROUND_BlueLine;
            this.m_backgroundPanel_BlueLine = panel3;
            position = null;
            colorMask = null;
            position = null;
            this.m_wheelLoading = new MyGuiControlRotatingWheel(position, colorMask, 0.36f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, MyPerGameSettings.GUI.MultipleSpinningWheels, position, 1.5f);
            base.Elements.Add(this.m_backgroundPanel);
            base.Elements.Add(this.m_backgroundPanel_BlueLine);
            base.Elements.Add(this.m_labelTitle);
            base.Elements.Add(this.m_labelDate);
            base.Elements.Add(this.m_separator);
            base.Elements.Add(this.m_textNewsEntry);
            base.Elements.Add(this.m_bottomPanel);
            base.Elements.Add(this.m_labelPages);
            base.Elements.Add(this.m_buttonPrev);
            base.Elements.Add(this.m_buttonNext);
            base.Elements.Add(this.m_textError);
            base.Elements.Add(this.m_wheelLoading);
            this.RefreshState();
            this.UpdatePositionsAndSizes();
            this.RefreshShownEntry();
            try
            {
                this.m_newsSerializer = new XmlSerializer(typeof(MyNews));
            }
            finally
            {
                this.DownloadNews();
            }
        }

        private void CheckVersion()
        {
            int result = 0;
            if (((this.m_downloadedNews != null) && ((this.m_downloadedNews.Entry.Count > 0) && (int.TryParse(this.m_downloadedNews.Entry[0].Version, out result) && (result > MyFinalBuildConstants.APP_VERSION)))) && (MySandboxGame.Config.LastCheckedVersion != MyFinalBuildConstants.APP_VERSION))
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.NewVersionAvailable), MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                MySandboxGame.Config.LastCheckedVersion = (int) MyFinalBuildConstants.APP_VERSION;
                MySandboxGame.Config.Save();
                MySandboxGame.ResetColdStartRegister();
            }
        }

        public void DownloadNews()
        {
            if ((this.m_downloadNewsTask == null) || this.m_downloadNewsTask.IsCompleted)
            {
                this.State = StateEnum.Loading;
                this.m_downloadNewsTask = Task.Run(() => this.DownloadNewsAsync()).ContinueWith(task => this.DownloadNewsCompleted());
            }
        }

        private void DownloadNewsAsync()
        {
            try
            {
                WebClient client1 = new WebClient();
                client1.Proxy = null;
                using (StringReader reader = new StringReader(client1.DownloadString(new Uri(MyPerGameSettings.ChangeLogUrl))))
                {
                    this.m_downloadedNews = (MyNews) this.m_newsSerializer.Deserialize(reader);
                    StringBuilder builder = new StringBuilder();
                    int num = 0;
                    while (true)
                    {
                        if (num >= this.m_downloadedNews.Entry.Count)
                        {
                            if (MyFakes.TEST_NEWS)
                            {
                                MyNewsEntry item = this.m_downloadedNews.Entry[this.m_downloadedNews.Entry.Count - 1];
                                item.Title = "Test";
                                base.ColorMask = new VRageMath.Vector4(1f, 1f, 1f, 0f);
                                item.Text = "ASDF\nASDF\n[www.spaceengineersgame.com Space engineers web]\n[[File:Textures\\GUI\\MouseCursor.dds|64x64px]]\n";
                                this.m_downloadedNews.Entry.Add(item);
                            }
                            this.m_downloadedNewsOK = true;
                            break;
                        }
                        MyNewsEntry entry = this.m_downloadedNews.Entry[num];
                        builder.Clear();
                        string[] strArray = entry.Text.Trim(m_trimArray).Split(m_splitArray);
                        int index = 0;
                        while (true)
                        {
                            if (index >= strArray.Length)
                            {
                                MyNewsEntry entry1 = new MyNewsEntry();
                                entry1.Title = entry.Title;
                                entry1.Version = entry.Version;
                                entry1.Date = entry.Date;
                                entry1.Text = builder.ToString();
                                this.m_downloadedNews.Entry[num] = entry1;
                                num++;
                                break;
                            }
                            string str = strArray[index].Trim();
                            builder.AppendLine(str);
                            index++;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error while downloading news: " + exception.ToString());
            }
            finally
            {
                this.m_downloadedNewsFinished = true;
            }
        }

        private void DownloadNewsCompleted()
        {
            MySandboxGame.Static.Invoke(() => this.CheckVersion(), "CheckVersion");
            if (this.m_downloadedNewsOK)
            {
                this.State = StateEnum.Entries;
                this.Show(this.m_downloadedNews);
            }
            else
            {
                this.State = StateEnum.Error;
                this.ErrorText = MyTexts.Get(MyCommonTexts.NewsDownloadingFailed);
            }
        }

        public override MyGuiControlBase HandleInput()
        {
            base.HandleInput();
            return base.HandleInputElements();
        }

        private void OnLinkClicked(MyGuiControlBase sender, string url)
        {
            m_stringCache.Clear();
            m_stringCache.AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextOpenBrowser), url);
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, m_stringCache, messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                if ((retval == MyGuiScreenMessageBox.ResultEnum.YES) && !MyBrowserHelper.OpenInternetBrowser(url))
                {
                    StringBuilder messageText = new StringBuilder();
                    messageText.AppendFormat(MyTexts.GetString(MyCommonTexts.TitleFailedToStartInternetBrowser), url);
                    MyStringId? nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    Vector2? nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.TitleFailedToStartInternetBrowser), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        protected override void OnSizeChanged()
        {
            this.UpdatePositionsAndSizes();
            base.OnSizeChanged();
        }

        private void RefreshShownEntry()
        {
            this.m_textNewsEntry.Clear();
            if (!this.m_news.IsValidIndex<MyNewsEntry>(this.m_currentEntryIndex))
            {
                this.m_labelTitle.Text = null;
                this.m_labelDate.Text = null;
                object[] args = new object[] { 0, 0 };
                this.m_labelPages.UpdateFormatParams(args);
            }
            else
            {
                MyNewsEntry entry = this.m_news[this.m_currentEntryIndex];
                this.m_labelTitle.Text = entry.Title;
                char[] separator = new char[] { '/' };
                string[] strArray = entry.Date.Split(separator);
                if (strArray[1].Length == 1)
                {
                    string[] textArray1 = new string[] { strArray[0], "/0", strArray[1], "/", strArray[2] };
                    this.m_labelDate.Text = string.Concat(textArray1);
                }
                else
                {
                    string[] textArray2 = new string[] { strArray[0], "/", strArray[1], "/", strArray[2] };
                    this.m_labelDate.Text = string.Concat(textArray2);
                }
                MyWikiMarkupParser.ParseText(entry.Text, ref this.m_textNewsEntry);
                this.m_textNewsEntry.AppendLine();
                object[] args = new object[] { this.m_currentEntryIndex + 1, this.m_news.Count };
                this.m_labelPages.UpdateFormatParams(args);
                this.m_buttonNext.Enabled = (this.m_currentEntryIndex + 1) != this.m_news.Count;
                this.m_buttonPrev.Enabled = (this.m_currentEntryIndex + 1) != 1;
            }
        }

        private void RefreshState()
        {
            bool flag = this.m_state == StateEnum.Entries;
            bool flag2 = this.m_state == StateEnum.Error;
            bool flag3 = this.m_state == StateEnum.Loading;
            this.m_labelTitle.Visible = flag;
            this.m_labelDate.Visible = flag;
            this.m_separator.Visible = flag;
            this.m_textNewsEntry.Visible = flag;
            this.m_labelPages.Visible = flag;
            this.m_bottomPanel.Visible = flag;
            this.m_buttonPrev.Visible = flag;
            this.m_buttonNext.Visible = flag;
            this.m_textError.Visible = flag2;
            this.m_wheelLoading.Visible = flag3;
        }

        internal void Show(MyNews news)
        {
            this.m_news.Clear();
            this.m_news.AddRange(news.Entry);
            this.m_currentEntryIndex = 0;
            this.RefreshShownEntry();
        }

        private void UpdateCurrentEntryIndex(int delta)
        {
            this.m_currentEntryIndex += delta;
            if (this.m_currentEntryIndex < 0)
            {
                this.m_currentEntryIndex = 0;
            }
            if (this.m_currentEntryIndex >= this.m_news.Count)
            {
                this.m_currentEntryIndex = this.m_news.Count - 1;
            }
            this.RefreshShownEntry();
        }

        private void UpdatePositionsAndSizes()
        {
            float num = 0.03f;
            float num2 = 0.004f;
            float y = (-0.5f * base.Size.Y) + num;
            float x = (-0.5f * base.Size.X) + num;
            float num5 = (0.5f * base.Size.X) - num;
            this.m_labelTitle.Position = new Vector2(x, y);
            this.m_labelDate.Position = new Vector2(num5, y);
            y += Math.Max(this.m_labelTitle.Size.Y, this.m_labelDate.Size.Y) + num2;
            this.m_separator.Size = base.Size;
            this.m_separator.Clear();
            this.m_textNewsEntry.Position = new Vector2(x, y + num2);
            this.m_buttonPrev.Position = new Vector2(this.m_textNewsEntry.Position.X + 0.02f, (0.5f * base.Size.Y) - (0.5f * num));
            this.m_labelPages.Position = new Vector2(this.m_buttonPrev.Position.X + (8.9f * num2), this.m_buttonPrev.Position.Y - 0.003f);
            this.m_buttonNext.Position = new Vector2(this.m_buttonPrev.Position.X + (20f * num2), this.m_buttonPrev.Position.Y);
            this.m_textNewsEntry.Size = new Vector2((num5 - x) + 0.013f, (this.m_buttonNext.Position.Y - this.m_textNewsEntry.Position.Y) - num);
            this.m_textError.Size = base.Size - (2f * num);
            this.m_bottomPanel.Size = new Vector2(0.125f, this.m_buttonPrev.Size.Y + 0.015f);
            this.m_backgroundPanel.Size = base.Size;
            this.m_backgroundPanel_BlueLine.Size = base.Size;
            this.m_backgroundPanel_BlueLine.Position = new Vector2(base.Size.X - num2, 0f);
        }

        public StateEnum State
        {
            get => 
                this.m_state;
            set
            {
                if (this.m_state != value)
                {
                    this.m_state = value;
                    this.RefreshState();
                }
            }
        }

        public StringBuilder ErrorText
        {
            get => 
                this.m_textError.Text;
            set => 
                (this.m_textError.Text = value);
        }

        public enum StateEnum
        {
            Entries,
            Loading,
            Error
        }
    }
}

