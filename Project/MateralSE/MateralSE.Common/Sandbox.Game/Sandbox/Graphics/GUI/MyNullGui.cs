namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyNullGui : IMyGuiSandbox
    {
        public void AddScreen(MyGuiScreenBase screen)
        {
        }

        public void BackToIntroLogos(Action afterLogosAction)
        {
        }

        public void BackToMainMenu()
        {
        }

        public void Draw()
        {
        }

        public void DrawBadge(string texture, float transitionAlpha, Vector2 position, Vector2 size)
        {
        }

        public void DrawGameLogo(float transitionAlpha, Vector2 position)
        {
        }

        public float GetDefaultTextScaleWithLanguage() => 
            0f;

        public static Vector2 GetNormalizedCoordsAndPreserveOriginalSize(int width, int height) => 
            Vector2.Zero;

        public void HandleInput()
        {
        }

        public void HandleInputAfterSimulation()
        {
        }

        public void HandleRenderProfilerInput()
        {
        }

        public void InsertScreen(MyGuiScreenBase screen, int index)
        {
        }

        public bool IsDebugScreenEnabled() => 
            false;

        public void LoadContent()
        {
        }

        public void LoadData()
        {
        }

        public bool OpenSteamOverlay(string url) => 
            false;

        public void RemoveScreen(MyGuiScreenBase screen)
        {
        }

        public void SetMouseCursorVisibility(bool visible, bool changePosition = true)
        {
        }

        public void ShowModErrors()
        {
        }

        public void SwitchDebugScreensEnabled()
        {
        }

        public void TakeScreenshot(string saveToPath = null, bool ignoreSprites = false, Vector2? sizeMultiplier = new Vector2?(), bool showNotification = true)
        {
        }

        public void TakeScreenshot(int width, int height, string saveToPath = null, bool ignoreSprites = false, bool showNotification = true)
        {
        }

        public void UnloadContent()
        {
        }

        public void Update(int totalTimeInMS)
        {
        }

        public Action<float, Vector2> DrawGameLogoHandler { get; set; }

        public Vector2 MouseCursorPosition
        {
            get => 
                Vector2.Zero;
            set
            {
            }
        }
    }
}

