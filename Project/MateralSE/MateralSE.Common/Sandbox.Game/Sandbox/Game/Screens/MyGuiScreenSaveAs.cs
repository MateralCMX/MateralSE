namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenSaveAs : MyGuiScreenBase
    {
        private MyGuiControlTextbox m_nameTextbox;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyWorldInfo m_copyFrom;
        private List<string> m_existingSessionNames;
        private string m_sessionPath;
        private bool m_fromMainMenu;
        [CompilerGenerated]
        private Action SaveAsConfirm;

        public event Action SaveAsConfirm
        {
            [CompilerGenerated] add
            {
                Action saveAsConfirm = this.SaveAsConfirm;
                while (true)
                {
                    Action a = saveAsConfirm;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    saveAsConfirm = Interlocked.CompareExchange<Action>(ref this.SaveAsConfirm, action3, a);
                    if (ReferenceEquals(saveAsConfirm, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action saveAsConfirm = this.SaveAsConfirm;
                while (true)
                {
                    Action source = saveAsConfirm;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    saveAsConfirm = Interlocked.CompareExchange<Action>(ref this.SaveAsConfirm, action3, source);
                    if (ReferenceEquals(saveAsConfirm, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenSaveAs(string sessionName) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.2805344f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionSaveAs, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.122f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(list2);
            this.m_existingSessionNames = null;
            this.m_fromMainMenu = true;
            float y = -0.027f;
            captionTextColor = null;
            this.m_nameTextbox = new MyGuiControlTextbox(new Vector2(0f, y), sessionName, 0x4b, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_nameTextbox.Size = new Vector2(0.385f, 1f);
            Vector2? position = null;
            position = null;
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            position = null;
            position = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            Vector2 vector = new Vector2(0.002f, (base.m_size.Value.Y / 2f) - 0.045f);
            Vector2 vector2 = new Vector2(0.018f, 0f);
            this.m_okButton.Position = vector - vector2;
            this.m_cancelButton.Position = vector + vector2;
            this.m_okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            this.m_cancelButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            this.m_nameTextbox.SetToolTip(string.Format(MyTexts.GetString(MyCommonTexts.ToolTipWorldSettingsName), 5, 0x80));
            this.Controls.Add(this.m_nameTextbox);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_nameTextbox.MoveCarriageToEnd();
            base.CloseButtonEnabled = true;
            base.OnEnterCallback = new Action(this.OnEnterPressed);
        }

        public MyGuiScreenSaveAs(MyWorldInfo copyFrom, string sessionPath, List<string> existingSessionNames) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.2805344f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionSaveAs, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(list2);
            float y = -0.027f;
            captionTextColor = null;
            this.m_nameTextbox = new MyGuiControlTextbox(new Vector2(0f, y), copyFrom.SessionName, 0x4b, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_nameTextbox.Size = new Vector2(0.385f, 1f);
            Vector2? position = null;
            position = null;
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            position = null;
            position = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            Vector2 vector = new Vector2(0.002f, (base.m_size.Value.Y / 2f) - 0.071f);
            Vector2 vector2 = new Vector2(0.018f, 0f);
            this.m_okButton.Position = vector - vector2;
            this.m_cancelButton.Position = vector + vector2;
            this.m_okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
            this.m_cancelButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            this.m_nameTextbox.SetToolTip(string.Format(MyTexts.GetString(MyCommonTexts.ToolTipWorldSettingsName), 5, 0x80));
            this.Controls.Add(this.m_nameTextbox);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            this.m_nameTextbox.MoveCarriageToEnd();
            this.m_copyFrom = copyFrom;
            this.m_sessionPath = sessionPath;
            this.m_existingSessionNames = existingSessionNames;
            base.CloseButtonEnabled = true;
            base.OnEnterCallback = new Action(this.OnEnterPressed);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenSaveAs";

        private void OnCancelButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void OnEnterPressed()
        {
            this.TrySaveAs();
        }

        private void OnOkButtonClick(MyGuiControlButton sender)
        {
            this.TrySaveAs();
        }

        private bool TrySaveAs()
        {
            MyStringId? nullable2;
            MyStringId? nullable = null;
            if (this.m_nameTextbox.Text.ToString().Replace(':', '-').IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                nullable = new MyStringId?(MyCommonTexts.ErrorNameInvalid);
            }
            else if (this.m_nameTextbox.Text.Length < 5)
            {
                nullable = new MyStringId?(MyCommonTexts.ErrorNameTooShort);
            }
            else if (this.m_nameTextbox.Text.Length > 0x80)
            {
                nullable = new MyStringId?(MyCommonTexts.ErrorNameTooLong);
            }
            if (this.m_existingSessionNames != null)
            {
                using (List<string>.Enumerator enumerator = this.m_existingSessionNames.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != this.m_nameTextbox.Text)
                        {
                            continue;
                        }
                        nullable = new MyStringId?(MyCommonTexts.ErrorNameAlreadyExists);
                    }
                }
            }
            if (nullable != null)
            {
                nullable2 = null;
                nullable2 = null;
                nullable2 = null;
                nullable2 = null;
                Vector2? size = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(nullable.Value), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable2, nullable2, nullable2, nullable2, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
                screen.SkipTransition = true;
                screen.InstantClose = false;
                MyGuiSandbox.AddScreen(screen);
                return false;
            }
            if (!this.m_fromMainMenu)
            {
                this.m_copyFrom.SessionName = this.m_nameTextbox.Text;
                nullable2 = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.SavingPleaseWait, nullable2, () => new SaveResult(MyUtils.StripInvalidChars(this.m_nameTextbox.Text), this.m_sessionPath, this.m_copyFrom), delegate (IMyAsyncResult result, MyGuiScreenProgressAsync screen) {
                    screen.CloseScreen();
                    this.CloseScreen();
                    Action saveAsConfirm = this.SaveAsConfirm;
                    if (saveAsConfirm != null)
                    {
                        saveAsConfirm();
                    }
                }, null));
                return true;
            }
            string str = MyUtils.StripInvalidChars(this.m_nameTextbox.Text);
            if (string.IsNullOrWhiteSpace(str))
            {
                str = MyLocalCache.GetSessionSavesPath(str + MyUtils.GetRandomInt(0x7fffffff).ToString("########"), false, false);
            }
            MyAsyncSaving.Start(null, str, false);
            MySession.Static.Name = this.m_nameTextbox.Text;
            this.CloseScreen();
            return true;
        }

        private class SaveResult : IMyAsyncResult
        {
            public SaveResult(string saveDir, string sessionPath, MyWorldInfo copyFrom)
            {
                this.Task = Parallel.Start(() => this.SaveAsync(saveDir, sessionPath, copyFrom));
            }

            private void SaveAsync(string newSaveName, string sessionPath, MyWorldInfo copyFrom)
            {
                ulong num;
                string path = MyLocalCache.GetSessionSavesPath(newSaveName, false, false);
                while (Directory.Exists(path))
                {
                    path = MyLocalCache.GetSessionSavesPath(newSaveName + MyUtils.GetRandomInt(0x7fffffff).ToString("########"), false, false);
                }
                Directory.CreateDirectory(path);
                MyUtils.CopyDirectory(sessionPath, path);
                MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(path, out num);
                checkpoint.SessionName = copyFrom.SessionName;
                checkpoint.WorkshopId = null;
                MyLocalCache.SaveCheckpoint(checkpoint, path);
            }

            public bool IsCompleted =>
                this.Task.IsComplete;

            public ParallelTasks.Task Task { get; private set; }
        }
    }
}

