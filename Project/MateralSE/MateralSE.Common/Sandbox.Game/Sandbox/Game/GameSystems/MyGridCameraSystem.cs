namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Input;

    public class MyGridCameraSystem
    {
        private MyCubeGrid m_grid;
        private readonly List<MyCameraBlock> m_cameras;
        private readonly List<MyCameraBlock> m_relayedCameras;
        private MyCameraBlock m_currentCamera;
        private bool m_ignoreNextInput;
        private static MyHudCameraOverlay m_cameraOverlay;

        public MyGridCameraSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
            this.m_cameras = new List<MyCameraBlock>();
            this.m_relayedCameras = new List<MyCameraBlock>();
        }

        private void AddValidCamerasFromGridToRelayed(MyCubeGrid grid)
        {
            foreach (MyCameraBlock block in grid.GridSystems.TerminalSystem.Blocks)
            {
                if (block == null)
                {
                    continue;
                }
                if (block.IsWorking && block.HasLocalPlayerAccess())
                {
                    this.m_relayedCameras.Add(block);
                }
            }
        }

        private void AddValidCamerasFromGridToRelayed(long gridId)
        {
            MyCubeGrid grid;
            MyEntities.TryGetEntityById<MyCubeGrid>(gridId, out grid, false);
            if (grid != null)
            {
                this.AddValidCamerasFromGridToRelayed(grid);
            }
        }

        public static bool CameraIsInRangeAndPlayerHasAccess(MyCameraBlock camera)
        {
            MyIDModule module;
            return (((MySession.Static.ControlledEntity != null) && ((!((IMyComponentOwner<MyIDModule>) camera).GetComponent(out module) || camera.HasPlayerAccess(MySession.Static.LocalPlayerId)) || (module.Owner == 0))) && (!(MySession.Static.ControlledEntity is MyCharacter) ? (!(MySession.Static.ControlledEntity is MyShipController) ? ((MySession.Static.ControlledEntity is MyCubeBlock) && MyAntennaSystem.Static.CheckConnection((MySession.Static.ControlledEntity as MyCubeBlock).CubeGrid, camera.CubeGrid, MySession.Static.LocalHumanPlayer, true)) : MyAntennaSystem.Static.CheckConnection((MySession.Static.ControlledEntity as MyShipController).CubeGrid, camera.CubeGrid, MySession.Static.LocalHumanPlayer, true)) : MyAntennaSystem.Static.CheckConnection(MySession.Static.LocalCharacter, camera.CubeGrid, MySession.Static.LocalHumanPlayer, true)));
        }

        public void CheckCurrentCameraStillValid()
        {
            if ((this.m_currentCamera != null) && !this.m_currentCamera.IsWorking)
            {
                this.ResetCamera();
            }
        }

        private void DisableCameraEffects()
        {
            MyHudCameraOverlay.Enabled = false;
            MyHud.CameraInfo.Disable();
            MySector.MainCamera.FieldOfView = MySandboxGame.Config.FieldOfView;
        }

        private MyCameraBlock GetNext(MyCameraBlock current)
        {
            if (this.m_relayedCameras.Count == 1)
            {
                return current;
            }
            int index = this.m_relayedCameras.IndexOf(current);
            if (index != -1)
            {
                return this.m_relayedCameras[(index + 1) % this.m_relayedCameras.Count];
            }
            this.ResetCamera();
            return null;
        }

        private MyCameraBlock GetPrev(MyCameraBlock current)
        {
            if (this.m_relayedCameras.Count == 1)
            {
                return current;
            }
            int index = this.m_relayedCameras.IndexOf(current);
            if (index == -1)
            {
                this.ResetCamera();
                return null;
            }
            int num2 = index - 1;
            if (num2 < 0)
            {
                num2 = this.m_relayedCameras.Count - 1;
            }
            return this.m_relayedCameras[num2];
        }

        public void PrepareForDraw()
        {
            MyCameraBlock currentCamera = this.m_currentCamera;
        }

        public void Register(MyCameraBlock camera)
        {
            this.m_cameras.Add(camera);
            this.m_grid.MarkForUpdate();
        }

        public void ResetCamera()
        {
            Vector3D? nullable;
            this.ResetCurrentCamera();
            this.DisableCameraEffects();
            bool flag = false;
            if (PreviousNonCameraBlockController != null)
            {
                MyEntity previousNonCameraBlockController = PreviousNonCameraBlockController as MyEntity;
                if ((previousNonCameraBlockController != null) && !previousNonCameraBlockController.Closed)
                {
                    nullable = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, previousNonCameraBlockController, nullable);
                    PreviousNonCameraBlockController = null;
                    flag = true;
                }
            }
            if (!flag && (MySession.Static.LocalCharacter != null))
            {
                nullable = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.LocalCharacter, nullable);
            }
        }

        public void ResetCurrentCamera()
        {
            if (this.m_currentCamera != null)
            {
                this.m_currentCamera.OnExitView();
                this.m_currentCamera = null;
            }
        }

        public void SetAsCurrent(MyCameraBlock newCamera)
        {
            if (!ReferenceEquals(this.m_currentCamera, newCamera))
            {
                if (newCamera.BlockDefinition.OverlayTexture == null)
                {
                    MyHudCameraOverlay.Enabled = false;
                }
                else
                {
                    MyHudCameraOverlay.TextureName = newCamera.BlockDefinition.OverlayTexture;
                    MyHudCameraOverlay.Enabled = true;
                }
                string shipName = "";
                if (MyAntennaSystem.Static != null)
                {
                    string displayName = MyAntennaSystem.Static.GetLogicalGroupRepresentative(this.m_grid).DisplayName;
                    shipName = displayName ?? "";
                }
                MyHud.CameraInfo.Enable(shipName, newCamera.DisplayNameText);
                this.m_currentCamera = newCamera;
                this.m_ignoreNextInput = true;
                MySessionComponentVoxelHand.Static.Enabled = false;
                MySession.Static.GameFocusManager.Clear();
                this.m_grid.MarkForUpdate();
            }
        }

        private void SetCamera(MyCameraBlock newCamera)
        {
            if (!ReferenceEquals(newCamera, this.m_currentCamera))
            {
                if (this.m_cameras.Contains(newCamera))
                {
                    this.SetAsCurrent(newCamera);
                    newCamera.SetView();
                }
                else
                {
                    MyHudCameraOverlay.Enabled = false;
                    MyHud.CameraInfo.Disable();
                    this.ResetCurrentCamera();
                    newCamera.RequestSetView();
                }
            }
        }

        private void SetNext()
        {
            this.UpdateRelayedCameras();
            MyCameraBlock next = this.GetNext(this.m_currentCamera);
            if (next != null)
            {
                this.SetCamera(next);
            }
        }

        private void SetPrev()
        {
            this.UpdateRelayedCameras();
            MyCameraBlock prev = this.GetPrev(this.m_currentCamera);
            if (prev != null)
            {
                this.SetCamera(prev);
            }
        }

        public void Unregister(MyCameraBlock camera)
        {
            if (ReferenceEquals(camera, this.m_currentCamera))
            {
                this.ResetCamera();
            }
            this.m_cameras.Remove(camera);
            this.m_grid.MarkForUpdate();
        }

        public void UpdateBeforeSimulation()
        {
            if (this.m_currentCamera != null)
            {
                if (!ReferenceEquals(MySession.Static.CameraController, this.m_currentCamera))
                {
                    if (!(MySession.Static.CameraController is MyCameraBlock))
                    {
                        this.DisableCameraEffects();
                    }
                    this.ResetCurrentCamera();
                }
                else if (this.m_ignoreNextInput)
                {
                    this.m_ignoreNextInput = false;
                }
                else
                {
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SWITCH_LEFT) && (MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay))
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                        this.SetPrev();
                    }
                    if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SWITCH_RIGHT) && (MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay))
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                        this.SetNext();
                    }
                    if (((MyInput.Static.DeltaMouseScrollWheelValue() != 0) && (MyGuiScreenToolbarConfigBase.Static == null)) && !MyGuiScreenTerminal.IsOpen)
                    {
                        this.m_currentCamera.ChangeZoom(MyInput.Static.DeltaMouseScrollWheelValue());
                    }
                }
            }
        }

        public void UpdateBeforeSimulation10()
        {
            if ((this.m_currentCamera != null) && !CameraIsInRangeAndPlayerHasAccess(this.m_currentCamera))
            {
                this.ResetCamera();
            }
        }

        private void UpdateRelayedCameras()
        {
            List<MyAntennaSystem.BroadcasterInfo> list1 = MyAntennaSystem.Static.GetConnectedGridsInfo(this.m_grid, null, true).ToList<MyAntennaSystem.BroadcasterInfo>();
            List<MyAntennaSystem.BroadcasterInfo> list2 = MyAntennaSystem.Static.GetConnectedGridsInfo(this.m_grid, null, true).ToList<MyAntennaSystem.BroadcasterInfo>();
            list2.Sort((b1, b2) => b1.EntityId.CompareTo(b2.EntityId));
            this.m_relayedCameras.Clear();
            foreach (MyAntennaSystem.BroadcasterInfo info in list2)
            {
                this.AddValidCamerasFromGridToRelayed(info.EntityId);
            }
            if (this.m_relayedCameras.Count == 0)
            {
                this.AddValidCamerasFromGridToRelayed(this.m_grid);
            }
        }

        public int CameraCount =>
            this.m_cameras.Count;

        public MyCameraBlock CurrentCamera =>
            this.m_currentCamera;

        public static IMyCameraController PreviousNonCameraBlockController
        {
            [CompilerGenerated]
            get => 
                <PreviousNonCameraBlockController>k__BackingField;
            [CompilerGenerated]
            set => 
                (<PreviousNonCameraBlockController>k__BackingField = value);
        }

        public bool NeedsPerFrameUpdate =>
            (this.m_currentCamera != null);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGridCameraSystem.<>c <>9 = new MyGridCameraSystem.<>c();
            public static Comparison<MyAntennaSystem.BroadcasterInfo> <>9__31_0;

            internal int <UpdateRelayedCameras>b__31_0(MyAntennaSystem.BroadcasterInfo b1, MyAntennaSystem.BroadcasterInfo b2) => 
                b1.EntityId.CompareTo(b2.EntityId);
        }
    }
}

