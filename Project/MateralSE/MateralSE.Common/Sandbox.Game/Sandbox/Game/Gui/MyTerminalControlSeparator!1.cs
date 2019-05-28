namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyTerminalControlSeparator<TBlock> : MyTerminalControl<TBlock>, IMyTerminalControlSeparator, IMyTerminalControl where TBlock: MyTerminalBlock
    {
        public MyTerminalControlSeparator() : base("Separator")
        {
        }

        protected override MyGuiControlBase CreateGui()
        {
            MyGuiControlSeparatorList list1 = new MyGuiControlSeparatorList();
            list1.Size = new Vector2(0.485f, 0.01f);
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            Vector4? color = null;
            list1.AddHorizontal(Vector2.Zero, 0.225f, 0f, color);
            return list1;
        }

        string IMyTerminalControl.Id =>
            "";
    }
}

