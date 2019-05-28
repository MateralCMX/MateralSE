namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyTextPanel : IMyTextSurface, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        [Obsolete("LCD private text is deprecated")]
        string GetPrivateText();
        [Obsolete("LCD private text is deprecated")]
        string GetPrivateTitle();
        [Obsolete("LCD public text is deprecated")]
        string GetPublicText();
        string GetPublicTitle();
        [Obsolete("LCD public text is deprecated")]
        void ReadPublicText(StringBuilder buffer, bool append = false);
        [Obsolete("LCD public text is deprecated")]
        void SetShowOnScreen(ShowTextOnScreenFlag set);
        [Obsolete("LCD private text is deprecated")]
        void ShowPrivateTextOnScreen();
        [Obsolete("LCD public text is deprecated")]
        void ShowPublicTextOnScreen();
        [Obsolete("LCD public text is deprecated")]
        void ShowTextureOnScreen();
        [Obsolete("LCD private text is deprecated")]
        bool WritePrivateText(string value, bool append = false);
        [Obsolete("LCD private text is deprecated")]
        bool WritePrivateTitle(string value, bool append = false);
        [Obsolete("LCD public text is deprecated")]
        bool WritePublicText(string value, bool append = false);
        [Obsolete("LCD public text is deprecated")]
        bool WritePublicText(StringBuilder value, bool append = false);
        bool WritePublicTitle(string value, bool append = false);

        [Obsolete("LCD public text is deprecated")]
        ShowTextOnScreenFlag ShowOnScreen { get; }

        [Obsolete("LCD public text is deprecated")]
        bool ShowText { get; }
    }
}

