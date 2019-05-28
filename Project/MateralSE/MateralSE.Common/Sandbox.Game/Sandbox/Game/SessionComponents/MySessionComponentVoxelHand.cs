namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MySessionComponentVoxelHand : MySessionComponentBase
    {
        private IMyVoxelBrush[] m_brushes;
        public static MySessionComponentVoxelHand Static;
        internal const float VOXEL_SIZE = 1f;
        internal const float VOXEL_HALF = 0.5f;
        internal static float GRID_SIZE = MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large);
        internal static float SCALE_MAX = (MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large) * 10f);
        internal static float MIN_BRUSH_ZOOM = GRID_SIZE;
        internal static float MAX_BRUSH_ZOOM = (GRID_SIZE * 20f);
        private static float DEG_IN_RADIANS = MathHelper.ToRadians((float) 1f);
        private byte m_selectedMaterial;
        private int m_materialCount;
        private float m_position;
        public MatrixD m_rotation;
        private MyVoxelBase m_currentVoxelMap;
        private MyGuiCompositeTexture m_texture;
        public Color ShapeColor;
        private MyHudNotification m_safezoneNotification;
        private bool m_buildMode;
        private bool m_enabled;
        private bool m_editing;
        private MyHudNotification m_voxelMaterialHint;
        private MyHudNotification m_voxelSettingsHint;
        private MyHudNotification m_joystickVoxelMaterialHint;
        private MyHudNotification m_joystickVoxelSettingsHint;
        private MyHudNotification m_buildModeHint;
        private static List<MyEntity> m_foundElements = new List<MyEntity>();

        public MySessionComponentVoxelHand()
        {
            MyGuiSizedTexture texture;
            Static = this;
            this.SnapToVoxel = true;
            this.ShowGizmos = true;
            this.ShapeColor = new VRageMath.Vector4(0.6f, 0.6f, 0.6f, 0.25f);
            this.m_selectedMaterial = 1;
            this.m_materialCount = MyDefinitionManager.Static.VoxelMaterialCount;
            this.m_position = MIN_BRUSH_ZOOM * 2f;
            this.m_rotation = MatrixD.Identity;
            this.m_texture = new MyGuiCompositeTexture(null);
            MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.m_selectedMaterial);
            MyDx11VoxelMaterialDefinition definition2 = voxelMaterialDefinition as MyDx11VoxelMaterialDefinition;
            if (definition2 != null)
            {
                texture = new MyGuiSizedTexture {
                    Texture = definition2.VoxelHandPreview
                };
                this.m_texture.Center = texture;
            }
            else
            {
                texture = new MyGuiSizedTexture {
                    Texture = voxelMaterialDefinition.VoxelHandPreview
                };
                this.m_texture.Center = texture;
            }
        }

        private void Activate()
        {
            this.AlignToGravity();
            this.ActivateHudNotifications();
        }

        private void ActivateHudBuildModeNotifications()
        {
            if ((MySession.Static.CreativeMode && MyInput.Static.IsJoystickConnected()) && MyInput.Static.IsJoystickLastUsed)
            {
                MyHud.Notifications.Add(this.m_joystickVoxelMaterialHint);
                MyHud.Notifications.Add(this.m_joystickVoxelSettingsHint);
                MyHud.Notifications.Remove(this.m_buildModeHint);
            }
        }

        private void ActivateHudNotifications()
        {
            if (MySession.Static.CreativeMode)
            {
                if (!MyInput.Static.IsJoystickConnected())
                {
                    goto TR_0000;
                }
                else if (MyInput.Static.IsJoystickLastUsed)
                {
                    MyHud.Notifications.Add(this.m_buildModeHint);
                }
                else
                {
                    goto TR_0000;
                }
            }
            return;
        TR_0000:
            MyHud.Notifications.Add(this.m_voxelMaterialHint);
            MyHud.Notifications.Add(this.m_voxelSettingsHint);
        }

        private void AlignToGravity()
        {
            if (this.CurrentShape.AutoRotate)
            {
                Vector3D vectord = MyGravityProviderSystem.CalculateNaturalGravityInPoint(MySector.MainCamera.Position);
                if (!vectord.Equals((Vector3D) Vector3.Zero))
                {
                    vectord.Normalize();
                    Vector3D result = vectord;
                    vectord.CalculatePerpendicularVector(out result);
                    MatrixD rotationMat = MatrixD.CreateFromDir(result, -vectord);
                    this.CurrentShape.SetRotation(ref rotationMat);
                    this.m_rotation = rotationMat;
                }
            }
        }

        private void CurrentToolbar_SelectedSlotChanged(MyToolbar toolbar, MyToolbar.SlotArgs args)
        {
            if (!(toolbar.SelectedItem is MyToolbarItemVoxelHand) && this.Enabled)
            {
                this.Enabled = false;
            }
        }

        private void CurrentToolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs args, bool userActivated)
        {
            if (!(toolbar.GetItemAtIndex(toolbar.SlotToIndex(args.SlotNumber.Value)) is MyToolbarItemVoxelHand) && this.Enabled)
            {
                this.Enabled = false;
            }
        }

        private void CurrentToolbar_Unselected(MyToolbar toolbar)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
            }
        }

        private void Deactivate()
        {
            this.DeactivateHudNotifications();
            this.CurrentShape = null;
            this.BuildMode = false;
        }

        private void DeactivateHudBuildModeNotifications()
        {
            if (MySession.Static.CreativeMode)
            {
                MyHud.Notifications.Remove(this.m_joystickVoxelMaterialHint);
                MyHud.Notifications.Remove(this.m_joystickVoxelSettingsHint);
                if ((this.Enabled && MyInput.Static.IsJoystickConnected()) && MyInput.Static.IsJoystickLastUsed)
                {
                    MyHud.Notifications.Add(this.m_buildModeHint);
                }
            }
        }

        private void DeactivateHudNotifications()
        {
            if (MySession.Static.CreativeMode)
            {
                MyHud.Notifications.Remove(this.m_voxelMaterialHint);
                MyHud.Notifications.Remove(this.m_voxelSettingsHint);
                MyHud.Notifications.Remove(this.m_joystickVoxelMaterialHint);
                MyHud.Notifications.Remove(this.m_joystickVoxelSettingsHint);
                MyHud.Notifications.Remove(this.m_buildModeHint);
            }
        }

        public override void Draw()
        {
            if (this.Enabled && (this.m_currentVoxelMap != null))
            {
                base.Draw();
                m_foundElements.Clear();
                BoundingBoxD worldAABB = this.m_currentVoxelMap.PositionComp.WorldAABB;
                Color color = new Color(0.2f, 0f, 0f, 0.2f);
                if (this.ShowGizmos)
                {
                    MyStringId? nullable;
                    if (MyFakes.SHOW_FORBIDDEN_ENITIES_VOXEL_HAND)
                    {
                        MyEntities.GetElementsInBox(ref worldAABB, m_foundElements);
                        foreach (MyEntity entity in m_foundElements)
                        {
                            if (entity is MyCharacter)
                            {
                                continue;
                            }
                            if (MyVoxelBase.IsForbiddenEntity(entity))
                            {
                                MatrixD worldMatrix = entity.PositionComp.WorldMatrix;
                                worldAABB = entity.PositionComp.LocalAABB;
                                nullable = null;
                                nullable = null;
                                MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref worldAABB, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 0, 1f, nullable, nullable, false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                            }
                        }
                    }
                    if (MyFakes.SHOW_CURRENT_VOXEL_MAP_AABB_IN_VOXEL_HAND)
                    {
                        worldAABB = this.m_currentVoxelMap.PositionComp.LocalAABB;
                        color = new VRageMath.Vector4(0f, 0.2f, 0f, 0.1f);
                        nullable = null;
                        nullable = null;
                        MySimpleObjectDraw.DrawTransparentBox(ref this.m_currentVoxelMap.PositionComp.WorldMatrix, ref worldAABB, ref color, MySimpleObjectRasterizer.Solid, 0, 1f, nullable, nullable, false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                    }
                }
                this.CurrentShape.Draw(ref this.ShapeColor);
                ConditionBase visibleCondition = MyHud.HudDefinition.Toolbar.VisibleCondition;
                if (((visibleCondition != null) && (visibleCondition.Eval() && (MyGuiScreenHudSpace.Static != null))) && MyGuiScreenHudSpace.Static.Visible)
                {
                    this.DrawMaterial();
                }
            }
        }

        public void DrawMaterial()
        {
            MyObjectBuilder_ToolbarControlVisualStyle toolbar = MyHud.HudDefinition.Toolbar;
            Vector2 voxelHandPosition = toolbar.ColorPanelStyle.VoxelHandPosition;
            Vector2 size = toolbar.ColorPanelStyle.Size;
            Vector2 vector3 = new Vector2(size.X, size.Y);
            this.m_texture.Draw(voxelHandPosition, vector3, Color.White, 1f);
            Vector2 vector4 = new Vector2(size.X + 0.005f, -0.0018f);
            string str = MyTexts.GetString(MyCommonTexts.VoxelHandSettingScreen_HandMaterial);
            Color? colorMask = null;
            MyGuiManager.DrawString("White", new StringBuilder(string.Format("{1}", str, MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.m_selectedMaterial).Id.SubtypeName)), voxelHandPosition + vector4, 0.5f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
        }

        public float GetBrushZoom() => 
            this.m_position;

        public override void HandleInput()
        {
            if (this.Enabled && (MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay))
            {
                if (!MySession.Static.CreativeMode && !MySession.Static.IsUserAdmin(Sync.MyId))
                {
                    this.Enabled = false;
                }
                else
                {
                    if (MyControllerHelper.IsControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.BUILD_MODE, MyControlStateType.NEW_PRESSED, false))
                    {
                        this.BuildMode = !this.BuildMode;
                    }
                    base.HandleInput();
                    MyStringId context = this.BuildMode ? MySpaceBindingCreator.CX_VOXEL : MySpaceBindingCreator.CX_CHARACTER;
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.VOXEL_HAND_SETTINGS))
                    {
                        MyScreenManager.AddScreen(new MyGuiScreenVoxelHandSetting());
                    }
                    if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE, MyControlStateType.PRESSED, false))
                    {
                        this.m_rotation *= MatrixD.CreateFromAxisAngle(this.m_rotation.Forward, (double) DEG_IN_RADIANS);
                    }
                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE, MyControlStateType.PRESSED, false))
                    {
                        this.m_rotation *= MatrixD.CreateFromAxisAngle(this.m_rotation.Forward, (double) -DEG_IN_RADIANS);
                    }
                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE, MyControlStateType.PRESSED, false))
                    {
                        this.m_rotation *= MatrixD.CreateFromAxisAngle(this.m_rotation.Up, (double) -DEG_IN_RADIANS);
                    }
                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE, MyControlStateType.PRESSED, false))
                    {
                        this.m_rotation *= MatrixD.CreateFromAxisAngle(this.m_rotation.Up, (double) DEG_IN_RADIANS);
                    }
                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE, MyControlStateType.PRESSED, false))
                    {
                        this.m_rotation *= MatrixD.CreateFromAxisAngle(this.m_rotation.Right, (double) -DEG_IN_RADIANS);
                    }
                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE, MyControlStateType.PRESSED, false))
                    {
                        this.m_rotation *= MatrixD.CreateFromAxisAngle(this.m_rotation.Right, (double) DEG_IN_RADIANS);
                    }
                    this.CurrentShape.SetRotation(ref this.m_rotation);
                    if (MyControllerHelper.IsControl(context, MyControlsSpace.SWITCH_LEFT, MyControlStateType.NEW_PRESSED, false))
                    {
                        this.SetMaterial(this.m_selectedMaterial, false);
                    }
                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.SWITCH_RIGHT, MyControlStateType.NEW_PRESSED, false))
                    {
                        this.SetMaterial(this.m_selectedMaterial, true);
                    }
                    if (this.m_currentVoxelMap != null)
                    {
                        MyBrushAutoLevel currentShape = this.CurrentShape as MyBrushAutoLevel;
                        if (currentShape != null)
                        {
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false) || MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                            {
                                currentShape.FixAxis();
                            }
                            else if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_RELEASED, false) || MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_TOOL_ACTION, MyControlStateType.NEW_RELEASED, false))
                            {
                                currentShape.UnFix();
                            }
                        }
                        bool flag = false;
                        MyVoxelPhysicsBody physics = (MyVoxelPhysicsBody) this.m_currentVoxelMap.Physics;
                        if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.PRESSED, false))
                        {
                            if (physics != null)
                            {
                                physics.QueueInvalidate = flag = this.FreezePhysics;
                            }
                            if (MySessionComponentSafeZones.IsActionAllowed(this.CurrentShape.GetWorldBoundaries(), MySafeZoneAction.VoxelHand, 0L))
                            {
                                this.CurrentShape.Fill(this.m_currentVoxelMap, this.m_selectedMaterial);
                            }
                            else
                            {
                                this.ShowSafeZoneNotification();
                            }
                        }
                        else if (MyInput.Static.IsMiddleMousePressed() || MyControllerHelper.IsControl(context, MyControlsSpace.VOXEL_PAINT, MyControlStateType.PRESSED, false))
                        {
                            if (MySessionComponentSafeZones.IsActionAllowed(this.CurrentShape.GetWorldBoundaries(), MySafeZoneAction.VoxelHand, 0L))
                            {
                                this.CurrentShape.Paint(this.m_currentVoxelMap, this.m_selectedMaterial);
                            }
                            else
                            {
                                this.ShowSafeZoneNotification();
                            }
                        }
                        else if (MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_TOOL_ACTION, MyControlStateType.PRESSED, false))
                        {
                            if (physics != null)
                            {
                                physics.QueueInvalidate = flag = this.FreezePhysics;
                            }
                            if (!MySessionComponentSafeZones.IsActionAllowed(this.CurrentShape.GetWorldBoundaries(), MySafeZoneAction.VoxelHand, 0L))
                            {
                                this.ShowSafeZoneNotification();
                            }
                            else if (!MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.CurrentShape.CutOut(this.m_currentVoxelMap);
                            }
                            else if (this.m_currentVoxelMap.Storage.DeleteSupported)
                            {
                                this.CurrentShape.Revert(this.m_currentVoxelMap);
                            }
                        }
                        int num = Math.Sign(MyInput.Static.DeltaMouseScrollWheelValue());
                        if ((num != 0) && MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            float num2 = ((float) this.CurrentShape.GetBoundaries().HalfExtents.Length()) * 0.5f;
                            this.SetBrushZoom(this.m_position + (num * num2));
                        }
                        if ((physics != null) && (this.m_editing != flag))
                        {
                            physics.QueueInvalidate = flag;
                            this.m_editing = flag;
                        }
                    }
                }
            }
        }

        private void InitializeHints()
        {
            string str = "[" + MyInput.Static.GetGameControl(MyControlsSpace.SWITCH_LEFT) + "]";
            string str2 = "[" + MyInput.Static.GetGameControl(MyControlsSpace.SWITCH_RIGHT) + "]";
            object[] args = new object[] { str, str2 };
            this.m_voxelMaterialHint = MyHudNotifications.CreateControlNotification(MyCommonTexts.NotificationVoxelMaterialFormat, args);
            object[] objArray2 = new object[] { "[Ctrl + H]" };
            this.m_voxelSettingsHint = MyHudNotifications.CreateControlNotification(MyCommonTexts.NotificationVoxelHandHintFormat, objArray2);
            char codeForControl = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_VOXEL, MyControlsSpace.SWITCH_LEFT);
            char ch2 = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.BUILD_MODE);
            object[] objArray3 = new object[] { codeForControl };
            this.m_joystickVoxelMaterialHint = MyHudNotifications.CreateControlNotification(MyCommonTexts.NotificationJoystickVoxelMaterialFormat, objArray3);
            object[] objArray4 = new object[] { "[Ctrl + H]" };
            this.m_joystickVoxelSettingsHint = MyHudNotifications.CreateControlNotification(MyCommonTexts.NotificationVoxelHandHintFormat, objArray4);
            object[] objArray5 = new object[] { ch2 };
            this.m_buildModeHint = MyHudNotifications.CreateControlNotification(MyCommonTexts.NotificationHintPressToOpenBuildMode, objArray5);
        }

        public override void LoadData()
        {
            base.LoadData();
            MyToolbarComponent.CurrentToolbar.SelectedSlotChanged += new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
            MyToolbarComponent.CurrentToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
            MyToolbarComponent.CurrentToolbar.Unselected += new Action<MyToolbar>(this.CurrentToolbar_Unselected);
            this.InitializeHints();
        }

        public void SetBrushZoom(float value)
        {
            this.m_position = MathHelper.Clamp(value, MIN_BRUSH_ZOOM, MAX_BRUSH_ZOOM);
        }

        private void SetMaterial(byte idx, bool next = true)
        {
            idx = next ? (idx = (byte) (idx + 1)) : (idx = (byte) (idx - 1));
            if (idx == 0xff)
            {
                idx = (byte) (this.m_materialCount - 1);
            }
            this.m_selectedMaterial = (byte) (idx % this.m_materialCount);
            MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(this.m_selectedMaterial);
            if ((voxelMaterialDefinition.Id.SubtypeName == "BrownMaterial") || (voxelMaterialDefinition.Id.SubtypeName == "DebugMaterial"))
            {
                this.SetMaterial(idx, next);
            }
            else
            {
                MyGuiSizedTexture texture;
                MyDx11VoxelMaterialDefinition definition2 = voxelMaterialDefinition as MyDx11VoxelMaterialDefinition;
                if (definition2 != null)
                {
                    texture = new MyGuiSizedTexture {
                        Texture = definition2.VoxelHandPreview
                    };
                    this.m_texture.Center = texture;
                }
                else
                {
                    texture = new MyGuiSizedTexture {
                        Texture = voxelMaterialDefinition.VoxelHandPreview
                    };
                    this.m_texture.Center = texture;
                }
            }
        }

        private void ShowSafeZoneNotification()
        {
            MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            if (this.m_safezoneNotification == null)
            {
                this.m_safezoneNotification = new MyHudNotification(MyCommonTexts.SafeZone_VoxelhandDisabled, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            }
            MyHud.Notifications.Add(this.m_safezoneNotification);
        }

        public bool TrySetBrush(string brushSubtypeName)
        {
            if (this.m_brushes == null)
            {
                this.m_brushes = new IMyVoxelBrush[] { MyBrushBox.Static, MyBrushCapsule.Static, MyBrushRamp.Static, MyBrushSphere.Static, MyBrushAutoLevel.Static, MyBrushEllipsoid.Static };
            }
            foreach (IMyVoxelBrush brush in this.m_brushes)
            {
                if (brushSubtypeName == brush.SubtypeName)
                {
                    this.CurrentShape = brush;
                    return true;
                }
            }
            return false;
        }

        protected override void UnloadData()
        {
            MyToolbarComponent.CurrentToolbar.Unselected -= new Action<MyToolbar>(this.CurrentToolbar_Unselected);
            MyToolbarComponent.CurrentToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
            MyToolbarComponent.CurrentToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {
            if (this.Enabled)
            {
                base.UpdateBeforeSimulation();
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if (localCharacter != null)
                {
                    if (localCharacter.ControllerInfo.Controller == null)
                    {
                        this.Enabled = false;
                    }
                    else
                    {
                        MyCamera mainCamera = MySector.MainCamera;
                        if (mainCamera != null)
                        {
                            Vector3D translation;
                            if (!MySession.Static.IsCameraUserControlledSpectator())
                            {
                                translation = localCharacter.GetHeadMatrix(true, true, false, false, false).Translation;
                            }
                            else
                            {
                                translation = mainCamera.Position;
                            }
                            Vector3D from = translation;
                            Vector3D targetPosition = from + (mainCamera.ForwardVector * Math.Max(2.0 * this.CurrentShape.GetBoundaries().TransformFast(mainCamera.ViewMatrix).HalfExtents.Z, (double) this.m_position));
                            MyVoxelBase currentVoxelMap = this.m_currentVoxelMap;
                            BoundingBoxD boundingBox = this.CurrentShape.PeekWorldBoundingBox(ref targetPosition);
                            this.m_currentVoxelMap = MySession.Static.VoxelMaps.GetVoxelMapWhoseBoundingBoxIntersectsBox(ref boundingBox, null);
                            if (this.ProjectToVoxel && (this.m_currentVoxelMap != null))
                            {
                                List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
                                MyPhysics.CastRay(from, from + (mainCamera.ForwardVector * this.m_currentVoxelMap.SizeInMetres), toList, 11);
                                bool flag = false;
                                foreach (MyPhysics.HitInfo info in toList)
                                {
                                    IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                                    if ((hitEntity is MyVoxelBase) && ReferenceEquals(((MyVoxelBase) hitEntity).RootVoxel, this.m_currentVoxelMap.RootVoxel))
                                    {
                                        targetPosition = info.Position;
                                        this.m_currentVoxelMap = (MyVoxelBase) hitEntity;
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    this.m_currentVoxelMap = null;
                                }
                            }
                            if ((!ReferenceEquals(currentVoxelMap, this.m_currentVoxelMap) && (currentVoxelMap != null)) && (currentVoxelMap.Physics != null))
                            {
                                ((MyVoxelPhysicsBody) currentVoxelMap.Physics).QueueInvalidate = false;
                            }
                            if (this.m_currentVoxelMap != null)
                            {
                                this.m_currentVoxelMap = this.m_currentVoxelMap.RootVoxel;
                                if (!this.SnapToVoxel)
                                {
                                    this.CurrentShape.SetPosition(ref targetPosition);
                                }
                                else
                                {
                                    Vector3I vectori;
                                    targetPosition += 0.5f;
                                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.m_currentVoxelMap.PositionLeftBottomCorner, ref targetPosition, out vectori);
                                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(this.m_currentVoxelMap.PositionLeftBottomCorner, ref vectori, out targetPosition);
                                    this.CurrentShape.SetPosition(ref targetPosition);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override System.Type[] Dependencies =>
            new System.Type[] { typeof(MyToolbarComponent) };

        public bool BuildMode
        {
            get => 
                this.m_buildMode;
            private set
            {
                this.m_buildMode = value;
                MyHud.IsBuildMode = value;
                if (value)
                {
                    this.ActivateHudBuildModeNotifications();
                }
                else
                {
                    this.DeactivateHudBuildModeNotifications();
                }
            }
        }

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set
            {
                if (this.m_enabled != value)
                {
                    if (value)
                    {
                        this.Activate();
                    }
                    else
                    {
                        this.Deactivate();
                    }
                    this.m_enabled = value;
                }
            }
        }

        public bool SnapToVoxel { get; set; }

        public bool ProjectToVoxel { get; set; }

        public bool ShowGizmos { get; set; }

        public bool FreezePhysics { get; set; }

        public IMyVoxelBrush CurrentShape { get; set; }

        public MyVoxelHandDefinition CurrentDefinition { get; set; }
    }
}

