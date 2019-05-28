namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlPanel))]
    public class MyGuiControlPanel : MyGuiControlBase
    {
        public MyGuiControlPanel() : this(nullable, nullable, nullable2, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlPanel(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string texture = null, string toolTip = null, MyGuiDrawAlignEnum originAlign = 4) : base(position, size, backgroundColor, toolTip, texture1, false, false, false, MyGuiControlHighlightType.NEVER, originAlign)
        {
            MyGuiSizedTexture texture2 = new MyGuiSizedTexture {
                Texture = texture
            };
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.Center = texture2;
            base.Visible = true;
        }
    }
}

