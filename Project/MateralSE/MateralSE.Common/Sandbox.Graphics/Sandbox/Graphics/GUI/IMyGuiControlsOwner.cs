namespace Sandbox.Graphics.GUI
{
    using System;
    using VRageMath;

    public interface IMyGuiControlsOwner
    {
        MyGuiControlBase GetNextFocusControl(MyGuiControlBase currentFocusControl, bool forwardMovement);
        Vector2 GetPositionAbsolute();
        Vector2 GetPositionAbsoluteCenter();
        Vector2 GetPositionAbsoluteTopLeft();
        Vector2? GetSize();

        string DebugNamePath { get; }

        string Name { get; }

        IMyGuiControlsOwner Owner { get; }
    }
}

