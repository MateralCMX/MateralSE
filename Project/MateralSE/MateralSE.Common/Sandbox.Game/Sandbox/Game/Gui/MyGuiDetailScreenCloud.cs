namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.ObjectBuilders;
    using VRageMath;

    internal class MyGuiDetailScreenCloud : MyGuiDetailScreenBase
    {
        private MyBlueprintItemInfo m_info;

        public MyGuiDetailScreenCloud(Action<MyGuiControlListbox.Item> callBack, MyGuiControlListbox.Item selectedItem, MyGuiBlueprintScreen parent, string thumbnailTexture, float textScale) : base(false, parent, thumbnailTexture, selectedItem, textScale)
        {
            base.callBack = callBack;
            this.m_info = selectedItem.UserData as MyBlueprintItemInfo;
            if (this.m_info == null)
            {
                base.m_killScreen = true;
            }
            else
            {
                base.m_loadedPrefab = MyBlueprintUtils.LoadPrefabFromCloud(this.m_info);
                if (base.m_loadedPrefab != null)
                {
                    this.RecreateControls(true);
                }
                else
                {
                    StringBuilder messageCaption = new StringBuilder("Error");
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Failed to load the blueprint file."), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    base.m_killScreen = true;
                }
            }
        }

        protected override void CreateButtons()
        {
            Vector2 vector = new Vector2(0.148f, -0.197f) + base.m_offset;
            Vector2 vector1 = new Vector2(0.132f, 0.045f);
            float usableWidth = 0.13f;
            float textScale = base.m_textScale;
            MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.DetailScreen_Button_Delete), new Action<MyGuiControlButton>(this.OnDelete), true, new MyStringId?(MyCommonTexts.Blueprints_DeleteTooltip), textScale).Position = vector;
        }

        public override string GetFriendlyName() => 
            "MyGuiDetailScreenCloud";

        private void OnDelete(MyGuiControlButton obj)
        {
            StringBuilder messageCaption = new StringBuilder("Delete");
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder("Are you sure you want to delete this blueprint?"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    string[] textArray1 = new string[] { MyBlueprintUtils.BLUEPRINT_CLOUD_DIRECTORY, "/", this.m_info.BlueprintName, "/", MyBlueprintUtils.BLUEPRINT_LOCAL_NAME };
                    string fileName = string.Concat(textArray1);
                    if (MyGameService.DeleteFromCloud(fileName))
                    {
                        MyGameService.DeleteFromCloud(fileName + MyObjectBuilderSerializer.ProtobufferExtension);
                    }
                    base.CallResultCallback(null);
                    this.CloseScreen();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnPublish(MyGuiControlButton obj)
        {
        }
    }
}

