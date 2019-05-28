namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("Game", "Testing Tool")]
    internal class MyGuiScreenDebugTestingTool : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugTestingTool() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugTestingTool";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Test Tool Control", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? textColor = null;
            captionOffset = null;
            this.AddButton("Almighty Button", x => MyTestingToolHelper.Instance.Action_SpawnBlockSaveTestReload(), null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            this.AddButton("Spawn monolith Button (less mighty)", x => MyTestingToolHelper.Instance.Action_Test(), null, textColor, captionOffset);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugTestingTool.<>c <>9 = new MyGuiScreenDebugTestingTool.<>c();
            public static Action<MyGuiControlButton> <>9__1_0;
            public static Action<MyGuiControlButton> <>9__1_1;

            internal void <RecreateControls>b__1_0(MyGuiControlButton x)
            {
                MyTestingToolHelper.Instance.Action_SpawnBlockSaveTestReload();
            }

            internal void <RecreateControls>b__1_1(MyGuiControlButton x)
            {
                MyTestingToolHelper.Instance.Action_Test();
            }
        }
    }
}

