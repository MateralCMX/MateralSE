namespace Sandbox.Game.Gui
{
    using Sandbox.Game.GUI;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Text;
    using VRageMath;

    internal class MyGuiDetailScreenDefault : MyGuiDetailScreenBase
    {
        public MyGuiDetailScreenDefault(Action<MyGuiControlListbox.Item> callBack, MyGuiControlListbox.Item selectedItem, MyGuiBlueprintScreen parent, string thumbnailTexture, float textScale) : base(false, parent, thumbnailTexture, selectedItem, textScale)
        {
            string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_DEFAULT_DIRECTORY, base.m_blueprintName, "bp.sbc");
            base.callBack = callBack;
            if (!File.Exists(path))
            {
                base.m_killScreen = true;
            }
            else
            {
                base.m_loadedPrefab = MyBlueprintUtils.LoadPrefab(path);
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
            Vector2 vector1 = new Vector2(0.215f, -0.173f) + base.m_offset;
            Vector2 vector2 = new Vector2(0.13f, 0f);
        }

        public override string GetFriendlyName() => 
            "MyGuiDetailScreenDefault";
    }
}

