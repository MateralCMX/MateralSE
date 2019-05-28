namespace Sandbox.Game.Entities.Blocks
{
    using System;

    public interface IMyTextPanelComponentOwner
    {
        void OpenWindow(bool isEditable, bool sync, bool isPublic);

        MyTextPanelComponent PanelComponent { get; }

        bool IsTextPanelOpen { get; }
    }
}

