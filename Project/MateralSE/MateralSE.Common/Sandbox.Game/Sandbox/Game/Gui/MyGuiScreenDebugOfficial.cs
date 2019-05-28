namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyGuiScreenDebugOfficial : MyGuiScreenDebugBase
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;

        public MyGuiScreenDebugOfficial() : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new Vector2?(SCREEN_SIZE), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), false)
        {
            base.CanBeHidden = true;
            base.CanHideOthers = false;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_canShareInput = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            this.RecreateControls(true);
        }

        public override bool CloseScreen() => 
            base.CloseScreen();

        private void CopyErrorLogToClipboard(MyGuiControlButton obj)
        {
            StringBuilder builder = new StringBuilder();
            if (MyDefinitionErrors.GetErrors().Count<MyDefinitionErrors.Error>() == 0)
            {
                builder.Append(MyTexts.Get(MyCommonTexts.ScreenDebugOfficial_NoErrorText));
            }
            foreach (MyDefinitionErrors.Error error in MyDefinitionErrors.GetErrors())
            {
                builder.Append(error.ToString());
                builder.AppendLine();
            }
            MyClipboardHelper.SetClipboard(builder.ToString());
        }

        private void CreateDebugButton(float usableWidth, MyStringId text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?())
        {
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlButton button = base.AddButton(MyTexts.Get(text), onClick, null, textColor, size, true, true);
            button.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
            button.TextScale = base.m_scale;
            button.Size = new Vector2(usableWidth, button.Size.Y);
            button.Position += new Vector2(-HIDDEN_PART_RIGHT / 2f, 0f);
            button.Enabled = enabled;
            if (tooltip != null)
            {
                button.SetToolTip(tooltip.Value);
            }
        }

        private void CreateErrorLogScreen(MyGuiControlButton obj)
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenDebugErrors());
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugOfficial";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.F11))
            {
                if (MySession.Static.IsServer && ((MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId)) || MySession.Static.IsUserAdmin(Sync.MyId)))
                {
                    MyScreenManager.AddScreen(new MyGuiScreenScriptingTools());
                }
                this.CloseScreen();
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.F12) || MyInput.Static.IsNewKeyPressed(MyKeys.F10))
            {
                this.CloseScreen();
            }
        }

        private void OpenBotsScreen(MyGuiControlButton obj)
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenBotSettings());
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector = new Vector2(-0.05f, 0f);
            Vector2 vector2 = new Vector2(0.02f, 0.02f);
            Vector2 vector3 = new Vector2(0.008f, 0.005f);
            float num = 0.8f;
            float num2 = 0.02f;
            float usableWidth = (SCREEN_SIZE.X - HIDDEN_PART_RIGHT) - (vector2.X * 2f);
            float y = (SCREEN_SIZE.Y - 1f) / 2f;
            base.m_currentPosition = -base.m_size.Value / 2f;
            base.m_currentPosition += vector2;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += y;
            base.m_scale = num;
            base.AddCaption(MyCommonTexts.ScreenDebugOfficial_Caption, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector2 + new Vector2(-HIDDEN_PART_RIGHT, y)), 0.8f);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += MyGuiConstants.SCREEN_CAPTION_DELTA_Y * 2f;
            this.AddCheckBox(MyCommonTexts.ScreenDebugOfficial_EnableDebugDraw, (Func<bool>) (() => MyDebugDrawSettings.ENABLE_DEBUG_DRAW), (Action<bool>) (b => (MyDebugDrawSettings.ENABLE_DEBUG_DRAW = b)), true, null, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector));
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += num2;
            this.AddCheckBox(MyCommonTexts.ScreenDebugOfficial_ModelDummies, (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES = b)), true, null, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector));
            this.AddCheckBox(MyCommonTexts.ScreenDebugOfficial_MountPoints, (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS = b)), true, null, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector));
            this.AddCheckBox(MyCommonTexts.ScreenDebugOfficial_PhysicsPrimitives, (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES), delegate (bool b) {
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS |= b;
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES = b;
            }, true, null, new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector));
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += num2;
            MyStringId? tooltip = null;
            this.CreateDebugButton(usableWidth, MyCommonTexts.ScreenDebugOfficial_ReloadTextures, new Action<MyGuiControlButton>(this.ReloadTextures), true, tooltip);
            tooltip = null;
            this.CreateDebugButton(usableWidth, MyCommonTexts.ScreenDebugOfficial_ReloadModels, new Action<MyGuiControlButton>(this.ReloadModels), true, tooltip);
            this.CreateDebugButton(usableWidth, MyCommonTexts.ScreenDebugOfficial_SavePrefab, new Action<MyGuiControlButton>(this.SavePrefab), (MyClipboardComponent.Static != null) ? MyClipboardComponent.Static.Clipboard.HasCopiedGrids() : false, new MyStringId?(MyCommonTexts.ToolTipSaveShip));
            if (MyPerGameSettings.Game == GameEnum.ME_GAME)
            {
                tooltip = null;
                this.CreateDebugButton(usableWidth, MyCommonTexts.ScreenDebugOfficial_BotSettings, new Action<MyGuiControlButton>(this.OpenBotsScreen), true, tooltip);
            }
            Color white = Color.White;
            base.AddSubcaption(MyTexts.GetString(MyCommonTexts.ScreenDebugOfficial_ErrorLogCaption), new VRageMath.Vector4?(white.ToVector4()), new Vector2(-HIDDEN_PART_RIGHT, 0f), 0.8f);
            tooltip = null;
            this.CreateDebugButton(usableWidth, MyCommonTexts.ScreenDebugOfficial_OpenErrorLog, new Action<MyGuiControlButton>(this.CreateErrorLogScreen), true, tooltip);
            tooltip = null;
            this.CreateDebugButton(usableWidth, MyCommonTexts.ScreenDebugOfficial_CopyErrorLogToClipboard, new Action<MyGuiControlButton>(this.CopyErrorLogToClipboard), true, tooltip);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += num2;
            Vector2 vector4 = (MyGuiManager.GetMaxMouseCoord() / 2f) - base.m_currentPosition;
            vector4.X = usableWidth;
            float* singlePtr6 = (float*) ref vector4.Y;
            singlePtr6[0] -= vector2.Y;
            float* singlePtr7 = (float*) ref base.m_currentPosition.X;
            singlePtr7[0] += vector3.X / 2f;
            VRageMath.Vector4? backgroundColor = null;
            MyGuiControlPanel control = new MyGuiControlPanel(new Vector2?(base.m_currentPosition - vector3), new Vector2?(vector4 + new Vector2(vector3.X, vector3.Y * 2f)), backgroundColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) {
                BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST
            };
            this.Controls.Add(control);
            Vector2? offset = null;
            MyGuiControlMultilineText text = base.AddMultilineText(new Vector2?(vector4), offset, 1f, false);
            if (MyDefinitionErrors.GetErrors().Count<MyDefinitionErrors.Error>() == 0)
            {
                text.AppendText(MyTexts.Get(MyCommonTexts.ScreenDebugOfficial_NoErrorText));
            }
            else
            {
                ListReader<MyDefinitionErrors.Error> errors = MyDefinitionErrors.GetErrors();
                Dictionary<string, Tuple<int, TErrorSeverity>> dictionary = new Dictionary<string, Tuple<int, TErrorSeverity>>();
                foreach (MyDefinitionErrors.Error error in errors)
                {
                    string key = error.ModName ?? "Local Content";
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary[key] = new Tuple<int, TErrorSeverity>(1, error.Severity);
                        continue;
                    }
                    if (((TErrorSeverity) dictionary[key].Item2) == error.Severity)
                    {
                        Tuple<int, TErrorSeverity> tuple = dictionary[key];
                        dictionary[key] = new Tuple<int, TErrorSeverity>(tuple.Item1 + 1, tuple.Item2);
                    }
                }
                List<Tuple<string, int, TErrorSeverity>> list = new List<Tuple<string, int, TErrorSeverity>>();
                foreach (KeyValuePair<string, Tuple<int, TErrorSeverity>> pair in dictionary)
                {
                    list.Add(new Tuple<string, int, TErrorSeverity>(pair.Key, pair.Value.Item1, pair.Value.Item2));
                }
                list.Sort((Comparison<Tuple<string, int, TErrorSeverity>>) ((e1, e2) => (e2.Item3 - e1.Item3)));
                foreach (Tuple<string, int, TErrorSeverity> tuple2 in list)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(tuple2.Item1);
                    builder.Append(" [");
                    if (((TErrorSeverity) tuple2.Item3) == TErrorSeverity.Critical)
                    {
                        builder.Append(MyDefinitionErrors.Error.GetSeverityName(tuple2.Item3, false));
                        builder.Append("]");
                    }
                    else
                    {
                        builder.Append(tuple2.Item2.ToString());
                        builder.Append(" ");
                        builder.Append(MyDefinitionErrors.Error.GetSeverityName(tuple2.Item3, tuple2.Item2 != 1));
                        builder.Append("]");
                    }
                    text.AppendText(builder, text.Font, text.TextScaleWithLanguage, MyDefinitionErrors.Error.GetSeverityColor(tuple2.Item3).ToVector4());
                    text.AppendLine();
                }
            }
        }

        private void ReloadModels(MyGuiControlButton obj)
        {
            MyRenderProxy.ReloadModels();
            MyHud.Notifications.Add(new MyHudNotificationDebug("Reloaded all models in the game (modder only feature)", 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug));
        }

        private void ReloadTextures(MyGuiControlButton obj)
        {
            MyRenderProxy.ReloadTextures();
            MyHud.Notifications.Add(new MyHudNotificationDebug("Reloaded all textures in the game (modder only feature)", 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug));
        }

        private void SavePrefab(MyGuiControlButton obj)
        {
            string name = MyUtils.StripInvalidChars(MyClipboardComponent.Static.Clipboard.CopiedGridsName);
            string path = Path.Combine(MyFileSystem.UserDataPath, "Export", name + ".sbc");
            int num = 1;
            try
            {
                while (true)
                {
                    if (!MyFileSystem.FileExists(path))
                    {
                        MyClipboardComponent.Static.Clipboard.SaveClipboardAsPrefab(name, path);
                        break;
                    }
                    object[] objArray1 = new object[] { name, "_", num, ".sbc" };
                    path = Path.Combine(MyFileSystem.UserDataPath, "Export", string.Concat(objArray1));
                    num++;
                }
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine($"Failed to write prefab at file {path}, message: {exception.Message}, stack:{exception.StackTrace}");
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugOfficial.<>c <>9 = new MyGuiScreenDebugOfficial.<>c();
            public static Func<bool> <>9__5_0;
            public static Action<bool> <>9__5_1;
            public static Func<bool> <>9__5_2;
            public static Action<bool> <>9__5_3;
            public static Func<bool> <>9__5_4;
            public static Action<bool> <>9__5_5;
            public static Func<bool> <>9__5_6;
            public static Action<bool> <>9__5_7;
            public static Comparison<Tuple<string, int, TErrorSeverity>> <>9__5_8;

            internal bool <RecreateControls>b__5_0() => 
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW;

            internal void <RecreateControls>b__5_1(bool b)
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = b;
            }

            internal bool <RecreateControls>b__5_2() => 
                MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES;

            internal void <RecreateControls>b__5_3(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES = b;
            }

            internal bool <RecreateControls>b__5_4() => 
                MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS;

            internal void <RecreateControls>b__5_5(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS = b;
            }

            internal bool <RecreateControls>b__5_6() => 
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES;

            internal void <RecreateControls>b__5_7(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS |= b;
                MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES = b;
            }

            internal int <RecreateControls>b__5_8(Tuple<string, int, TErrorSeverity> e1, Tuple<string, int, TErrorSeverity> e2) => 
                (e2.Item3 - e1.Item3);
        }
    }
}

