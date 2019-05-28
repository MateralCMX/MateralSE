namespace Sandbox
{
    using System;

    [Flags]
    public enum MessageBoxOptions : uint
    {
        OkOnly = 0,
        OkCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5,
        CancelTryContinue = 6,
        IconHand = 0x10,
        IconQuestion = 0x20,
        IconExclamation = 0x30,
        IconAsterisk = 0x40,
        UserIcon = 0x80,
        DefButton2 = 0x100,
        DefButton3 = 0x200,
        DefButton4 = 0x300,
        SystemModal = 0x1000,
        TaskModal = 0x2000,
        Help = 0x4000,
        NoFocus = 0x8000,
        SetForeground = 0x10000,
        DefaultDesktopOnly = 0x20000,
        Topmost = 0x40000,
        Right = 0x80000,
        RTLReading = 0x100000
    }
}

