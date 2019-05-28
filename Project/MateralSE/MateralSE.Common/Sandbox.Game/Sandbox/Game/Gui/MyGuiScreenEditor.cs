namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.Graphics.GUI.IME;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using VRage;
    using VRage.Compiler;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Scripting;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenEditor : MyGuiScreenBase
    {
        private const string CODE_WRAPPER_BEFORE = "using System;\nusing System.Collections.Generic;\nusing VRageMath;\nusing VRage.Game;\nusing System.Text;\nusing Sandbox.ModAPI.Interfaces;\nusing Sandbox.ModAPI.Ingame;\nusing Sandbox.Game.EntityComponents;\nusing VRage.Game.Components;\nusing VRage.Collections;\nusing VRage.Game.ObjectBuilders.Definitions;\nusing VRage.Game.ModAPI.Ingame;\nusing SpaceEngineers.Game.ModAPI.Ingame;\npublic class Program: MyGridProgram\n{\n";
        private const string CODE_WRAPPER_AFTER = "\n}";
        private Action<VRage.Game.ModAPI.ResultEnum> m_resultCallback;
        private Action m_saveCodeCallback;
        private string m_description;
        private VRage.Game.ModAPI.ResultEnum m_screenResult;
        public const int MAX_NUMBER_CHARACTERS = 0x186a0;
        private List<string> m_compilerErrors;
        private MyGuiControlMultilineText m_descriptionBox;
        private MyGuiControlCompositePanel m_descriptionBackgroundPanel;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_openWorkshopButton;
        private MyGuiControlButton m_checkCodeButton;
        private MyGuiControlButton m_help;
        private MyGuiControlLabel m_lineCounter;
        private MyGuiControlLabel m_TextTooLongMessage;
        private MyGuiControlLabel m_LetterCounter;
        private MyGuiControlMultilineEditableText m_editorWindow;

        public MyGuiScreenEditor(string description, Action<VRage.Game.ModAPI.ResultEnum> resultCallback, Action saveCodeCallback) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(1f, 0.9f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_description = "";
            this.m_screenResult = VRage.Game.ModAPI.ResultEnum.CANCEL;
            this.m_compilerErrors = new List<string>();
            this.m_description = description;
            this.m_saveCodeCallback = saveCodeCallback;
            this.m_resultCallback = resultCallback;
            base.CanBeHidden = true;
            base.CanHideOthers = true;
            base.m_closeOnEsc = true;
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        protected MyGuiControlMultilineText AddMultilineText(Vector2? size = new Vector2?(), Vector2? offset = new Vector2?(), float textScale = 1f, bool selectable = false, MyGuiDrawAlignEnum textAlign = 0, MyGuiDrawAlignEnum textBoxAlign = 0)
        {
            Vector2 valueOrDefault;
            Vector2? nullable = size;
            if (nullable != null)
            {
                valueOrDefault = nullable.GetValueOrDefault();
            }
            else
            {
                Vector2? nullable2 = base.Size;
                valueOrDefault = (nullable2 != null) ? nullable2.GetValueOrDefault() : new Vector2(1.2f, 0.5f);
            }
            Vector2 vector = valueOrDefault;
            nullable = offset;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineEditableText control = new MyGuiControlMultilineEditableText(new Vector2?((vector / 2f) + ((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero)), new Vector2?(vector), new VRageMath.Vector4?(Color.White.ToVector4()), "White", 0.8f, textAlign, null, true, true, textBoxAlign, visibleLinesCount, null, textPadding);
            this.m_editorWindow = control;
            this.Controls.Add(control);
            return control;
        }

        public void AppendTextToDescription(string text, string font = "White", float scale = 1f)
        {
            this.m_description = this.m_description + text;
            this.m_descriptionBox.AppendText(text, font, scale, VRageMath.Vector4.One);
        }

        public void AppendTextToDescription(string text, VRageMath.Vector4 color, string font = "White", float scale = 1f)
        {
            this.m_description = this.m_description + text;
            this.m_descriptionBox.AppendText(text, font, scale, color);
        }

        protected void CallResultCallback(VRage.Game.ModAPI.ResultEnum result)
        {
            if (this.m_resultCallback != null)
            {
                this.m_resultCallback(result);
            }
        }

        protected override void Canceling()
        {
            base.Canceling();
            this.m_screenResult = VRage.Game.ModAPI.ResultEnum.CANCEL;
        }

        private void CheckCodeButtonClicked(MyGuiControlButton button)
        {
            this.m_compilerErrors.Clear();
            Assembly assembly = null;
            if (!CompileProgram(this.Description.Text.ToString(), this.m_compilerErrors, ref assembly))
            {
                string str2;
                if (MyFakes.ENABLE_ROSLYN_SCRIPTS && (this.m_compilerErrors.Count > 0))
                {
                    str2 = string.Join("\n", this.m_compilerErrors);
                }
                else
                {
                    str2 = "";
                    foreach (string str3 in this.m_compilerErrors)
                    {
                        str2 = str2 + this.FormatError(str3) + "\n";
                    }
                }
                MyScreenManager.AddScreen(new MyGuiScreenEditorError(str2));
            }
            else if (!MyFakes.ENABLE_ROSLYN_SCRIPTS || (this.m_compilerErrors.Count <= 0))
            {
                StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.ProgrammableBlock_CodeEditor_Title);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ProgrammableBlock_Editor_CompilationOk), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach (string str in this.m_compilerErrors)
                {
                    builder.Append(str);
                    builder.Append('\n');
                }
                MyScreenManager.AddScreen(new MyGuiScreenEditorError(builder.ToString()));
            }
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.RegisterActiveScreen(this);
            }
            base.FocusedControl = this.m_descriptionBox;
        }

        public override bool CloseScreen()
        {
            this.CallResultCallback(this.m_screenResult);
            return base.CloseScreen();
        }

        public static bool CompileProgram(string program, List<string> errors, ref Assembly assembly)
        {
            if (string.IsNullOrEmpty(program))
            {
                return false;
            }
            if (!MyFakes.ENABLE_ROSLYN_SCRIPTS)
            {
                string str = "using System;\nusing System.Collections.Generic;\nusing VRageMath;\nusing VRage.Game;\nusing System.Text;\nusing Sandbox.ModAPI.Interfaces;\nusing Sandbox.ModAPI.Ingame;\nusing Sandbox.Game.EntityComponents;\nusing VRage.Game.Components;\nusing VRage.Collections;\nusing VRage.Game.ObjectBuilders.Definitions;\nusing VRage.Game.ModAPI.Ingame;\nusing SpaceEngineers.Game.ModAPI.Ingame;\npublic class Program: MyGridProgram\n{\n" + program + "\n}";
                string[] source = new string[] { str };
                return IlCompiler.CompileStringIngame(Path.Combine(MyFileSystem.UserDataPath, "IngameScript.dll"), source, out assembly, errors);
            }
            List<MyScriptCompiler.Message> messages = new List<MyScriptCompiler.Message>();
            assembly = MyScriptCompiler.Static.Compile(MyApiTarget.Ingame, Path.Combine(MyFileSystem.UserDataPath, "EditorCode.dll"), MyScriptCompiler.Static.GetIngameScript(program, "Program", typeof(MyGridProgram).Name, "sealed"), messages, "PB Code editor", false).Result;
            errors.Clear();
            errors.AddRange(from m in messages
                orderby m.Severity descending
                select m.Text);
            return (assembly != null);
        }

        private string FormatError(string error)
        {
            string str2;
            try
            {
                char[] separator = new char[] { ':', ')', '(', ',' };
                string[] strArray = error.Split(separator);
                if (strArray.Length <= 2)
                {
                    str2 = error;
                }
                else
                {
                    int num = Convert.ToInt32(strArray[2]) - this.m_editorWindow.MeasureNumLines("using System;\nusing System.Collections.Generic;\nusing VRageMath;\nusing VRage.Game;\nusing System.Text;\nusing Sandbox.ModAPI.Interfaces;\nusing Sandbox.ModAPI.Ingame;\nusing Sandbox.Game.EntityComponents;\nusing VRage.Game.Components;\nusing VRage.Collections;\nusing VRage.Game.ObjectBuilders.Definitions;\nusing VRage.Game.ModAPI.Ingame;\nusing SpaceEngineers.Game.ModAPI.Ingame;\npublic class Program: MyGridProgram\n{\n");
                    string str = strArray[6];
                    int index = 7;
                    while (true)
                    {
                        if (index >= strArray.Length)
                        {
                            str2 = string.Format(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_CompilationFailedErrorFormat), num, str);
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(strArray[index]))
                        {
                            str = str + "," + strArray[index];
                        }
                        index++;
                    }
                }
            }
            catch (Exception)
            {
                return error;
            }
            return str2;
        }

        private string GetCode() => 
            this.m_descriptionBox.Text.ToString();

        public override string GetFriendlyName() => 
            "MyGuiScreenEditor";

        private void HelpButtonClicked(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_INGAMESCRIPTS_HELP, "Steam Workshop", false);
        }

        private void OkButtonClicked(MyGuiControlButton button)
        {
            this.m_screenResult = VRage.Game.ModAPI.ResultEnum.OK;
            this.CloseScreen();
        }

        private void OpenWorkshopButtonClicked(MyGuiControlButton button)
        {
            this.m_openWorkshopButton.Enabled = false;
            this.m_checkCodeButton.Enabled = false;
            this.m_editorWindow.Enabled = false;
            this.m_okButton.Enabled = false;
            this.HideScreen();
            if (MyFakes.I_AM_READY_FOR_NEW_SCRIPT_SCREEN)
            {
                MyScreenManager.AddScreen(MyGuiBlueprintScreen_Reworked.CreateScriptScreen(new Action<string>(this.ScriptSelected), new Func<string>(this.GetCode), new Action(this.WorkshopWindowClosed)));
            }
            else
            {
                MyScreenManager.AddScreen(new MyGuiIngameScriptsPage(new Action<string>(this.ScriptSelected), new Func<string>(this.GetCode), new Action(this.WorkshopWindowClosed)));
            }
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MySpaceTexts.ProgrammableBlock_CodeEditor_Title, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.905f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.905f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.905f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.905f, 0f, captionTextColor);
            this.Controls.Add(control);
            captionTextColor = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.Ok);
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(new Vector2(-0.184f, 0.378f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ProgrammableBlock_CodeEditor_SaveExit_Tooltip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_okButton);
            captionTextColor = null;
            text = MyTexts.Get(MySpaceTexts.ProgrammableBlock_Editor_CheckCode);
            buttonIndex = null;
            this.m_checkCodeButton = new MyGuiControlButton(new Vector2(-0.001f, 0.378f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_CheckCode_Tooltip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.CheckCodeButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_checkCodeButton);
            captionTextColor = null;
            text = MyTexts.Get(MySpaceTexts.ProgrammableBlock_Editor_Help);
            buttonIndex = null;
            this.m_help = new MyGuiControlButton(new Vector2(0.182f, 0.378f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, string.Format(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_HelpTooltip), MySession.Platform), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.HelpButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_help);
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.ProgrammableBlock_Editor_BrowseScripts);
            buttonIndex = null;
            this.m_openWorkshopButton = new MyGuiControlButton(new Vector2(0.365f, 0.378f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_BrowseWorkshop_Tooltip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OpenWorkshopButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_openWorkshopButton);
            this.m_descriptionBackgroundPanel = new MyGuiControlCompositePanel();
            this.m_descriptionBackgroundPanel.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            this.m_descriptionBackgroundPanel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_descriptionBackgroundPanel.Position = new Vector2(-0.451f, -0.356f);
            this.m_descriptionBackgroundPanel.Size = new Vector2(0.902f, 0.664f);
            this.Controls.Add(this.m_descriptionBackgroundPanel);
            Vector2? offset = new Vector2(-0.446f, -0.356f);
            this.m_descriptionBox = this.AddMultilineText(new Vector2(0.5f, 0.44f), offset, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.m_descriptionBox.TextPadding = new MyGuiBorderThickness(0.012f, 0f, 0f, 0f);
            this.m_descriptionBox.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_descriptionBox.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_descriptionBox.Text = new StringBuilder(this.m_description);
            this.m_descriptionBox.Position = Vector2.Zero;
            this.m_descriptionBox.Size = this.m_descriptionBackgroundPanel.Size - new Vector2(0f, 0.03f);
            this.m_descriptionBox.Position = new Vector2(0f, -0.024f);
            offset = null;
            captionTextColor = null;
            this.m_lineCounter = new MyGuiControlLabel(new Vector2(-0.45f, 0.357f), offset, string.Format(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_LineNo), 1, this.m_editorWindow.GetTotalNumLines()), captionTextColor, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_lineCounter);
            offset = null;
            captionTextColor = null;
            this.m_LetterCounter = new MyGuiControlLabel(new Vector2(-0.45f, -0.397f), offset, null, captionTextColor, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_LetterCounter);
            offset = null;
            captionTextColor = null;
            this.m_TextTooLongMessage = new MyGuiControlLabel(new Vector2(-0.34f, -0.4f), offset, null, captionTextColor, 0.8f, "Red", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            base.Elements.Add(this.m_TextTooLongMessage);
            base.FocusedControl = this.m_descriptionBox;
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.RegisterActiveScreen(this);
            }
        }

        private void SaveCodeButtonClicked(MyGuiControlButton button)
        {
            if (this.m_saveCodeCallback != null)
            {
                this.m_saveCodeCallback();
            }
        }

        private void ScriptSelected(string scriptPath)
        {
            string input = null;
            string extension = Path.GetExtension(scriptPath);
            if ((extension == ".cs") && File.Exists(scriptPath))
            {
                input = File.ReadAllText(scriptPath);
            }
            else if (extension == ".bin")
            {
                foreach (string str3 in MyFileSystem.GetFiles(scriptPath, ".cs", MySearchOption.AllDirectories))
                {
                    if (MyFileSystem.FileExists(str3))
                    {
                        Stream stream = MyFileSystem.OpenRead(str3);
                        try
                        {
                            StreamReader reader = new StreamReader(stream);
                            try
                            {
                                input = reader.ReadToEnd();
                            }
                            finally
                            {
                                if (reader == null)
                                {
                                    continue;
                                }
                                reader.Dispose();
                            }
                        }
                        finally
                        {
                            if (stream == null)
                            {
                                continue;
                            }
                            stream.Dispose();
                        }
                    }
                }
            }
            else if (MyFileSystem.IsDirectory(scriptPath))
            {
                foreach (string str4 in MyFileSystem.GetFiles(scriptPath, "*.cs", MySearchOption.AllDirectories))
                {
                    if (MyFileSystem.FileExists(str4))
                    {
                        input = File.ReadAllText(str4);
                        break;
                    }
                }
            }
            if (input != null)
            {
                this.SetDescription(Regex.Replace(input, "\r\n", " \n"));
                this.m_lineCounter.Text = string.Format(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_LineNo), this.m_editorWindow.GetCurrentCarriageLine(), this.m_editorWindow.GetTotalNumLines());
                this.m_openWorkshopButton.Enabled = true;
                this.m_checkCodeButton.Enabled = true;
                this.m_editorWindow.Enabled = true;
                this.m_okButton.Enabled = true;
            }
        }

        public void SetDescription(string desc)
        {
            this.m_description = desc;
            this.m_descriptionBox.Clear();
            this.m_descriptionBox.Text = new StringBuilder(this.m_description);
        }

        public bool TextTooLong() => 
            (this.m_editorWindow.Text.Length > 0x186a0);

        public override bool Update(bool hasFocus)
        {
            if (hasFocus && this.m_editorWindow.CarriageMoved())
            {
                this.m_lineCounter.Text = string.Format(MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_LineNo), this.m_editorWindow.GetCurrentCarriageLine(), this.m_editorWindow.GetTotalNumLines());
            }
            if (hasFocus)
            {
                this.m_LetterCounter.Text = MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_CharacterLimit) + " " + $"{this.m_editorWindow.Text.Length} / {0x186a0}";
                this.m_LetterCounter.Font = !this.TextTooLong() ? "White" : "Red";
                this.m_TextTooLongMessage.Text = this.TextTooLong() ? MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_TextTooLong) : "";
            }
            return base.Update(hasFocus);
        }

        private void WorkshopWindowClosed()
        {
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.RegisterActiveScreen(this);
            }
            this.UnhideScreen();
            base.FocusedControl = this.m_descriptionBox;
            this.m_openWorkshopButton.Enabled = true;
            this.m_checkCodeButton.Enabled = true;
            this.m_editorWindow.Enabled = true;
            this.m_okButton.Enabled = true;
        }

        public MyGuiControlMultilineText Description =>
            this.m_descriptionBox;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenEditor.<>c <>9 = new MyGuiScreenEditor.<>c();
            public static Func<MyScriptCompiler.Message, TErrorSeverity> <>9__37_0;
            public static Func<MyScriptCompiler.Message, string> <>9__37_1;

            internal TErrorSeverity <CompileProgram>b__37_0(MyScriptCompiler.Message m) => 
                m.Severity;

            internal string <CompileProgram>b__37_1(MyScriptCompiler.Message m) => 
                m.Text;
        }
    }
}

