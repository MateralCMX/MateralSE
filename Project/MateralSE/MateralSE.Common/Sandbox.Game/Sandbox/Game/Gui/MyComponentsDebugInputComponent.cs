namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner]
    public class MyComponentsDebugInputComponent : MyDebugComponent
    {
        public static List<BoundingBoxD> Boxes = null;
        public static List<MyEntity> DetectedEntities = new List<MyEntity>();

        public MyComponentsDebugInputComponent()
        {
            this.AddShortcut(MyKeys.G, true, false, false, false, () => "Show components Config Screen.", new Func<bool>(this.ShowComponentsConfigScreen));
            this.AddShortcut(MyKeys.H, true, false, false, false, () => "Show entity spawn screen.", new Func<bool>(this.ShowEntitySpawnScreen));
            this.AddShortcut(MyKeys.J, true, false, false, false, () => "Show defined entites spawn screen.", new Func<bool>(this.ShowDefinedEntitySpawnScreen));
        }

        public override void Draw()
        {
            base.Draw();
            bool flag1 = MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
        }

        public override string GetName() => 
            "Components config";

        private bool ShowComponentsConfigScreen()
        {
            if (DetectedEntities.Count == 0)
            {
                return false;
            }
            MyGuiSandbox.AddScreen(new MyGuiScreenConfigComponents(DetectedEntities));
            return true;
        }

        private bool ShowDefinedEntitySpawnScreen()
        {
            MyEntity controlledEntity = MySession.Static.ControlledEntity as MyEntity;
            if (controlledEntity != null)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenSpawnDefinedEntity((Vector3) ((controlledEntity.WorldMatrix.Translation + controlledEntity.WorldMatrix.Forward) + controlledEntity.WorldMatrix.Up)));
            }
            return true;
        }

        private bool ShowEntitySpawnScreen()
        {
            MyEntity controlledEntity = MySession.Static.ControlledEntity as MyEntity;
            if (controlledEntity != null)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenSpawnEntity((Vector3) ((controlledEntity.WorldMatrix.Translation + controlledEntity.WorldMatrix.Forward) + controlledEntity.WorldMatrix.Up)));
            }
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyComponentsDebugInputComponent.<>c <>9 = new MyComponentsDebugInputComponent.<>c();
            public static Func<string> <>9__2_0;
            public static Func<string> <>9__2_1;
            public static Func<string> <>9__2_2;

            internal string <.ctor>b__2_0() => 
                "Show components Config Screen.";

            internal string <.ctor>b__2_1() => 
                "Show entity spawn screen.";

            internal string <.ctor>b__2_2() => 
                "Show defined entites spawn screen.";
        }
    }
}

