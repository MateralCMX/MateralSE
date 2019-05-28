namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Game;
    using VRageMath;

    internal abstract class MyGuiScreenDebugHandItemBase : MyGuiScreenDebugBase
    {
        private List<MyHandItemDefinition> m_handItemDefinitions;
        private MyGuiControlCombobox m_handItemsCombo;
        protected MyHandItemDefinition CurrentSelectedItem;
        private MyCharacter m_playerCharacter;

        protected MyGuiScreenDebugHandItemBase() : base(nullable, false)
        {
            this.m_handItemDefinitions = new List<MyHandItemDefinition>();
        }

        protected virtual void handItemsCombo_ItemSelected()
        {
            this.CurrentSelectedItem = this.m_handItemDefinitions[(int) this.m_handItemsCombo.GetSelectedKey()];
        }

        protected override void OnClosed()
        {
            if (this.m_playerCharacter != null)
            {
                this.m_playerCharacter.OnWeaponChanged -= new EventHandler(this.OnWeaponChanged);
            }
            base.OnClosed();
        }

        private void OnReload(MyGuiControlButton button)
        {
            MyDefinitionManager.Static.ReloadHandItems();
        }

        private void OnSave(MyGuiControlButton button)
        {
            MyDefinitionManager.Static.SaveHandItems();
        }

        protected override void OnShow()
        {
            this.m_playerCharacter = MySession.Static.LocalCharacter;
            if (this.m_playerCharacter != null)
            {
                this.m_playerCharacter.OnWeaponChanged += new EventHandler(this.OnWeaponChanged);
            }
            base.OnShow();
        }

        private void OnTransform(MyGuiControlButton button)
        {
            this.TransformItem(this.CurrentSelectedItem);
        }

        private void OnTransformAll(MyGuiControlButton button)
        {
            foreach (MyHandItemDefinition definition in MyDefinitionManager.Static.GetHandItemDefinitions())
            {
                this.TransformItem(definition);
            }
        }

        public void OnWeaponChanged(object sender, System.EventArgs e)
        {
            this.SelectFirstHandItem();
        }

        protected void RecreateHandItemsCombo()
        {
            Vector4? textColor = null;
            Vector2? size = null;
            this.m_handItemsCombo = base.AddCombo(null, textColor, size, 10);
            this.m_handItemDefinitions.Clear();
            foreach (MyHandItemDefinition definition in MyDefinitionManager.Static.GetHandItemDefinitions())
            {
                MyDefinitionBase base2 = MyDefinitionManager.Static.GetDefinition(definition.PhysicalItemId);
                int count = this.m_handItemDefinitions.Count;
                this.m_handItemDefinitions.Add(definition);
                int? sortOrder = null;
                this.m_handItemsCombo.AddItem((long) count, base2.DisplayNameText, sortOrder, null);
            }
            this.m_handItemsCombo.SortItemsByValueText();
            this.m_handItemsCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.handItemsCombo_ItemSelected);
        }

        protected void RecreateSaveAndReloadButtons()
        {
            Vector4? textColor = null;
            Vector2? size = null;
            base.AddButton(new StringBuilder("Save"), new Action<MyGuiControlButton>(this.OnSave), null, textColor, size, true, true);
            textColor = null;
            size = null;
            base.AddButton(new StringBuilder("Reload"), new Action<MyGuiControlButton>(this.OnReload), null, textColor, size, true, true);
            textColor = null;
            size = null;
            base.AddButton(new StringBuilder("Transform"), new Action<MyGuiControlButton>(this.OnTransform), null, textColor, size, true, true);
            textColor = null;
            size = null;
            base.AddButton(new StringBuilder("Transform All"), new Action<MyGuiControlButton>(this.OnTransformAll), null, textColor, size, true, true);
        }

        private void Reorientate(ref Matrix m)
        {
            Matrix matrix = (Matrix) new MatrixD(-1.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0);
            Vector3 translation = m.Translation;
            m = matrix * m;
            m.Translation = translation;
        }

        private void Reorientate(ref Vector3 v)
        {
            v.X = -v.X;
            v.Y = -v.Y;
        }

        protected void SelectFirstHandItem()
        {
            IMyGunObject<MyDeviceBase> currentWeapon = MySession.Static.LocalCharacter.CurrentWeapon;
            if (currentWeapon == null)
            {
                if (this.m_handItemsCombo.GetItemsCount() > 0)
                {
                    this.m_handItemsCombo.SelectItemByIndex(0);
                }
            }
            else if (this.m_handItemsCombo.GetItemsCount() > 0)
            {
                try
                {
                    if (currentWeapon.DefinitionId.TypeId != typeof(MyObjectBuilder_PhysicalGunObject))
                    {
                        MyDefinitionId physicalItemId = MyDefinitionManager.Static.GetPhysicalItemForHandItem(currentWeapon.DefinitionId).Id;
                        int num = this.m_handItemDefinitions.FindIndex(x => x.PhysicalItemId == physicalItemId);
                        this.m_handItemsCombo.SelectItemByKey((long) num, true);
                    }
                    else
                    {
                        MyDefinitionBase def = MyDefinitionManager.Static.GetDefinition(currentWeapon.DefinitionId);
                        int num2 = this.m_handItemDefinitions.FindIndex(x => x.DisplayNameText == def.DisplayNameText);
                        this.m_handItemsCombo.SelectItemByKey((long) num2, true);
                    }
                }
                catch (Exception)
                {
                    this.m_handItemsCombo.SelectItemByIndex(0);
                }
            }
        }

        private void SwapYZ(ref Matrix m)
        {
            Vector3 translation = m.Translation;
            translation.Y = m.Translation.Z;
            translation.Z = m.Translation.Y;
            m.Translation = translation;
        }

        private void SwapYZ(ref Vector3 v)
        {
            float y = v.Y;
            v.Y = v.Z;
            v.Z = y;
        }

        private void TransformItem(MyHandItemDefinition item)
        {
            this.Reorientate(ref item.LeftHand);
            this.Reorientate(ref item.RightHand);
        }
    }
}

