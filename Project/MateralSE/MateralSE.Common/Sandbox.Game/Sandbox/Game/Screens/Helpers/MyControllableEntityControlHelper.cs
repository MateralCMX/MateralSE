namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Utils;

    public class MyControllableEntityControlHelper : MyAbstractControlMenuItem
    {
        protected IMyControllableEntity m_entity;
        private Action<IMyControllableEntity> m_action;
        private Func<IMyControllableEntity, bool> m_valueGetter;
        private string m_label;
        private string m_value;
        private string m_onValue;
        private string m_offValue;

        public MyControllableEntityControlHelper(MyStringId controlId, Action<IMyControllableEntity> action, Func<IMyControllableEntity, bool> valueGetter, MyStringId label, MySupportKeysEnum supportKeys = 0) : this(controlId, action, valueGetter, label, MyCommonTexts.ControlMenuItemValue_On, MyCommonTexts.ControlMenuItemValue_Off, supportKeys)
        {
        }

        public MyControllableEntityControlHelper(MyStringId controlId, Action<IMyControllableEntity> action, Func<IMyControllableEntity, bool> valueGetter, MyStringId label, MyStringId onValue, MyStringId offValue, MySupportKeysEnum supportKeys = 0) : base(controlId, supportKeys)
        {
            this.m_action = action;
            this.m_valueGetter = valueGetter;
            this.m_label = MyTexts.GetString(label);
            this.m_onValue = MyTexts.GetString(onValue);
            this.m_offValue = MyTexts.GetString(offValue);
        }

        public override void Activate()
        {
            this.m_action(this.m_entity);
        }

        public override void Next()
        {
            this.Activate();
        }

        public override void Previous()
        {
            this.Activate();
        }

        public void SetEntity(IMyControllableEntity entity)
        {
            this.m_entity = entity;
        }

        public override void UpdateValue()
        {
            if (this.m_valueGetter(this.m_entity))
            {
                this.m_value = this.m_onValue;
            }
            else
            {
                this.m_value = this.m_offValue;
            }
        }

        public override string Label =>
            this.m_label;

        public override string CurrentValue =>
            this.m_value;
    }
}

