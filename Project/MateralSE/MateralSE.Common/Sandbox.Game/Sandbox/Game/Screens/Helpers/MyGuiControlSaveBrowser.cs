namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.GUI;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui.DirectoryBrowser;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlSaveBrowser : MyGuiControlDirectoryBrowser
    {
        private readonly List<FileInfo> m_saveEntriesToCreate;
        private readonly Dictionary<string, MyWorldInfo> m_loadedWorldsByFilePaths;
        private readonly HashSet<string> m_loadedDirectories;
        public string SearchTextFilter;

        public MyGuiControlSaveBrowser() : this(MyFileSystem.SavesPath, MyFileSystem.SavesPath, null, info => false)
        {
            this.m_saveEntriesToCreate = new List<FileInfo>();
            this.m_loadedWorldsByFilePaths = new Dictionary<string, MyWorldInfo>();
            this.m_loadedDirectories = new HashSet<string>();
            base.SetColumnName(1, MyTexts.Get(MyCommonTexts.Date));
            base.SetColumnComparison(1, delegate (MyGuiControlTable.Cell cellA, MyGuiControlTable.Cell cellB) {
                if (cellA == null)
                {
                    return -1;
                }
                if (cellB == null)
                {
                    return -1;
                }
                FileInfo userData = cellA.UserData as FileInfo;
                FileInfo objB = cellB.UserData as FileInfo;
                if (ReferenceEquals(userData, objB))
                {
                    if (userData == null)
                    {
                        return 0;
                    }
                }
                else
                {
                    if (userData == null)
                    {
                        return -1;
                    }
                    if (objB == null)
                    {
                        return 1;
                    }
                }
                return this.m_loadedWorldsByFilePaths[userData.DirectoryName].LastSaveTime.CompareTo(this.m_loadedWorldsByFilePaths[objB.DirectoryName].LastSaveTime);
            });
        }

        public void AccessBackups()
        {
            if (base.SelectedRow != null)
            {
                DirectoryInfo info = (base.SelectedRow.UserData as FileInfo).Directory.GetDirectories().FirstOrDefault<DirectoryInfo>(dir => dir.Name.StartsWith("Backup"));
                if (info != null)
                {
                    this.InBackupsFolder = true;
                    base.CurrentDirectory = info.FullName;
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SaveBrowserMissingBackup), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        protected override void AddFolderRow(DirectoryInfo dir)
        {
            if (this.SearchFilterTest(dir.Name))
            {
                bool flag = false;
                FileInfo[] files = dir.GetFiles();
                int index = 0;
                while (true)
                {
                    if (index < files.Length)
                    {
                        FileInfo item = files[index];
                        if (item.Name != "Sandbox.sbc")
                        {
                            index++;
                            continue;
                        }
                        if (this.m_loadedWorldsByFilePaths.ContainsKey(item.DirectoryName))
                        {
                            this.m_saveEntriesToCreate.Add(item);
                        }
                        flag = true;
                    }
                    if (!flag)
                    {
                        base.AddFolderRow(dir);
                    }
                    return;
                }
            }
        }

        private void AddSavedGame(FileInfo fileInfo)
        {
            MyWorldInfo info = this.m_loadedWorldsByFilePaths[fileInfo.DirectoryName];
            if (this.SearchFilterTest(info.SessionName))
            {
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(fileInfo);
                Color? textColor = null;
                MyGuiControlTable.Cell cell = new MyGuiControlTable.Cell(info.SessionName, fileInfo, null, textColor, new MyGuiHighlightTexture?(base.FileCellIconTexture), base.FileCellIconAlign);
                if (info.IsCorrupted)
                {
                    cell.TextColor = new Color?(Color.Red);
                }
                row.AddCell(cell);
                textColor = null;
                MyGuiHighlightTexture? icon = null;
                row.AddCell(new MyGuiControlTable.Cell(info.LastSaveTime.ToString(), fileInfo, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                base.Add(row);
            }
        }

        public void ForceRefresh()
        {
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.StartLoadingWorldInfos), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.OnLoadingFinished), null));
        }

        public DirectoryInfo GetDirectory(MyGuiControlTable.Row row) => 
            ((row != null) ? (row.UserData as DirectoryInfo) : null);

        public Tuple<string, MyWorldInfo> GetSave(MyGuiControlTable.Row row)
        {
            if (row == null)
            {
                return null;
            }
            FileInfo userData = row.UserData as FileInfo;
            if (userData == null)
            {
                return null;
            }
            return new Tuple<string, MyWorldInfo>(Path.GetDirectoryName(userData.FullName), this.m_loadedWorldsByFilePaths[Path.GetDirectoryName(userData.FullName)]);
        }

        protected override void OnBackDoubleclicked()
        {
            if (!base.m_currentDir.Name.StartsWith("Backup"))
            {
                base.OnBackDoubleclicked();
            }
            else
            {
                base.CurrentDirectory = base.m_currentDir.Parent.Parent.FullName;
                this.InBackupsFolder = false;
                base.IgnoreFirstRowForSort = false;
            }
        }

        private void OnLoadingFinished(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            MyLoadListResult result2 = (MyLoadListResult) result;
            this.m_loadedDirectories.Add(base.CurrentDirectory);
            foreach (Tuple<string, MyWorldInfo> tuple in result2.AvailableSaves)
            {
                this.m_loadedWorldsByFilePaths[tuple.Item1] = tuple.Item2;
            }
            if (result2.ContainsCorruptedWorlds)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SomeWorldFilesCouldNotBeLoaded), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            this.RefreshAfterLoaded();
            screen.CloseScreen();
        }

        public override void Refresh()
        {
            this.RefreshTheWorldInfos();
        }

        private void RefreshAfterLoaded()
        {
            base.Refresh();
            this.m_saveEntriesToCreate.Sort((fileA, fileB) => this.m_loadedWorldsByFilePaths[fileB.DirectoryName].LastSaveTime.CompareTo(this.m_loadedWorldsByFilePaths[fileA.DirectoryName].LastSaveTime));
            foreach (FileInfo info in this.m_saveEntriesToCreate)
            {
                this.AddSavedGame(info);
            }
            this.m_saveEntriesToCreate.Clear();
        }

        private void RefreshTheWorldInfos()
        {
            if (this.m_loadedDirectories.Contains(base.CurrentDirectory))
            {
                this.RefreshAfterLoaded();
            }
            else
            {
                MyStringId? cancelText = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.StartLoadingWorldInfos), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.OnLoadingFinished), null));
            }
        }

        private bool SearchFilterTest(string testString)
        {
            if ((this.SearchTextFilter != null) && (this.SearchTextFilter.Length != 0))
            {
                char[] separator = new char[] { ' ' };
                string str = testString.ToLower();
                foreach (string str2 in this.SearchTextFilter.Split(separator))
                {
                    if (!str.Contains(str2.ToLower()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private IMyAsyncResult StartLoadingWorldInfos() => 
            new MyLoadWorldInfoListResult(base.CurrentDirectory);

        public bool InBackupsFolder { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiControlSaveBrowser.<>c <>9 = new MyGuiControlSaveBrowser.<>c();
            public static Predicate<FileInfo> <>9__8_0;
            public static Func<DirectoryInfo, bool> <>9__11_0;

            internal bool <.ctor>b__8_0(FileInfo info) => 
                false;

            internal bool <AccessBackups>b__11_0(DirectoryInfo dir) => 
                dir.Name.StartsWith("Backup");
        }
    }
}

