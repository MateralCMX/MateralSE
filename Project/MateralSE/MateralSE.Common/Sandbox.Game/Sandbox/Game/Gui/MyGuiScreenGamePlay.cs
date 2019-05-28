namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.VoiceChat;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Ansel;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Profiler;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;
    using VRageRender.Utils;

    public class MyGuiScreenGamePlay : MyGuiScreenBase
    {
        private bool audioSet;
        public static MyGuiScreenGamePlay Static;
        private int[] m_lastBeginShootTime;
        private static MyGuiScreenBase m_activeGameplayScreen;
        public static MyGuiScreenBase TmpGameplayScreenHolder;
        public static bool DisableInput;
        private IMyControlMenuInitializer m_controlMenu;
        private MyAnselCamera m_anselCamera;
        private bool m_isAnselCameraInit;
        private bool m_recording;

        public MyGuiScreenGamePlay() : base(new Vector2?(Vector2.Zero), nullable, nullable2, false, null, 0f, 0f)
        {
            Static = this;
            base.DrawMouseCursor = false;
            base.m_closeOnEsc = false;
            base.m_drawEvenWithoutFocus = true;
            base.EnabledBackgroundFade = false;
            base.m_canShareInput = false;
            base.CanBeHidden = false;
            base.m_isAlwaysFirst = true;
            DisableInput = false;
            this.m_controlMenu = Activator.CreateInstance(MyPerGameSettings.ControlMenuInitializerType) as IMyControlMenuInitializer;
            MyGuiScreenToolbarConfigBase.ReinitializeBlockScrollbarPosition();
            this.m_lastBeginShootTime = new int[((MyShootActionEnum) MyEnum<MyShootActionEnum>.Range.Max) + MyShootActionEnum.SecondaryAction];
            DoubleClickDetected = new bool[this.m_lastBeginShootTime.Length];
        }

        public override unsafe bool Draw()
        {
            MyEnvironmentData* dataPtr1;
            if (!MyAnsel.IsAnselSessionRunning)
            {
                this.m_isAnselCameraInit = false;
                if (!MyAnsel.IsAnselCaptureRunning && (MySector.MainCamera != null))
                {
                    MySession.Static.CameraController.ControlCamera(MySector.MainCamera);
                    MySector.MainCamera.Update(0.01666667f);
                    MySector.MainCamera.UploadViewMatrixToRender();
                }
            }
            else
            {
                if (!this.m_isAnselCameraInit)
                {
                    this.m_anselCamera = new MyAnselCamera(MySector.MainCamera.ViewMatrix, MySector.MainCamera.FieldOfView, MySector.MainCamera.AspectRatio, MySector.MainCamera.NearPlaneDistance, MySector.MainCamera.FarPlaneDistance, MySector.MainCamera.FarFarPlaneDistance, MySector.MainCamera.Position, 0f, 0f);
                    this.m_isAnselCameraInit = true;
                }
                this.m_anselCamera.Update();
                MyRenderProxy.SetCameraViewMatrix(this.m_anselCamera.ViewMatrix, this.m_anselCamera.ProjectionMatrix, this.m_anselCamera.ProjectionFarMatrix, this.m_anselCamera.FOV, this.m_anselCamera.FOV, this.m_anselCamera.NearPlane, this.m_anselCamera.FarPlane, this.m_anselCamera.FarFarPlane, this.m_anselCamera.Position, 0f, 0f, 1);
                MySector.MainCamera.SetViewMatrix(this.m_anselCamera.ViewMatrix);
                MySector.MainCamera.Update(0f);
            }
            MySector.UpdateSunLight();
            MyRenderProxy.UpdateGameplayFrame(MySession.Static.GameplayFrameCounter);
            MyRenderFogSettings settings = new MyRenderFogSettings {
                FogMultiplier = MySector.FogProperties.FogMultiplier,
                FogColor = MySector.FogProperties.FogColor,
                FogDensity = MySector.FogProperties.FogDensity
            };
            MyRenderProxy.UpdateFogSettings(ref settings);
            MyRenderPlanetSettings settings4 = new MyRenderPlanetSettings {
                AtmosphereIntensityMultiplier = MySector.PlanetProperties.AtmosphereIntensityMultiplier,
                AtmosphereIntensityAmbientMultiplier = MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier,
                AtmosphereDesaturationFactorForward = MySector.PlanetProperties.AtmosphereDesaturationFactorForward,
                CloudsIntensityMultiplier = MySector.PlanetProperties.CloudsIntensityMultiplier
            };
            MyRenderProxy.UpdatePlanetSettings(ref settings4);
            MyRenderProxy.UpdateSSAOSettings(ref MySector.SSAOSettings);
            MyRenderProxy.UpdateHBAOSettings(ref MySector.HBAOSettings);
            MyEnvironmentData environmentData = MySector.SunProperties.EnvironmentData;
            dataPtr1->Skybox = !string.IsNullOrEmpty(MySession.Static.CustomSkybox) ? MySession.Static.CustomSkybox : MySector.EnvironmentDefinition.EnvironmentTexture;
            dataPtr1 = (MyEnvironmentData*) ref environmentData;
            environmentData.SkyboxOrientation = MySector.EnvironmentDefinition.EnvironmentOrientation.ToQuaternion();
            environmentData.EnvironmentLight.SunLightDirection = -MySector.SunProperties.SunDirectionNormalized;
            Vector3D position = MySector.MainCamera.Position;
            MyPlanet closestPlanet = MyPlanets.Static.GetClosestPlanet(position);
            if ((closestPlanet != null) && (closestPlanet.PositionComp.WorldAABB.Contains(position) != ContainmentType.Disjoint))
            {
                float airDensity = closestPlanet.GetAirDensity(position);
                if (closestPlanet.AtmosphereSettings.SunColorLinear != null)
                {
                    Vector3 vector = environmentData.EnvironmentLight.SunColorRaw / MySector.SunProperties.SunIntensity;
                    Vector3 vector2 = closestPlanet.AtmosphereSettings.SunColorLinear.Value;
                    Vector3.Lerp(ref vector, ref vector2, airDensity, out environmentData.EnvironmentLight.SunColorRaw);
                    Vector3* vectorPtr1 = (Vector3*) ref environmentData.EnvironmentLight.SunColorRaw;
                    vectorPtr1[0] *= MySector.SunProperties.SunIntensity;
                }
                if (closestPlanet.AtmosphereSettings.SunSpecularColorLinear != null)
                {
                    Vector3 sunSpecularColorRaw = environmentData.EnvironmentLight.SunSpecularColorRaw;
                    Vector3 vector4 = closestPlanet.AtmosphereSettings.SunSpecularColorLinear.Value;
                    Vector3.Lerp(ref sunSpecularColorRaw, ref vector4, airDensity, out environmentData.EnvironmentLight.SunSpecularColorRaw);
                }
            }
            MyRenderProxy.UpdateRenderEnvironment(ref environmentData, MySector.ResetEyeAdaptation);
            MySector.ResetEyeAdaptation = false;
            MyRenderProxy.UpdateEnvironmentMap();
            if ((MyVideoSettingsManager.CurrentGraphicsSettings.PostProcessingEnabled != MyPostprocessSettingsWrapper.AllEnabled) || MyPostprocessSettingsWrapper.IsDirty)
            {
                if (MyVideoSettingsManager.CurrentGraphicsSettings.PostProcessingEnabled)
                {
                    MyPostprocessSettingsWrapper.ReloadSettingsFrom(MySector.EnvironmentDefinition.PostProcessSettings);
                }
                else
                {
                    MyPostprocessSettingsWrapper.ReducePostProcessing();
                }
            }
            MyRenderProxy.SwitchPostprocessSettings(ref MyPostprocessSettingsWrapper.Settings);
            if (MyRenderProxy.SettingsDirty)
            {
                MyRenderProxy.SwitchRenderSettings(MyRenderProxy.Settings);
            }
            MyRenderProxy.Draw3DScene();
            using (Stats.Generic.Measure("GamePrepareDraw"))
            {
                if (MySession.Static != null)
                {
                    MySession.Static.Draw();
                }
            }
            if ((MySession.Static.ControlledEntity != null) && (MySession.Static.CameraController != null))
            {
                MySession.Static.ControlledEntity.DrawHud(MySession.Static.CameraController, MySession.Static.LocalPlayerId);
            }
            if (MySandboxGame.IsPaused && !MyHud.MinimalHud)
            {
                this.DrawPauseIndicator();
            }
            return true;
        }

        private unsafe void DrawPauseIndicator()
        {
            Rectangle safeFullscreenRectangle = MyGuiManager.GetSafeFullscreenRectangle();
            int* numPtr1 = (int*) ref safeFullscreenRectangle.Height;
            numPtr1[0] /= 0x12;
            StringBuilder text = MyTexts.Get(MyCommonTexts.GamePaused);
            MyGuiManager.DrawSpriteBatch(MyGuiConstants.TEXTURE_HUD_BG_MEDIUM_RED2.Texture, safeFullscreenRectangle, Color.White, true);
            Color? colorMask = null;
            MyGuiManager.DrawString("Blue", text, new Vector2(0.5f, 0.024f), 1f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenGamePlay";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            bool flag = false;
            if ((!MyAnsel.IsInitializedSuccessfuly && (MyInput.Static.IsKeyPress(MyKeys.F2) && MyInput.Static.IsKeyPress(MyKeys.Alt))) && (!MyInput.Static.WasKeyPress(MyKeys.F2) || !MyInput.Static.WasKeyPress(MyKeys.Alt)))
            {
                MyStringId? nullable;
                Vector2? nullable2;
                if (MyVideoSettingsManager.IsCurrentAdapterNvidia())
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextAnselWrongDriverOrCard), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextAnselNotNvidiaGpu), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
            if (MyClipboardComponent.Static != null)
            {
                flag = MyClipboardComponent.Static.HandleGameInput();
            }
            if (!flag && (MyCubeBuilder.Static != null))
            {
                flag = MyCubeBuilder.Static.HandleGameInput();
            }
            if (!flag)
            {
                base.HandleInput(receivedFocusInThisUpdate);
            }
        }

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            Sandbox.Game.Entities.IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            IMyCameraController cameraController = MySession.Static.CameraController;
            MyStringId context = (controlledEntity != null) ? controlledEntity.ControlContext : MySpaceBindingCreator.CX_BASE;
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape) || MyControllerHelper.IsControl(context, MyControlsGUI.MAIN_MENU, MyControlStateType.NEW_PRESSED, false))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                if (MySessionComponentReplay.Static.IsReplaying || MySessionComponentReplay.Static.IsRecording)
                {
                    MySessionComponentReplay.Static.StopRecording();
                    MySessionComponentReplay.Static.StopReplay();
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.AdminMenuScreen, Array.Empty<object>()));
                }
                else
                {
                    object[] args = new object[] { !MySandboxGame.IsPaused };
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.MainMenu, args));
                }
            }
            if (DisableInput)
            {
                if (MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning && (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyInput.Static.IsNewKeyPressed(MyKeys.Space)))
                {
                    MySession.Static.GetComponent<MySessionComponentCutscenes>().CutsceneSkip();
                }
                MySession.Static.ControlledEntity.MoveAndRotate(Vector3.Zero, Vector2.Zero, 0f);
            }
            else
            {
                Vector3D? nullable;
                if (MyInput.Static.ENABLE_DEVELOPER_KEYS || ((MySession.Static.LocalHumanPlayer != null) && MySession.Static.HasPlayerSpectatorRights(MySession.Static.LocalHumanPlayer.Id.SteamId)))
                {
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SPECTATOR_NONE) && (MySession.Static.ControlledEntity != null))
                    {
                        if (!MyInput.Static.ENABLE_DEVELOPER_KEYS)
                        {
                            SetCameraController();
                        }
                        else
                        {
                            MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                            if ((cameraControllerEnum != MyCameraControllerEnum.Entity) && (cameraControllerEnum != MyCameraControllerEnum.ThirdPersonSpectator))
                            {
                                SetCameraController();
                            }
                            else if (MySession.Static.VirtualClients.Any() && (Sync.Clients.LocalClient != null))
                            {
                                MyPlayer nextControlledPlayer = MySession.Static.VirtualClients.GetNextControlledPlayer(MySession.Static.LocalHumanPlayer);
                                MyPlayer player = nextControlledPlayer ?? Sync.Clients.LocalClient.GetPlayer(0);
                                if (player != null)
                                {
                                    Sync.Clients.LocalClient.ControlledPlayerSerialId = player.Id.SerialId;
                                }
                            }
                            else
                            {
                                long identityId = MySession.Static.LocalHumanPlayer.Identity.IdentityId;
                                List<MyEntity> list = new List<MyEntity>();
                                foreach (MyEntity entity3 in MyEntities.GetEntities())
                                {
                                    MyCharacter character = entity3 as MyCharacter;
                                    if (((character != null) && (!character.IsDead && (character.GetIdentity() != null))) && (character.GetIdentity().IdentityId == identityId))
                                    {
                                        list.Add(entity3);
                                    }
                                    MyCubeGrid grid = entity3 as MyCubeGrid;
                                    if (grid != null)
                                    {
                                        using (HashSet<MySlimBlock>.Enumerator enumerator2 = grid.GetBlocks().GetEnumerator())
                                        {
                                            while (enumerator2.MoveNext())
                                            {
                                                MyCockpit fatBlock = enumerator2.Current.FatBlock as MyCockpit;
                                                if ((fatBlock != null) && ((fatBlock.Pilot != null) && ((fatBlock.Pilot.GetIdentity() != null) && (fatBlock.Pilot.GetIdentity().IdentityId == identityId))))
                                                {
                                                    list.Add(fatBlock);
                                                }
                                            }
                                        }
                                    }
                                }
                                int index = list.IndexOf(MySession.Static.ControlledEntity.Entity);
                                List<MyEntity> list2 = new List<MyEntity>();
                                if ((index + 1) < list.Count)
                                {
                                    list2.AddRange(list.GetRange(index + 1, (list.Count - index) - 1));
                                }
                                if (index != -1)
                                {
                                    list2.AddRange(list.GetRange(0, index + 1));
                                }
                                Sandbox.Game.Entities.IMyControllableEntity entity = null;
                                int num3 = 0;
                                while (true)
                                {
                                    if (num3 < list2.Count)
                                    {
                                        if (!(list2[num3] is Sandbox.Game.Entities.IMyControllableEntity))
                                        {
                                            num3++;
                                            continue;
                                        }
                                        entity = list2[num3] as Sandbox.Game.Entities.IMyControllableEntity;
                                    }
                                    if ((MySession.Static.LocalHumanPlayer != null) && (entity != null))
                                    {
                                        MySession.Static.LocalHumanPlayer.Controller.TakeControl(entity);
                                        MyCharacter character = MySession.Static.ControlledEntity as MyCharacter;
                                        if ((character == null) && (MySession.Static.ControlledEntity is MyCockpit))
                                        {
                                            character = (MySession.Static.ControlledEntity as MyCockpit).Pilot;
                                        }
                                        if (character != null)
                                        {
                                            MySession.Static.LocalHumanPlayer.Identity.ChangeCharacter(character);
                                        }
                                    }
                                    break;
                                }
                            }
                            if (!(MySession.Static.ControlledEntity is MyCharacter))
                            {
                                MySession.Static.GameFocusManager.Clear();
                            }
                        }
                    }
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SPECTATOR_DELTA))
                    {
                        if (SpectatorEnabled)
                        {
                            MySpectatorCameraController.Static.TurnLightOff();
                            nullable = null;
                            MySession.Static.SetCameraController(MyCameraControllerEnum.SpectatorDelta, null, nullable);
                        }
                        if (MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            if (MySession.Static.ControlledEntity != null)
                            {
                                MySpectator.Static.Position = MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition() + MySpectator.Static.ThirdPersonCameraDelta;
                                MySpectator.Static.SetTarget(MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition(), new Vector3D?(MySession.Static.ControlledEntity.Entity.PositionComp.WorldMatrix.Up));
                                MySpectatorCameraController.Static.TrackedEntity = MySession.Static.ControlledEntity.Entity.EntityId;
                            }
                            else
                            {
                                MyEntity targetEntity = MyCubeGrid.GetTargetEntity();
                                if (targetEntity != null)
                                {
                                    MySpectator.Static.Position = targetEntity.PositionComp.GetPosition() + MySpectator.Static.ThirdPersonCameraDelta;
                                    MySpectator.Static.SetTarget(targetEntity.PositionComp.GetPosition(), new Vector3D?(targetEntity.PositionComp.WorldMatrix.Up));
                                    MySpectatorCameraController.Static.TrackedEntity = targetEntity.EntityId;
                                }
                            }
                        }
                    }
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SPECTATOR_FREE) && SpectatorEnabled)
                    {
                        if (!MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            nullable = null;
                            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, nullable);
                        }
                        else
                        {
                            nullable = null;
                            MySession.Static.SetCameraController(MyCameraControllerEnum.SpectatorOrbit, null, nullable);
                            MySpectatorCameraController.Static.Reset();
                        }
                        if (MyInput.Static.IsAnyCtrlKeyPressed() && (MySession.Static.ControlledEntity != null))
                        {
                            MySpectator.Static.Position = MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition() + MySpectator.Static.ThirdPersonCameraDelta;
                            MySpectator.Static.SetTarget(MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition(), new Vector3D?(MySession.Static.ControlledEntity.Entity.PositionComp.WorldMatrix.Up));
                        }
                    }
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SPECTATOR_STATIC) && (MySession.Static.ControlledEntity != null))
                    {
                        MySpectatorCameraController.Static.TurnLightOff();
                        nullable = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.SpectatorFixed, null, nullable);
                        if (MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            MySpectator.Static.Position = MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition() + MySpectator.Static.ThirdPersonCameraDelta;
                            MySpectator.Static.SetTarget(MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition(), new Vector3D?(MySession.Static.ControlledEntity.Entity.PositionComp.WorldMatrix.Up));
                        }
                    }
                    if ((MyInput.Static.IsNewKeyPressed(MyKeys.Space) && (MyInput.Static.IsAnyCtrlKeyPressed() && (ReferenceEquals(MySession.Static.CameraController, MySpectator.Static) && (MySession.Static != null)))) && MySession.Static.IsUserSpaceMaster(Sync.MyId))
                    {
                        MyMultiplayer.TeleportControlledEntity(MySpectator.Static.Position);
                    }
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CONSOLE) && MyInput.Static.IsAnyAltKeyPressed())
                    {
                        MyGuiScreenConsole.Show();
                    }
                }
                if ((MyInput.Static.ENABLE_DEVELOPER_KEYS && ((MySession.Static != null) && ((MySession.Static.LocalCharacter != null) && MyInput.Static.IsAnyShiftKeyPressed()))) && MyInput.Static.IsNewKeyPressed(MyKeys.B))
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                    MyGuiScreenAssetModifier screen = new MyGuiScreenAssetModifier(MySession.Static.LocalCharacter);
                    ActiveGameplayScreen = screen;
                    MyGuiSandbox.AddScreen(screen);
                }
                if (MyDefinitionErrors.ShouldShowModErrors)
                {
                    MyDefinitionErrors.ShouldShowModErrors = false;
                    MyGuiSandbox.ShowModErrors();
                }
                if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CAMERA_MODE) || MyControllerHelper.IsControl(MyControllerHelper.CX_CHARACTER, MyControlsSpace.CAMERA_MODE, MyControlStateType.NEW_PRESSED, false)) && this.CanSwitchCamera)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                    this.SwitchCamera();
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HELP_SCREEN))
                {
                    if (!MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        if (MyInput.Static.IsAnyShiftKeyPressed() && (MyPerGameSettings.GUI.PerformanceWarningScreen != null))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                            MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PerformanceWarningScreen, Array.Empty<object>());
                            ActiveGameplayScreen = screen;
                            MyGuiSandbox.AddScreen(screen);
                        }
                        else if (ActiveGameplayScreen == null)
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                            MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HelpScreen, Array.Empty<object>());
                            ActiveGameplayScreen = screen;
                            MyGuiSandbox.AddScreen(screen);
                        }
                    }
                    else
                    {
                        switch (MySandboxGame.Config.DebugComponentsInfo)
                        {
                            case MyDebugComponent.MyDebugComponentInfoState.NoInfo:
                                MySandboxGame.Config.DebugComponentsInfo = MyDebugComponent.MyDebugComponentInfoState.EnabledInfo;
                                break;

                            case MyDebugComponent.MyDebugComponentInfoState.EnabledInfo:
                                MySandboxGame.Config.DebugComponentsInfo = MyDebugComponent.MyDebugComponentInfoState.FullInfo;
                                break;

                            case MyDebugComponent.MyDebugComponentInfoState.FullInfo:
                                MySandboxGame.Config.DebugComponentsInfo = MyDebugComponent.MyDebugComponentInfoState.NoInfo;
                                break;

                            default:
                                break;
                        }
                        MySandboxGame.Config.Save();
                    }
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOGGLE_HUD))
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                }
                if (MyPerGameSettings.SimplePlayerNames && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.BROADCASTING))
                {
                    MyHud.LocationMarkers.Visible = !MyHud.LocationMarkers.Visible;
                }
                if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.MISSION_SETTINGS) && ((ActiveGameplayScreen == null) && (MyPerGameSettings.Game == GameEnum.SE_GAME))) && MyFakes.ENABLE_MISSION_TRIGGERS)
                {
                    if (MySession.Static.Settings.ScenarioEditMode)
                    {
                        MyGuiSandbox.AddScreen(new MyGuiScreenMissionTriggers());
                    }
                    else if (MySession.Static.IsScenario)
                    {
                        MyGuiSandbox.AddScreen(new MyGuiScreenBriefing());
                    }
                }
                bool flag = false;
                if (MySession.Static.ControlledEntity is IMyUseObject)
                {
                    flag = (MySession.Static.ControlledEntity as IMyUseObject).HandleInput();
                }
                if ((controlledEntity != null) && !flag)
                {
                    if (MySandboxGame.IsPaused)
                    {
                        if (controlledEntity.ShouldEndShootingOnPause(MyShootActionEnum.PrimaryAction) && controlledEntity.ShouldEndShootingOnPause(MyShootActionEnum.SecondaryAction))
                        {
                            controlledEntity.EndShoot(MyShootActionEnum.PrimaryAction);
                            controlledEntity.EndShoot(MyShootActionEnum.SecondaryAction);
                        }
                    }
                    else
                    {
                        if ((MyFakes.ENABLE_NON_PUBLIC_GUI_ELEMENTS && (MyInput.Static.IsNewKeyPressed(MyKeys.F2) && (MyInput.Static.IsAnyShiftKeyPressed() && !MyInput.Static.IsAnyCtrlKeyPressed()))) && !MyInput.Static.IsAnyAltKeyPressed())
                        {
                            MySession.Static.Settings.GameMode = (MySession.Static.Settings.GameMode != MyGameModeEnum.Creative) ? MyGameModeEnum.Creative : MyGameModeEnum.Survival;
                        }
                        if (((context == MySpaceBindingCreator.CX_BUILD_MODE) || (context == MySpaceBindingCreator.CX_CHARACTER)) || (context == MySpaceBindingCreator.CX_SPACESHIP))
                        {
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                            {
                                if (MyToolbarComponent.CurrentToolbar.ShouldActivateSlot)
                                {
                                    MyToolbarComponent.CurrentToolbar.ActivateStagedSelectedItem();
                                }
                                else
                                {
                                    if (context == MySpaceBindingCreator.CX_CHARACTER)
                                    {
                                        if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastBeginShootTime[0]) < 500f)
                                        {
                                            DoubleClickDetected[0] = true;
                                        }
                                        else
                                        {
                                            DoubleClickDetected[0] = false;
                                            this.m_lastBeginShootTime[0] = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                                        }
                                    }
                                    controlledEntity.BeginShoot(MyShootActionEnum.PrimaryAction);
                                }
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_RELEASED, false))
                            {
                                if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastBeginShootTime[0]) > 500f)
                                {
                                    DoubleClickDetected[0] = false;
                                }
                                controlledEntity.EndShoot(MyShootActionEnum.PrimaryAction);
                                DoubleClickDetected[0] = false;
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                            {
                                if (context == MySpaceBindingCreator.CX_CHARACTER)
                                {
                                    if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastBeginShootTime[1]) < 500f)
                                    {
                                        DoubleClickDetected[1] = true;
                                    }
                                    else
                                    {
                                        DoubleClickDetected[1] = false;
                                        this.m_lastBeginShootTime[1] = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                                    }
                                }
                                controlledEntity.BeginShoot(MyShootActionEnum.SecondaryAction);
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_TOOL_ACTION, MyControlStateType.NEW_RELEASED, false))
                            {
                                if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastBeginShootTime[1]) > 500f)
                                {
                                    DoubleClickDetected[1] = false;
                                }
                                controlledEntity.EndShoot(MyShootActionEnum.SecondaryAction);
                                DoubleClickDetected[1] = false;
                            }
                        }
                        if ((context == MySpaceBindingCreator.CX_CHARACTER) || (context == MySpaceBindingCreator.CX_SPACESHIP))
                        {
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.USE, MyControlStateType.NEW_PRESSED, false))
                            {
                                if (cameraController == null)
                                {
                                    controlledEntity.Use();
                                    this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                        UseData data = new UseData {
                                            Use = true
                                        };
                                        x.UseData = new UseData?(data);
                                    });
                                }
                                else if (!cameraController.HandleUse())
                                {
                                    this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                        UseData data = new UseData {
                                            Use = true
                                        };
                                        x.UseData = new UseData?(data);
                                    });
                                    controlledEntity.Use();
                                }
                            }
                            else if (MyControllerHelper.IsControl(context, MyControlsSpace.USE, MyControlStateType.PRESSED, false))
                            {
                                controlledEntity.UseContinues();
                                this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                    UseData data = new UseData {
                                        UseContinues = true
                                    };
                                    x.UseData = new UseData?(data);
                                });
                            }
                            else if (MyControllerHelper.IsControl(context, MyControlsSpace.USE, MyControlStateType.NEW_RELEASED, false))
                            {
                                controlledEntity.UseFinished();
                                this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                    UseData data = new UseData {
                                        UseFinished = true
                                    };
                                    x.UseData = new UseData?(data);
                                });
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.PICK_UP, MyControlStateType.NEW_PRESSED, false))
                            {
                                if (cameraController == null)
                                {
                                    controlledEntity.PickUp();
                                }
                                else if (!cameraController.HandlePickUp())
                                {
                                    controlledEntity.PickUp();
                                }
                            }
                            else if (MyControllerHelper.IsControl(context, MyControlsSpace.PICK_UP, MyControlStateType.PRESSED, false))
                            {
                                controlledEntity.PickUpContinues();
                            }
                            else if (MyControllerHelper.IsControl(context, MyControlsSpace.PICK_UP, MyControlStateType.NEW_RELEASED, false))
                            {
                                controlledEntity.PickUpFinished();
                            }
                            if (!MySession.Static.IsCameraUserControlledSpectator())
                            {
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.CROUCH, MyControlStateType.NEW_PRESSED, false))
                                {
                                    controlledEntity.Crouch();
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.CROUCH, MyControlStateType.PRESSED, false))
                                {
                                    controlledEntity.Down();
                                }
                                controlledEntity.Sprint(MyControllerHelper.IsControl(context, MyControlsSpace.SPRINT, MyControlStateType.PRESSED, false));
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.JUMP, MyControlStateType.NEW_PRESSED, false))
                                {
                                    controlledEntity.Jump(MyInput.Static.GetPositionDelta());
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.JUMP, MyControlStateType.PRESSED, false))
                                {
                                    controlledEntity.Up();
                                }
                                MyShipController controller2 = controlledEntity as MyShipController;
                                if (controller2 != null)
                                {
                                    controller2.WheelJump(MyControllerHelper.IsControl(context, MyControlsSpace.WHEEL_JUMP, MyControlStateType.PRESSED, false));
                                    if (MyControllerHelper.IsControl(context, MyControlsSpace.JUMP, MyControlStateType.NEW_PRESSED, false))
                                    {
                                        controller2.TryEnableBrakes(true);
                                    }
                                    else if (MyControllerHelper.IsControl(context, MyControlsSpace.JUMP, MyControlStateType.NEW_RELEASED, false))
                                    {
                                        controller2.TryEnableBrakes(false);
                                    }
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.SWITCH_WALK, MyControlStateType.NEW_PRESSED, false))
                                {
                                    controlledEntity.SwitchWalk();
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.BROADCASTING, MyControlStateType.NEW_PRESSED, false))
                                {
                                    controlledEntity.SwitchBroadcasting();
                                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.HELMET, MyControlStateType.NEW_PRESSED, false))
                                {
                                    controlledEntity.SwitchHelmet();
                                    this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                        ControlSwitchesData data = new ControlSwitchesData {
                                            SwitchHelmet = true
                                        };
                                        x.ControlSwitchesData = new ControlSwitchesData?(data);
                                    });
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.DAMPING, MyControlStateType.NEW_PRESSED, false))
                                {
                                    EndpointId id2;
                                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                    if (!MyInput.Static.IsAnyCtrlKeyPressed())
                                    {
                                        controlledEntity.SwitchDamping();
                                        id2 = new EndpointId();
                                        nullable = null;
                                        MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.ClearDampeningEntity), controlledEntity.Entity.EntityId, id2, nullable);
                                    }
                                    else
                                    {
                                        if (!controlledEntity.EnabledDamping)
                                        {
                                            controlledEntity.SwitchDamping();
                                        }
                                        id2 = new EndpointId();
                                        nullable = null;
                                        MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.SetDampeningEntity), controlledEntity.Entity.EntityId, id2, nullable);
                                    }
                                    this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                        ControlSwitchesData data = new ControlSwitchesData {
                                            SwitchDamping = true
                                        };
                                        x.ControlSwitchesData = new ControlSwitchesData?(data);
                                    });
                                }
                                if (MyControllerHelper.IsControl(context, MyControlsSpace.THRUSTS, MyControlStateType.NEW_PRESSED, false))
                                {
                                    if (!(controlledEntity is MyCharacter))
                                    {
                                        MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                    }
                                    controlledEntity.SwitchThrusts();
                                    this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                        ControlSwitchesData data = new ControlSwitchesData {
                                            SwitchThrusts = true
                                        };
                                        x.ControlSwitchesData = new ControlSwitchesData?(data);
                                    });
                                }
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.HEADLIGHTS, MyControlStateType.NEW_PRESSED, false))
                            {
                                if (MySession.Static.IsCameraUserControlledSpectator())
                                {
                                    MySpectatorCameraController.Static.SwitchLight();
                                }
                                else
                                {
                                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                    controlledEntity.SwitchLights();
                                    this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                        ControlSwitchesData data = new ControlSwitchesData {
                                            SwitchLights = true
                                        };
                                        x.ControlSwitchesData = new ControlSwitchesData?(data);
                                    });
                                }
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.TOGGLE_REACTORS, MyControlStateType.NEW_PRESSED, false))
                            {
                                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                                controlledEntity.SwitchReactors();
                                this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                    ControlSwitchesData data = new ControlSwitchesData {
                                        SwitchReactors = true
                                    };
                                    x.ControlSwitchesData = new ControlSwitchesData?(data);
                                });
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.LANDING_GEAR, MyControlStateType.NEW_PRESSED, false))
                            {
                                controlledEntity.SwitchLandingGears();
                                this.MakeRecord(controlledEntity, delegate (ref PerFrameData x) {
                                    ControlSwitchesData data = new ControlSwitchesData {
                                        SwitchLandingGears = true
                                    };
                                    x.ControlSwitchesData = new ControlSwitchesData?(data);
                                });
                            }
                            if (MyControllerHelper.IsControl(context, MyControlsSpace.SUICIDE, MyControlStateType.NEW_PRESSED, false))
                            {
                                controlledEntity.Die();
                            }
                            if ((controlledEntity is MyCockpit) && MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_COLOR_CHANGE, MyControlStateType.NEW_PRESSED, false))
                            {
                                (controlledEntity as MyCockpit).SwitchWeaponMode();
                            }
                        }
                    }
                    if (!MySandboxGame.IsPaused)
                    {
                        if (MyControllerHelper.IsControl(context, MyControlsSpace.TERMINAL, MyControlStateType.NEW_PRESSED, false) && (ActiveGameplayScreen == null))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                            controlledEntity.ShowTerminal();
                        }
                        if (MyControllerHelper.IsControl(context, MyControlsSpace.INVENTORY, MyControlStateType.NEW_PRESSED, false) && (ActiveGameplayScreen == null))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                            controlledEntity.ShowInventory();
                        }
                        if (MyControllerHelper.IsControl(context, MyControlsSpace.CONTROL_MENU, MyControlStateType.NEW_PRESSED, false) && (ActiveGameplayScreen == null))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                            this.m_controlMenu.OpenControlMenu(controlledEntity);
                        }
                    }
                }
                if ((!VRage.Profiler.MyRenderProfiler.ProfilerVisible && MyControllerHelper.IsControl(context, MyControlsSpace.CHAT_SCREEN, MyControlStateType.NEW_PRESSED, false)) && (MyGuiScreenChat.Static == null))
                {
                    Vector2 hudPos = new Vector2(0.029f, 0.8f);
                    MyGuiSandbox.AddScreen(new MyGuiScreenChat(MyGuiScreenHudBase.ConvertHudToNormalizedGuiPosition(ref hudPos)));
                }
                if (MyPerGameSettings.VoiceChatEnabled && (MyVoiceChatSessionComponent.Static != null))
                {
                    if (MyControllerHelper.IsControl(context, MyControlsSpace.VOICE_CHAT, MyControlStateType.NEW_PRESSED, false))
                    {
                        MyVoiceChatSessionComponent.Static.StartRecording();
                    }
                    else if (MyVoiceChatSessionComponent.Static.IsRecording && !MyControllerHelper.IsControl(context, MyControlsSpace.VOICE_CHAT, MyControlStateType.PRESSED, false))
                    {
                        MyVoiceChatSessionComponent.Static.StopRecording();
                    }
                }
                this.MoveAndRotatePlayerOrCamera();
                if (MyInput.Static.IsNewKeyPressed(MyKeys.F5))
                {
                    if (!MySession.Static.Settings.EnableSaving)
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.CannotSave);
                    }
                    else
                    {
                        MyStringId? nullable2;
                        Vector2? nullable3;
                        MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                        string currentPath = MySession.Static.CurrentPath;
                        if (MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            if (!Sync.IsServer)
                            {
                                MyHud.Notifications.Add(MyNotificationSingletons.ClientCannotSave);
                            }
                            else if (!MyAsyncSaving.InProgress)
                            {
                                nullable2 = null;
                                nullable2 = null;
                                nullable2 = null;
                                nullable2 = null;
                                nullable3 = null;
                                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureYouWantToQuickSave), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable2, nullable2, nullable2, nullable2, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                                    if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                                    {
                                        MyAsyncSaving.Start(() => MySector.ResetEyeAdaptation = true, null, false);
                                    }
                                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable3);
                                screen.SkipTransition = true;
                                screen.CloseBeforeCallback = true;
                                MyGuiSandbox.AddScreen(screen);
                            }
                        }
                        else if (!Sync.IsServer)
                        {
                            this.ShowReconnectMessageBox();
                        }
                        else if (!MyAsyncSaving.InProgress)
                        {
                            this.ShowLoadMessageBox(currentPath);
                        }
                        else
                        {
                            nullable2 = null;
                            nullable2 = null;
                            nullable2 = null;
                            nullable2 = null;
                            nullable3 = null;
                            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextSavingInProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable2, nullable2, nullable2, nullable2, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable3);
                            screen.SkipTransition = true;
                            screen.InstantClose = false;
                            MyGuiSandbox.AddScreen(screen);
                        }
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.F3))
                {
                    if (!Sync.MultiplayerActive)
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.MultiplayerDisabled);
                    }
                    else
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PlayersScreen, Array.Empty<object>()));
                    }
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.FACTIONS_MENU) && !MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                    MyScreenManager.AddScreenNow(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.FactionScreen, Array.Empty<object>()));
                }
                if ((!(MyInput.Static.IsKeyPress(MyKeys.LeftWindows) || MyInput.Static.IsKeyPress(MyKeys.RightWindows)) && (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.BUILD_SCREEN) && (!MyInput.Static.IsAnyCtrlKeyPressed() && ((ActiveGameplayScreen == null) && (MyPerGameSettings.GUI.EnableToolbarConfigScreen && (MyGuiScreenToolbarConfigBase.Static == null)))))) && ((MySession.Static.ControlledEntity is MyShipController) || (MySession.Static.ControlledEntity is MyCharacter)))
                {
                    int num4 = 0;
                    if (MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        num4 += 6;
                    }
                    if (MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        num4 += 12;
                    }
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                    object[] args = new object[] { num4, MySession.Static.ControlledEntity as MyShipController };
                    MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    ActiveGameplayScreen = screen;
                    MyGuiSandbox.AddScreen(screen);
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.PAUSE_GAME) && (Sync.Clients.Count < 2))
                {
                    MySandboxGame.PauseToggle();
                }
                if ((MySession.Static != null) && MyInput.Static.IsNewKeyPressed(MyKeys.F10))
                {
                    if (MyInput.Static.IsAnyAltKeyPressed())
                    {
                        if (!MySession.Static.IsAdminMenuEnabled || (MyPerGameSettings.Game == GameEnum.UNKNOWN_GAME))
                        {
                            MyHud.Notifications.Add(MyNotificationSingletons.AdminMenuNotAvailable);
                        }
                        else
                        {
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.AdminMenuScreen, Array.Empty<object>()));
                        }
                    }
                    else if (((MyPerGameSettings.GUI.VoxelMapEditingScreen != null) && (MySession.Static.CreativeToolsEnabled(Sync.MyId) || MySession.Static.CreativeMode)) && MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.VoxelMapEditingScreen, Array.Empty<object>()));
                    }
                    else if (MyFakes.I_AM_READY_FOR_NEW_BLUEPRINT_SCREEN)
                    {
                        MyGuiSandbox.AddScreen(MyGuiBlueprintScreen_Reworked.CreateBlueprintScreen(MyClipboardComponent.Static.Clipboard, MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId), MyBlueprintAccessType.NORMAL));
                    }
                    else
                    {
                        MyGuiSandbox.AddScreen(new MyGuiBlueprintScreen(MyClipboardComponent.Static.Clipboard, MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId), MyBlueprintAccessType.NORMAL));
                    }
                }
                if ((MyInput.Static.IsNewKeyPressed(MyKeys.F11) && !MyInput.Static.IsAnyShiftKeyPressed()) && !MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    MyDX9Gui.SwitchModDebugScreen();
                }
            }
        }

        public override void InputLost()
        {
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.InputLost();
            }
        }

        public override void LoadContent()
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.LoadContent - START");
            MySandboxGame.Log.IncreaseIndent();
            Static = this;
            base.LoadContent();
            MySandboxGame.IsUpdateReady = true;
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.LoadContent - END");
        }

        public override void LoadData()
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.LoadData - START");
            MySandboxGame.Log.IncreaseIndent();
            base.LoadData();
            MyCharacter.Preload();
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.LoadData - END");
        }

        private void MakeRecord(Sandbox.Game.Entities.IMyControllableEntity controlledObject, MySessionComponentReplay.ActionRef<PerFrameData> action)
        {
            if (MySessionComponentReplay.Static.IsEntityBeingRecorded(controlledObject.Entity.GetTopMostParent(null).EntityId))
            {
                PerFrameData item = new PerFrameData();
                action(ref item);
                MySessionComponentReplay.Static.ProvideEntityRecordData(controlledObject.Entity.GetTopMostParent(null).EntityId, item);
            }
        }

        public void MoveAndRotatePlayerOrCamera()
        {
            MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
            bool flag = cameraControllerEnum == MyCameraControllerEnum.Spectator;
            bool flag2 = flag || ((cameraControllerEnum == MyCameraControllerEnum.ThirdPersonSpectator) && MyInput.Static.IsAnyAltKeyPressed());
            bool flag3 = (MyScreenManager.GetScreenWithFocus() is MyGuiScreenDebugBase) && !MyInput.Static.IsAnyAltKeyPressed();
            bool flag4 = !MySessionComponentVoxelHand.Static.BuildMode && !MyCubeBuilder.Static.IsBuildMode;
            float rollIndicator = (!MySessionComponentVoxelHand.Static.BuildMode && !MyCubeBuilder.Static.IsBuildMode) ? MyInput.Static.GetRoll() : 0f;
            Vector2 rotation = MyInput.Static.GetRotation();
            Vector3 moveIndicator = flag4 ? MyInput.Static.GetPositionDelta() : Vector3.Zero;
            if (MyPetaInputComponent.MovementDistanceCounter > 0)
            {
                moveIndicator = Vector3.Forward;
                MyPetaInputComponent.MovementDistanceCounter--;
            }
            if (MySession.Static.ControlledEntity == null)
            {
                MySpectatorCameraController.Static.MoveAndRotate(moveIndicator, rotation, rollIndicator);
            }
            else
            {
                if (MySandboxGame.IsPaused)
                {
                    if (!flag && !flag2)
                    {
                        return;
                    }
                    if (!flag2 | flag3)
                    {
                        rotation = Vector2.Zero;
                    }
                    rollIndicator = 0f;
                }
                if (MySession.Static.IsCameraUserControlledSpectator())
                {
                    MySpectatorCameraController.Static.MoveAndRotate(moveIndicator, rotation, rollIndicator);
                }
                else
                {
                    if (!MySession.Static.CameraController.IsInFirstPersonView)
                    {
                        MyThirdPersonSpectator.Static.UpdateZoom();
                    }
                    if (!MySessionComponentReplay.Static.IsEntityBeingReplayed(MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).EntityId))
                    {
                        if (!MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND))
                        {
                            MySession.Static.ControlledEntity.MoveAndRotate(moveIndicator, rotation, rollIndicator);
                        }
                        else
                        {
                            if (MySession.Static.ControlledEntity is MyRemoteControl)
                            {
                                rotation = Vector2.Zero;
                                rollIndicator = 0f;
                            }
                            else if ((MySession.Static.ControlledEntity is MyCockpit) || !MySession.Static.CameraController.IsInFirstPersonView)
                            {
                                rotation = Vector2.Zero;
                            }
                            MySession.Static.ControlledEntity.MoveAndRotate(moveIndicator, rotation, rollIndicator);
                            if (!MySession.Static.CameraController.IsInFirstPersonView)
                            {
                                MyThirdPersonSpectator.Static.SaveSettings();
                            }
                        }
                    }
                }
            }
        }

        private static void SetAudioVolumes()
        {
            MyAudio.Static.StopMusic();
            MyAudio.Static.ChangeGlobalVolume(1f, 5f);
            if ((MyPerGameSettings.UseMusicController && (MyFakes.ENABLE_MUSIC_CONTROLLER && (MySandboxGame.Config.EnableDynamicMusic && !Sandbox.Engine.Platform.Game.IsDedicated))) && (MyMusicController.Static == null))
            {
                MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
            }
            MyAudio.Static.MusicAllowed = ReferenceEquals(MyMusicController.Static, null);
            if (MyMusicController.Static != null)
            {
                MyMusicController.Static.Active = true;
            }
            else
            {
                MyMusicTrack track = new MyMusicTrack {
                    TransitionCategory = MyStringId.GetOrCompute("Default")
                };
                MyAudio.Static.PlayMusic(new MyMusicTrack?(track), 0);
            }
        }

        public static void SetCameraController()
        {
            if (MySession.Static.ControlledEntity != null)
            {
                Vector3D? nullable;
                MyRemoteControl entity = MySession.Static.ControlledEntity.Entity as MyRemoteControl;
                if (entity == null)
                {
                    nullable = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.ControlledEntity.Entity, nullable);
                }
                else if (entity.PreviousControlledEntity is IMyCameraController)
                {
                    nullable = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, entity.PreviousControlledEntity.Entity, nullable);
                }
            }
        }

        public void ShowLoadMessageBox(string currentSession)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureYouWantToQuickLoad), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MySessionLoader.Unload();
                    MyOnlineModeEnum? onlineMode = null;
                    MySessionLoader.LoadSingleplayerSession(currentSession, null, null, onlineMode, 0);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            screen.SkipTransition = true;
            screen.CloseBeforeCallback = true;
            MyGuiSandbox.AddScreen(screen);
        }

        public void ShowReconnectMessageBox()
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureYouWantToReconnect), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    if (MyMultiplayer.Static is MyMultiplayerLobbyClient)
                    {
                        MySessionLoader.UnloadAndExitToMenu();
                        MyJoinGameHelper.JoinGame(MyMultiplayer.Static.LobbyId);
                    }
                    else if (MyMultiplayer.Static is MyMultiplayerClient)
                    {
                        MySessionLoader.UnloadAndExitToMenu();
                        MyJoinGameHelper.JoinGame((MyMultiplayer.Static as MyMultiplayerClient).Server, true);
                    }
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            screen.SkipTransition = true;
            screen.CloseBeforeCallback = true;
            MyGuiSandbox.AddScreen(screen);
        }

        public static void StartLoading(Action loadingAction, string backgroundOverride = null)
        {
            if (MySpaceAnalytics.Instance != null)
            {
                MySpaceAnalytics.Instance.StoreLoadingStartTime();
            }
            MyGuiScreenGamePlay screenToLoad = new MyGuiScreenGamePlay();
            screenToLoad.OnLoadingAction = (Action) Delegate.Combine(screenToLoad.OnLoadingAction, loadingAction);
            MyGuiScreenLoading loading1 = new MyGuiScreenLoading(screenToLoad, Static, backgroundOverride, null);
            MyGuiScreenLoading screen = new MyGuiScreenLoading(screenToLoad, Static, backgroundOverride, null);
            screen.OnScreenLoadingFinished += () => MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HUDScreen, Array.Empty<object>()));
            MyGuiSandbox.AddScreen(screen);
        }

        public void SwitchCamera()
        {
            if (MySession.Static.CameraController != null)
            {
                MySession.Static.CameraController.IsInFirstPersonView = !MySession.Static.CameraController.IsInFirstPersonView;
                if (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator)
                {
                    MyEntityCameraSettings cameraSettings = null;
                    if ((MySession.Static.LocalHumanPlayer != null) && (MySession.Static.ControlledEntity != null))
                    {
                        MyThirdPersonSpectator.Static.ResetInternalTimers();
                        if (MySession.Static.Cameras.TryGetCameraSettings(MySession.Static.LocalHumanPlayer.Id, MySession.Static.ControlledEntity.Entity.EntityId, (MySession.Static.ControlledEntity is MyCharacter) && ReferenceEquals(MySession.Static.LocalCharacter, MySession.Static.ControlledEntity), out cameraSettings))
                        {
                            MyThirdPersonSpectator.Static.ResetViewerDistance(new double?(cameraSettings.Distance));
                        }
                        else
                        {
                            MyThirdPersonSpectator.Static.RecalibrateCameraPosition(false);
                            MySession.Static.ControlledEntity.ControllerInfo.Controller.SaveCamera();
                        }
                    }
                }
                MySession.Static.SaveControlledEntityCameraSettings(MySession.Static.CameraController.IsInFirstPersonView);
            }
        }

        public override void UnloadContent()
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.UnloadContent - START");
            MySandboxGame.Log.IncreaseIndent();
            base.UnloadContent();
            GC.Collect();
            Static = null;
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.UnloadContent - END");
        }

        public override void UnloadData()
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.UnloadData - START");
            MySandboxGame.Log.IncreaseIndent();
            base.UnloadData();
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyGuiScreenGamePlay.UnloadData - END");
        }

        public override bool Update(bool hasFocus)
        {
            base.Update(hasFocus);
            if ((!this.audioSet && (MySandboxGame.IsGameReady && ((MyAudio.Static != null) && (MyRenderProxy.VisibleObjectsRead != null)))) && (MyRenderProxy.VisibleObjectsRead.Count > 0))
            {
                SetAudioVolumes();
                this.audioSet = true;
                MyVisualScriptLogicProvider.GameIsReady = true;
                MyHud.MinimalHud = false;
                MyAudio.Static.EnableReverb = MySandboxGame.Config.EnableReverb && MyFakes.AUDIO_ENABLE_REVERB;
            }
            MySpectator.Static.Update();
            return true;
        }

        public static bool[] DoubleClickDetected
        {
            [CompilerGenerated]
            get => 
                <DoubleClickDetected>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<DoubleClickDetected>k__BackingField = value);
        }

        public static MyGuiScreenBase ActiveGameplayScreen
        {
            get => 
                m_activeGameplayScreen;
            set => 
                (m_activeGameplayScreen = value);
        }

        public bool CanSwitchCamera
        {
            get
            {
                if (!MyClipboardComponent.Static.Clipboard.AllowSwitchCameraMode || !MySession.Static.Settings.Enable3rdPersonView)
                {
                    return false;
                }
                MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                return ((cameraControllerEnum == MyCameraControllerEnum.Entity) || (cameraControllerEnum == MyCameraControllerEnum.ThirdPersonSpectator));
            }
        }

        public static bool SpectatorEnabled
        {
            get
            {
                if (MySession.Static == null)
                {
                    return false;
                }
                if (!MySession.Static.CreativeToolsEnabled(Sync.MyId))
                {
                    if (!MySession.Static.SurvivalMode)
                    {
                        return true;
                    }
                    if ((MyMultiplayer.Static == null) || !MySession.Static.IsUserModerator(Sync.MyId))
                    {
                        return (!MyInput.Static.ENABLE_DEVELOPER_KEYS ? MySession.Static.Settings.EnableSpectator : true);
                    }
                }
                return true;
            }
        }

        public bool MouseCursorVisible
        {
            get => 
                base.DrawMouseCursor;
            set => 
                (base.DrawMouseCursor = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenGamePlay.<>c <>9 = new MyGuiScreenGamePlay.<>c();
            public static Action <>9__24_0;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_0;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_1;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_2;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_3;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_7;
            public static Func<IMyEventOwner, Action<long>> <>9__34_8;
            public static Func<IMyEventOwner, Action<long>> <>9__34_9;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_10;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_11;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_4;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_5;
            public static MySessionComponentReplay.ActionRef<PerFrameData> <>9__34_6;
            public static Action <>9__34_13;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__34_12;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__39_0;

            internal void <HandleUnhandledInput>b__34_0(ref PerFrameData x)
            {
                UseData data = new UseData {
                    Use = true
                };
                x.UseData = new UseData?(data);
            }

            internal void <HandleUnhandledInput>b__34_1(ref PerFrameData x)
            {
                UseData data = new UseData {
                    Use = true
                };
                x.UseData = new UseData?(data);
            }

            internal void <HandleUnhandledInput>b__34_10(ref PerFrameData x)
            {
                ControlSwitchesData data = new ControlSwitchesData {
                    SwitchDamping = true
                };
                x.ControlSwitchesData = new ControlSwitchesData?(data);
            }

            internal void <HandleUnhandledInput>b__34_11(ref PerFrameData x)
            {
                ControlSwitchesData data = new ControlSwitchesData {
                    SwitchThrusts = true
                };
                x.ControlSwitchesData = new ControlSwitchesData?(data);
            }

            internal void <HandleUnhandledInput>b__34_12(MyGuiScreenMessageBox.ResultEnum callbackReturn)
            {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MyAsyncSaving.Start(() => MySector.ResetEyeAdaptation = true, null, false);
                }
            }

            internal void <HandleUnhandledInput>b__34_13()
            {
                MySector.ResetEyeAdaptation = true;
            }

            internal void <HandleUnhandledInput>b__34_2(ref PerFrameData x)
            {
                UseData data = new UseData {
                    UseContinues = true
                };
                x.UseData = new UseData?(data);
            }

            internal void <HandleUnhandledInput>b__34_3(ref PerFrameData x)
            {
                UseData data = new UseData {
                    UseFinished = true
                };
                x.UseData = new UseData?(data);
            }

            internal void <HandleUnhandledInput>b__34_4(ref PerFrameData x)
            {
                ControlSwitchesData data = new ControlSwitchesData {
                    SwitchLights = true
                };
                x.ControlSwitchesData = new ControlSwitchesData?(data);
            }

            internal void <HandleUnhandledInput>b__34_5(ref PerFrameData x)
            {
                ControlSwitchesData data = new ControlSwitchesData {
                    SwitchReactors = true
                };
                x.ControlSwitchesData = new ControlSwitchesData?(data);
            }

            internal void <HandleUnhandledInput>b__34_6(ref PerFrameData x)
            {
                ControlSwitchesData data = new ControlSwitchesData {
                    SwitchLandingGears = true
                };
                x.ControlSwitchesData = new ControlSwitchesData?(data);
            }

            internal void <HandleUnhandledInput>b__34_7(ref PerFrameData x)
            {
                ControlSwitchesData data = new ControlSwitchesData {
                    SwitchHelmet = true
                };
                x.ControlSwitchesData = new ControlSwitchesData?(data);
            }

            internal Action<long> <HandleUnhandledInput>b__34_8(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.SetDampeningEntity);

            internal Action<long> <HandleUnhandledInput>b__34_9(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.ClearDampeningEntity);

            internal void <ShowReconnectMessageBox>b__39_0(MyGuiScreenMessageBox.ResultEnum callbackReturn)
            {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    if (MyMultiplayer.Static is MyMultiplayerLobbyClient)
                    {
                        MySessionLoader.UnloadAndExitToMenu();
                        MyJoinGameHelper.JoinGame(MyMultiplayer.Static.LobbyId);
                    }
                    else if (MyMultiplayer.Static is MyMultiplayerClient)
                    {
                        MySessionLoader.UnloadAndExitToMenu();
                        MyJoinGameHelper.JoinGame((MyMultiplayer.Static as MyMultiplayerClient).Server, true);
                    }
                }
            }

            internal void <StartLoading>b__24_0()
            {
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HUDScreen, Array.Empty<object>()));
            }
        }
    }
}

