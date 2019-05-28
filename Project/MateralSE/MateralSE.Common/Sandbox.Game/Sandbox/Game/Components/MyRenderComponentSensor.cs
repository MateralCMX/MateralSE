namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentSensor : MyRenderComponent
    {
        private MySensorBase m_sensor;
        private float m_lastHighlight;
        protected Vector4 m_color;
        private bool DrawSensor = true;

        public override void Draw()
        {
            if (this.DrawSensor)
            {
                this.SetHighlight();
                MatrixD worldMatrix = base.Container.Entity.PositionComp.WorldMatrix;
                if (ReferenceEquals(MySession.Static.ControlledEntity, this))
                {
                    Vector4 color = Color.Red.ToVector4();
                    MyStringId? material = null;
                    MySimpleObjectDraw.DrawLine(worldMatrix.Translation, worldMatrix.Translation + ((worldMatrix.Forward * base.Container.Entity.PositionComp.LocalVolume.Radius) * 1.2000000476837158), material, ref color, 0.05f, MyBillboard.BlendTypeEnum.Standard);
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_sensor = base.Container.Entity as MySensorBase;
        }

        protected void SetHighlight()
        {
            this.SetHighlight(new Vector4(0f, 0f, 0f, 0.3f), false);
            if (this.m_sensor.AnyEntityWithState(MySensorBase.EventType.Add))
            {
                this.SetHighlight(new Vector4(1f, 0f, 0f, 0.3f), true);
            }
            else if (this.m_sensor.AnyEntityWithState(MySensorBase.EventType.Delete))
            {
                this.SetHighlight(new Vector4(1f, 0f, 1f, 0.3f), true);
            }
            else if (this.m_sensor.HasAnyMoved())
            {
                this.SetHighlight(new Vector4(0f, 0f, 1f, 0.3f), false);
            }
            else if (this.m_sensor.AnyEntityWithState(MySensorBase.EventType.None))
            {
                this.SetHighlight(new Vector4(0.4f, 0.4f, 0.4f, 0.3f), false);
            }
        }

        private void SetHighlight(Vector4 color, bool keepForMinimalTime = false)
        {
            if (MySandboxGame.TotalGamePlayTimeInMilliseconds > (this.m_lastHighlight + 300f))
            {
                this.m_color = color;
                if (keepForMinimalTime)
                {
                    this.m_lastHighlight = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
            }
        }
    }
}

