namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Input;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenDebugStatistics : MyGuiScreenDebugBase
    {
        private static StringBuilder m_frameDebugText = new StringBuilder(0x400);
        private static StringBuilder m_frameDebugTextRA = new StringBuilder(0x800);
        private static List<StringBuilder> m_texts = new List<StringBuilder>(0x20);
        private static List<StringBuilder> m_rightAlignedtexts = new List<StringBuilder>(0x20);
        private List<MyKeys> m_pressedKeys;
        private static List<StringBuilder> m_statsStrings = new List<StringBuilder>();
        private static int m_stringIndex = 0;

        public MyGuiScreenDebugStatistics() : base(new Vector2(0.5f, 0.5f), new Vector2?(vector), nullable, true)
        {
            this.m_pressedKeys = new List<MyKeys>(10);
            Vector2 vector = new Vector2();
            base.m_isTopMostScreen = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHaveFocus = false;
            base.m_canShareInput = false;
        }

        public void AddDebugTextRA(string s)
        {
            m_frameDebugTextRA.Append(s);
            m_frameDebugTextRA.AppendLine();
        }

        public void AddDebugTextRA(StringBuilder s)
        {
            m_frameDebugTextRA.AppendStringBuilder(s);
            m_frameDebugTextRA.AppendLine();
        }

        private void AddPressedKeys(string groupName, List<MyKeys> keys)
        {
            StringBuilder stringBuilderCache = StringBuilderCache;
            stringBuilderCache.Append(groupName);
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > 0)
                {
                    stringBuilderCache.Append(", ");
                }
                stringBuilderCache.Append(MyInput.Static.GetKeyName(keys[i]));
            }
            m_texts.Add(stringBuilderCache);
        }

        public void AddToFrameDebugText(string s)
        {
            m_frameDebugText.AppendLine(s);
        }

        public void AddToFrameDebugText(StringBuilder s)
        {
            m_frameDebugText.AppendStringBuilder(s);
            m_frameDebugText.AppendLine();
        }

        public void ClearFrameDebugText()
        {
            m_frameDebugText.Clear();
            m_frameDebugTextRA.Clear();
        }

        public override bool Draw()
        {
            if (!base.Draw())
            {
                return false;
            }
            float num = MyGuiConstants.DEBUG_STATISTICS_ROW_DISTANCE;
            float scale = MyGuiConstants.DEBUG_STATISTICS_TEXT_SCALE;
            m_stringIndex = 0;
            m_texts.Clear();
            m_rightAlignedtexts.Clear();
            m_texts.Add(StringBuilderCache.GetFormatedFloat("FPS: ", (float) MyFpsManager.GetFps(), ""));
            m_texts.Add(new StringBuilder("Renderer: ").Append(MyRenderProxy.RendererInterfaceName()));
            if (MySector.MainCamera != null)
            {
                m_texts.Add(GetFormatedVector3(StringBuilderCache, "Camera pos: ", MySector.MainCamera.Position, ""));
            }
            m_texts.Add(MyScreenManager.GetGuiScreensForDebug());
            m_texts.Add(StringBuilderCache.GetFormatedBool("Paused: ", MySandboxGame.IsPaused, ""));
            m_texts.Add(StringBuilderCache.GetFormatedDateTimeOffset("System Time: ", TimeUtil.LocalTime, ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total GAME-PLAY Time: ", TimeSpan.FromMilliseconds((double) MySandboxGame.TotalGamePlayTimeInMilliseconds), ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total Session Time: ", (MySession.Static == null) ? new TimeSpan(0L) : MySession.Static.ElapsedPlayTime, ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total Foot Time: ", (MySession.Static == null) ? new TimeSpan(0L) : MySession.Static.TimeOnFoot, ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total Jetpack Time: ", (MySession.Static == null) ? new TimeSpan(0L) : MySession.Static.TimeOnJetpack, ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total Small Ship Time: ", (MySession.Static == null) ? new TimeSpan(0L) : MySession.Static.TimePilotingSmallShip, ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total Big Ship Time: ", (MySession.Static == null) ? new TimeSpan(0L) : MySession.Static.TimePilotingBigShip, ""));
            m_texts.Add(StringBuilderCache.GetFormatedTimeSpan("Total Time: ", TimeSpan.FromMilliseconds((double) MySandboxGame.TotalTimeInMilliseconds), ""));
            m_texts.Add(StringBuilderCache.GetFormatedLong("GC.GetTotalMemory: ", GC.GetTotalMemory(false), " bytes"));
            m_texts.Add(StringBuilderCache.GetFormatedLong("Environment.WorkingSet: ", WinApi.WorkingSet, " bytes"));
            m_texts.Add(StringBuilderCache.GetFormatedFloat("Allocated videomemory: ", 0f, " MB"));
            m_texts.Add(StringBuilderCache.GetFormatedInt("Sound Instances Total: ", MyAudio.Static.GetSoundInstancesTotal2D(), "").Append(" 2d / ").AppendInt32(MyAudio.Static.GetSoundInstancesTotal3D()).Append(" 3d"));
            if (MyMusicController.Static != null)
            {
                if (MyMusicController.Static.CategoryPlaying.Equals(MyStringId.NullOrEmpty))
                {
                    m_texts.Add(StringBuilderCache.Append("No music playing, last category: " + MyMusicController.Static.CategoryLast.ToString() + ", next track in ").AppendDecimal(Math.Max(0f, MyMusicController.Static.NextMusicTrackIn), 1).Append("s"));
                }
                else
                {
                    m_texts.Add(StringBuilderCache.Append("Playing music category: " + MyMusicController.Static.CategoryPlaying.ToString()));
                }
            }
            if (MyPerGameSettings.UseReverbEffect && MyFakes.AUDIO_ENABLE_REVERB)
            {
                m_texts.Add(StringBuilderCache.Append("Current reverb effect: " + (MyAudio.Static.EnableReverb ? MyEntityReverbDetectorComponent.CurrentReverbPreset.ToLower() : "disabled")));
            }
            StringBuilder stringBuilderCache = StringBuilderCache;
            MyAudio.Static.WriteDebugInfo(stringBuilderCache);
            m_texts.Add(stringBuilderCache);
            for (int i = 0; i < 8; i++)
            {
                m_texts.Add(StringBuilderCache.Clear());
            }
            MyInput.Static.GetPressedKeys(this.m_pressedKeys);
            this.AddPressedKeys("Current keys              : ", this.m_pressedKeys);
            m_texts.Add(StringBuilderCache.Clear());
            m_texts.Add(m_frameDebugText);
            m_rightAlignedtexts.Add(m_frameDebugTextRA);
            Vector2 screenLeftTopPosition = this.GetScreenLeftTopPosition();
            Vector2 screenRightTopPosition = this.GetScreenRightTopPosition();
            for (int j = 0; j < m_texts.Count; j++)
            {
                MyGuiManager.DrawString("White", m_texts[j], screenLeftTopPosition + new Vector2(0f, j * num), scale, new Color?(Color.Yellow), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            }
            for (int k = 0; k < m_rightAlignedtexts.Count; k++)
            {
                MyGuiManager.DrawString("White", m_rightAlignedtexts[k], screenRightTopPosition + new Vector2(-0.3f, k * num), scale, new Color?(Color.Yellow), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            }
            this.ClearFrameDebugText();
            return true;
        }

        private static StringBuilder GetFormatedVector3(StringBuilder sb, string before, Vector3D value, string after = "")
        {
            sb.Clear();
            sb.Append(before);
            sb.Append("{");
            sb.ConcatFormat<double>("{0: #,000} ", value.X, null);
            sb.ConcatFormat<double>("{0: #,000} ", value.Y, null);
            sb.ConcatFormat<double>("{0: #,000} ", value.Z, null);
            sb.Append("}");
            sb.Append(after);
            return sb;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugStatistics";

        private StringBuilder GetLodText(string text, int lod, int value)
        {
            StringBuilder stringBuilderCache = StringBuilderCache;
            stringBuilderCache.Clear();
            stringBuilderCache.ConcatFormat<string, int>("{0}_LOD{1}: ", text, lod, null);
            stringBuilderCache.Concat(value);
            return stringBuilderCache;
        }

        public Vector2 GetScreenLeftTopPosition()
        {
            MyGuiManager.GetSafeFullscreenRectangle();
            return MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(25f * MyGuiManager.GetSafeScreenScale(), 25f * MyGuiManager.GetSafeScreenScale()));
        }

        public Vector2 GetScreenRightTopPosition()
        {
            float y = 25f * MyGuiManager.GetSafeScreenScale();
            return MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(MyGuiManager.GetSafeFullscreenRectangle().Width - y, y));
        }

        private StringBuilder GetShadowText(string text, int cascade, int value)
        {
            StringBuilder stringBuilderCache = StringBuilderCache;
            stringBuilderCache.Clear();
            stringBuilderCache.ConcatFormat<string, int>("{0} (c {1}): ", text, cascade, null);
            stringBuilderCache.Concat(value);
            return stringBuilderCache;
        }

        public static StringBuilder StringBuilderCache
        {
            get
            {
                if (m_stringIndex >= m_statsStrings.Count)
                {
                    m_statsStrings.Add(new StringBuilder(0x400));
                }
                m_stringIndex++;
                return m_statsStrings[m_stringIndex].Clear();
            }
        }
    }
}

