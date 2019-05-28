namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    internal interface IMyGuiSandbox
    {
        void AddScreen(MyGuiScreenBase screen);
        void BackToIntroLogos(Action afterLogosAction);
        void BackToMainMenu();
        void Draw();
        void DrawBadge(string texture, float transitionAlpha, Vector2 position, Vector2 size);
        void DrawGameLogo(float transitionAlpha, Vector2 position);
        float GetDefaultTextScaleWithLanguage();
        void HandleInput();
        void HandleInputAfterSimulation();
        void HandleRenderProfilerInput();
        void InsertScreen(MyGuiScreenBase screen, int index);
        bool IsDebugScreenEnabled();
        void LoadContent();
        void LoadData();
        bool OpenSteamOverlay(string url);
        void RemoveScreen(MyGuiScreenBase screen);
        void SetMouseCursorVisibility(bool visible, bool changePosition = true);
        void ShowModErrors();
        void SwitchDebugScreensEnabled();
        void TakeScreenshot(string saveToPath = null, bool ignoreSprites = false, Vector2? sizeMultiplier = new Vector2?(), bool showNotification = true);
        void TakeScreenshot(int width, int height, string saveToPath = null, bool ignoreSprites = false, bool showNotification = true);
        void UnloadContent();
        void Update(int totalTimeInMS);

        Action<float, Vector2> DrawGameLogoHandler { get; set; }

        Vector2 MouseCursorPosition { get; }
    }
}

