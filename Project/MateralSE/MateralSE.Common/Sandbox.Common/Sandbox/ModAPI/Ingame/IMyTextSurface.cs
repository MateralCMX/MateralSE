namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game.GUI.TextPanel;
    using VRageMath;

    public interface IMyTextSurface
    {
        void AddImagesToSelection(List<string> ids, bool checkExistence = false);
        void AddImageToSelection(string id, bool checkExistence = false);
        void ClearImagesFromSelection();
        MySpriteDrawFrame DrawFrame();
        void GetFonts(List<string> fonts);
        void GetScripts(List<string> scripts);
        void GetSelectedImages(List<string> output);
        void GetSprites(List<string> sprites);
        string GetText();
        Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale);
        void ReadText(StringBuilder buffer, bool append = false);
        void RemoveImageFromSelection(string id, bool removeDuplicates = false);
        void RemoveImagesFromSelection(List<string> ids, bool removeDuplicates = false);
        bool WriteText(string value, bool append = false);
        bool WriteText(StringBuilder value, bool append = false);

        string CurrentlyShownImage { get; }

        float FontSize { get; set; }

        Color FontColor { get; set; }

        Color BackgroundColor { get; set; }

        byte BackgroundAlpha { get; set; }

        float ChangeInterval { get; set; }

        string Font { get; set; }

        TextAlignment Alignment { get; set; }

        string Script { get; set; }

        VRage.Game.GUI.TextPanel.ContentType ContentType { get; set; }

        Vector2 SurfaceSize { get; }

        Vector2 TextureSize { get; }

        bool PreserveAspectRatio { get; set; }

        float TextPadding { get; set; }

        Color ScriptBackgroundColor { get; set; }

        Color ScriptForegroundColor { get; set; }

        string Name { get; }

        string DisplayName { get; }
    }
}

