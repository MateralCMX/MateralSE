namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Models;
    using VRageMath;

    [MyDebugScreen("Game", "Cube Blocks")]
    internal class MyGuiScreenDebugCubeBlocks : MyGuiScreenDebugBase
    {
        public static MySymmetryAxisEnum? DebugXMirroringAxis;
        public static MySymmetryAxisEnum? DebugYMirroringAxis;
        public static MySymmetryAxisEnum? DebugZMirroringAxis;
        private MyGuiControlSlider m_dummyDrawDistanceSlider;
        private MyGuiControlCombobox m_xMirroringCombo;
        private MyGuiControlCombobox m_yMirroringCombo;
        private MyGuiControlCombobox m_zMirroringCombo;
        private MyGuiControlLabel m_currentMirrorAxisLabel;
        private MyGuiControlLabel m_currentAxisLabel;

        public MyGuiScreenDebugCubeBlocks() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void AddMirroringTypes(MyGuiControlCombobox combo)
        {
            combo.Clear();
            foreach (object obj2 in Enum.GetValues(typeof(MySymmetryAxisEnum)))
            {
                int? sortOrder = null;
                combo.AddItem((long) ((int) obj2), Enum.GetName(typeof(MySymmetryAxisEnum), obj2), sortOrder, null);
            }
        }

        private void DummyDrawDistanceSliderChanged(MyGuiControlSlider slider)
        {
            MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES_DISTANCE = slider.Value;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugCubeBlocks";

        private void m_xMirroringCombo_ItemSelected()
        {
            if (this.m_xMirroringCombo.GetSelectedIndex() != -1)
            {
                DebugXMirroringAxis = new MySymmetryAxisEnum?((MySymmetryAxisEnum) ((int) this.m_xMirroringCombo.GetSelectedKey()));
            }
            else
            {
                DebugXMirroringAxis = null;
            }
        }

        private void m_yMirroringCombo_ItemSelected()
        {
            if (this.m_yMirroringCombo.GetSelectedIndex() != -1)
            {
                DebugYMirroringAxis = new MySymmetryAxisEnum?((MySymmetryAxisEnum) ((int) this.m_yMirroringCombo.GetSelectedKey()));
            }
            else
            {
                DebugYMirroringAxis = null;
            }
        }

        private void m_zMirroringCombo_ItemSelected()
        {
            if (this.m_zMirroringCombo.GetSelectedIndex() != -1)
            {
                DebugZMirroringAxis = new MySymmetryAxisEnum?((MySymmetryAxisEnum) ((int) this.m_zMirroringCombo.GetSelectedKey()));
            }
            else
            {
                DebugZMirroringAxis = null;
            }
        }

        private void onClick_DebugDrawMountPoints(MyGuiControlCheckbox sender)
        {
            MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS = sender.IsChecked;
        }

        private void onClick_DebugDrawMountPointsAll(MyGuiControlCheckbox obj)
        {
            MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_ALL = obj.IsChecked;
        }

        private void onClick_Save(MyGuiControlButton sender)
        {
            foreach (MyCubeBlockDefinition definition in MyDefinitionManager.Static.GetAllDefinitions())
            {
                if (definition == null)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(definition.Model))
                {
                    definition.MountPoints = MyCubeBuilder.AutogenerateMountpoints(MyModels.GetModel(definition.Model), MyDefinitionManager.Static.GetCubeSize(definition.CubeSize)).ToArray();
                }
            }
            MyDefinitionManager.Static.Save("CubeBlocks_*.*");
        }

        private void onClick_SymmetryReset(MyGuiControlButton sender)
        {
            this.m_xMirroringCombo.SelectItemByIndex(-1);
            this.m_yMirroringCombo.SelectItemByIndex(-1);
            this.m_zMirroringCombo.SelectItemByIndex(-1);
            DebugXMirroringAxis = null;
            DebugYMirroringAxis = null;
            DebugZMirroringAxis = null;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Cube blocks", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            this.m_currentMirrorAxisLabel = base.AddLabel("Mirror axis: " + MyCubeBuilderGizmo.CurrentBlockMirrorAxis.ToString(), Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.m_currentAxisLabel = base.AddLabel("Block axis: " + MyCubeBuilderGizmo.CurrentBlockMirrorOption.ToString(), Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            base.m_currentPosition += new Vector2(0f, 0.015f);
            base.AddLabel("X symmetry", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? textColor = null;
            captionOffset = null;
            this.m_xMirroringCombo = base.AddCombo(null, textColor, captionOffset, 10);
            this.AddMirroringTypes(this.m_xMirroringCombo);
            this.m_xMirroringCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_xMirroringCombo_ItemSelected);
            base.AddLabel("Y symmetry", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            textColor = null;
            captionOffset = null;
            this.m_yMirroringCombo = base.AddCombo(null, textColor, captionOffset, 10);
            this.AddMirroringTypes(this.m_yMirroringCombo);
            this.m_yMirroringCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_yMirroringCombo_ItemSelected);
            base.AddLabel("Z symmetry", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            textColor = null;
            captionOffset = null;
            this.m_zMirroringCombo = base.AddCombo(null, textColor, captionOffset, 10);
            this.AddMirroringTypes(this.m_zMirroringCombo);
            this.m_zMirroringCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_zMirroringCombo_ItemSelected);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Symmetry reset"), new Action<MyGuiControlButton>(this.onClick_SymmetryReset), null, textColor, captionOffset, true, true);
            textColor = null;
            this.m_dummyDrawDistanceSlider = base.AddSlider("Dummies draw distance", MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES_DISTANCE, 0f, 100f, textColor);
            this.m_dummyDrawDistanceSlider.ValueChanged = new Action<MyGuiControlSlider>(this.DummyDrawDistanceSliderChanged);
            base.m_currentPosition += new Vector2(0f, 0.15f);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Debug draw all mount points", MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_ALL, new Action<MyGuiControlCheckbox>(this.onClick_DebugDrawMountPointsAll), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Debug draw mount points", MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS, new Action<MyGuiControlCheckbox>(this.onClick_DebugDrawMountPoints), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Forward", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS0)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Backward", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS1)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Left", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS2)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Right", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS3)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Up", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS4)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Down", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS5)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw autogenerated", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AUTOGENERATE)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("CubeBlock Integrity", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_BLOCK_INTEGRITY)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Resave mountpoints"), new Action<MyGuiControlButton>(this.onClick_Save), null, textColor, captionOffset, true, true).VisualStyle = MyGuiControlButtonStyleEnum.Default;
        }

        public override bool Update(bool hasFocus)
        {
            this.m_currentMirrorAxisLabel.Text = "Mirror axis: " + MyCubeBuilderGizmo.CurrentBlockMirrorAxis.ToString();
            this.m_currentAxisLabel.Text = "Block axis: " + MyCubeBuilderGizmo.CurrentBlockMirrorOption.ToString();
            return base.Update(hasFocus);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugCubeBlocks.<>c <>9 = new MyGuiScreenDebugCubeBlocks.<>c();
        }
    }
}

