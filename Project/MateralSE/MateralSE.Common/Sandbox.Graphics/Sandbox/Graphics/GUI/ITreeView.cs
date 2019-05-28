namespace Sandbox.Graphics.GUI
{
    using System;

    internal interface ITreeView
    {
        MyTreeViewItem GetItem(int index);
        int GetItemCount();
    }
}

