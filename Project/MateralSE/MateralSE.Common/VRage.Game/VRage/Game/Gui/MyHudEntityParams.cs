namespace VRage.Game.Gui
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.ModAPI;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyHudEntityParams
    {
        private IMyEntity m_entity;
        private Vector3D m_position;
        public IMyEntity Entity
        {
            get => 
                this.m_entity;
            set
            {
                this.m_entity = value;
                if (value != null)
                {
                    this.EntityId = value.EntityId;
                }
            }
        }
        public long EntityId { get; set; }
        public Vector3D Position
        {
            get
            {
                if ((this.m_entity == null) || (this.m_entity.PositionComp == null))
                {
                    return this.m_position;
                }
                return this.m_entity.PositionComp.GetPosition();
            }
            set => 
                (this.m_position = value);
        }
        public StringBuilder Text { get; set; }
        public MyHudIndicatorFlagsEnum FlagsEnum { get; set; }
        public long Owner { get; set; }
        public MyOwnershipShareModeEnum Share { get; set; }
        public float BlinkingTime { get; set; }
        public Func<bool> ShouldDraw { get; set; }
        public MyHudEntityParams(StringBuilder text, long Owner, MyHudIndicatorFlagsEnum flagsEnum)
        {
            this = new MyHudEntityParams();
            this.Text = text;
            this.FlagsEnum = flagsEnum;
            this.Owner = Owner;
        }

        public MyHudEntityParams(MyObjectBuilder_HudEntityParams builder)
        {
            this = new MyHudEntityParams();
            this.m_entity = null;
            this.m_position = builder.Position;
            this.EntityId = builder.EntityId;
            this.Text = new StringBuilder(builder.Text);
            this.FlagsEnum = builder.FlagsEnum;
            this.Owner = builder.Owner;
            this.Share = builder.Share;
            this.BlinkingTime = builder.BlinkingTime;
        }

        public MyObjectBuilder_HudEntityParams GetObjectBuilder()
        {
            MyObjectBuilder_HudEntityParams params1 = new MyObjectBuilder_HudEntityParams();
            params1.EntityId = this.EntityId;
            params1.Position = this.Position;
            params1.Text = this.Text.ToString();
            params1.FlagsEnum = this.FlagsEnum;
            params1.Owner = this.Owner;
            params1.Share = this.Share;
            params1.BlinkingTime = this.BlinkingTime;
            return params1;
        }
    }
}

