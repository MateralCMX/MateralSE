namespace Sandbox.Graphics
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.Plugins;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public static class MyGuiManager
    {
        private static Vector2 vector2Zero = Vector2.Zero;
        private static VRageMath.Rectangle? nullRectangle;
        public static int TotalTimeInMilliseconds;
        public const int FAREST_TIME_IN_PAST = -60000;
        private static MyCamera m_camera;
        private static VRageMath.Rectangle m_safeGuiRectangle;
        private static VRageMath.Rectangle m_safeFullscreenRectangle = new VRageMath.Rectangle(0, 0, 640, 480);
        private static float m_safeScreenScale;
        private static VRageMath.Rectangle m_fullscreenRectangle;
        private static bool m_debugScreensEnabled = true;
        private static Vector2 m_hudSize = new Vector2(1f, 0.8f);
        private static Vector2 m_hudSizeHalf;
        private static Vector2 m_minMouseCoord;
        private static Vector2 m_maxMouseCoord;
        private static Vector2 m_minMouseCoordFullscreenHud;
        private static Vector2 m_maxMouseCoordFullscreenHud;
        private static string m_mouseCursorTexture;
        private static Bitmap m_mouseCursorBitmap;
        private static List<MyGuiTextureScreen> m_backgroundScreenTextures;
        private static MyGuiScreenBase m_lastScreenWithFocus;
        private static bool m_fullScreenHudEnabled = false;
        private static Dictionary<MyStringHash, MyFont> m_fontsById = new Dictionary<MyStringHash, MyFont>();
        private static MyScreenShot m_screenshot;
        private static MyLanguagesEnum m_currentLanguage;
        private static Vector2 m_mouseCursorPosition;
        private static HashSet<string> m_sizes = new HashSet<string>();

        static MyGuiManager()
        {
            MyGuiControlsFactory.RegisterDescriptorsFromAssembly(typeof(MyGuiManager).Assembly);
        }

        private static Vector2 CalculateHudSize() => 
            new Vector2(1f, ((float) m_safeFullscreenRectangle.Height) / ((float) m_safeFullscreenRectangle.Width));

        public static Vector2 ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum align, int pixelOffsetX = 0x36, int pixelOffsetY = 0x36)
        {
            Vector2 vector = GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(pixelOffsetX * m_safeScreenScale, pixelOffsetY * m_safeScreenScale));
            switch (align)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return vector;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return new Vector2(vector.X, 0.5f);

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return new Vector2(vector.X, 1f - vector.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return new Vector2(0.5f, vector.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return new Vector2(0.5f, 0.5f);

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return new Vector2(0.5f, 1f - vector.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return new Vector2(1f - vector.X, vector.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return new Vector2(1f - vector.X, 0.5f);

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return new Vector2(1f - vector.X, 1f - vector.Y);
            }
            return vector;
        }

        internal static int ComputeNumCharsThatFit(string font, StringBuilder text, float scale, float maxTextWidth)
        {
            float num = scale * m_safeScreenScale;
            return m_fontsById[MyStringHash.Get(font)].ComputeCharsThatFit(text, num, GetScreenSizeFromNormalizedSize(new Vector2(maxTextWidth, 0f), false).X);
        }

        [Conditional("DEBUG")]
        private static void DebugTextSize(StringBuilder text, ref Vector2 size)
        {
            string item = text.ToString();
            if (m_sizes.Add(item))
            {
                object[] objArray1 = new object[] { "Text = \"", item, "\", Width = ", size.X };
                Console.WriteLine(string.Concat(objArray1));
            }
        }

        public static unsafe void DrawBorders(Vector2 topLeftPosition, Vector2 size, VRageMath.Color color, int borderSize)
        {
            Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(size, false);
            Vector2* vectorPtr1 = (Vector2*) ref screenSizeFromNormalizedSize;
            vectorPtr1 = (Vector2*) new Vector2((float) ((int) screenSizeFromNormalizedSize.X), (float) ((int) screenSizeFromNormalizedSize.Y));
            Vector2 screenCoordinateFromNormalizedCoordinate = GetScreenCoordinateFromNormalizedCoordinate(topLeftPosition, false);
            Vector2* vectorPtr2 = (Vector2*) ref screenCoordinateFromNormalizedCoordinate;
            vectorPtr2 = (Vector2*) new Vector2((float) ((int) screenCoordinateFromNormalizedCoordinate.X), (float) ((int) screenCoordinateFromNormalizedCoordinate.Y));
            Vector2 vector3 = screenCoordinateFromNormalizedCoordinate + new Vector2(screenSizeFromNormalizedSize.X, 0f);
            Vector2 vector4 = screenCoordinateFromNormalizedCoordinate + new Vector2(0f, screenSizeFromNormalizedSize.Y);
            DrawSpriteBatch(@"Textures\GUI\Blank.dds", (int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y, (int) screenSizeFromNormalizedSize.X, borderSize, color, true);
            DrawSpriteBatch(@"Textures\GUI\Blank.dds", ((int) vector3.X) - borderSize, ((int) vector3.Y) + borderSize, borderSize, ((int) screenSizeFromNormalizedSize.Y) - (borderSize * 2), color, true);
            DrawSpriteBatch(@"Textures\GUI\Blank.dds", (int) vector4.X, ((int) vector4.Y) - borderSize, (int) screenSizeFromNormalizedSize.X, borderSize, color, true);
            DrawSpriteBatch(@"Textures\GUI\Blank.dds", (int) screenCoordinateFromNormalizedCoordinate.X, ((int) screenCoordinateFromNormalizedCoordinate.Y) + borderSize, borderSize, ((int) screenSizeFromNormalizedSize.Y) - (borderSize * 2), color, true);
        }

        public static void DrawSprite(string texture, VRageMath.Rectangle rectangle, VRageMath.Color color, bool waitTillLoaded = true)
        {
            VRageMath.RectangleF destination = new VRageMath.RectangleF((float) rectangle.X, (float) rectangle.Y, (float) rectangle.Width, (float) rectangle.Height);
            DrawSprite(texture, ref destination, false, ref nullRectangle, color, 0f, ref vector2Zero, SpriteEffects.None, 0f, waitTillLoaded);
        }

        public static void DrawSprite(string texture, Vector2 position, VRageMath.Color color, bool waitTillLoaded = true)
        {
            VRageMath.RectangleF destination = new VRageMath.RectangleF(position.X, position.Y, 1f, 1f);
            DrawSprite(texture, ref destination, true, ref nullRectangle, color, 0f, ref vector2Zero, SpriteEffects.None, 0f, waitTillLoaded);
        }

        private static void DrawSprite(string texture, VRageMath.Rectangle destinationRectangle, VRageMath.Rectangle? sourceRectangle, VRageMath.Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, bool waitTillLoaded = true)
        {
            VRageMath.RectangleF destination = new VRageMath.RectangleF((float) destinationRectangle.X, (float) destinationRectangle.Y, (float) destinationRectangle.Width, (float) destinationRectangle.Height);
            DrawSprite(texture, ref destination, false, ref sourceRectangle, color, rotation, ref origin, effects, layerDepth, waitTillLoaded);
        }

        private static void DrawSprite(string texture, ref VRageMath.RectangleF destination, bool scaleDestination, ref VRageMath.Rectangle? sourceRectangle, VRageMath.Color color, float rotation, ref Vector2 origin, SpriteEffects effects, float depth, bool waitTillLoaded = true)
        {
            MyRenderProxy.DrawSprite(texture, ref destination, scaleDestination, ref sourceRectangle, color, rotation, Vector2.UnitX, ref origin, effects, depth, waitTillLoaded, null);
        }

        private static void DrawSprite(string texture, Vector2 position, VRageMath.Rectangle? sourceRectangle, VRageMath.Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, bool waitTillLoaded = true)
        {
            VRageMath.RectangleF destination = new VRageMath.RectangleF(position.X, position.Y, scale, scale);
            DrawSprite(texture, ref destination, true, ref sourceRectangle, color, rotation, ref origin, effects, layerDepth, waitTillLoaded);
        }

        public static void DrawSprite(string texture, Vector2 position, VRageMath.Rectangle? sourceRectangle, VRageMath.Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool waitTillLoaded = true)
        {
            VRageMath.RectangleF destination = new VRageMath.RectangleF(position.X, position.Y, scale.X, scale.Y);
            DrawSprite(texture, ref destination, true, ref sourceRectangle, color, rotation, ref origin, effects, layerDepth, waitTillLoaded);
        }

        public static void DrawSpriteBatch(string texture, VRageMath.Rectangle destinationRectangle, VRageMath.Color color, bool waitTillLoaded = true)
        {
            if (!string.IsNullOrEmpty(texture))
            {
                VRageMath.RectangleF destination = new VRageMath.RectangleF((float) destinationRectangle.X, (float) destinationRectangle.Y, (float) destinationRectangle.Width, (float) destinationRectangle.Height);
                DrawSprite(texture, ref destination, false, ref nullRectangle, color, 0f, ref vector2Zero, SpriteEffects.None, 0f, waitTillLoaded);
            }
        }

        public static void DrawSpriteBatch(string texture, Vector2 pos, VRageMath.Color color, bool waitTillLoaded = true)
        {
            if (!string.IsNullOrEmpty(texture))
            {
                DrawSprite(texture, pos, color, waitTillLoaded);
            }
        }

        public static void DrawSpriteBatch(string texture, int x, int y, int width, int height, VRageMath.Color color, bool waitTillLoaded = true)
        {
            DrawSprite(texture, new VRageMath.Rectangle(x, y, width, height), color, waitTillLoaded);
        }

        public static void DrawSpriteBatch(string texture, Vector2 normalizedCoord, int screenWidth, float normalizedHeight, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, bool waitTillLoaded = true)
        {
            if (!string.IsNullOrEmpty(texture))
            {
                Vector2 screenCoordinateFromNormalizedCoordinate = GetScreenCoordinateFromNormalizedCoordinate(normalizedCoord, false);
                Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(new Vector2(0f, normalizedHeight), false);
                screenSizeFromNormalizedSize.X = screenWidth;
                screenCoordinateFromNormalizedCoordinate = MyUtils.GetCoordAligned(screenCoordinateFromNormalizedCoordinate, screenSizeFromNormalizedSize, drawAlign);
                DrawSprite(texture, new VRageMath.Rectangle((int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y, (int) screenSizeFromNormalizedSize.X, (int) screenSizeFromNormalizedSize.Y), color, waitTillLoaded);
            }
        }

        public static void DrawSpriteBatch(string texture, Vector2 normalizedCoord, float normalizedWidth, int screenHeight, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, bool waitTillLoaded = true)
        {
            if (!string.IsNullOrEmpty(texture))
            {
                Vector2 screenCoordinateFromNormalizedCoordinate = GetScreenCoordinateFromNormalizedCoordinate(normalizedCoord, false);
                Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(new Vector2(normalizedWidth, 0f), false);
                screenSizeFromNormalizedSize.Y = screenHeight;
                screenCoordinateFromNormalizedCoordinate = MyUtils.GetCoordAligned(screenCoordinateFromNormalizedCoordinate, screenSizeFromNormalizedSize, drawAlign);
                DrawSprite(texture, new VRageMath.Rectangle((int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y, (int) screenSizeFromNormalizedSize.X, (int) screenSizeFromNormalizedSize.Y), color, waitTillLoaded);
            }
        }

        public static void DrawSpriteBatch(string texture, Vector2 normalizedCoord, Vector2 normalizedSize, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, bool useFullClientArea = false, bool waitTillLoaded = true)
        {
            if (!string.IsNullOrEmpty(texture))
            {
                Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(normalizedSize, useFullClientArea);
                Vector2 vector = MyUtils.GetCoordAligned(GetScreenCoordinateFromNormalizedCoordinate(normalizedCoord, useFullClientArea), screenSizeFromNormalizedSize, drawAlign);
                VRageMath.Rectangle rectangle = new VRageMath.Rectangle((int) vector.X, (int) vector.Y, (int) screenSizeFromNormalizedSize.X, (int) screenSizeFromNormalizedSize.Y);
                DrawSprite(texture, rectangle, color, waitTillLoaded);
            }
        }

        public static void DrawSpriteBatch(string texture, Vector2 normalizedCoord, Vector2 normalizedSize, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, float rotation, bool waitTillLoaded = true)
        {
            Vector2? originNormalized = null;
            MyRenderProxy.DrawSprite(texture, normalizedCoord, normalizedSize, color, drawAlign, rotation, Vector2.UnitX, 1f, originNormalized, 0f, waitTillLoaded, null);
        }

        public static void DrawSpriteBatch(string texture, Vector2 normalizedCoord, float scale, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, float rotation, Vector2? originNormalized = new Vector2?(), bool waitTillLoaded = true)
        {
            MyRenderProxy.DrawSprite(texture, normalizedCoord, Vector2.One, color, drawAlign, rotation, Vector2.UnitX, scale, originNormalized, 0f, waitTillLoaded, null);
        }

        public static void DrawSpriteBatch(string texture, Vector2 position, VRageMath.Rectangle? sourceRectangle, VRageMath.Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, bool waitTillLoaded = true)
        {
            DrawSprite(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth, waitTillLoaded);
        }

        public static void DrawSpriteBatch(string texture, Vector2 position, VRageMath.Rectangle? sourceRectangle, VRageMath.Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool waitTillLoaded = true)
        {
            DrawSprite(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth, waitTillLoaded);
        }

        public static void DrawSpriteBatchRotate(string texture, Vector2 normalizedCoord, float scale, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, float rotation, Vector2? originNormalized, float rotationSpeed, bool waitTillLoaded = true)
        {
            MyRenderProxy.DrawSprite(texture, normalizedCoord, Vector2.One, color, drawAlign, rotation, Vector2.UnitX, scale, originNormalized, rotationSpeed, waitTillLoaded, null);
        }

        public static void DrawSpriteBatchRoundUp(string texture, Vector2 normalizedCoord, Vector2 normalizedSize, VRageMath.Color color, MyGuiDrawAlignEnum drawAlign, bool waitTillLoaded = true)
        {
            if (!string.IsNullOrEmpty(texture))
            {
                Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(normalizedSize, false);
                Vector2 vector = MyUtils.GetCoordAligned(GetScreenCoordinateFromNormalizedCoordinate(normalizedCoord, false), screenSizeFromNormalizedSize, drawAlign);
                DrawSprite(texture, new VRageMath.Rectangle((int) Math.Floor((double) vector.X), (int) Math.Floor((double) vector.Y), (int) Math.Ceiling((double) screenSizeFromNormalizedSize.X), (int) Math.Ceiling((double) screenSizeFromNormalizedSize.Y)), color, waitTillLoaded);
            }
        }

        public static unsafe void DrawString(string font, StringBuilder text, Vector2 normalizedCoord, float scale, VRageMath.Color? colorMask = new VRageMath.Color?(), MyGuiDrawAlignEnum drawAlign = 0, bool useFullClientArea = false, float maxTextWidth = (float) 1.0 / (float) 0.0)
        {
            Vector2 size = MeasureString(font, text, scale);
            Vector2* vectorPtr1 = (Vector2*) ref size;
            vectorPtr1->X = Math.Min(maxTextWidth, size.X);
            Vector2 screenCoordinateFromNormalizedCoordinate = GetScreenCoordinateFromNormalizedCoordinate(MyUtils.GetCoordTopLeftFromAligned(normalizedCoord, size, drawAlign), useFullClientArea);
            float screenScale = scale * m_safeScreenScale;
            float x = GetScreenSizeFromNormalizedSize(new Vector2(maxTextWidth, 0f), false).X;
            VRageMath.Color? nullable = colorMask;
            MyRenderProxy.DrawString((int) MyStringHash.Get(font), screenCoordinateFromNormalizedCoordinate, (nullable != null) ? nullable.GetValueOrDefault() : new VRageMath.Color(MyGuiConstants.LABEL_TEXT_COLOR), text.ToString(), screenScale, x, null);
        }

        public static string GetBackgroundTextureFilenameByAspectRatio(Vector2 normalizedSize)
        {
            Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(normalizedSize, false);
            float num = screenSizeFromNormalizedSize.X / screenSizeFromNormalizedSize.Y;
            float maxValue = float.MaxValue;
            string filename = null;
            foreach (MyGuiTextureScreen screen in m_backgroundScreenTextures)
            {
                float num3 = Math.Abs((float) (num - screen.GetAspectRatio()));
                if (num3 < maxValue)
                {
                    maxValue = num3;
                    filename = screen.GetFilename();
                }
            }
            return filename;
        }

        public static MyFont GetFont(MyStringHash fontId) => 
            m_fontsById[fontId];

        public static float GetFontHeight(string font, float scale) => 
            GetNormalizedSizeFromScreenSize(new Vector2(0f, ((scale * m_safeScreenScale) * 0.7783784f) * m_fontsById[MyStringHash.Get(font)].LineHeight)).Y;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        public static VRageMath.Rectangle GetFullscreenRectangle() => 
            m_fullscreenRectangle;

        public static Vector2 GetHudNormalizedCoordFromPixelCoord(Vector2 pixelCoord) => 
            new Vector2((pixelCoord.X - m_safeFullscreenRectangle.Left) / ((float) m_safeFullscreenRectangle.Width), ((pixelCoord.Y - m_safeFullscreenRectangle.Top) / ((float) m_safeFullscreenRectangle.Height)) * m_hudSize.Y);

        public static Vector2 GetHudNormalizedSizeFromPixelSize(Vector2 pixelSize) => 
            new Vector2(pixelSize.X / ((float) m_safeFullscreenRectangle.Width), (pixelSize.Y / ((float) m_safeFullscreenRectangle.Height)) * m_hudSize.Y);

        public static Vector2 GetHudPixelCoordFromNormalizedCoord(Vector2 normalizedCoord) => 
            new Vector2(normalizedCoord.X * m_safeFullscreenRectangle.Width, normalizedCoord.Y * m_safeFullscreenRectangle.Height);

        public static Vector2 GetHudSize() => 
            m_hudSize;

        public static Vector2 GetHudSizeHalf() => 
            m_hudSizeHalf;

        public static Vector2 GetMaxMouseCoord() => 
            (FullscreenHudEnabled ? m_maxMouseCoordFullscreenHud : m_maxMouseCoord);

        public static Vector2 GetMinMouseCoord() => 
            (FullscreenHudEnabled ? m_minMouseCoordFullscreenHud : m_minMouseCoord);

        public static string GetMouseCursorTexture() => 
            m_mouseCursorTexture;

        public static Vector2 GetNormalizedCoordinateFromScreenCoordinate(Vector2 screenCoord) => 
            new Vector2((screenCoord.X - m_safeGuiRectangle.Left) / ((float) m_safeGuiRectangle.Width), (screenCoord.Y - m_safeGuiRectangle.Top) / ((float) m_safeGuiRectangle.Height));

        public static Vector2 GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(Vector2 fullScreenCoord) => 
            GetNormalizedCoordinateFromScreenCoordinate(new Vector2(m_safeFullscreenRectangle.Left + fullScreenCoord.X, m_safeFullscreenRectangle.Top + fullScreenCoord.Y));

        public static Vector2 GetNormalizedMousePosition(Vector2 mousePosition, Vector2 mouseAreaSize)
        {
            Vector2 vector;
            vector.X = mousePosition.X * (((float) m_fullscreenRectangle.Width) / mouseAreaSize.X);
            vector.Y = mousePosition.Y * (((float) m_fullscreenRectangle.Height) / mouseAreaSize.Y);
            return GetNormalizedCoordinateFromScreenCoordinate(vector);
        }

        public static Vector2 GetNormalizedSize(Vector2 size, float scale) => 
            GetNormalizedSizeFromScreenSize((size * scale) * m_safeScreenScale);

        public static Vector2 GetNormalizedSizeFromScreenSize(Vector2 screenSize) => 
            new Vector2((m_safeGuiRectangle.Width != 0) ? (screenSize.X / ((float) m_safeGuiRectangle.Width)) : 0f, (m_safeGuiRectangle.Height != 0) ? (screenSize.Y / ((float) m_safeGuiRectangle.Height)) : 0f);

        public static void GetSafeAspectRatioFullScreenPictureSize(Vector2I originalSize, out VRageMath.Rectangle outRect)
        {
            GetSafeAspectRatioPictureSize(originalSize, m_safeFullscreenRectangle, out outRect);
        }

        private static void GetSafeAspectRatioPictureSize(Vector2I originalSize, VRageMath.Rectangle boundingArea, out VRageMath.Rectangle outRect)
        {
            outRect.Width = boundingArea.Width;
            outRect.Height = (int) ((((float) outRect.Width) / ((float) originalSize.X)) * originalSize.Y);
            if (outRect.Height > boundingArea.Height)
            {
                outRect.Height = boundingArea.Height;
                outRect.Width = (int) (outRect.Height * (((float) originalSize.X) / ((float) originalSize.Y)));
            }
            outRect.X = boundingArea.Left + ((boundingArea.Width - outRect.Width) / 2);
            outRect.Y = boundingArea.Top + ((boundingArea.Height - outRect.Height) / 2);
        }

        public static VRageMath.Rectangle GetSafeFullscreenRectangle() => 
            m_safeFullscreenRectangle;

        public static VRageMath.Rectangle GetSafeGuiRectangle() => 
            m_safeGuiRectangle;

        public static void GetSafeHeightFullScreenPictureSize(Vector2I originalSize, out VRageMath.Rectangle outRect)
        {
            GetSafeHeightPictureSize(originalSize, m_safeFullscreenRectangle, out outRect);
        }

        private static void GetSafeHeightPictureSize(Vector2I originalSize, VRageMath.Rectangle boundingArea, out VRageMath.Rectangle outRect)
        {
            outRect.Height = boundingArea.Height;
            outRect.Width = (int) ((((float) outRect.Height) / ((float) originalSize.Y)) * originalSize.X);
            outRect.X = boundingArea.Left + ((boundingArea.Width - outRect.Width) / 2);
            outRect.Y = boundingArea.Top + ((boundingArea.Height - outRect.Height) / 2);
        }

        public static float GetSafeScreenScale() => 
            m_safeScreenScale;

        public static Vector2 GetScreenCoordinateFromNormalizedCoordinate(Vector2 normalizedCoord, bool useFullClientArea = false) => 
            (!useFullClientArea ? new Vector2(m_safeGuiRectangle.Left + (m_safeGuiRectangle.Width * normalizedCoord.X), m_safeGuiRectangle.Top + (m_safeGuiRectangle.Height * normalizedCoord.Y)) : new Vector2(m_safeFullscreenRectangle.Left + (m_safeFullscreenRectangle.Width * normalizedCoord.X), m_safeFullscreenRectangle.Top + (m_safeFullscreenRectangle.Height * normalizedCoord.Y)));

        public static MyScreenShot GetScreenshot() => 
            m_screenshot;

        public static Vector2 GetScreenSizeFromNormalizedSize(Vector2 normalizedSize, bool useFullClientArea = false) => 
            (!useFullClientArea ? new Vector2((m_safeGuiRectangle.Width + 1) * normalizedSize.X, m_safeGuiRectangle.Height * normalizedSize.Y) : new Vector2((m_safeFullscreenRectangle.Width + 1) * normalizedSize.X, m_safeFullscreenRectangle.Height * normalizedSize.Y));

        public static Vector2 GetScreenTextLeftBottomPosition()
        {
            float x = 25f * GetSafeScreenScale();
            return GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(x, GetSafeFullscreenRectangle().Height - (2f * x)));
        }

        public static Vector2 GetScreenTextLeftTopPosition() => 
            GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(25f * GetSafeScreenScale(), 25f * GetSafeScreenScale()));

        public static Vector2 GetScreenTextRightBottomPosition()
        {
            float y = 25f * GetSafeScreenScale();
            GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(GetSafeFullscreenRectangle().Width - y, y));
            return GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(GetSafeFullscreenRectangle().Width - y, GetSafeFullscreenRectangle().Height - (2f * y)));
        }

        public static Vector2 GetScreenTextRightTopPosition()
        {
            float y = 25f * GetSafeScreenScale();
            return GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(GetSafeFullscreenRectangle().Width - y, y));
        }

        public static void InitFont(MyFontDefinition fontDefinition)
        {
            fontDefinition.UseLanguage(m_currentLanguage.ToString());
            if (!string.IsNullOrEmpty(fontDefinition.CompatibilityPath))
            {
                m_fontsById[fontDefinition.Id.SubtypeId] = new MyFont(fontDefinition.CompatibilityPath, 1);
                MyRenderProxy.CreateFont((int) fontDefinition.Id.SubtypeId, fontDefinition.CompatibilityPath, fontDefinition.Default, null, fontDefinition.ColorMask);
            }
            else
            {
                MyObjectBuilder_FontData data = fontDefinition.Resources.FirstOrDefault<MyObjectBuilder_FontData>();
                m_fontsById[fontDefinition.Id.SubtypeId] = new MyFont(data.Path, 1);
                MyRenderProxy.CreateFont((int) fontDefinition.Id.SubtypeId, data.Path, fontDefinition.Default, null, fontDefinition.ColorMask);
            }
        }

        public static void InitFonts()
        {
            InitFonts(MyDefinitionManagerBase.Static.GetDefinitions<MyFontDefinition>());
        }

        public static void InitFonts(IEnumerable<MyFontDefinition> fontDefinitions)
        {
            fontDefinitions.ForEach<MyFontDefinition>(new Action<MyFontDefinition>(MyGuiManager.InitFont));
        }

        public static bool IsDebugScreenEnabled() => 
            m_debugScreensEnabled;

        public static void LoadContent()
        {
            MyRenderProxy.Log.WriteLine("MyGuiManager2.LoadContent() - START");
            MyRenderProxy.Log.IncreaseIndent();
            using (Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.ContentPath, Path.Combine("Textures", "GUI", "MouseCursorHW.png"))))
            {
                m_mouseCursorBitmap = Image.FromStream(stream) as Bitmap;
            }
            SetHWCursorBitmap(m_mouseCursorBitmap);
            SetMouseCursorTexture(@"Textures\GUI\MouseCursor.dds");
            List<MyGuiTextureScreen> list1 = new List<MyGuiTextureScreen>();
            list1.Add(new MyGuiTextureScreen(MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.Texture, (int) MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.SizePx.X, (int) MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.SizePx.Y));
            m_backgroundScreenTextures = list1;
            InitFonts();
            MouseCursorPosition = new Vector2(0.5f, 0.5f);
            MyRenderProxy.Log.DecreaseIndent();
            MyRenderProxy.Log.WriteLine("MyGuiManager2.LoadContent() - END");
        }

        public static void LoadData()
        {
            MyGuiControlsFactory.RegisterDescriptorsFromAssembly(MyPlugins.SandboxGameAssembly);
        }

        public static Vector2 MeasureString(string font, StringBuilder text, float scale)
        {
            float num = scale * m_safeScreenScale;
            return GetNormalizedSizeFromScreenSize(m_fontsById[MyStringHash.GetOrCompute(font)].MeasureString(text, num));
        }

        public static Vector2 MeasureStringRaw(string font, StringBuilder text, float scale)
        {
            MyFont font2;
            return (m_fontsById.TryGetValue(MyStringHash.GetOrCompute(font), out font2) ? font2.MeasureString(text, scale) : Vector2.Zero);
        }

        public static void SetCamera(MyCamera camera)
        {
            m_camera = camera;
        }

        public static void SetHWCursorBitmap(Bitmap b)
        {
            Form f = Control.FromHandle(MyInput.Static.WindowHandle) as Form;
            if (f != null)
            {
                f.Invoke(() => f.Cursor = new Cursor(b.GetHicon()));
            }
        }

        public static void SetMouseCursorTexture(string texture)
        {
            m_mouseCursorTexture = texture;
        }

        public static void Update(int totalTimeInMS)
        {
            TotalTimeInMilliseconds = totalTimeInMS;
        }

        public static bool UpdateScreenSize(Vector2I screenSize, Vector2I screenSizeHalf, bool isTriple)
        {
            int y = screenSize.Y;
            int width = Math.Min(screenSize.X, (int) (y * 1.333333f));
            int x = screenSize.X;
            int height = screenSize.Y;
            m_fullscreenRectangle = new VRageMath.Rectangle(0, 0, screenSize.X, screenSize.Y);
            if (isTriple)
            {
                x /= 3;
            }
            m_safeGuiRectangle = new VRageMath.Rectangle((screenSize.X / 2) - (width / 2), 0, width, y);
            m_safeFullscreenRectangle = new VRageMath.Rectangle((screenSize.X / 2) - (x / 2), 0, x, height);
            m_safeScreenScale = ((float) y) / 1080f;
            m_minMouseCoord = GetNormalizedCoordinateFromScreenCoordinate(new Vector2((float) m_safeFullscreenRectangle.Left, (float) m_safeFullscreenRectangle.Top));
            m_maxMouseCoord = GetNormalizedCoordinateFromScreenCoordinate(new Vector2((float) (m_safeFullscreenRectangle.Left + m_safeFullscreenRectangle.Width), (float) (m_safeFullscreenRectangle.Top + m_safeFullscreenRectangle.Height)));
            m_minMouseCoordFullscreenHud = GetNormalizedCoordinateFromScreenCoordinate(new Vector2((float) m_fullscreenRectangle.Left, (float) m_fullscreenRectangle.Top));
            m_maxMouseCoordFullscreenHud = GetNormalizedCoordinateFromScreenCoordinate(new Vector2((float) (m_fullscreenRectangle.Left + m_fullscreenRectangle.Width), (float) (m_fullscreenRectangle.Top + m_fullscreenRectangle.Height)));
            m_hudSize = CalculateHudSize();
            m_hudSizeHalf = m_hudSize / 2f;
            return (m_fullscreenRectangle != new VRageMath.Rectangle(0, 0, screenSize.X, screenSize.Y));
        }

        public static SpriteScissorToken UsingScissorRectangle(ref VRageMath.RectangleF normalizedRectangle)
        {
            Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(normalizedRectangle.Size, false);
            Vector2 screenCoordinateFromNormalizedCoordinate = GetScreenCoordinateFromNormalizedCoordinate(normalizedRectangle.Position, false);
            MyRenderProxy.SpriteScissorPush(new VRageMath.Rectangle((int) Math.Round((double) screenCoordinateFromNormalizedCoordinate.X, MidpointRounding.AwayFromZero), (int) Math.Round((double) screenCoordinateFromNormalizedCoordinate.Y, MidpointRounding.AwayFromZero), (int) Math.Round((double) screenSizeFromNormalizedSize.X, MidpointRounding.AwayFromZero), (int) Math.Round((double) screenSizeFromNormalizedSize.Y, MidpointRounding.AwayFromZero)), null);
            return new SpriteScissorToken();
        }

        public static MatrixD Camera =>
            m_camera.WorldMatrix;

        public static MatrixD CameraView =>
            m_camera.ViewMatrix;

        public static bool FullscreenHudEnabled
        {
            get => 
                m_fullScreenHudEnabled;
            set => 
                (m_fullScreenHudEnabled = value);
        }

        public static MyLanguagesEnum CurrentLanguage
        {
            get => 
                m_currentLanguage;
            set
            {
                if (m_currentLanguage != value)
                {
                    m_currentLanguage = value;
                    InitFonts();
                }
            }
        }

        public static Vector2 MouseCursorPosition
        {
            get => 
                m_mouseCursorPosition;
            set => 
                (m_mouseCursorPosition = value);
        }

        public static float LanguageTextScale
        {
            [CompilerGenerated]
            get => 
                <LanguageTextScale>k__BackingField;
            [CompilerGenerated]
            set => 
                (<LanguageTextScale>k__BackingField = value);
        }

        public class MyGuiTextureScreen
        {
            private string m_filename;
            private float m_aspectRatio;

            private MyGuiTextureScreen()
            {
            }

            public MyGuiTextureScreen(string filename, int width, int height)
            {
                this.m_filename = filename;
                this.m_aspectRatio = ((float) width) / ((float) height);
            }

            public float GetAspectRatio() => 
                this.m_aspectRatio;

            public string GetFilename() => 
                this.m_filename;
        }

        public class MyScreenShot
        {
            public bool IgnoreSprites;
            public Vector2 SizeMultiplier;
            public string Path;

            public MyScreenShot(Vector2 sizeMultiplier, string path, bool ignoreSprites)
            {
                this.IgnoreSprites = ignoreSprites;
                this.Path = path;
                this.SizeMultiplier = sizeMultiplier;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        public struct SpriteScissorToken : IDisposable
        {
            public void Dispose()
            {
                MyRenderProxy.SpriteScissorPop(null);
            }
        }
    }
}

