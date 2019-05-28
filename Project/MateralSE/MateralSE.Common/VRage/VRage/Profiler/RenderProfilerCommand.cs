namespace VRage.Profiler
{
    using System;

    public enum RenderProfilerCommand
    {
        Enable,
        ToggleEnabled,
        JumpToLevel,
        JumpToRoot,
        Pause,
        NextFrame,
        PreviousFrame,
        DisableFrameSelection,
        NextThread,
        PreviousThread,
        IncreaseLevel,
        DecreaseLevel,
        IncreaseLocalArea,
        DecreaseLocalArea,
        IncreaseRange,
        DecreaseRange,
        Reset,
        SetLevel,
        ChangeSortingOrder,
        CopyPathToClipboard,
        TryGoToPathInClipboard,
        GetFomServer,
        GetFromClient,
        SaveToFile,
        LoadFromFile,
        SwapBlockOptimized,
        ToggleOptimizationsEnabled,
        ResetAllOptimizations,
        SwitchBlockRender,
        SwitchGraphContent,
        SwitchShallowProfile,
        ToggleAutoScale,
        SwitchAverageTimes,
        SubtractFromFile,
        EnableAutoScale
    }
}

