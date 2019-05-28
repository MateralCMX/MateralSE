namespace Sandbox.Gui.DirectoryBrowser
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Input;
    using VRage.Utils;

    public class MyGuiControlDirectoryBrowser : MyGuiControlTable
    {
        protected readonly MyDirectoryChangeCancelEventArgs m_cancelEvent;
        protected DirectoryInfo m_topMostDir;
        protected DirectoryInfo m_currentDir;
        protected MyGuiControlTable.Row m_backRow;
        [CompilerGenerated]
        private Action<MyDirectoryChangeCancelEventArgs> DirectoryChanging;
        [CompilerGenerated]
        private Action<MyGuiControlDirectoryBrowser, string> DirectoryChanged;
        [CompilerGenerated]
        private Action<MyGuiControlDirectoryBrowser, string> FileDoubleClick;
        [CompilerGenerated]
        private Action<MyGuiControlDirectoryBrowser, string> DirectoryDoubleclick;
        [CompilerGenerated]
        private Action<MyDirectoryChangeCancelEventArgs> DirectoryDoubleclicking;

        public event Action<MyGuiControlDirectoryBrowser, string> DirectoryChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlDirectoryBrowser, string> directoryChanged = this.DirectoryChanged;
                while (true)
                {
                    Action<MyGuiControlDirectoryBrowser, string> a = directoryChanged;
                    Action<MyGuiControlDirectoryBrowser, string> action3 = (Action<MyGuiControlDirectoryBrowser, string>) Delegate.Combine(a, value);
                    directoryChanged = Interlocked.CompareExchange<Action<MyGuiControlDirectoryBrowser, string>>(ref this.DirectoryChanged, action3, a);
                    if (ReferenceEquals(directoryChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlDirectoryBrowser, string> directoryChanged = this.DirectoryChanged;
                while (true)
                {
                    Action<MyGuiControlDirectoryBrowser, string> source = directoryChanged;
                    Action<MyGuiControlDirectoryBrowser, string> action3 = (Action<MyGuiControlDirectoryBrowser, string>) Delegate.Remove(source, value);
                    directoryChanged = Interlocked.CompareExchange<Action<MyGuiControlDirectoryBrowser, string>>(ref this.DirectoryChanged, action3, source);
                    if (ReferenceEquals(directoryChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyDirectoryChangeCancelEventArgs> DirectoryChanging
        {
            [CompilerGenerated] add
            {
                Action<MyDirectoryChangeCancelEventArgs> directoryChanging = this.DirectoryChanging;
                while (true)
                {
                    Action<MyDirectoryChangeCancelEventArgs> a = directoryChanging;
                    Action<MyDirectoryChangeCancelEventArgs> action3 = (Action<MyDirectoryChangeCancelEventArgs>) Delegate.Combine(a, value);
                    directoryChanging = Interlocked.CompareExchange<Action<MyDirectoryChangeCancelEventArgs>>(ref this.DirectoryChanging, action3, a);
                    if (ReferenceEquals(directoryChanging, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyDirectoryChangeCancelEventArgs> directoryChanging = this.DirectoryChanging;
                while (true)
                {
                    Action<MyDirectoryChangeCancelEventArgs> source = directoryChanging;
                    Action<MyDirectoryChangeCancelEventArgs> action3 = (Action<MyDirectoryChangeCancelEventArgs>) Delegate.Remove(source, value);
                    directoryChanging = Interlocked.CompareExchange<Action<MyDirectoryChangeCancelEventArgs>>(ref this.DirectoryChanging, action3, source);
                    if (ReferenceEquals(directoryChanging, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlDirectoryBrowser, string> DirectoryDoubleclick
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlDirectoryBrowser, string> directoryDoubleclick = this.DirectoryDoubleclick;
                while (true)
                {
                    Action<MyGuiControlDirectoryBrowser, string> a = directoryDoubleclick;
                    Action<MyGuiControlDirectoryBrowser, string> action3 = (Action<MyGuiControlDirectoryBrowser, string>) Delegate.Combine(a, value);
                    directoryDoubleclick = Interlocked.CompareExchange<Action<MyGuiControlDirectoryBrowser, string>>(ref this.DirectoryDoubleclick, action3, a);
                    if (ReferenceEquals(directoryDoubleclick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlDirectoryBrowser, string> directoryDoubleclick = this.DirectoryDoubleclick;
                while (true)
                {
                    Action<MyGuiControlDirectoryBrowser, string> source = directoryDoubleclick;
                    Action<MyGuiControlDirectoryBrowser, string> action3 = (Action<MyGuiControlDirectoryBrowser, string>) Delegate.Remove(source, value);
                    directoryDoubleclick = Interlocked.CompareExchange<Action<MyGuiControlDirectoryBrowser, string>>(ref this.DirectoryDoubleclick, action3, source);
                    if (ReferenceEquals(directoryDoubleclick, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyDirectoryChangeCancelEventArgs> DirectoryDoubleclicking
        {
            [CompilerGenerated] add
            {
                Action<MyDirectoryChangeCancelEventArgs> directoryDoubleclicking = this.DirectoryDoubleclicking;
                while (true)
                {
                    Action<MyDirectoryChangeCancelEventArgs> a = directoryDoubleclicking;
                    Action<MyDirectoryChangeCancelEventArgs> action3 = (Action<MyDirectoryChangeCancelEventArgs>) Delegate.Combine(a, value);
                    directoryDoubleclicking = Interlocked.CompareExchange<Action<MyDirectoryChangeCancelEventArgs>>(ref this.DirectoryDoubleclicking, action3, a);
                    if (ReferenceEquals(directoryDoubleclicking, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyDirectoryChangeCancelEventArgs> directoryDoubleclicking = this.DirectoryDoubleclicking;
                while (true)
                {
                    Action<MyDirectoryChangeCancelEventArgs> source = directoryDoubleclicking;
                    Action<MyDirectoryChangeCancelEventArgs> action3 = (Action<MyDirectoryChangeCancelEventArgs>) Delegate.Remove(source, value);
                    directoryDoubleclicking = Interlocked.CompareExchange<Action<MyDirectoryChangeCancelEventArgs>>(ref this.DirectoryDoubleclicking, action3, source);
                    if (ReferenceEquals(directoryDoubleclicking, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlDirectoryBrowser, string> FileDoubleClick
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlDirectoryBrowser, string> fileDoubleClick = this.FileDoubleClick;
                while (true)
                {
                    Action<MyGuiControlDirectoryBrowser, string> a = fileDoubleClick;
                    Action<MyGuiControlDirectoryBrowser, string> action3 = (Action<MyGuiControlDirectoryBrowser, string>) Delegate.Combine(a, value);
                    fileDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlDirectoryBrowser, string>>(ref this.FileDoubleClick, action3, a);
                    if (ReferenceEquals(fileDoubleClick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlDirectoryBrowser, string> fileDoubleClick = this.FileDoubleClick;
                while (true)
                {
                    Action<MyGuiControlDirectoryBrowser, string> source = fileDoubleClick;
                    Action<MyGuiControlDirectoryBrowser, string> action3 = (Action<MyGuiControlDirectoryBrowser, string>) Delegate.Remove(source, value);
                    fileDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlDirectoryBrowser, string>>(ref this.FileDoubleClick, action3, source);
                    if (ReferenceEquals(fileDoubleClick, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlDirectoryBrowser(string topMostDirectory = null, string initialDirectory = null, Predicate<DirectoryInfo> dirPredicate = null, Predicate<FileInfo> filePredicate = null)
        {
            if (!string.IsNullOrEmpty(topMostDirectory))
            {
                this.m_topMostDir = new DirectoryInfo(topMostDirectory);
            }
            this.m_currentDir = string.IsNullOrEmpty(initialDirectory) ? new DirectoryInfo(Directory.GetCurrentDirectory()) : new DirectoryInfo(initialDirectory);
            this.DirPredicate = dirPredicate;
            this.FilePredicate = filePredicate;
            this.FolderCellIconTexture = MyGuiConstants.TEXTURE_ICON_MODS_LOCAL;
            this.FolderCellIconAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            base.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnItemDoubleClicked);
            base.ColumnsCount = 2;
            float[] p = new float[] { 0.65f, 0.35f };
            base.SetCustomColumnWidths(p);
            base.SetColumnName(0, MyTexts.Get(MyCommonTexts.Name));
            base.SetColumnName(1, MyTexts.Get(MyCommonTexts.Created));
            this.SetColumnComparison(0, (cellA, cellB) => cellA.Text.CompareToIgnoreCase(cellB.Text));
            this.SetColumnComparison(1, (cellA, cellB) => cellB.Text.CompareToIgnoreCase(cellA.Text));
            this.m_cancelEvent = new MyDirectoryChangeCancelEventArgs(null, null, this);
            this.Refresh();
        }

        protected virtual void AddBackRow()
        {
            if (this.m_backRow == null)
            {
                this.m_backRow = new MyGuiControlTable.Row(null);
                Color? textColor = null;
                this.m_backRow.AddCell(new MyGuiControlTable.Cell("..", null, null, textColor, new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_BLUEPRINTS_ARROW), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            base.Add(this.m_backRow);
            base.IgnoreFirstRowForSort = true;
        }

        protected virtual void AddFileRow(FileInfo file)
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(file);
            Color? textColor = null;
            row.AddCell(new MyGuiControlTable.Cell(file.Name, file, null, textColor, new MyGuiHighlightTexture?(this.FileCellIconTexture), this.FileCellIconAlign));
            textColor = null;
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(file.CreationTime.ToString(), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            base.Add(row);
        }

        protected virtual void AddFolderRow(DirectoryInfo dir)
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(dir);
            Color? textColor = null;
            row.AddCell(new MyGuiControlTable.Cell(dir.Name, dir, null, textColor, new MyGuiHighlightTexture?(this.FolderCellIconTexture), this.FolderCellIconAlign));
            textColor = null;
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(string.Empty, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            base.Add(row);
        }

        public override MyGuiControlBase HandleInput()
        {
            if (MyInput.Static.IsNewXButton1MousePressed() || MyInput.Static.IsNewKeyPressed(MyKeys.Back))
            {
                this.OnBackDoubleclicked();
            }
            return base.HandleInput();
        }

        protected bool NotifyDirectoryChanging(string from, string to)
        {
            if (this.DirectoryChanging == null)
            {
                return false;
            }
            this.m_cancelEvent.From = from;
            this.m_cancelEvent.To = to;
            this.m_cancelEvent.Cancel = false;
            this.DirectoryChanging(this.m_cancelEvent);
            return this.m_cancelEvent.Cancel;
        }

        protected virtual void OnBackDoubleclicked()
        {
            if (this.m_currentDir.Parent != null)
            {
                string fullName = this.m_currentDir.Parent.FullName;
                if (!this.NotifyDirectoryChanging(this.m_currentDir.FullName, fullName))
                {
                    this.TraverseToDirectory(fullName);
                }
            }
        }

        protected virtual void OnDirectoryDoubleclicked(DirectoryInfo info)
        {
            if (!this.NotifyDirectoryChanging(this.m_currentDir.FullName, info.FullName))
            {
                this.TraverseToDirectory(info.FullName);
            }
        }

        protected virtual void OnFileDoubleclicked(FileInfo info)
        {
            if (this.FileDoubleClick != null)
            {
                this.FileDoubleClick(this, info.FullName);
            }
        }

        private void OnItemDoubleClicked(MyGuiControlTable myGuiControlTable, MyGuiControlTable.EventArgs eventArgs)
        {
            if (eventArgs.RowIndex < base.RowsCount)
            {
                MyGuiControlTable.Row objA = base.GetRow(eventArgs.RowIndex);
                if (objA != null)
                {
                    if (ReferenceEquals(objA, this.m_backRow))
                    {
                        this.OnBackDoubleclicked();
                    }
                    else
                    {
                        DirectoryInfo userData = objA.UserData as DirectoryInfo;
                        if (userData != null)
                        {
                            this.OnDirectoryDoubleclicked(userData);
                        }
                        else
                        {
                            FileInfo info = objA.UserData as FileInfo;
                            if (info != null)
                            {
                                this.OnFileDoubleclicked(info);
                            }
                        }
                    }
                }
            }
        }

        public virtual void Refresh()
        {
            base.Clear();
            DirectoryInfo[] directories = this.m_currentDir.GetDirectories();
            FileInfo[] files = this.m_currentDir.GetFiles();
            char[] trimChars = new char[] { Path.DirectorySeparatorChar };
            if (!this.m_topMostDir.FullName.TrimEnd(trimChars).Equals(this.m_currentDir.FullName, StringComparison.OrdinalIgnoreCase))
            {
                this.AddBackRow();
            }
            foreach (DirectoryInfo info in directories)
            {
                if ((this.DirPredicate == null) || this.DirPredicate(info))
                {
                    this.AddFolderRow(info);
                }
            }
            foreach (FileInfo info2 in files)
            {
                if ((this.FilePredicate == null) || this.FilePredicate(info2))
                {
                    this.AddFileRow(info2);
                }
            }
            base.ScrollToSelection();
        }

        public bool SetTopMostAndCurrentDir(string directory)
        {
            DirectoryInfo info = new DirectoryInfo(directory);
            if (!info.Exists)
            {
                return false;
            }
            this.m_topMostDir = info;
            this.m_currentDir = info;
            return true;
        }

        private void TraverseToDirectory(string path)
        {
            if (((path != this.m_currentDir.FullName) && ((this.m_topMostDir == null) || this.m_topMostDir.IsParentOf(path))) && !this.NotifyDirectoryChanging(this.m_currentDir.FullName, path))
            {
                this.m_currentDir = new DirectoryInfo(path);
                this.Refresh();
                if (this.DirectoryChanged != null)
                {
                    this.DirectoryChanged(this, this.m_currentDir.FullName);
                }
            }
        }

        public Color? FolderCellColor { get; set; }

        public MyGuiHighlightTexture FolderCellIconTexture { get; set; }

        public MyGuiDrawAlignEnum FolderCellIconAlign { get; set; }

        public Color? FileCellColor { get; set; }

        public MyGuiHighlightTexture FileCellIconTexture { get; set; }

        public MyGuiDrawAlignEnum FileCellIconAlign { get; set; }

        public Predicate<DirectoryInfo> DirPredicate { get; private set; }

        public Predicate<FileInfo> FilePredicate { get; private set; }

        public string CurrentDirectory
        {
            get => 
                this.m_currentDir.FullName;
            set => 
                this.TraverseToDirectory(value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiControlDirectoryBrowser.<>c <>9 = new MyGuiControlDirectoryBrowser.<>c();
            public static Comparison<MyGuiControlTable.Cell> <>9__54_0;
            public static Comparison<MyGuiControlTable.Cell> <>9__54_1;

            internal int <.ctor>b__54_0(MyGuiControlTable.Cell cellA, MyGuiControlTable.Cell cellB) => 
                cellA.Text.CompareToIgnoreCase(cellB.Text);

            internal int <.ctor>b__54_1(MyGuiControlTable.Cell cellA, MyGuiControlTable.Cell cellB) => 
                cellB.Text.CompareToIgnoreCase(cellA.Text);
        }
    }
}

