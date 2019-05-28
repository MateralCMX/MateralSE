namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public static class MyAsyncSaving
    {
        private static Action m_callbackOnFinished;
        private static int m_inProgressCount;

        private static void OnSnapshotDone(bool snapshotSuccess, MySessionSnapshot snapshot, bool wait)
        {
            if (!snapshotSuccess)
            {
                if (!Game.IsDedicated)
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.WorldNotSaved), MySession.Static.Name), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                PopInProgress();
            }
            else
            {
                if (!Game.IsDedicated)
                {
                    string thumbPath = MySession.Static.ThumbPath;
                    try
                    {
                        if (File.Exists(thumbPath))
                        {
                            File.Delete(thumbPath);
                        }
                        MyRenderProxy.TakeScreenshot(new Vector2(0.5f, 0.5f), thumbPath, false, true, false);
                    }
                    catch (Exception exception)
                    {
                        MySandboxGame.Log.WriteLine("Could not take session thumb screenshot. Exception:");
                        MySandboxGame.Log.WriteLine(exception);
                    }
                }
                if (!wait)
                {
                    snapshot.SaveParallel(() => SaveFinished(snapshot));
                }
                else
                {
                    snapshot.Save();
                    SaveFinished(snapshot);
                }
            }
            if (m_callbackOnFinished != null)
            {
                m_callbackOnFinished();
            }
            m_callbackOnFinished = null;
        }

        private static void PopInProgress()
        {
            m_inProgressCount--;
        }

        private static void PushInProgress()
        {
            m_inProgressCount++;
        }

        private static void SaveFinished(MySessionSnapshot snapshot)
        {
            if (!Game.IsDedicated && (MySession.Static != null))
            {
                if (snapshot.SavingSuccess)
                {
                    MyHudNotification notification = new MyHudNotification(MyCommonTexts.WorldSaved, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    object[] arguments = new object[] { MySession.Static.Name };
                    notification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(notification);
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.WorldNotSaved), MySession.Static.Name), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
            PopInProgress();
        }

        public static void Start(Action callbackOnFinished = null, string customName = null, bool wait = false)
        {
            MySessionSnapshot snapshot;
            PushInProgress();
            m_callbackOnFinished = callbackOnFinished;
            bool snapshotSuccess = MySession.Static.Save(out snapshot, customName);
            OnSnapshotDone(snapshotSuccess, snapshot, wait);
        }

        public static bool InProgress =>
            (m_inProgressCount > 0);
    }
}

