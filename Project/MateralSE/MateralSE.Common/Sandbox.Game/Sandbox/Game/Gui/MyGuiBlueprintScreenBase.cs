namespace Sandbox.Game.Gui
{
    using Sandbox.Game.GUI;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiBlueprintScreenBase : MyGuiScreenDebugBase
    {
        protected string m_localRoot;

        public MyGuiBlueprintScreenBase(Vector2 position, Vector2 size, Vector4 backgroundColor, bool isTopMostScreen) : base(position, new Vector2?(size), new Vector4?(backgroundColor), isTopMostScreen)
        {
            this.m_localRoot = string.Empty;
            this.m_localRoot = MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL;
            base.m_canShareInput = false;
            base.CanBeHidden = true;
            base.CanHideOthers = false;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
        }

        protected MyGuiControlCompositePanel AddCompositePanel(MyGuiCompositeTexture texture, Vector2 position, Vector2 size, MyGuiDrawAlignEnum panelAlign)
        {
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.BackgroundTexture = texture;
            MyGuiControlCompositePanel control = panel1;
            control.Position = position;
            control.Size = size;
            control.OriginAlign = panelAlign;
            this.Controls.Add(control);
            return control;
        }

        public override bool CloseScreen() => 
            base.CloseScreen();

        protected bool DeleteItem(string file)
        {
            if (!Directory.Exists(file))
            {
                return false;
            }
            Directory.Delete(file, true);
            return true;
        }

        protected MyGuiControlLabel MakeLabel(string text, Vector2 position, float textScale = 1f)
        {
            Vector2? size = null;
            return new MyGuiControlLabel(new Vector2?(position), size, text, null, textScale, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
        }

        public virtual void RefreshBlueprintList(bool fromTask = false)
        {
        }
    }
}

