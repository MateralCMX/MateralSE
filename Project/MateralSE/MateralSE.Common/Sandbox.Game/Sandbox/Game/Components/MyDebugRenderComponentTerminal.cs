namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentTerminal : MyDebugRenderComponent
    {
        private MyTerminalBlock m_terminal;

        public MyDebugRenderComponentTerminal(MyTerminalBlock terminal) : base(terminal)
        {
            this.m_terminal = terminal;
        }

        public override unsafe void DebugDraw()
        {
            base.DebugDraw();
            if ((MyDebugDrawSettings.DEBUG_DRAW_BLOCK_NAMES && (this.m_terminal.CustomName != null)) && (MySession.Static.ControlledEntity != null))
            {
                Vector3D up;
                MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                if (controlledEntity != null)
                {
                    up = controlledEntity.WorldMatrix.Up;
                }
                else
                {
                    up = Vector3D.Zero;
                }
                Vector3D vectord = up;
                Vector3D worldCoord = this.m_terminal.PositionComp.WorldMatrix.Translation + ((vectord * this.m_terminal.CubeGrid.GridSize) * 0.40000000596046448);
                double num = (worldCoord - MySession.Static.ControlledEntity.Entity.WorldMatrix.Translation).Length();
                if (num <= 35.0)
                {
                    Color* colorPtr1;
                    Color lightSteelBlue = Color.LightSteelBlue;
                    colorPtr1.A = (num < 15.0) ? ((byte) 0xff) : ((byte) ((15.0 - num) * 12.75));
                    colorPtr1 = (Color*) ref lightSteelBlue;
                    MyRenderProxy.DebugDrawText3D(worldCoord, "<- " + this.m_terminal.CustomName.ToString(), lightSteelBlue, (float) Math.Min((double) (8.0 / num), (double) 1.0), false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
                }
            }
        }
    }
}

