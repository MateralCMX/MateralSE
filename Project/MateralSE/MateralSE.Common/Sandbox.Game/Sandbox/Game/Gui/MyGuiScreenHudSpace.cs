namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Definitions.GUI;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GUI;
    using Sandbox.Game.GUI.HudViewers;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenHudSpace : MyGuiScreenHudBase
    {
        public static MyGuiScreenHudSpace Static;
        private const float ALTITUDE_CHANGE_THRESHOLD = 500f;
        public const int PING_THRESHOLD_MILLISECONDS = 250;
        public const float SERVER_SIMSPEED_THRESHOLD = 0.8f;
        private MyGuiControlToolbar m_toolbarControl;
        private MyGuiControlContextHelp m_contextHelp;
        private MyGuiControlBlockInfo m_blockInfo;
        private MyGuiControlLabel m_rotatingWheelLabel;
        private MyGuiControlRotatingWheel m_rotatingWheelControl;
        private MyGuiControlMultilineText m_cameraInfoMultilineControl;
        private MyGuiControlQuestlog m_questlogControl;
        private MyGuiControlLabel m_buildModeLabel;
        private MyGuiControlLabel m_blocksLeft;
        private MyHudControlChat m_chatControl;
        private MyHudMarkerRender m_markerRender;
        private int m_oreHudMarkerStyle;
        private int m_gpsHudMarkerStyle;
        private int m_buttonPanelHudMarkerStyle;
        private MyHudEntityParams m_tmpHudEntityParams;
        private MyTuple<Vector3D, MyEntityOreDeposit>[] m_nearestOreDeposits;
        private float[] m_nearestDistanceSquared;
        private MyHudControlGravityIndicator m_gravityIndicator;
        private MyObjectBuilder_GuiTexture m_visorOverlayTexture;
        private readonly List<MyStatControls> m_statControls = new List<MyStatControls>();
        private bool m_hiddenToolbar;
        public float m_gravityHudWidth;
        private float m_altitude;
        private List<MyStringId> m_warningNotifications = new List<MyStringId>();
        private readonly byte m_warningFrameCount = 200;
        private byte m_currentFrameCount;

        public MyGuiScreenHudSpace()
        {
            Static = this;
            this.RecreateControls(true);
            this.m_markerRender = new MyHudMarkerRender(this);
            this.m_oreHudMarkerStyle = this.m_markerRender.AllocateMarkerStyle("White", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_neutral, Color.White);
            this.m_gpsHudMarkerStyle = this.m_markerRender.AllocateMarkerStyle("DarkBlue", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_me, MyHudConstants.GPS_COLOR);
            this.m_buttonPanelHudMarkerStyle = this.m_markerRender.AllocateMarkerStyle("DarkBlue", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_me, MyHudConstants.GPS_COLOR);
            MyHudEntityParams @params = new MyHudEntityParams {
                Text = new StringBuilder(),
                FlagsEnum = ~MyHudIndicatorFlagsEnum.NONE
            };
            this.m_tmpHudEntityParams = @params;
        }

        public override unsafe bool Draw()
        {
            int num1;
            int num2;
            int num3;
            int num4;
            int num5;
            int cutsceneHud;
            int num7;
            if ((base.m_transitionAlpha < 1f) || !MyHud.IsVisible)
            {
                return false;
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.J) && MyFakes.ENABLE_OBJECTIVE_LINE)
            {
                MyHud.ObjectiveLine.AdvanceObjective();
            }
            if (!MyHud.MinimalHud && !MyHud.CutsceneHud)
            {
                using (List<MyStatControls>.Enumerator enumerator = this.m_statControls.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Draw(base.m_transitionAlpha, base.m_backgroundTransition);
                    }
                }
                this.m_gravityIndicator.Draw(base.m_transitionAlpha);
                base.DrawTexts();
            }
            if (this.m_hiddenToolbar || MyHud.MinimalHud)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) !MyHud.CutsceneHud;
            }
            this.m_toolbarControl.Visible = (bool) num1;
            Vector2 hudPos = new Vector2(0.99f, 0.985f);
            if (MySession.Static.ControlledEntity is MyShipController)
            {
                hudPos.Y = 0.65f;
            }
            hudPos = ConvertHudToNormalizedGuiPosition(ref hudPos);
            if (MyVideoSettingsManager.IsTripleHead())
            {
                float* singlePtr1 = (float*) ref hudPos.X;
                singlePtr1[0]++;
            }
            bool flag = MyHud.BlockInfo.Components.Count > 0;
            if (!MyHud.BlockInfo.Visible || MyHud.MinimalHud)
            {
                num2 = 0;
            }
            else
            {
                num2 = (int) !MyHud.CutsceneHud;
            }
            this.m_contextHelp.Visible = (bool) num2;
            IMyHudStat stat = MyHud.Stats.GetStat(MyStringHash.GetOrCompute("hud_mode"));
            if (stat.CurrentValue == 1f)
            {
                this.m_contextHelp.Visible &= !string.IsNullOrEmpty(MyHud.BlockInfo.ContextHelp);
            }
            IMyHudStat local2 = stat;
            if (local2.CurrentValue == 2f)
            {
                this.m_contextHelp.Visible &= flag;
            }
            this.m_contextHelp.BlockInfo = MyHud.BlockInfo.Visible ? MyHud.BlockInfo : null;
            this.m_contextHelp.Position = MyHud.ShipInfo.Visible ? new Vector2(hudPos.X, 0.1f) : new Vector2(hudPos.X, 0.28f);
            if (local2.CurrentValue != 2f)
            {
                this.m_contextHelp.ShowJustTitle = false;
            }
            else
            {
                this.m_contextHelp.Position = new Vector2(hudPos.X, 0.38f);
                this.m_contextHelp.ShowJustTitle = true;
            }
            this.m_contextHelp.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_contextHelp.ShowBuildInfo = flag;
            if (!MyHud.BlockInfo.Visible || MyHud.MinimalHud)
            {
                num3 = 0;
            }
            else
            {
                num3 = (int) !MyHud.CutsceneHud;
            }
            this.m_blockInfo.Visible = ((bool) num3) & flag;
            this.m_blockInfo.BlockInfo = this.m_blockInfo.Visible ? MyHud.BlockInfo : null;
            this.m_blockInfo.Position = this.m_contextHelp.Position + new Vector2(0f, this.m_contextHelp.Size.Y + 0.006f);
            this.m_blockInfo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            if ((!MyHud.Questlog.Visible || MyHud.IsHudMinimal) || MyHud.MinimalHud)
            {
                num4 = 0;
            }
            else
            {
                num4 = (int) !MyHud.CutsceneHud;
            }
            this.m_questlogControl.Visible = (bool) num4;
            if (!MyHud.RotatingWheelVisible || MyHud.MinimalHud)
            {
                num5 = 0;
            }
            else
            {
                num5 = (int) !MyHud.CutsceneHud;
            }
            this.m_rotatingWheelControl.Visible = (bool) num5;
            this.m_rotatingWheelLabel.Visible = this.m_rotatingWheelControl.Visible;
            if (MyScreenManager.GetScreenWithFocus() is MyGuiScreenScenarioMpBase)
            {
                cutsceneHud = 0;
            }
            else if (!MyHud.MinimalHud || this.m_chatControl.HasFocus)
            {
                cutsceneHud = 1;
            }
            else
            {
                cutsceneHud = (int) MyHud.CutsceneHud;
            }
            this.m_chatControl.Visible = (bool) num7;
            if (!base.Draw())
            {
                return false;
            }
            Vector2 vector2 = new Vector2(0.014f, 0.81f);
            vector2 = ConvertHudToNormalizedGuiPosition(ref vector2);
            this.m_chatControl.Position = vector2 + new Vector2(0.002f, -0.07f);
            this.m_chatControl.TextScale = 0.7f;
            vector2 = new Vector2(0.03f, 0.1f);
            vector2 = ConvertHudToNormalizedGuiPosition(ref vector2);
            this.m_cameraInfoMultilineControl.Position = vector2;
            this.m_cameraInfoMultilineControl.TextScale = 0.9f;
            if (!MyHud.MinimalHud && !MyHud.CutsceneHud)
            {
                bool flag2 = false;
                MyShipController controlledEntity = MySession.Static.ControlledEntity as MyShipController;
                if (controlledEntity != null)
                {
                    flag2 = !(MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(controlledEntity.PositionComp.GetPosition()) == 0f);
                }
                if (flag2)
                {
                    this.DrawArtificialHorizonAndAltitude();
                }
            }
            if (!MyHud.IsHudMinimal)
            {
                MyHud.Notifications.Draw();
            }
            if (MyHud.MinimalHud || MyHud.CutsceneHud)
            {
                this.m_buildModeLabel.Visible = false;
                this.m_blocksLeft.Visible = false;
            }
            else
            {
                this.m_buildModeLabel.Visible = MyHud.IsBuildMode;
                if (!MyHud.BlocksLeft.Visible)
                {
                    this.m_blocksLeft.Visible = false;
                }
                else
                {
                    StringBuilder stringBuilder = MyHud.BlocksLeft.GetStringBuilder();
                    if (!this.m_blocksLeft.Text.EqualsStrFast(stringBuilder))
                    {
                        this.m_blocksLeft.Text = stringBuilder.ToString();
                    }
                    this.m_blocksLeft.Visible = true;
                }
                if (MyHud.ObjectiveLine.Visible && MyFakes.ENABLE_OBJECTIVE_LINE)
                {
                    this.DrawObjectiveLine(MyHud.ObjectiveLine);
                }
                if (MySandboxGame.Config.EnablePerformanceWarnings)
                {
                    if (MySession.Static.IsSettingsExperimental() && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_ExperimentalMode))
                    {
                        this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_ExperimentalMode);
                    }
                    if ((MyUnsafeGridsSessionComponent.UnsafeGrids.Count > 0) && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_UnsafeGrids))
                    {
                        this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_UnsafeGrids);
                    }
                    foreach (KeyValuePair<MySimpleProfiler.MySimpleProfilingBlock, MySimpleProfiler.PerformanceWarning> pair in MySimpleProfiler.CurrentWarnings)
                    {
                        if (!this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading))
                        {
                            if (pair.Value.Time >= 120)
                            {
                                continue;
                            }
                            this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading);
                        }
                        break;
                    }
                    if (((MyGeneralStats.Static.LowNetworkQuality || (!MySession.Static.MultiplayerDirect || (!MySession.Static.MultiplayerAlive && !MySession.Static.ServerSaving))) || (!Sync.IsServer && (MySession.Static.MultiplayerPing.Milliseconds > 250.0))) && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_Connection))
                    {
                        this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_Connection);
                    }
                    if (!MySession.Static.HighSimulationQualityNotification && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_LowSimulationQuality))
                    {
                        this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_LowSimulationQuality);
                    }
                    if ((!Sync.IsServer && (Sync.ServerSimulationRatio < 0.8f)) && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_SimSpeed))
                    {
                        this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_SimSpeed);
                    }
                    if (MySession.Static.ServerSaving && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_Saving))
                    {
                        this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_Saving);
                    }
                }
            }
            if (MyFakes.PUBLIC_BETA_MP_TEST && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_ExperimentalBetaBuild))
            {
                this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_ExperimentalBetaBuild);
            }
            if ((MySandboxGame.Config.EnablePerformanceWarnings && (MySession.Static.MultiplayerLastMsg > 1.0)) && !this.m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_Connection))
            {
                this.m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_Connection);
            }
            if (MyPetaInputComponent.DRAW_WARNINGS && (this.m_warningNotifications.Count != 0))
            {
                this.DrawPerformanceWarning();
            }
            MyHud.BlockInfo.Visible = false;
            this.m_blockInfo.BlockInfo = null;
            MyHudObjectHighlightStyleData data = new MyHudObjectHighlightStyleData {
                AtlasTexture = base.m_atlas,
                TextureCoord = base.GetTextureCoord(MyHudTexturesEnum.corner)
            };
            HandleSelectedObjectHighlight(MyHud.SelectedObjectHighlight, new MyHudObjectHighlightStyleData?(data));
            if (((!MyHud.IsHudMinimal && !MyHud.MinimalHud) && !MyHud.CutsceneHud) || MyPetaInputComponent.SHOW_HUD_ALWAYS)
            {
                if (MyHud.SinkGroupInfo.Visible && MyFakes.LEGACY_HUD)
                {
                    this.DrawPowerGroupInfo(MyHud.SinkGroupInfo);
                }
                if (MyHud.LocationMarkers.Visible)
                {
                    this.m_markerRender.DrawLocationMarkers(MyHud.LocationMarkers);
                }
                if (MyHud.GpsMarkers.Visible && MyFakes.ENABLE_GPS)
                {
                    this.DrawGpsMarkers(MyHud.GpsMarkers);
                }
                if (MyHud.ButtonPanelMarkers.Visible)
                {
                    this.DrawButtonPanelMarkers(MyHud.ButtonPanelMarkers);
                }
                if (MyHud.OreMarkers.Visible)
                {
                    this.DrawOreMarkers(MyHud.OreMarkers);
                }
                if (MyHud.LargeTurretTargets.Visible)
                {
                    this.DrawLargeTurretTargets(MyHud.LargeTurretTargets);
                }
                this.DrawWorldBorderIndicator(MyHud.WorldBorderChecker);
                if (MyHud.HackingMarkers.Visible)
                {
                    this.DrawHackingMarkers(MyHud.HackingMarkers);
                }
                this.m_markerRender.Draw();
            }
            this.DrawCameraInfo(MyHud.CameraInfo);
            if (MyHud.VoiceChat.Visible)
            {
                this.DrawVoiceChat(MyHud.VoiceChat);
            }
            return true;
        }

        private unsafe void DrawArtificialHorizonAndAltitude()
        {
            MyCubeBlock controlledEntity = MySession.Static.ControlledEntity as MyCubeBlock;
            if ((controlledEntity != null) && (controlledEntity.CubeGrid.Physics != null))
            {
                Vector3D centerOfMassWorld = controlledEntity.CubeGrid.Physics.CenterOfMassWorld;
                Vector3D worldPoint = controlledEntity.GetTopMostParent(null).Physics.CenterOfMassWorld;
                MyShipController controller = controlledEntity as MyShipController;
                if ((controller == null) || controller.HorizonIndicatorEnabled)
                {
                    MyPlanet planet;
                    this.FindDistanceToNearestPlanetSeaLevel(controlledEntity.PositionComp.WorldAABB, out planet);
                    if (planet != null)
                    {
                        Vector3D closestSurfacePointGlobal = planet.GetClosestSurfacePointGlobal(ref centerOfMassWorld);
                        string font = "Blue";
                        MyGuiDrawAlignEnum drawAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                        float number = (float) Vector3D.Distance(closestSurfacePointGlobal, centerOfMassWorld);
                        if ((Math.Abs((float) (number - this.m_altitude)) > 500f) && (controlledEntity.CubeGrid.GridSystems.GasSystem != null))
                        {
                            controlledEntity.CubeGrid.GridSystems.GasSystem.OnAltitudeChanged();
                            this.m_altitude = number;
                        }
                        StringBuilder text = new StringBuilder().AppendDecimal(number, 0).Append(" m");
                        float num2 = 0.03f;
                        int num3 = MyGuiManager.GetFullscreenRectangle().Width / MyGuiManager.GetSafeFullscreenRectangle().Width;
                        int num4 = MyGuiManager.GetFullscreenRectangle().Height / MyGuiManager.GetSafeFullscreenRectangle().Height;
                        Vector2 normalizedCoord = new Vector2((MyHud.Crosshair.Position.X * num3) / MyGuiManager.GetHudSize().X, ((MyHud.Crosshair.Position.Y * num4) / MyGuiManager.GetHudSize().Y) + num2);
                        if (MyVideoSettingsManager.IsTripleHead())
                        {
                            float* singlePtr1 = (float*) ref normalizedCoord.X;
                            singlePtr1[0]--;
                        }
                        Color? colorMask = null;
                        MyGuiManager.DrawString(font, text, normalizedCoord, base.m_textScale, colorMask, drawAlign, true, float.PositiveInfinity);
                        Vector3 v = -planet.Components.Get<MyGravityProviderComponent>().GetWorldGravity(worldPoint);
                        v.Normalize();
                        float num5 = 0.4f;
                        Vector2 vector3 = (MyHud.Crosshair.Position / MyGuiManager.GetHudSize()) * new Vector2((float) MyGuiManager.GetSafeFullscreenRectangle().Width, (float) MyGuiManager.GetSafeFullscreenRectangle().Height);
                        MyGuiPaddedTexture texture = MyGuiConstants.TEXTURE_HUD_GRAVITY_HORIZON;
                        float num6 = (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator) ? (0.35f * MySector.MainCamera.Viewport.Height) : (0.45f * MySector.MainCamera.Viewport.Height);
                        double num7 = v.Dot((Vector3) controlledEntity.WorldMatrix.Forward) * num6;
                        Vector2D vectord3 = new Vector2D(controlledEntity.WorldMatrix.Right.Dot(v), controlledEntity.WorldMatrix.Up.Dot(v));
                        double a = (vectord3.LengthSquared() > 9.9999997473787516E-06) ? Math.Atan2(vectord3.Y, vectord3.X) : 0.0;
                        Vector2 size = texture.SizePx * num5;
                        RectangleF destination = new RectangleF((vector3 - (size * 0.5f)) + new Vector2(0f, (float) num7), size);
                        Rectangle? sourceRectangle = null;
                        Vector2 rightVector = new Vector2((float) Math.Sin(a), (float) Math.Cos(a));
                        Vector2 origin = vector3;
                        MyRenderProxy.DrawSprite(texture.Texture, ref destination, false, ref sourceRectangle, Color.White, 0f, rightVector, ref origin, SpriteEffects.None, 0f, true, null);
                    }
                }
            }
        }

        private void DrawButtonPanelMarkers(MyHudGpsMarkers buttonPanelMarkers)
        {
            foreach (MyGps gps in buttonPanelMarkers.MarkerEntities)
            {
                this.m_markerRender.AddButtonMarker(gps.Coords, gps.Name);
            }
        }

        private void DrawCameraInfo(MyHudCameraInfo cameraInfo)
        {
            cameraInfo.Draw(this.m_cameraInfoMultilineControl);
        }

        private void DrawGpsMarkers(MyHudGpsMarkers gpsMarkers)
        {
            this.m_tmpHudEntityParams.FlagsEnum = ~MyHudIndicatorFlagsEnum.NONE;
            MySession.Static.Gpss.updateForHud();
            foreach (MyGps gps in gpsMarkers.MarkerEntities)
            {
                this.m_markerRender.AddGPS(gps);
            }
        }

        private void DrawHackingMarkers(MyHudHackingMarkers hackingMarkers)
        {
            try
            {
                hackingMarkers.UpdateMarkers();
                if ((MySandboxGame.TotalTimeInMilliseconds % 200) <= 100)
                {
                    foreach (KeyValuePair<long, MyHudEntityParams> pair in hackingMarkers.MarkerEntities)
                    {
                        MyHudEntityParams @params = pair.Value;
                        if ((@params.ShouldDraw == null) || @params.ShouldDraw())
                        {
                            this.m_markerRender.AddHacking(pair.Value.Position, @params.Text);
                        }
                    }
                }
            }
            finally
            {
            }
        }

        private void DrawLargeTurretTargets(MyHudLargeTurretTargets largeTurretTargets)
        {
            foreach (KeyValuePair<VRage.Game.Entity.MyEntity, MyHudEntityParams> pair in largeTurretTargets.Targets)
            {
                MyHudEntityParams @params = pair.Value;
                if ((@params.ShouldDraw == null) || @params.ShouldDraw())
                {
                    this.m_markerRender.AddTarget(pair.Key.PositionComp.WorldAABB.Center);
                }
            }
        }

        private void DrawObjectiveLine(MyHudObjectiveLine objective)
        {
            MyGuiDrawAlignEnum drawAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            Vector2 hudPos = new Vector2(0.45f, 0.01f);
            Vector2 vector2 = new Vector2(0f, 0.02f);
            Vector2 normalizedCoord = ConvertHudToNormalizedGuiPosition(ref hudPos);
            MyGuiDrawAlignEnum enum3 = drawAlign;
            MyGuiManager.DrawString("Debug", new StringBuilder(objective.Title), normalizedCoord, 1f, new Color?(Color.AliceBlue), enum3, false, float.PositiveInfinity);
            normalizedCoord = ConvertHudToNormalizedGuiPosition(ref hudPos + vector2);
            Color? colorMask = null;
            MyGuiManager.DrawString("Debug", new StringBuilder("- " + objective.CurrentObjective), normalizedCoord, 1f, colorMask, drawAlign, false, float.PositiveInfinity);
        }

        private void DrawOreMarkers(MyHudOreMarkers oreMarkers)
        {
            if ((this.m_nearestOreDeposits == null) || (this.m_nearestOreDeposits.Length < MyDefinitionManager.Static.VoxelMaterialCount))
            {
                this.m_nearestOreDeposits = new MyTuple<Vector3D, MyEntityOreDeposit>[MyDefinitionManager.Static.VoxelMaterialCount];
                this.m_nearestDistanceSquared = new float[this.m_nearestOreDeposits.Length];
            }
            for (int i = 0; i < this.m_nearestOreDeposits.Length; i++)
            {
                this.m_nearestOreDeposits[i] = new MyTuple<Vector3D, MyEntityOreDeposit>();
                this.m_nearestDistanceSquared[i] = float.MaxValue;
            }
            Vector3D zero = Vector3D.Zero;
            if ((MySession.Static != null) && (MySession.Static.ControlledEntity != null))
            {
                zero = (MySession.Static.ControlledEntity as VRage.Game.Entity.MyEntity).WorldMatrix.Translation;
            }
            foreach (MyEntityOreDeposit deposit in oreMarkers)
            {
                for (int k = 0; k < deposit.Materials.Count; k++)
                {
                    Vector3D vectord2;
                    MyEntityOreDeposit.Data data = deposit.Materials[k];
                    MyVoxelMaterialDefinition material = data.Material;
                    data.ComputeWorldPosition(deposit.VoxelMap, out vectord2);
                    Vector3D vectord3 = zero - vectord2;
                    float num3 = (float) vectord3.LengthSquared();
                    float num4 = this.m_nearestDistanceSquared[material.Index];
                    if (num3 < num4)
                    {
                        this.m_nearestOreDeposits[material.Index] = MyTuple.Create<Vector3D, MyEntityOreDeposit>(vectord2, deposit);
                        this.m_nearestDistanceSquared[material.Index] = num3;
                    }
                }
            }
            for (int j = 0; j < this.m_nearestOreDeposits.Length; j++)
            {
                MyTuple<Vector3D, MyEntityOreDeposit> tuple = this.m_nearestOreDeposits[j];
                if (((tuple.Item2 != null) && (tuple.Item2.VoxelMap != null)) && !tuple.Item2.VoxelMap.Closed)
                {
                    string minedOre = MyDefinitionManager.Static.GetVoxelMaterialDefinition((byte) j).MinedOre;
                    this.m_markerRender.AddOre(tuple.Item1, MyTexts.GetString(MyStringId.GetOrCompute(minedOre)));
                }
            }
        }

        private void DrawPerformanceWarning()
        {
            Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 4, 0x2a) - new Vector2(MyGuiConstants.TEXTURE_HUD_BG_PERFORMANCE.SizeGui.X / 1.5f, 0f);
            MyGuiPaddedTexture texture = MyGuiConstants.TEXTURE_HUD_BG_PERFORMANCE;
            MyGuiManager.DrawSpriteBatch(texture.Texture, normalizedCoord, texture.SizeGui / 1.5f, Color.White, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, true);
            Color? colorMask = null;
            MyGuiManager.DrawString("White", new StringBuilder(MyTexts.GetString(this.m_warningNotifications[0])), normalizedCoord + new Vector2(0.09f, -0.011f), 0.7f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
            StringBuilder stringBuilder = new StringBuilder();
            colorMask = null;
            MyGuiManager.DrawString("White", stringBuilder.AppendFormat(MyCommonTexts.PerformanceWarningCombination, MyGuiSandbox.GetKeyName(MyControlsSpace.HELP_SCREEN)), normalizedCoord + new Vector2(0.09f, 0.011f), 0.7f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
            stringBuilder.Clear();
            colorMask = null;
            MyGuiManager.DrawString("White", stringBuilder.AppendFormat("({0})", this.m_warningNotifications.Count), normalizedCoord + new Vector2(0.177f, -0.023f), 0.55f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
            if (this.m_currentFrameCount < this.m_warningFrameCount)
            {
                this.m_currentFrameCount = (byte) (this.m_currentFrameCount + 1);
            }
            else
            {
                this.m_currentFrameCount = 0;
                this.m_warningNotifications.RemoveAt(0);
            }
        }

        private void DrawPowerGroupInfo(MyHudSinkGroupInfo info)
        {
            Rectangle safeFullscreenRectangle = MyGuiManager.GetSafeFullscreenRectangle();
            float num = -0.25f / (((float) safeFullscreenRectangle.Width) / ((float) safeFullscreenRectangle.Height));
            Vector2 hudPos = new Vector2(0.985f, 0.65f);
            Vector2 vector2 = new Vector2(hudPos.X + num, hudPos.Y);
            Vector2 namesBottomLeft = ConvertHudToNormalizedGuiPosition(ref vector2);
            info.Data.DrawBottomUp(namesBottomLeft, ConvertHudToNormalizedGuiPosition(ref hudPos), base.m_textScale);
        }

        private void DrawVoiceChat(MyHudVoiceChat voiceChat)
        {
            MyGuiDrawAlignEnum drawAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            MyGuiPaddedTexture texture = MyGuiConstants.TEXTURE_VOICE_CHAT;
            Vector2 hudPos = new Vector2(0.01f, 0.99f);
            Vector2 normalizedCoord = ConvertHudToNormalizedGuiPosition(ref hudPos);
            MyGuiManager.DrawSpriteBatch(texture.Texture, normalizedCoord, texture.SizeGui, Color.White, drawAlign, false, true);
        }

        private void DrawWorldBorderIndicator(MyHudWorldBorderChecker checker)
        {
            if (checker.WorldCenterHintVisible)
            {
                this.m_markerRender.AddPOI(Vector3D.Zero, MyHudWorldBorderChecker.HudEntityParams.Text, MyRelationsBetweenPlayerAndBlock.Enemies);
            }
        }

        private float FindDistanceToNearestPlanetSeaLevel(BoundingBoxD worldBB, out MyPlanet closestPlanet)
        {
            closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldBB);
            double maxValue = double.MaxValue;
            if (closestPlanet != null)
            {
                maxValue = (worldBB.Center - closestPlanet.PositionComp.GetPosition()).Length() - closestPlanet.AverageRadius;
            }
            return (float) maxValue;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenHudSpace";

        private static unsafe Vector2 GetRealPositionOnCenterScreen(Vector2 value)
        {
            Vector2 vector = !MyGuiManager.FullscreenHudEnabled ? MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate(value) : MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(value);
            if (MyVideoSettingsManager.IsTripleHead())
            {
                float* singlePtr1 = (float*) ref vector.X;
                singlePtr1[0]++;
            }
            return vector;
        }

        private void InitHudStatControls()
        {
            MyHudDefinition hudDefinition = MyHud.HudDefinition;
            this.m_statControls.Clear();
            if (hudDefinition.StatControls != null)
            {
                foreach (MyObjectBuilder_StatControls controls in hudDefinition.StatControls)
                {
                    MyStatControls item = new MyStatControls(controls, controls.ApplyHudScale ? (MyGuiManager.GetSafeScreenScale() * MyHud.HudElementsScaleMultiplier) : MyGuiManager.GetSafeScreenScale()) {
                        Position = MyUtils.AlignCoord(controls.Position * MySandboxGame.ScreenSize, (Vector2) MySandboxGame.ScreenSize, controls.OriginAlign)
                    };
                    this.m_statControls.Add(item);
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            if (MyHud.VoiceChat.Visible)
            {
                MyHud.VoiceChat.Hide();
            }
        }

        public override void RecreateControls(bool constructor)
        {
            MyGuiControlLabel label;
            base.RecreateControls(constructor);
            this.InitHudStatControls();
            MyHudDefinition hudDefinition = MyHud.HudDefinition;
            this.m_gravityIndicator = new MyHudControlGravityIndicator(hudDefinition.GravityIndicator);
            if (hudDefinition.VisorOverlayTexture != null)
            {
                this.m_visorOverlayTexture = MyGuiTextures.Static.GetTexture(hudDefinition.VisorOverlayTexture.Value);
            }
            this.m_toolbarControl = new MyGuiControlToolbar(hudDefinition.Toolbar);
            this.m_toolbarControl.Position = hudDefinition.Toolbar.CenterPosition;
            this.m_toolbarControl.OriginAlign = hudDefinition.Toolbar.OriginAlign;
            this.m_toolbarControl.IsActiveControl = false;
            base.Elements.Add(this.m_toolbarControl);
            base.m_textScale = 0.8f * MyGuiManager.LanguageTextScale;
            MyGuiControlBlockInfo.MyControlBlockInfoStyle style = new MyGuiControlBlockInfo.MyControlBlockInfoStyle {
                BackgroundColormask = new VRageMath.Vector4(0.1529412f, 0.2039216f, 0.2313726f, 0.9f),
                BlockNameLabelFont = "Blue",
                EnableBlockTypeLabel = true,
                ComponentsLabelText = MySpaceTexts.HudBlockInfo_Components,
                ComponentsLabelFont = "Blue",
                InstalledRequiredLabelText = MySpaceTexts.HudBlockInfo_Installed_Required,
                InstalledRequiredLabelFont = "Blue",
                RequiredLabelText = MyCommonTexts.HudBlockInfo_Required,
                IntegrityLabelFont = "White",
                IntegrityBackgroundColor = new VRageMath.Vector4(0.2666667f, 0.3019608f, 0.3372549f, 0.9f),
                IntegrityForegroundColor = new VRageMath.Vector4(0.4509804f, 0.2705882f, 0.3137255f, 1f),
                IntegrityForegroundColorOverCritical = new VRageMath.Vector4(0.4784314f, 0.5490196f, 0.6039216f, 1f),
                LeftColumnBackgroundColor = new VRageMath.Vector4(0.1803922f, 0.2980392f, 0.3686275f, 1f),
                TitleBackgroundColor = new VRageMath.Vector4(0.2078431f, 0.2666667f, 0.2980392f, 0.9f),
                ComponentLineMissingFont = "Red",
                ComponentLineAllMountedFont = "White",
                ComponentLineAllInstalledFont = "Blue",
                ComponentLineDefaultFont = "Blue",
                ComponentLineDefaultColor = new VRageMath.Vector4(0.6f, 0.6f, 0.6f, 1f),
                ShowAvailableComponents = false,
                EnableBlockTypePanel = false
            };
            this.m_contextHelp = new MyGuiControlContextHelp(style, true, true);
            this.m_contextHelp.IsActiveControl = false;
            this.Controls.Add(this.m_contextHelp);
            this.m_blockInfo = new MyGuiControlBlockInfo(style, true, true);
            this.m_blockInfo.IsActiveControl = false;
            MyGuiControlBlockInfo.ShowComponentProgress = true;
            MyGuiControlBlockInfo.CriticalIntegrityColor = (VRageMath.Vector4) new Color(0x73, 0x45, 80);
            MyGuiControlBlockInfo.OwnershipIntegrityColor = (VRageMath.Vector4) new Color(0x38, 0x43, 0x93);
            this.Controls.Add(this.m_blockInfo);
            this.m_questlogControl = new MyGuiControlQuestlog(new Vector2(20f, 20f));
            this.m_questlogControl.IsActiveControl = false;
            this.m_questlogControl.RecreateControls();
            this.Controls.Add(this.m_questlogControl);
            VRageMath.Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            this.m_chatControl = new MyHudControlChat(MyHud.Chat, new Vector2?(Vector2.Zero), new Vector2(0.339f, 0.28f), backgroundColor, "White", 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, visibleLinesCount, false);
            base.Elements.Add(this.m_chatControl);
            backgroundColor = null;
            visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_cameraInfoMultilineControl = new MyGuiControlMultilineText(new Vector2?(Vector2.Zero), new Vector2(0.4f, 0.25f), backgroundColor, "White", 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, false, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, visibleLinesCount, false, false, null, textPadding);
            this.m_cameraInfoMultilineControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            base.Elements.Add(this.m_cameraInfoMultilineControl);
            backgroundColor = null;
            Vector2? textureResolution = null;
            this.m_rotatingWheelControl = new MyGuiControlRotatingWheel(new Vector2(0.5f, 0.8f), backgroundColor, 0.36f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, textureResolution, 1.5f);
            this.Controls.Add(this.m_rotatingWheelControl);
            this.m_rotatingWheelLabel = label = new MyGuiControlLabel();
            this.Controls.Add(label);
            Vector2 hudPos = new Vector2(0.5f, 0.02f);
            hudPos = ConvertHudToNormalizedGuiPosition(ref hudPos);
            textureResolution = null;
            backgroundColor = null;
            this.m_buildModeLabel = new MyGuiControlLabel(new Vector2?(hudPos), textureResolution, MyTexts.GetString(MyCommonTexts.Hud_BuildMode), backgroundColor, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_buildModeLabel);
            textureResolution = null;
            backgroundColor = null;
            this.m_blocksLeft = new MyGuiControlLabel(new Vector2(0.238f, 0.89f), textureResolution, MyHud.BlocksLeft.GetStringBuilder().ToString(), backgroundColor, 0.8f, "White", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            this.Controls.Add(this.m_blocksLeft);
            this.RegisterAlphaMultiplier(VisualStyleCategory.Background, MySandboxGame.Config.HUDBkOpacity);
            MyHud.ReloadTexts();
        }

        private void RefreshRotatingWheel()
        {
            this.m_rotatingWheelLabel.Visible = MyHud.RotatingWheelVisible;
            this.m_rotatingWheelControl.Visible = MyHud.RotatingWheelVisible;
            if (MyHud.RotatingWheelVisible && !ReferenceEquals(this.m_rotatingWheelLabel.TextToDraw, MyHud.RotatingWheelText))
            {
                this.m_rotatingWheelLabel.Position = this.m_rotatingWheelControl.Position + new Vector2(0f, 0.05f);
                this.m_rotatingWheelLabel.TextToDraw = MyHud.RotatingWheelText;
                Vector2 textSize = this.m_rotatingWheelLabel.GetTextSize();
                this.m_rotatingWheelLabel.PositionX -= textSize.X / 2f;
            }
        }

        public void RegisterAlphaMultiplier(VisualStyleCategory category, float multiplier)
        {
            this.m_statControls.ForEach(c => c.RegisterAlphaMultiplier(category, multiplier));
        }

        public void SetToolbarVisible(bool visible)
        {
            if (this.m_toolbarControl != null)
            {
                this.m_toolbarControl.Visible = visible;
                this.m_hiddenToolbar = !visible;
            }
        }

        public override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public override bool Update(bool hasFocus)
        {
            this.m_markerRender.Update();
            this.RefreshRotatingWheel();
            return base.Update(hasFocus);
        }
    }
}

