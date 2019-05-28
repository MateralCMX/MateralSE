namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.SessionComponents;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;

    public class MyGuiControlVoxelHandSettings : MyGuiControlBase
    {
        public MyGuiControlVoxelHandSettings() : base(nullable, new Vector2(0.263f, 0.4f), nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
        {
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                base2 = base.HandleInputElements();
            }
            return base2;
        }

        public void UpdateControls()
        {
            IMyVoxelBrush @static = null;
            if (this.Item.Definition.Id.SubtypeName == "Box")
            {
                @static = MyBrushBox.Static;
            }
            else if (this.Item.Definition.Id.SubtypeName == "Capsule")
            {
                @static = MyBrushCapsule.Static;
            }
            else if (this.Item.Definition.Id.SubtypeName == "Ramp")
            {
                @static = MyBrushRamp.Static;
            }
            else if (this.Item.Definition.Id.SubtypeName == "Sphere")
            {
                @static = MyBrushSphere.Static;
            }
            else if (this.Item.Definition.Id.SubtypeName == "AutoLevel")
            {
                @static = MyBrushAutoLevel.Static;
            }
            if (@static != null)
            {
                base.Elements.Clear();
                foreach (MyGuiControlBase base2 in @static.GetGuiControls())
                {
                    base.Elements.Add(base2);
                }
            }
        }

        internal void UpdateFromBrush(IMyVoxelBrush shape)
        {
            base.Elements.Clear();
            foreach (MyGuiControlBase base2 in shape.GetGuiControls())
            {
                base.Elements.Add(base2);
            }
        }

        public MyToolbarItemVoxelHand Item { get; set; }
    }
}

